using UnityEngine;
using DG.Tweening; 

public class ColumnA : ObjData  
{
    [SerializeField] private bool isResetButton;
    public bool IsResetBtn { get { return isResetButton; } }
    private ColumnAPuzzle rule;
    private bool representative; //true�� �� ���� material�� active���¸� üũ�ϰ� ����Ǿ� ������ rule�� ���� ���� �����Ų��
                                  //0��°�� ���� true��
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
                GameManager.Instance.OnSystemMsg("���� �ش� ����� ��ȣ�ۿ� �� �� �����ϴ�.", 1.5f);
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
