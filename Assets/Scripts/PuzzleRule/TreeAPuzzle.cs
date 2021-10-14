using System.Collections.Generic;
using UnityEngine;

public class TreeAPuzzle : ObjData
{
    public bool IsStart { get; set; }
    [SerializeField] private float limitTime;

    [SerializeField] private List<TreeA> treeList;

    public GameObject dropObject;
    public List<GameObject> dropObjList = new List<GameObject>();

    public string startObjName = "시작";
    public string resetObjName = "초기화";

    private void Start()
    {
        if (GameManager.Instance.ContainKeyActiveId(saveActiveStateId))
        {
            active = GameManager.Instance.savedData.objActiveInfo[saveActiveStateId];
        }
        if (active)
        {
            dropObjList = FunctionGroup.CreatePoolList(dropObject, transform, 6);
            treeList = new List<TreeA>(transform.parent.GetComponentsInChildren<TreeA>());
        }
    }

    public override void Interaction()
    {
        if (IsStart)
        {
            UIManager.Instance.TimeAttackMission(false);
            return;
        }

        {
            foreach (int x in FunctionGroup.GetRandomList(treeList.Count, 2))
            {
                treeList[x].active = true;
            }

            IsStart = true;
            objName = resetObjName;
        }

        {
            UIManager.Instance.clearEvent += () =>
            {
                IsStart = false;
                active = false;
                base.Interaction();
            };
            UIManager.Instance.timeOverEvent += () =>
            {
                treeList.ForEach(x => x.active = false);
                objName = startObjName;
                IsStart = false;
            };

            UIManager.Instance.OnTimer((int)limitTime);
        }
    }
}
