using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser1Puzzle : ObjData, IReward
{
    public List<Laser1> lasers;

    public float ratio;
    public float rotateTime;

    public bool IsMoving { get; set; }

    private void Awake()
    {
        lasers = new List<Laser1>(transform.parent.GetComponentsInChildren<Laser1>());
    }

    private void Start()
    {
        
    }

    public override void Interaction()
    {
        
    }

    public void GetReward()
    {
        
    }
}
