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
        if (id == -1) //�̰��� ������ ĳ���� ������
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
        if(id==-10)  //�̰��� ������ ������ �� ����Ʈ�� �ٲ� (mapIdx��)
        {
            if(other.CompareTag("Player"))
            {
                gameManager.savedData.userInfo.mapIndex = mapIdx;
            }
        }
    }
}
