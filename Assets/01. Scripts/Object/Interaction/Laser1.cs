using UnityEngine;

public class Laser1 : ObjData
{
    private Laser1Puzzle rule;
    
    [SerializeField] private Laser1[] relevantLaers;
    [SerializeField] private float[] relevantRotateY;

    public bool bShootingLaer;
    public Transform laserPoint;
    public Color laserColor;
    public Light laserLight;

    public bool IsMoving { get; set; }

    private Quaternion startRotation;

    private void Awake()
    {
        rule = transform.parent.GetChild(0).GetComponent<Laser1Puzzle>();
        startRotation = transform.rotation;
        laserPoint = transform.GetChild(1).transform;
        laserLight = laserPoint.GetChild(0).GetComponent<Light>();
    }

    /*private void Update()
    {
        if(bShootingLaer)
           Debug.DrawRay(laserPoint.position, -transform.right * rule.laserDist, laserColor);
    }*/

    public override void Interaction()
    {
        
    }
}
