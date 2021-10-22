using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public List<GameObject> mapList;
    public List<int> mapObjIndexList; //mapList의 게임 오브젝트 순서와 SceneSaveObjects의 obj에서 그 옵젝에 해당하는 인덱스 값 순서대로 한다

    [SerializeField] private bool isDevMode;

    private void InitData()
    {
        MeteorIE = MeteorCo();
        StartCoroutine(MeteorIE);
    }

    public void ActiveMap(MapType type)
    {
        for(int i=0; i<mapList.Count; i++)
        {
            bool active = (i == (int)type);

            mapList[i].SetActive(active);
            GameManager.Instance.SaveObjActiveInfo(mapObjIndexList[i], active);
        }
    }

    private IEnumerator MeteorCo()
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

    public void ChangeSky(int index)
    {
        RenderSettings.skybox = dayAndNight.skyMaterials[index];
        GameManager.Instance.savedData.userInfo.skyIndex = index;
    }
}
