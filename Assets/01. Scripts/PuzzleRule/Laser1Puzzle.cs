using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Laser1Puzzle : ObjData, IReward
{
    public List<Laser1> lasers;

    public float rotateTime;
    public float laserDist;

    /*private void Awake()
    {
        lasers = new List<Laser1>(transform.parent.GetComponentsInChildren<Laser1>());
    }*/

    private void Start()
    {
        base.BaseStart();
    }

    public void Rotate(Laser1[] lasers, float[] ratios)
    {

    }

    public override void Interaction()
    {
        
    }

    public void GetReward()
    {
        
    }
}
