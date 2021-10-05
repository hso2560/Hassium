using UnityEngine;
using DG.Tweening; 

public class ColumnA : ObjData  
{
    private ColumnAPuzzle rule;
    [SerializeField] private bool representative; //true인 놈만 현재 material과 active상태를 체크하고 변경되어 있으면 rule을 통해 전부 변경시킨다

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
