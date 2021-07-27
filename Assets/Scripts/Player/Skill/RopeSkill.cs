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

    private bool isMoving;
    private Vector3 dirVec;
    private float moveSpeed;
    [SerializeField] private float minMoveSpeed, maxMoveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float gravity;

    private void Start()
    {
        player = GetComponent<PlayerScript>();

        base.Init(isFirstSkillUseTreat);

        SkillManager.Instance.playerSkills.Add(this);
    }

    public override void UseSkill()
    {
        if(!isUsingSkill && !isUsedSkill)
        {
            if (ropeStatePhase == 0)
                StartGrapple();
        }

        if(isUsingSkill && !isUsedSkill)
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
            Vector3 force = new Vector3(v.x - player.rigid.velocity.x, -gravity, v.z - player.rigid.velocity.z);
            player.rigid.AddForce(force, ForceMode.VelocityChange);

            moveSpeed += acceleration * Time.deltaTime;
        }
    }
    private void LateUpdate()
    {
        DrawRope();
    }

    private void StartGrapple()
    {
        RaycastHit hit;

        if(Physics.Raycast(ropeStartPoint.position ,player.playerModel.forward, out hit, maxDist, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            joint.maxDistance = distanceFromPoint*0.8f;
            joint.minDistance = distanceFromPoint * 0.01f;

            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;

            rope.positionCount = 2;

            isUsingSkill = true;
            skillOffTime = Time.time + skillContnTime;

            ropeStatePhase = 1;
        }
    }
    private void StopGrapple()
    {
        rope.positionCount = 0;
        Destroy(joint);

        isUsingSkill = false;

        isUsedSkill = true;
        skillRechargeTime = Time.time + coolTime;

        moveSpeed = minMoveSpeed;
        player.isMovable = true;
        isMoving = false;
        ropeStatePhase = 0;
    }

    private void DrawRope()
    {
        if (!joint) return;

        if (ropeStatePhase > 0)
        {
            rope.SetPosition(0, ropeStartPoint.position);
            rope.SetPosition(1, grapplePoint);
        }
    }

    private void MoveToTarget()
    {
        dirVec = (grapplePoint - transform.position).normalized;

        ropeStatePhase = 2;
        isMoving = true;
        player.isMovable = false;
    }

    public override void SetData()
    {
        player.joystickCtrl.ClearSkillBtn();
        player.joystickCtrl.skillBtn.onClick.AddListener(UseSkill);

        moveSpeed = minMoveSpeed;
        isMoving = false;
        ropeStatePhase = 0;
    }
}
