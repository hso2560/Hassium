using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Laser1Puzzle : ObjData, IReward
{
    public List<Laser1> laserList;
    [SerializeField] private int[] saveIndexArr;

    public float rotateTime;
    public float laserDist;

    [SerializeField] private int chestId;
    [SerializeField] private NPCAI npc;

    public string npcFightingClearMsg;

    /*private void Awake()
    {
        lasers = new List<Laser1>(transform.parent.GetComponentsInChildren<Laser1>());
    }*/

    private void Start()
    {
        base.BaseStart(null,()=>
        {
            laserList.ForEach(x => x.active = false);
        });
        CheckLaser();
    }

    public void Rotate(Laser1[] lasers, float[] ratios)  
    {
        int i;
        for(i=0; i<lasers.Length; i++)
        {
            if(lasers[i].IsMoving)
            {
                PoolManager.GetItem<SystemTxt>().OnText("������ ������ �� �����ϴ�.");
                return;
            }
        }

        Vector3 target;
        for (i=0; i<lasers.Length; i++)
        {
            lasers[i].IsMoving = true;

            if (lasers[i].bShootingLaser) lasers[i].line.gameObject.SetActive(false);
            else lasers[i].pair.line.gameObject.SetActive(false);
            target = lasers[i].transform.rotation.eulerAngles + new Vector3(0, ratios[i], 0);
            int si = i;  //�̰� ���ϸ� ���߿� ������ �Ŀ� i���� length�� �ǹ����� out of range�� ��

            lasers[i].transform.DORotate(target, rotateTime).OnComplete(() =>  
            {
                lasers[si].IsMoving = false;  
                if (si == lasers.Length - 1)
                {
                    CheckLaser();
                    if (IsClear())
                    {
                        if (npc != null && !npc.info.dead && (npc.info.isFighting || npc.info.bRunaway))
                            PoolManager.GetItem<SystemTxt>().OnText(npcFightingClearMsg);
                        else
                            GetReward();

                        npc.info.talkId=1;
                        base.Interaction();
                        active = false;

                        for(int i=0; i<laserList.Count; i++)
                        {
                            laserList[i].active = false;
                            Transform t = laserList[i].transform;
                            GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(saveIndexArr[i],SaveObjInfoType.TRANSFORM,t.position,t.rotation,t.localScale));
                        }
                    }
                }
            });
        }
    }

    public override void Interaction()
    {
        int i;
        for (i = 0; i < laserList.Count; i++)
        {
            if (laserList[i].IsMoving)
            {
                PoolManager.GetItem<SystemTxt>().OnText("������ ������ �� �����ϴ�.");
                return;
            }
        }

        transform.GetChild(0).GetComponent<Animator>().SetTrigger("move");
        for (i = 0; i < laserList.Count; i++)
        {
            laserList[i].IsMoving = true;

            if (laserList[i].line != null) laserList[i].line.gameObject.SetActive(false);
            else laserList[i].pair.line.gameObject.SetActive(false);
            int si = i;

            laserList[i].transform.DORotateQuaternion(laserList[i].startRotation, rotateTime).OnComplete(() =>
            {
                laserList[si].IsMoving = false;
                if (si == laserList.Count - 1)
                {
                    CheckLaser();
                }
            });
        }
    }

    private bool IsClear()
    {
        /*foreach(Laser1 l in laserList.FindAll(x => x.bShootingLaer))  //�̷��� �ص� ��
        {
            if (!l.line.gameObject.activeSelf) return false;
        }
        return true;*/

        for(int i=0; i<laserList.Count; i++)
        {
            if (!laserList[i].IsComplete) return false;
        }

        return true;
    }

    private void CheckLaser() //�������� ���¿� �°� �̰����� ����
    {
        laserList.ForEach(x => x.RaycastCheck());
        laserList.FindAll(x => x.bShootingLaser).ForEach(y => y.CheckPair());
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
