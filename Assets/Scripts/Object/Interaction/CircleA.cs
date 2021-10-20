using UnityEngine;

public class CircleA : ObjData
{
    private Transform thisTr;
    public Transform target;
    private CircleAPuzzle rule;

    private void Awake()
    {
        rule = transform.parent.parent.GetChild(0).GetComponent<CircleAPuzzle>();
        thisTr = transform.parent.GetChild(0).transform;
    }

    public override void Interaction()
    {
        rule.MoveSphere(id,target, (target.position-thisTr.position).normalized);
    }
}
