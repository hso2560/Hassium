using DG.Tweening;
using UnityEngine;

public class CircleAPuzzle : ObjData, IReward  //원래는 퍼즐 룰 이것도 부모 스크립트 만들고 룰 스크립트들이 그걸 상속받아야함
{
    private int currentId = 1;
    public GameObject movingSphere, movingMagnet;
    private RigidMagnet magnet;
    public Transform defaultPosition;
    public float magnetMoveInterval = 4.5f;
    public int[] magnetObjsRemoveIds; //이 아이디 위치에 공이 존재하면 자석에 붙은 옵젝들 해제

    private Vector3 orgMagnetPos;
    public bool IsMoving { get; set; }

    [SerializeField] private int targetId = 9;
    [SerializeField] private int needMoveCount = 4;  //needMoveCount번째마다 자석에 붙은 옵젝들 버려야 함
    [SerializeField] private int dumpCycle = 4;  // dumpCycle에 needMoveCount를 곱한 수 만큼 옵젝 움직였을 때 정확히 목표 위치에 공이 있어야 함
    private int maxMoveCount; // needMoveCount * dumpCycle
    private int curMoveCnt = 0;

    [SerializeField] private int magnetIndex;
    [SerializeField] private int sphereIndex;

    [SerializeField] private int chestId;
    [SerializeField] private NPCAI npc;

    public string npcFightingClearMsg;

    private void Awake()
    {
        orgMagnetPos = movingMagnet.transform.position;
        magnet = movingMagnet.transform.GetChild(0).GetComponent<RigidMagnet>();
        maxMoveCount = needMoveCount * dumpCycle;
    }

    private void Start()
    {
        base.BaseStart(null,() =>
        {
            foreach(CircleA a in transform.parent.GetComponentsInChildren<CircleA>())
            {
                a.active = false;
            }
            magnet.attachObjs.ForEach(x => x.SetActive(false));
        });
    }

    public void MoveSphere(int id,Transform tr, Vector3 dir)
    {
        if(IsMoving)
        {
            PoolManager.GetItem<SystemTxt>().OnText("지금은 움직일 수 없습니다.");
            return;
        }
        if(currentId != id)
        {
            PoolManager.GetItem<SystemTxt>().OnText("현재 해당 오브젝트와는 상호작용을 할 수 없습니다.");
            return;
        }

        IsMoving = true;
        curMoveCnt++;
        movingSphere.transform.DOMove(tr.position, 2.5f);
        TweenCallback tc = () => { };
       
        currentId = tr.parent.GetChild(1).GetComponent<CircleA>().id;
        if (currentId != targetId)
        {
            if(curMoveCnt==maxMoveCount)
            {
                tc = () =>
                {
                    Fail();
                };
            }
            else if (FunctionGroup.IsContainValue(magnetObjsRemoveIds, currentId))
            {
                tc = () =>
                {
                    if (curMoveCnt % needMoveCount == 0)
                    {
                        magnet.RemoveCaughtObj();
                    }
                    else
                    {
                        Fail();
                    }
                };
            }
        }
        else  //목적지에 도달했을 때
        {
            tc = () =>
            {
                magnet.RemoveCaughtObj();
                if(curMoveCnt==maxMoveCount && magnet.IsClear())
                {
                    if (npc != null && !npc.info.dead && (npc.info.isFighting || npc.info.bRunaway))
                        PoolManager.GetItem<SystemTxt>().OnText(npcFightingClearMsg);
                    else
                        GetReward();

                    npc.info.talkId++;
                    base.Interaction();
                    active = false;
                    foreach (CircleA a in transform.parent.GetComponentsInChildren<CircleA>())
                    {
                        a.active = false;
                    }
                    Transform tm = magnet.transform.parent;
                    Transform tm2 = movingSphere.transform;
                    GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(magnetIndex, SaveObjInfoType.TRANSFORM, tm.position, tm.rotation, tm.localScale));
                    GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(sphereIndex,SaveObjInfoType.TRANSFORM,tm2.position,tm2.rotation,tm2.localScale));
                }
                else
                {
                    Fail();
                }
            };
        }

        tc += () => IsMoving = false;
        dir.y = 0;
        Vector3 target = movingMagnet.transform.position + dir * magnetMoveInterval;
        movingMagnet.transform.DOMove(target, 2.5f).OnComplete(tc);
    }

    private void Fail()
    {
        PoolManager.GetItem<SystemTxt>().OnText("<color=purple>실패</color>", 3.5f, 70);
        IsMoving = false;
        Interaction();
    }

    public override void Interaction()
    {
        if(IsMoving)
        {
            PoolManager.GetItem<SystemTxt>().OnText("지금은 할 수 없습니다.");
            return;
        }

        IsMoving = true;
        movingSphere.transform.DOMove(defaultPosition.position, 2.5f);
        movingMagnet.transform.DOMove(orgMagnetPos, 2.5f).OnComplete(() =>
        {
            magnet.ResetData();
            currentId = 1;
            curMoveCnt = 0;
            IsMoving = false;
        });
    }

    public void GetReward()
    {
        if (!GameManager.Instance.IsContainChest(chestId))
        {
            PuzzleReward.RequestReward(id);
        }
        else
        {
            PoolManager.GetItem<SystemTxt>().OnText("이곳의 보물상자는 이미 가져갔습니다.");
        }
    }
}
