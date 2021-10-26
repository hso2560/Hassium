using UnityEngine;

public class TrmPoint : MonoBehaviour
{
    [SerializeField] private short id;

    private GameManager gameManager;
    private UIManager uiManager;

    [SerializeField] private short mapIdx;

    [SerializeField] private MapType mapType;

    public Vector3 camMinPos, camMaxPos;
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
        switch (id)  //��ü�� other.CompareTag("Player") ���ǹ� �� ���� �ְ����� �ٸ� ���� ����� �� ������ ó���ϴ� �� ���� ���� ������ �ϴ� �̷���
        {
            case -10:
                if (other.CompareTag("Player"))  //�̰��� ������ ������ �� ����Ʈ�� �ٲ� (mapIdx��)
                {
                    gameManager.savedData.userInfo.mapIndex = mapIdx;
                }
                break;

            case -50:
                if (other.CompareTag("Player"))
                {
                    gameManager.eventPointAction[eventId]();
                    SaveActive(false);
                }
                break;

            case -100:
                if(other.CompareTag("Player")) //�� ��ȯ
                {
                    gameManager.LoadingFuncEvent += () =>
                    {
                        MapManager.Instance.ActiveMap(mapType);
                        gameManager.savedData.userInfo.mapIndex = mapIdx;
                        gameManager.camMove.camMinPos = camMinPos;
                        gameManager.camMove.camMaxPos = camMaxPos;
                        GameManager.Instance.keyToVoidFunction[LoadingType.RESPAWN]();
                    };
                    UIManager.Instance.LoadingFade(false);
                }
                break;
        }
    }

    private void SaveActive(bool active)
    {
        GameManager.Instance.SaveObjActiveInfo(mapIdx, active); //mapIdx�� objIndex�� ����Ѵ�(������)
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