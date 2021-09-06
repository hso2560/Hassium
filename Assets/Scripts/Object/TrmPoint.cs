using UnityEngine;

public class TrmPoint : MonoBehaviour
{
    [SerializeField] private short id;

    private GameManager gameManager;
    private UIManager uiManager;

    [SerializeField] private short mapIdx;

    private void Start()
    {
        gameManager = GameManager.Instance;
        uiManager = UIManager.Instance;

        if(id>=0)
           MapManager.Instance.mapCenterDict.Add(id, transform);
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
        if(id==-10)  //이곳에 닿으면 유저의 맵 포인트를 바꿈 (mapIdx로)
        {
            if(other.CompareTag("Player"))
            {
                gameManager.savedData.userInfo.mapIndex = mapIdx;
            }
        }
    }
}
