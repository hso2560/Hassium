using System.Collections.Generic;
using UnityEngine;

public class TreeAPuzzle : ObjData, IReward
{
    public bool IsStart { get; set; }
    [SerializeField] private float limitTime;

    [SerializeField] private List<TreeA> treeList;
    private List<TreeA> questTrees = new List<TreeA>();  //변해진 나무(때려야되는 나무들)
    private float changeTime;
    [SerializeField] private float changeDelay = 24f;
    public Material changeMat;

    /*public GameObject dropObject;
    public List<GameObject> dropObjList = new List<GameObject>();*/
    [SerializeField] private int catchMaxCount = 14; //나무를 총 몇 번 바꾸어야 하는가
    private int currentCatchCount = 0;
    [SerializeField] private int maxHp = 5;  //나무 체력
    public int MaxHp { get { return maxHp; } }

    public string startObjName = "시작";
    public string resetObjName = "초기화";
    public string npcFightingClearMsg;  //이 퍼즐 관리하는 NPC있고 NPC가 살아있고 NPC가 싸우거나 도망가는 중에 퍼즐을 클리어하면 띄울 메시지
    [SerializeField] private int chestId;

    [SerializeField] private NPCAI npc;

    private void Start()
    {
        base.BaseStart(() =>
        {
            //dropObjList = FunctionGroup.CreatePoolList(dropObject, transform, 6);
            treeList = new List<TreeA>(transform.parent.GetComponentsInChildren<TreeA>());
        }, null);
    }

    public override void Interaction()
    {
        if (UIManager.Instance.runningMission && UIManager.Instance.missionObj != gameObject)
        {
            PoolManager.GetItem<SystemTxt>().OnText("현재 다른 미션을 수행중입니다.");
            return;
        }

        transform.GetChild(0).GetComponent<Animator>().SetTrigger("move");
        if (IsStart)
        {
            UIManager.Instance.TimeAttackMission(false);
            return;
        }

        {
            UIManager.Instance.missionObj = gameObject;
            foreach (int x in FunctionGroup.GetRandomList(treeList.Count, 3))
            {
                treeList[x].active = true;
                treeList[x].mainMesh.material = changeMat;
                questTrees.Add(treeList[x]);
            }
            IsStart = true;
            objName = resetObjName;
            changeTime = Time.time + changeDelay;
            UIManager.Instance.UpdateCountInMission(currentCatchCount, catchMaxCount);
        }

        {
            UIManager.Instance.clearEvent += () =>
            {
                IsStart = false;
                active = false;
                base.Interaction();
                npc.info.talkId++;

                if(npc!=null && !npc.info.dead && (npc.info.isFighting || npc.info.bRunaway) )
                    PoolManager.GetItem<SystemTxt>().OnText(npcFightingClearMsg);
                else 
                    GetReward();
            };
            UIManager.Instance.timeOverEvent += () =>
            {
                treeList.ForEach(x => x.ResetData());
                objName = startObjName;
                currentCatchCount = 0;
                questTrees.Clear();
                IsStart = false;
                active = false;
                Invoke("DelayActive", 2);
            };

            UIManager.Instance.OnTimer((int)limitTime,true);
        }
    }

    private void Update()
    {
        if (IsStart)
        {
            if (changeTime < Time.time)
            {
                PoolManager.GetItem<SystemTxt>().OnText("나무들에게 변화가 일어납니다.",2);
                questTrees.ForEach(x => x.ResetData());
                questTrees.Clear();
                foreach (int x in FunctionGroup.GetRandomList(treeList.Count, 4))
                {
                    treeList[x].active = true;
                    treeList[x].mainMesh.material = changeMat;
                    questTrees.Add(treeList[x]);
                }

                changeTime = Time.time + changeDelay;
            }
        }
    }

    public void CheckCount()
    {
        currentCatchCount++;
        UIManager.Instance.UpdateCountInMission(currentCatchCount, catchMaxCount);
        if(currentCatchCount==catchMaxCount)
        {
            UIManager.Instance.TimeAttackMission(true);
        }
    }

    private void DelayActive() => active = true;

    public void GetReward()
    {
        if(GameManager.Instance.IsContainChest(chestId))
        {
            PuzzleReward.RequestReward(id);
        }
        else
        {
            PoolManager.GetItem<SystemTxt>().OnText("이곳의 보물상자는 이미 가져갔습니다.");
        }
    }
}
