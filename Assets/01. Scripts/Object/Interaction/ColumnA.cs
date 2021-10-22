using UnityEngine;
using DG.Tweening; 

public class ColumnA : ObjData  
{
    [SerializeField] private bool isResetButton;
    public bool IsResetBtn { get { return isResetButton; } }
    private ColumnAPuzzle rule;
    private bool representative; //true인 놈만 현재 material과 active상태를 체크하고 변경되어 있으면 rule을 통해 전부 변경시킨다
                                  //0번째인 놈이 true임
    public bool Representative { set { representative = value; } }
    [SerializeField] private ColumnA[] relevantCols;
    [SerializeField] private bool[] increase;

    [HideInInspector] public MeshRenderer mesh;
    private float startY;

    private void Awake()
    {
        rule = transform.parent.GetComponent<ColumnAPuzzle>();
        mesh = transform.GetChild(0).GetComponent<MeshRenderer>();
        startY = transform.localScale.y;
    }

    private void Start()
    {
        if (representative)
        {
            if (GameManager.Instance.ContainKeyActiveId(saveActiveStateId))
            {
                rule.AllColActiveState(GameManager.Instance.savedData.objActiveInfo[saveActiveStateId]);
            }
        }
    }

    public override void Interaction()
    {
        if (rule.IsMove) return;

        if(isResetButton)
        {
            rule.Move(true);
            mesh.GetComponent<Animator>().SetTrigger("move");
            return;
        }

        int i;

        for(i=0; i<increase.Length; i++)
        {
            if(increase[i]&&relevantCols[i].transform.localScale.y>=rule.maxHeight
                || !increase[i] && relevantCols[i].transform.localScale.y <= rule.minHeight)
            {
                GameManager.Instance.OnSystemMsg("현재 해당 기둥은 상호작용 할 수 없습니다.", 1.5f);
                return;
            }
        }

        for(i=0; i<increase.Length; i++)
        {
            float y = relevantCols[i].transform.localScale.y + (increase[i] ? rule.ratio : -rule.ratio);
            relevantCols[i].transform.DOScaleY(y, rule.moveTime);
        }

        rule.Move();
    }

    public void ResetMove()
    {
        transform.DOScaleY(startY, rule.moveTime);
    }

    public void Save()
    {
        base.Interaction();
        GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(rule.objIndex[0], SaveObjInfoType.TRANSFORM, transform.position, transform.rotation, transform.localScale));
        GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(rule.objIndex[1], SaveObjInfoType.MATERIAL, rule.Complete));
        rule.AllMatChange(Resources.Load<Material>("materials/" + rule.Complete));
    }
}
