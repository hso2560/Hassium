using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Ground1Puzzle : ObjData, IReward
{
    public List<GameObject> groundPathList;
    public List<IndexListClass<Transform>> waypointList;

    public float moveTime = 0.6f;

    [SerializeField] private int chestId;
    [SerializeField] private NPCAI npc;
    public string npcFightingClearMsg;

    private Queue<bool> moveCheckQueue = new Queue<bool>();
    private Vector3[] groundStartPosArr;

    private void Start()
    {
        base.BaseStart(()=> 
        {
            groundStartPosArr = new Vector3[groundPathList.Count];
            for (int i = 0; i < groundPathList.Count; i++) groundStartPosArr[i] = groundPathList[i].transform.position;
        }, ()=> 
        {
            foreach(Ground1 g in transform.parent.GetComponentsInChildren<Ground1>())
                g.active = false;

            for (int i = 0; i < groundPathList.Count; ++i)
            {
                groundPathList[i].transform.position = waypointList[i][waypointList[i].targetIndex].position;
            }
        });
    }

    public void Move(int[] groundIndexArr)
    {
        if (moveCheckQueue.Count > 0)
        {
            PoolManager.GetItem<SystemTxt>().OnText("지금은 움직일 수 없습니다.");
            return;
        }

        for(int i=0; i<groundIndexArr.Length; i++)
        {
            moveCheckQueue.Enqueue(false);
            int idx = groundIndexArr[i];
            waypointList[idx].NextIndex();
            groundPathList[idx].transform.DOMove(waypointList[idx].GetCurrentItem.position, moveTime).SetEase(Ease.Linear).OnComplete(()=> 
            {
                if (moveCheckQueue.Count > 0) moveCheckQueue.Dequeue();
            });
        }
    }

    public override void Interaction()
    {
        if (moveCheckQueue.Count > 0)
        {
            PoolManager.GetItem<SystemTxt>().OnText("지금은 움직일 수 없습니다.");
            return;
        }

        float time = moveTime + 0.25f;
        for(int i=0; i<groundPathList.Count; i++)
        {
            moveCheckQueue.Enqueue(false);
            groundPathList[i].transform.DOMove(groundStartPosArr[i], time).SetEase(Ease.Linear).OnComplete(() =>
            {
                if (moveCheckQueue.Count > 0) moveCheckQueue.Dequeue();
            });
        }

        waypointList.ForEach(x => x.index=0);
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
