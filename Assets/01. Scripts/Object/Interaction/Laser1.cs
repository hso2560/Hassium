using UnityEngine;

public class Laser1 : ObjData
{
    private Laser1Puzzle rule;
    
    [SerializeField] private Laser1[] relevantLasers;
    [SerializeField] private float[] relevantRotateY;

    public bool bShootingLaser;
    public Transform laserPoint;
    public Color laserColor;
    public Light laserLight;
    public LineRenderer line;

    public Gradient lineGradient;

    public bool IsMoving { get; set; }
    public bool IsComplete { get; set; }

    [HideInInspector] public Quaternion startRotation;
    public Laser1 pair;

    private void Awake()
    {
        rule = transform.parent.GetChild(0).GetComponent<Laser1Puzzle>();
        startRotation = transform.rotation;
        laserPoint = transform.GetChild(1).transform;
        laserLight = laserPoint.GetChild(0).GetComponent<Light>();
        if (bShootingLaser)
        {
            line = transform.GetChild(2).GetComponent<LineRenderer>();
            line.colorGradient = lineGradient;
        }
    }

    /*private void Update()
    {
        Debug.DrawRay(laserPoint.position, -transform.right * rule.laserDist, laserColor);
    }*/

    public override void Interaction()
    {
        rule.Rotate(relevantLasers, relevantRotateY);
    }

    public void RaycastCheck()
    {
        IsComplete = false;
        if(Physics.Raycast(laserPoint.position, -transform.right, out RaycastHit hit, rule.laserDist))
        {
            Laser1 l = hit.transform.GetComponent<Laser1>();
            if(l!=null && l!=this && l.id == id)
            {
                IsComplete = true;
            }
        }
    }

    public void CheckPair()
    {
        if(IsComplete && pair.IsComplete)
        {
            line.gameObject.SetActive(true);
            line.SetPosition(0, laserPoint.InverseTransformPoint(laserPoint.position));
            line.SetPosition(1, line.transform.InverseTransformPoint(pair.laserPoint.position));
        }
    }
}
