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
    private Vector2 screenCenterPos;

    private void Awake()
    {
        player = GetComponent<PlayerScript>();

        base.Init(isFirstSkillUseTreat);
        skillExplain = $"스킬 버튼 클릭시 캐릭터의 앞을 향해서 일정 범위 내에 벽이 있을 경우 로프를 발사해서 벽에 로프를 걸친다. 그 후에 {skillContnTime}초 내에 한 번 더 버튼을 클릭할 경우 로프가 걸쳐진 곳으로 캐릭터를 이동시킨다. (재사용 대기시간: {coolTime}초)";
    }

    private void Start()
    {
        cam = GameManager.Instance.camMove.transform;
        aim = UIManager.Instance.crosshairImg.gameObject;
        d = dist * dist;
        screenCenterPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
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
        if(isMoving)  //Vector3.Lerp 써서 함 해보자
        {
            moveSpeed = Mathf.Clamp(moveSpeed, minMoveSpeed, maxMoveSpeed);

            //rigid이동  minSpeed 10  maxSpeed 33 dist 2.8  acceleration 10
            /*Vector3 v = dirVec * moveSpeed;
            Vector3 force = new Vector3(v.x, v.y - gravity , v.z);
            player.rigid.AddForce(force, ForceMode.Impulse);
            player.playerModel.position = transform.position;*/

            //lerp이동  minSpeed 2  maxSpeed 8 dist 1  acceleration 2.5
            transform.position = Vector3.Lerp(transform.position, grapplePoint, moveSpeed * Time.deltaTime);
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

        //Debug.DrawRay(ropeStartPoint.position, cam.forward * maxDist, Color.red);
    }

    private void StartGrapple() //로프를 건다
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPos);

        if(Physics.Raycast(ray, out hit, maxDist, whatIsGrappleable))  //if(Physics.Raycast(ropeStartPoint.position , cam.forward, out hit, maxDist, whatIsGrappleable))
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
    private void StopGrapple()  //로프 해제 => 스킬 해제
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
        EffectManager.Instance.SkillEffectVolume(PSkillType.ROPE, false);
        trailEffect.SetActive(false);
    }

    private void DrawRope()  //라인렌더러로 로프 그려줌
    {
        if (ropeStatePhase > 0)
        {
            //rope.SetPosition(0, ropeStartPoint.position);
            rope.SetPosition(1, rope.transform.InverseTransformPoint(grapplePoint));
        }
    }

    private void MoveToTarget()  //로프 방향으로 움직임 시작
    {
        dirVec = (grapplePoint - transform.position).normalized;

        aim.SetActive(false);
        ropeStatePhase = 2;
        isMoving = true;
        player.isMovable = false;
        player.rigid.velocity = Vector3.zero;
        EffectManager.Instance.SkillEffectVolume(PSkillType.ROPE, true);
        trailEffect.SetActive(true);
    }

    public override void SetData()
    {
        //player.joystickCtrl.ClearSkillBtn();
        //player.joystickCtrl.skillBtn.onClick.AddListener(UseSkill);

        moveSpeed = minMoveSpeed;
        isMoving = false;
        ropeStatePhase = 0;
        
        SetAim(true);
    }

    public override void Change() => GameManager.Instance.objActionHandle += ()=>SetAim(false); 

    private void SetAim(bool active) //에임 버튼 상태 결정
    {
        player.joystickCtrl.aimBtn.gameObject.SetActive(active);

        if(player.joystickCtrl.crosshair!=null)
           player.joystickCtrl.crosshair.SetActive(false);
    }
}
