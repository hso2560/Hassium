using System.Collections.Generic;
using UnityEngine;

public class TreeAPuzzle : ObjData
{
    public bool IsStart { get; set; }
    [SerializeField] private float limitTime;

    [SerializeField] private List<TreeA> treeList;
    private List<TreeA> questTrees = new List<TreeA>();  //������ ����(�����ߵǴ� ������)
    private float changeTime;
    [SerializeField] private float changeDelay = 24f;
    public Material changeMat;

    /*public GameObject dropObject;
    public List<GameObject> dropObjList = new List<GameObject>();*/
    [SerializeField] private int catchMaxCount = 14; //������ �� �� �� �ٲپ�� �ϴ°�
    private int currentCatchCount = 0;
    [SerializeField] private int maxHp = 5;  //���� ü��
    public int MaxHp { get { return maxHp; } }

    public string startObjName = "����";
    public string resetObjName = "�ʱ�ȭ";

    private void Start()
    {
        if (GameManager.Instance.ContainKeyActiveId(saveActiveStateId))
        {
            active = GameManager.Instance.savedData.objActiveInfo[saveActiveStateId];
        }
        if (active)
        {
            //dropObjList = FunctionGroup.CreatePoolList(dropObject, transform, 6);
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
            foreach (int x in FunctionGroup.GetRandomList(treeList.Count, 3))
            {
                treeList[x].active = true;
                treeList[x].mainMesh.material = changeMat;
                questTrees.Add(treeList[x]);
            }
            IsStart = true;
            objName = resetObjName;
            changeTime = Time.time + changeDelay;
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
                treeList.ForEach(x => x.ResetData());
                objName = startObjName;
                currentCatchCount = 0;
                questTrees.Clear();
                IsStart = false;
            };

            UIManager.Instance.OnTimer((int)limitTime);
        }
    }

    private void Update()
    {
        if (IsStart)
        {
            if (changeTime < Time.time)
            {
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
}
