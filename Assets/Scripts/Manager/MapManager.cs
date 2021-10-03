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
                for(int i=70; i<random; i+=23)  //¾à 30ÆÛ È®·ü·Î ÇÑ °³ ¶³¾îÁö°í ¾à 7ÆÛ È®·ü·Î 2°³ ¶³¾îÁü
                {
                    PoolManager.GetItem<Meteor>();
                }
            }
            else
            {
                for (int i = 50; i < random; i += 20) //¾à 50ÆÛ È®·ü·Î ÇÑ °³, 30ÆÛ 2°³ 10ÆÛ 3°³
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
