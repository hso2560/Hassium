using UnityEngine;

public class TrmPoint : MonoBehaviour
{
    [SerializeField] private short id;

    private GameManager gameManager;
    private UIManager uiManager;

    [SerializeField] private short mapIdx;

    [SerializeField] private MapType mapType;

    //public Vector3 camMinPos, camMaxPos;  // x:-1607.23 y:-300 z:-332.5   x:-610.33 y:300 z:664.04
    [SerializeField] private int eventId;

    private void Start()
    {
        gameManager = GameManager.Instance;
        uiManager = UIManager.Instance;

        if (id >= 0)
            MapManager.Instance.mapCenterDict.Add(id, transform);
        else if (id == -50)
            SaveActive(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (id == -1) //이곳에 맞으면 캐릭터 리스폰
        {
            if (collision.transform.CompareTag("Player"))
            {
                gameManager.LoadingFuncEvent += gameManager.keyToVoidFunction[LoadingType.RESPAWN];
                uiManager.LoadingFade(false);
                //gameManager.PlayerSc.transform.position = MapManager.Instance.mapCenterDict[gameManager.savedData.userInfo.mapIndex].position;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (id)  //전체에 other.CompareTag("Player") 조건문 달 수도 있겠지만 다른 옵젝 닿았을 때 뭔가를 처리하는 게 나올 수도 있으니 일단 이렇게
        {
            case -10:
                if (other.CompareTag("Player"))  //이곳에 닿으면 유저의 맵 포인트를 바꿈 (mapIdx로)
                {
                    gameManager.savedData.userInfo.mapIndex = mapIdx;
                }
                break;

            case -50:
                if (other.CompareTag("Player"))  //특정 이벤트 발생
                {
                    gameManager.eventPointAction[eventId]();
                    SaveActive(false);
                }
                break;

            case -100:
                if(other.CompareTag("Player")) //맵 전환
                {
                    gameManager.LoadingFuncEvent += () =>
                    {
                        MapManager.Instance.ActiveMap(mapType);
                        gameManager.savedData.userInfo.mapIndex = mapIdx;
                        //gameManager.camMove.camMinPos = camMinPos;
                        //gameManager.camMove.camMaxPos = camMaxPos;
                        GameManager.Instance.keyToVoidFunction[LoadingType.RESPAWN]();
                    };
                    UIManager.Instance.LoadingFade(false);
                }
                break;

            case -200:                //비
                MapManager.Instance.ChangeSky(1);
                SoundManager.Instance.PlayerBGM(BGMSound.RAIN);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch(id)
        {
            case -200:
                MapManager.Instance.ChangeSky(3);
                SoundManager.Instance.PlayerBGM(BGMSound.NULL);
                break;
        }
    }

    private void SaveActive(bool active)
    {
        GameManager.Instance.SaveObjActiveInfo(mapIdx, active); //mapIdx가 objIndex를 대신한다(변수명)
        if (!active) gameObject.SetActive(active);
    }

    /*private void OnEnable()
    {
        if (id == -50)
            GameManager.Instance.SaveObjActiveInfo(mapIdx, true); 
    }

    private void OnDisable()
    {
        if (id == -50)
            GameManager.Instance.SaveObjActiveInfo(mapIdx, false); 
    }*/
}
