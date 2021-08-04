using UnityEngine;

public class RopeSkill : Skill
{
    [HideInInspector] public short ropeStatePhase;

    private Vector3 grapplePoint;
    [SerializeField] private LineRenderer rope;
    [SerializeField] private LayerMask whatIsGrappleable;  //Wall
    [SerializeField] private Transform ropeStartPoint;
    [SerializeField] private float maxDist=70f;
    private SpringJoint joint;

    [SerializeField] private float spring=4.5f;
    [SerializeField] private float damper = 7f;
    [SerializeField] private float massScale = 4.5f;

    private GameObject aim;
    private Transform cam;
    private bool isMoving;
    private Vector3 dirVec;
    private float moveSpeed;
    [SerializeField] private float minMoveSpeed, maxMoveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float gravity;

    public float dist=16f;
    private float d;

    private void Awake()
    {
        player = GetComponent<PlayerScript>();

        base.Init(isFirstSkillUseTreat);
    }

    private void Start()
    {
        cam = GameManager.Instance.camMove.transform;
        aim = UIManager.Instance.crosshairImg.gameObject;
        d = dist * dist;
    }

    public override void UseSkill()
    {
        if (!aim.activeSelf)
        {
            aim.SetActive(true);
            return;
        }

        if(!isUsingSkill && !isUsedSkill)
        {
            if (ropeStatePhase == 0)
                StartGrapple();
        }

        else if(isUsingSkill && !isUsedSkill)
        {
            if (ropeStatePhase == 1)
                MoveToTarget();
        }
    }

    public override void OffSkill()  //PlayerScript나 GameManager에서 isResetIfChangeChar==true이면 이 함수를 실행시키도록 한다
    {
        if(isUsingSkill)
        {
            StopGrapple();
        }
    }

    private void Update()
    {
        if(isMoving)
        {
            moveSpeed = Mathf.Clamp(moveSpeed, minMoveSpeed, maxMoveSpeed);
            Vector3 v = dirVec * moveSpeed;
            Vector3 force = new Vector3(v.x, v.y - gravity , v.z);
            player.rigid.AddForce(force, ForceMode.Impulse);
            player.playerModel.position = transform.position;

            moveSpeed += acceleration * Time.deltaTime;

            if (Vector3.SqrMagnitude(grapplePoint-transform.position)<d)
            {
                OffSkill();
            }
        }
    }
    private void LateUpdate()
    {
        DrawRope();

        Debug.DrawRay(ropeStartPoint.position, cam.forward * maxDist, Color.red);
    }

    private void StartGrapple()
    {
        RaycastHit hit;

        if(Physics.Raycast(ropeStartPoint.position , cam.forward, out hit, maxDist, whatIsGrappleable))
        {
            rope.gameObject.SetActive(true);

            grapplePoint = hit.point;
            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            joint.maxDistance = distanceFromPoint*0.9f;
            joint.minDistance = distanceFromPoint * 0.05f;

            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;

            isUsingSkill = true;
            skillOffTime = Time.time + skillContnTime;

            ropeStatePhase = 1;
        }
    }
    private void StopGrapple()
    {
        Destroy(joint);

        isUsingSkill = false;

        rope.gameObject.SetActive(false);

        isUsedSkill = true;
        skillRechargeTime = Time.time + coolTime;

        moveSpeed = minMoveSpeed;
        player.isMovable = true;
        isMoving = false;
        ropeStatePhase = 0;

        player.rigid.velocity = Vector3.zero;
    }

    private void DrawRope()
    {
        if (ropeStatePhase > 0)
        {
            //rope.SetPosition(0, ropeStartPoint.position);
            rope.SetPosition(1, rope.transform.InverseTransformPoint(grapplePoint));
        }
    }

    private void MoveToTarget()
    {
        dirVec = (grapplePoint - transform.position).normalized;

        aim.SetActive(false);
        ropeStatePhase = 2;
        isMoving = true;
        player.isMovable = false;
        player.rigid.velocity = Vector3.zero;
    }

    public override void SetData()
    {
        //player.joystickCtrl.ClearSkillBtn();
        //player.joystickCtrl.skillBtn.onClick.AddListener(UseSkill);

        moveSpeed = minMoveSpeed;
        isMoving = false;
        ropeStatePhase = 0;
    }
}
