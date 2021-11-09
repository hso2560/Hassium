using UnityEngine;

public class TimeAttack1 : ObjData, IReward
{
    public float limitTime = 5f;

    private bool isTrying = false;

    public GameObject[] needItems;
    public int itemID;

    private int current;
    [SerializeField] int maxCnt = 2;

    [SerializeField] NPCAI npc;
    public string npcFightingClearMsg;
    public int chestId;

    private void Start()
    {
        base.BaseStart();
    }

    public override void Interaction()
    {
        if (UIManager.Instance.runningMission && UIManager.Instance.missionObj != gameObject)
        {
            PoolManager.GetItem<SystemTxt>().OnText("���� �ٸ� �̼��� �������Դϴ�.");
            return;
        }

        transform.GetChild(0).GetComponent<Animator>().SetTrigger("move");
        if (isTrying)
        {
            UIManager.Instance.TimeAttackMission(false);
            return;
        }

        {
            for (int i = 0; i < needItems.Length; i++) needItems[i].SetActive(true);
            isTrying = true;
            objName = "����";
        }

        {
            UIManager.Instance.clearEvent += () =>
            {
                Inventory.Instance.UseItem(itemID, Inventory.Instance.idToItem[itemID].count);
                Inventory.Instance.acquisitionEvent -= ItemAcquisition;

                active = false;
                base.Interaction();
                isTrying = false;

                npc.info.talkId=1;

                if (npc != null && !npc.info.dead && (npc.info.isFighting || npc.info.bRunaway))
                    PoolManager.GetItem<SystemTxt>().OnText(npcFightingClearMsg);
                else
                    GetReward();
            };
        }
        {
            UIManager.Instance.timeOverEvent += ()=>
            {
                for (int i = 0; i < needItems.Length; i++) needItems[i].SetActive(false);

                Inventory.Instance.acquisitionEvent -= ItemAcquisition;
                if (Inventory.Instance.ExistItem(itemID))
                {
                    Inventory.Instance.UseItem(itemID, Inventory.Instance.idToItem[itemID].count);
                }
                isTrying = false;
                objName = "����";
            };
        }

        UIManager.Instance.OnTimer((int)limitTime, true);
        UIManager.Instance.UpdateCountInMission(current,maxCnt);

        Inventory.Instance.acquisitionEvent += ItemAcquisition;
    }

    void ItemAcquisition(int id)
    {
        if(id==itemID)
        {
            UIManager.Instance.UpdateCountInMission(current, maxCnt);
            if(current==maxCnt)
            {
                UIManager.Instance.TimeAttackMission(true);
            }
        }
    }

    public void GetReward()
    {
        if (!GameManager.Instance.IsContainChest(chestId))
        {
            PuzzleReward.RequestReward(id);
        }
        else
        {
            PoolManager.GetItem<SystemTxt>().OnText("�̰��� �������ڴ� �̹� ���������ϴ�.");
        }
    }
}
