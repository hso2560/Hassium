using UnityEngine;
using DG.Tweening; 

public class ColumnA : ObjData  
{
    private ColumnAPuzzle rule;
    [SerializeField] private bool representative; //true�� �� ���� material�� active���¸� üũ�ϰ� ����Ǿ� ������ rule�� ���� ���� �����Ų��

    [SerializeField] private ColumnA[] relevantCols;
    [SerializeField] private bool[] increase;

    private void Awake()
    {
        rule = transform.parent.GetComponent<ColumnAPuzzle>();
    }

    private void Start()
    {
        if (representative)
        {

        }
    }

    public override void Interaction()
    {
        int i;

        for(i=0; i<increase.Length; i++)
        {
            if(increase[i]&&relevantCols[i].transform.position.y>=rule.maxHeight
                || !increase[i] && relevantCols[i].transform.position.y <= rule.minHeight)
            {
                return;
            }
        }

        for(i=0; i<increase.Length; i++)
        {
            float y = relevantCols[i].transform.position.y + (increase[i] ? rule.ratio : -rule.ratio);
            relevantCols[i].transform.DOMoveY(y, rule.moveTime);
        }

        rule.Move();
    }
}
