using UnityEngine;

public class TreeA : Tree
{
    private TreeAPuzzle rule;

    public bool active = false;

    private void Awake()
    {
        rule = transform.parent.GetChild(0).GetComponent<TreeAPuzzle>();
    }

    public override void AddWork()
    {
        if (rule.IsStart && active)
        {

        }
    }
}
