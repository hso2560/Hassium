using DG.Tweening;
using UnityEngine;

public class CircleAPuzzle : ObjData, IReward
{
    private int currentId = 1;
    public GameObject movingSphere, movingMagnet;
    private RigidMagnet magnet;
    public Transform defaultPosition;
    public float magnetMoveInterval = 4.5f;
    public int[] magnetObjsRemoveIds; //�� ���̵� ��ġ�� ���� �����ϸ� �ڼ��� ���� ������ ����

    private Vector3 orgMagnetPos;
    public bool IsMoving { get; set; }

    [SerializeField] private int targetId = 9;
    [SerializeField] private int needMoveCount = 4;  //needMoveCount��°���� �ڼ��� ���� ������ ������ ��
    [SerializeField] private int dumpCycle = 4;  // dumpCycle�� needMoveCount�� ���� �� ��ŭ ���� �������� �� ��Ȯ�� ��ǥ ��ġ�� ���� �־�� ��
    private int maxMoveCount; // needMoveCount * dumpCycle
    private int curMoveCnt = 0;

    [SerializeField] private int magnetIndex;
    [SerializeField] private int sphereIndex;

    [SerializeField] private int chestId;
    [SerializeField] private NPCAI npc;

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
            PoolManager.GetItem<SystemTxt>().OnText("������ ������ �� �����ϴ�.");
            return;
        }
        if(currentId != id)
        {
            PoolManager.GetItem<SystemTxt>().OnText("���� �ش� ������Ʈ�ʹ� ��ȣ�ۿ��� �� �� �����ϴ�.");
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
                    IsMoving = false;
                    Interaction();
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
                        IsMoving = false;
                        Interaction();
                    }
                };
            }
        }
        else  //�������� �������� ��
        {
            tc = () =>
            {
                magnet.RemoveCaughtObj();
                if(curMoveCnt==maxMoveCount && magnet.IsClear())
                {
                    GetReward();
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
                    IsMoving = false;
                    Interaction();
                }
            };
        }

        tc += () => IsMoving = false;
        dir.y = 0;
        Vector3 target = movingMagnet.transform.position + dir * magnetMoveInterval;
        movingMagnet.transform.DOMove(target, 2.5f).OnComplete(tc);
    }

    public override void Interaction()
    {
        if(IsMoving)
        {
            PoolManager.GetItem<SystemTxt>().OnText("������ �� �� �����ϴ�.");
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
            PoolManager.GetItem<SystemTxt>().OnText("�̰��� �������ڴ� �̹� ���������ϴ�.");
        }
    }
}
