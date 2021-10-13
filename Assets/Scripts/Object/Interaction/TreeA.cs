using System.Collections.Generic;
using UnityEngine;

public class TreeA : Tree
{
    private TreeAPuzzle rule;

    private void Awake()
    {
        rule = transform.parent.GetChild(0).GetComponent<TreeAPuzzle>();
    }

    public override void AddWork()
    {
        if (rule.IsStart)
        {

        }
    }
}
