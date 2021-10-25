using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Laser1 : ObjData
{
    private Laser1Puzzle rule;
    
    [SerializeField] private Laser1[] relevantLaers;
    
    private Quaternion startRotation;

    private void Awake()
    {
        rule = transform.parent.GetChild(0).GetComponent<Laser1Puzzle>();
    }

    public override void Interaction()
    {
        
    }
}
