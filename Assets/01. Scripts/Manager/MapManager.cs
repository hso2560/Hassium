using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public enum MapType  //mapList�� ������ �����
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
    public List<int> mapObjIndexList; //mapList�� ���� ������Ʈ ������ SceneSaveObjects�� obj���� �� ������ �ش��ϴ� �ε��� �� ������� �Ѵ�
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

    public void SetWeather() //���� ���� �ݿ�
    {
        string wt = System.IO.File.ReadAllText(string.Concat(Application.persistentDataPath, "/", "wt01"));
        string[] strs = wt.Split('$');

        if (strs[0] != "NONE")  //����� ���� ���� �޾Ҵٸ�
        {
            string[] strs2 = strs[0].Split('#');
            if(strs2[0].Contains("��") || strs2[0].Contains("��")) //���� ���ų� �帲. �׸��� ����ų� �� ���ų� ��� �� ���ÿ� ��
            {
                if(strs2[0].Contains("/"))  //���� �� ���ÿ�
                {
                    rain.SetActive(true);
                    snow.SetActive(true);
                    rainLocation.SetActive(false);
                }
                else  
                {
                    if(strs2[0].Contains("��"))  //��
                    {
                        ChangeSky(1);
                        rain.SetActive(true);
                        rainLocation.SetActive(false);
                    }
                    else  //��
                    {
                        snow.SetActive(true);
                    }
                }
            }
            else
            {
                switch (strs2[0])
                {
                    case "����":
                        ChangeSky(5);
                        break;
                    case "���� ����":
                        //�� ���� ����
                        break;
                    case "���� ����":
                        ChangeSky(1);
                        break;
                    case "�帲":
                        ChangeSky(1);
                        break;
                    default:
                        //�⺻��
                        break;
                }
            }

            float avgTp = float.Parse(strs2[1]);
            if(-90f<avgTp && avgTp<100f)
            {
                colorGrading.temperature.value = Mathf.Clamp(avgTp-9f , minTemp, maxTemp);  //-9f�� ������
            }
        }

        //���� �ð� ���� (����� �� ����) ������ �������
        curTime = int.Parse(strs[1]);
        ApplyRealTime(true);
    }

    public void ApplyRealTime(bool on)
    {
        if (on)
        {
            if (curTime >= 5 && curTime <= 15) //���� 5~���� 3
            {
                colorGrading.postExposure.value = maxPostExposure * (curTime - 4) / 11f;  //5-1=4 , 15-5+1=11
            }
            else if (curTime >= 16 && curTime <= 23) //���� 4~11 
            {
                dayAndNight.isNight = true;
                colorGrading.postExposure.value = minPostExposure * (curTime - 15) / 8f;
                lightLens.brightness = 0.2f;
                if (colorGrading.postExposure.value < -1f) ChangeSky(0);  //or 2
            }
            else  //���� 0~ 4
            {
                dayAndNight.isNight = true;
                if (curTime == 24)
                {
                    colorGrading.postExposure.value = minPostExposure;
                }
                else
                {
                    colorGrading.postExposure.value = minPostExposure * (5 - curTime) / 6f;  //���������� �и� 1�� ����
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
    
    public void ResetWeather() //����, �ð� ���� �ʱ�ȭ
    {
        ApplyRealTime(false);
        colorGrading.temperature.value = 0;
        rain.SetActive(false);
        snow.SetActive(false);
        StartCoroutine(GameManager.Instance.FuncHandlerCo(1.5f,null,null,()=> 
        {
            if (!rainLocation.activeSelf) SoundManager.Instance.PlayBGM(BGMSound.NULL);
            rainLocation.SetActive(true);  //�̷��� ������ ������ ���� ����� �����ϰ�� ÷�� �ణ ���Ҹ� �鸮�� ����. ������ �ð��� �󸶾����� �ϴ� �̷����ϰ� �Ѿ��
        }));
    }

    private void InitData()
    {
        MeteorIE = MeteorCo();
        StartCoroutine(MeteorIE);
    }

    public void ActiveMap(MapType type)  //���� Ȱ��ȭ�ϰ� ������ ��Ȱ
    {
        for(int i=0; i<mapList.Count; i++)
        {
            bool active = (i == (int)type);

            mapList[i].SetActive(active);
            GameManager.Instance.SaveObjActiveInfo(mapObjIndexList[i], active);
        }
    }

    /*public void ResetLivingEnemy()  //����ִ� ������ ���¸� �ʱ�ȭ
    {
        enemys.FindAll(x => x.NeedReset()).ForEach(a => a.ResetData());
    }*/

    private IEnumerator MeteorCo()  //���� ������ (�ְų� ���ų���. ������ ���� ����)
    {
        while (true)
        {
            yield return new WaitForSeconds(!dayAndNight.isNight ? Random.Range(140f, 200f) : Random.Range(30f, 55f));

            int random = Random.Range(0, 100);

            if (!dayAndNight.isNight)
            {
                for(int i=70; i<random; i+=23)  //�� 30�� Ȯ���� �� �� �������� �� 7�� Ȯ���� 2�� ������
                {
                    PoolManager.GetItem<Meteor>();
                }
            }
            else
            {
                for (int i = 50; i < random; i += 20) //�� 50�� Ȯ���� �� ��, 30�� 2�� 10�� 3��
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

    public void ChangeSky(int index)  //�ϴ� ��ü
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
