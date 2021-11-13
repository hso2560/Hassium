using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public enum MapType  //mapList와 순서를 맞춘다
{
    TUTORIAL,
    MAINMAP
}

public class MapManager : MonoSingleton<MapManager>, ISceneDataLoad
{
    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public Dictionary<short, Transform> mapCenterDict = new Dictionary<short, Transform>();
    public DayAndNight dayAndNight;
    private IEnumerator MeteorIE=null;
    public LensFlare lightLens;

    public List<GameObject> mapList;
    public List<int> mapObjIndexList; //mapList의 게임 오브젝트 순서와 SceneSaveObjects의 obj에서 그 옵젝에 해당하는 인덱스 값 순서대로 한다
    //public List<EnemyBase> enemys;

    public PostProcessVolume postVolume;
    private ColorGrading colorGrading;

    [SerializeField] private float minPostExposure = -3f;
    [SerializeField] private float maxPostExposure = 1.5f;

    [SerializeField] private float minTemp = -7f;
    [SerializeField] private float maxTemp = 15f;

    public GameObject rain, snow;
    public GameObject rainLocation;

    [SerializeField] private bool isDevMode;
    [SerializeField] GameObject testMark;

    private int curTime;

    private void Awake()
    {
        testMark.SetActive(isDevMode);
        postVolume.profile.TryGetSettings(out colorGrading);
        SetWeather();
    }

    private void Start()
    {
        if (!rainLocation.activeSelf) SoundManager.Instance.PlayBGM(BGMSound.RAIN);
    }

    public void SetWeather() //현실 날씨 반영
    {
        string wt = System.IO.File.ReadAllText(string.Concat(Application.persistentDataPath, "/", "wt01"));
        string[] strs = wt.Split('$');

        if (strs[0] != "NONE")  //제대로 날씨 정보 받았다면
        {
            string[] strs2 = strs[0].Split('#');
            if(strs2[0].Contains("비") || strs2[0].Contains("눈")) //구름 많거나 흐림. 그리고 비오거나 눈 오거나 비와 눈 동시에 옴
            {
                if(strs2[0].Contains("/"))  //눈과 비 동시에
                {
                    rain.SetActive(true);
                    snow.SetActive(true);
                    rainLocation.SetActive(false);
                }
                else  
                {
                    if(strs2[0].Contains("비"))  //비
                    {
                        ChangeSky(1);
                        rain.SetActive(true);
                        rainLocation.SetActive(false);
                    }
                    else  //눈
                    {
                        snow.SetActive(true);
                    }
                }
            }
            else
            {
                switch (strs2[0])
                {
                    case "맑음":
                        ChangeSky(5);
                        break;
                    case "구름 조금":
                        //뭐 딱히 없음
                        break;
                    case "구름 많음":
                        ChangeSky(1);
                        break;
                    case "흐림":
                        ChangeSky(1);
                        break;
                    default:
                        //기본값
                        break;
                }
            }

            float avgTp = float.Parse(strs2[1]);
            if(-90f<avgTp && avgTp<100f)
            {
                colorGrading.temperature.value = Mathf.Clamp(avgTp-9f , minTemp, maxTemp);  //-9f는 보정값
            }
        }

        //현재 시간 대입 (사용자 폰 기준) 어차피 상관없음
        curTime = int.Parse(strs[1]);
        ApplyRealTime(true);
    }

    public void ApplyRealTime(bool on)
    {
        if (on)
        {
            if (curTime >= 5 && curTime <= 15) //오전 5~오후 3
            {
                colorGrading.postExposure.value = maxPostExposure * (curTime - 4) / 11f;  //5-1=4 , 15-5+1=11
            }
            else if (curTime >= 16 && curTime <= 23) //오후 4~11 
            {
                dayAndNight.isNight = true;
                colorGrading.postExposure.value = minPostExposure * (curTime - 15) / 8f;
                lightLens.brightness = 0.2f;
                if (colorGrading.postExposure.value < -1f) ChangeSky(0);  //or 2
            }
            else  //오전 0~ 4
            {
                dayAndNight.isNight = true;
                if (curTime == 24)
                {
                    colorGrading.postExposure.value = minPostExposure;
                }
                else
                {
                    colorGrading.postExposure.value = minPostExposure * (5 - curTime) / 6f;  //보정값으로 분모에 1을 더함
                }
                lightLens.brightness = 0.2f;
                if (colorGrading.postExposure.value < -1f) ChangeSky(0); //or 2
            }
        }
        else
        {
            if(RenderSettings.skybox == dayAndNight.skyMaterials[0]) ChangeSky(3);

            dayAndNight.isNight = false;
            colorGrading.postExposure.value = 0.9f;
            lightLens.brightness = 0.7f;
        }
    }
    
    public void ResetWeather() //날씨, 시간 상태 초기화
    {
        ApplyRealTime(false);
        colorGrading.temperature.value = 0;
        rain.SetActive(false);
        snow.SetActive(false);
        StartCoroutine(GameManager.Instance.FuncHandlerCo(1.5f,null,null,()=> 
        {
            if (!rainLocation.activeSelf) SoundManager.Instance.PlayBGM(BGMSound.NULL);
            rainLocation.SetActive(true);  //이러면 딜레이 때문에 만약 비오는 날씨일경우 첨에 약간 빗소리 들리고 꺼짐. 하지만 시간이 얼마없으니 일단 이렇게하고 넘어간다
        }));
    }

    private void InitData()
    {
        MeteorIE = MeteorCo();
        StartCoroutine(MeteorIE);
    }

    public void ActiveMap(MapType type)  //맵을 활성화하고 나머지 비활
    {
        for(int i=0; i<mapList.Count; i++)
        {
            bool active = (i == (int)type);

            mapList[i].SetActive(active);
            GameManager.Instance.SaveObjActiveInfo(mapObjIndexList[i], active);
        }
    }

    /*public void ResetLivingEnemy()  //살아있는 적들의 상태를 초기화
    {
        enemys.FindAll(x => x.NeedReset()).ForEach(a => a.ResetData());
    }*/

    private IEnumerator MeteorCo()  //유성 떨어짐 (있거나 없거나임. 어차피 보기 힘듦)
    {
        while (true)
        {
            yield return new WaitForSeconds(!dayAndNight.isNight ? Random.Range(140f, 200f) : Random.Range(30f, 55f));

            int random = Random.Range(0, 100);

            if (!dayAndNight.isNight)
            {
                for(int i=70; i<random; i+=23)  //약 30퍼 확률로 한 개 떨어지고 약 7퍼 확률로 2개 떨어짐
                {
                    PoolManager.GetItem<Meteor>();
                }
            }
            else
            {
                for (int i = 50; i < random; i += 20) //약 50퍼 확률로 한 개, 30퍼 2개 10퍼 3개
                    PoolManager.GetItem<Meteor>();
            }
        }
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        MapManager[] managers = FindObjectsOfType<MapManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        if (MeteorIE != null) StopCoroutine(MeteorIE);

        if (this.sceneObjs.ScType == SceneType.MAIN)
        {
            InitData();
        }

        isReady = true;
    }

    public void ChangeSky(int index)  //하늘 교체
    {
        RenderSettings.skybox = dayAndNight.skyMaterials[index];
        //GameManager.Instance.savedData.userInfo.skyIndex = index;
        /*switch(index)
        {
            case 1:
                dayAndNight.mainLight.intensity = 0.6f;
                break;
            case 2:
                dayAndNight.mainLight.intensity = 0;
                break;
            case 3:
                dayAndNight.mainLight.intensity = 1.3f;
                break;
        }*/
    }
}
