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
        skillExplain = $"��ų ��ư Ŭ���� ĳ������ ���� ���ؼ� ���� ���� ���� ���� ���� ��� ������ �߻��ؼ� ���� ������ ��ģ��. �� �Ŀ� {skillContnTime}�� ���� �� �� �� ��ư�� Ŭ���� ��� ������ ������ ������ ĳ���͸� �̵���Ų��. (���� ���ð�: {coolTime}��)";
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

    public override void OffSkill()  //PlayerScript�� GameManager���� isResetIfChangeChar==true�̸� �� �Լ��� �����Ű���� �Ѵ�
    {
        if(isUsingSkill)
        {
            StopGrapple();
        }
    }

    private void Update()
    {
        if(isMoving)  //Vector3.Lerp �Ἥ �� �غ���
        {
            moveSpeed = Mathf.Clamp(moveSpeed, minMoveSpeed, maxMoveSpeed);

            //rigid�̵�  minSpeed 10  maxSpeed 33 dist 2.8  acceleration 10
            /*Vector3 v = dirVec * moveSpeed;
            Vector3 force = new Vector3(v.x, v.y - gravity , v.z);
            player.rigid.AddForce(force, ForceMode.Impulse);
            player.playerModel.position = transform.position;*/

            //lerp�̵�  minSpeed 2  maxSpeed 8 dist 1  acceleration 2.5
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

    private void StartGrapple() //������ �Ǵ�
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
    private void StopGrapple()  //���� ���� => ��ų ����
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

    private void DrawRope()  //���η������� ���� �׷���
    {
        if (ropeStatePhase > 0)
        {
            //rope.SetPosition(0, ropeStartPoint.position);
            rope.SetPosition(1, rope.transform.InverseTransformPoint(grapplePoint));
        }
    }

    private void MoveToTarget()  //���� �������� ������ ����
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

    private void SetAim(bool active) //���� ��ư ���� ����
    {
        player.joystickCtrl.aimBtn.gameObject.SetActive(active);

        if(player.joystickCtrl.crosshair!=null)
           player.joystickCtrl.crosshair.SetActive(false);
    }
}
