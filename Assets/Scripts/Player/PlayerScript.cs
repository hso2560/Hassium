using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public PSkillType skillType;
    public Rigidbody rigid;
    [SerializeField] Animator ani;
    public Collider col;
    public Skill skill;

    [HideInInspector] public JoystickControl joystickCtrl;

    [SerializeField] private float speed = 8.5f;
    public float runSpeed = 17.8f;
    public float jumpPower = 10f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private int maxHp;
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaDownSpeed=7f;  //스테미나 감소 속도
    
    [SerializeField] private float groundRayDist=3f;  //플레이어가 땅위를 밟고 있는지 체크하는 레이의 길이
    public float rotateSpeed = 3.5f;
    [SerializeField] private float needStaminaMin;  //스테미나 바닥난 후에 다시 런하기 위해서 필요한 최소 스테미나
    public float staminaRecoverySpeed;  //스테미나 회복 속도
    [SerializeField] private float interactionRadius = 3.5f;  //오브젝트와 상호작용 가능한 범위

    [Header("고유 값")] [SerializeField] private short id;
    public short Id { get { return id; } }
    [Header("고유 값")] [SerializeField] private string charName;  //캐릭터 이름
    [SerializeField] private short level;
    public int str;
    public int def;
    [SerializeField] private int exp;
    [Header("고유 값")] [SerializeField] private string resoName;  //Resources폴더에서 꺼낼 때의 파일 이름(부모)

    private Vector3 moveDir, worldDir;  //움직임 방향, 움직임 월드 방향
    [SerializeField] public int hp;
    [SerializeField] public float stamina;
    [HideInInspector] public bool isDie, isJumping;
    
    [HideInInspector] public bool isStamina0;  //스테미나가 0인지 체크
    public Transform center;  //플레이어 오브젝트에서의 중심 부분
    public Transform playerModel;  //플레이어의 실제 형태(모델)가 있는 오브젝트
    public Transform footCenter;
    public LayerMask whatIsGround, whatIsObj;
    public GameObject parent;
    private int speedFloat;  //움직임 애니메이션 처리할 애니메이션 이름의 아이디
    private int jumpTrigger, landingTrigger;

    private float checkTime;

    public GameCharacter gameChar;
    [HideInInspector] public bool isMovable = true;
                                            
    private void Start() //문제점(2): 점프 애니메이션이 위치까지 가져와지면서 움직임이 어색함. 착지 애니메이션 때문에 점프하다가 가끔씩 맛나가고 점프가 짧게 실행되고 끊김.
    {                               // --> 땅 체크 레이가 발에서 나가는 것으로 반해결.             --> 제자리 점프일 때만 그래서 어색. => fixed duration을 줘서 반해결
        InitData();
    }

    private void InitData()
    {
        gameObject.AddComponent<AudioListener>();
        skill = GetComponent<Skill>();
        speedFloat = Animator.StringToHash("moveSpeed");
        jumpTrigger = Animator.StringToHash("jump");
        landingTrigger = Animator.StringToHash("landing");
        gameChar = new GameCharacter();
    }

    /*private void GiveSkill()
    {
        switch(skill)
        {
            case PSkillType.REINFORCE:
                gameObject.AddComponent<ReinforceSkill>();
                break;

            case PSkillType.TIME:
                gameObject.AddComponent<TimeSkill>();
                break;

            case PSkillType.ROPE:
                gameObject.AddComponent<RopeSkill>();
                break;
        }
    }*/

    private void Update()
    {
        StaminaRecovery();
        Rotate();
        CheckObj();

        _Input();
    }

    void _Input()  //컴퓨터용
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        else if(Input.GetKeyDown(KeyCode.E))
        {
            skill.UseSkill();
        }
    }

    private void FixedUpdate()
    {
        Move();
        GroundHit();
        rigid.angularVelocity = Vector3.zero;
    }

    private void Move()
    {
        if (!isMovable) return;

        if(!isJumping)
        {
            moveDir.x = joystickCtrl.isTouch ? (joystickCtrl.dirVec.x * (joystickCtrl.isRun ? runSpeed : speed)) : 0;
            moveDir.z = joystickCtrl.isTouch ? (joystickCtrl.dirVec.y * (joystickCtrl.isRun ? runSpeed : speed)) : 0;
        }

        worldDir = transform.TransformDirection(moveDir);
        Vector3 force = new Vector3(worldDir.x - rigid.velocity.x, -gravity, worldDir.z - rigid.velocity.z);
        rigid.AddForce(force, ForceMode.VelocityChange);

        ani.SetFloat(speedFloat,joystickCtrl.isTouch?(joystickCtrl.isRun?runSpeed:speed):0);
        playerModel.position = transform.position;

        StaminaCheck();
    }
    private void Rotate()
    {
        if (!joystickCtrl.isTouch || isJumping || !isMovable) return;

        float angle = Mathf.Atan2(worldDir.x, worldDir.z) * Mathf.Rad2Deg;

        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * rotateSpeed);
        
    }

    private void StaminaCheck()  //스테미나 0이하인지 체크와 스테미나 감소 처리
    {
        if (ani.GetFloat(speedFloat) >= runSpeed && !isJumping)
        {
            stamina -= staminaDownSpeed * Time.deltaTime;
            if (stamina <= 0)
            {
                stamina = 0;
                isStamina0 = true;
                joystickCtrl.isRun = false;
            }
        }
    }

    private void StaminaRecovery()  //스테미나 회복
    {
        if(stamina<maxStamina && (!joystickCtrl.isRun || moveDir==Vector3.zero))
        {
            stamina += staminaRecoverySpeed * Time.deltaTime;

            if (isStamina0 && stamina >= needStaminaMin) isStamina0 = false;
            if (stamina >= maxStamina) stamina = maxStamina;
        }
    }

    public void Jump()
    {
        if(!isJumping && isMovable)
        {
            ani.SetTrigger(jumpTrigger);
            rigid.velocity = Vector3.up * jumpPower;
        }
    }

    private void GroundHit()  //땅을 밟고 있는지 체크
    {
        //Debug.DrawRay(footCenter.position, Vector3.down * groundRayDist, Color.blue);
        if(Physics.Raycast(footCenter.position, Vector3.down, groundRayDist, whatIsGround))
        {
            if (isJumping)
            {
                isJumping = false;
                ani.SetTrigger(landingTrigger);
            }
        }
        else
        {
            isJumping = true;
        }
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(center.position, interactionRadius);
        Gizmos.color = Color.red;
    }*/

    private void CheckObj()  //주변에 상호작용 가능한 옵젝 있는지 체크하고 처리
    {
        if (checkTime < Time.time) checkTime = Time.time + 0.5f;
        else return;

        Collider[] cols = Physics.OverlapSphere(center.position, interactionRadius, whatIsObj);
        if(cols.Length>0)
        {
            for(int i=0; i<cols.Length; i++)
            {
                ObjData objData = cols[i].GetComponent<ObjData>();

                if(objData!=null)
                {
                    UIManager.Instance.ActiveItrBtn(objData);
                }
            }

            UIManager.Instance.DisableItrBtn(cols);
        }
        else
        {
            UIManager.Instance.OffInterBtn();
        }
    }

    public void SetData(JoystickControl jc, Vector3 pos, Quaternion rot, Quaternion modelRot)  //플레이어 스폰되거나 교체될 때마다 실행
    {
        gameChar = GameManager.Instance.savedData.userInfo.currentChar;

        maxHp = gameChar.maxHp;
        hp = gameChar.hp;
        maxStamina = gameChar.maxStamina;
        stamina = gameChar.stamina;
        level = gameChar.level;
        exp = gameChar.exp;
        str = gameChar.str;
        def = gameChar.def;
        runSpeed = gameChar.runSpeed;
        jumpPower = gameChar.jumpPower;
        staminaRecoverySpeed = gameChar.staminaRecoverySpeed;

        joystickCtrl = jc;
        joystickCtrl.player = this;

        transform.position = pos;
        transform.rotation = rot;
        playerModel.rotation = modelRot;

        skill.SetData();
        joystickCtrl.skillBtn.image.sprite = skill.skillBtnImg;
        joystickCtrl.isHoldSkill = skill.isHoldSkill;
    }

    public void Save()  //플레이어 능력치 정보 저장
    {
        gameChar = new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName);
        gameChar.exp = exp;
        gameChar.level = level;
        gameChar.stamina = stamina;
        gameChar.hp = hp;

        GameManager.Instance.savedData.userInfo.currentChar = gameChar;
    }

    public void AddInfo()  //플레이어 추가
      => GameManager.Instance.savedData.userInfo.characters.Add(new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName));
    
}
