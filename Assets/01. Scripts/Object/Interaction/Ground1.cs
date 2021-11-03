using UnityEngine;

public class Ground1 : ObjData
{
    private Ground1Puzzle rule;
    [SerializeField] int[] relevantGrounds;

    private void Awake()
    {
        rule = transform.parent.GetChild(0).GetComponent<Ground1Puzzle>();
    }

    public override void Interaction()
    {
        rule.Move(relevantGrounds);
    }
}
