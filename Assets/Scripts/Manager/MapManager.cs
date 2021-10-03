using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoSingleton<MapManager>, ISceneDataLoad
{
    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public Dictionary<short, Transform> mapCenterDict = new Dictionary<short, Transform>();
    public DayAndNight dayAndNight;
    private IEnumerator MeteorIE=null;

    [SerializeField] private bool isDevMode;

    private void InitData()
    {
        MeteorIE = MeteorCo();
        StartCoroutine(MeteorIE);
    }

    private IEnumerator MeteorCo()
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
}
