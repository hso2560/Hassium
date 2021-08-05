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
    [SerializeField] private float staminaDownSpeed=7f;  //���׹̳� ���� �ӵ�
    
    [SerializeField] private float groundRayDist=3f;  //�÷��̾ ������ ��� �ִ��� üũ�ϴ� ������ ����
    public float rotateSpeed = 3.5f;
    [SerializeField] private float needStaminaMin;  //���׹̳� �ٴڳ� �Ŀ� �ٽ� ���ϱ� ���ؼ� �ʿ��� �ּ� ���׹̳�
    public float staminaRecoverySpeed;  //���׹̳� ȸ�� �ӵ�
    [SerializeField] private float interactionRadius = 3.5f;  //������Ʈ�� ��ȣ�ۿ� ������ ����

    [Header("���� ��")] [SerializeField] private short id;
    public short Id { get { return id; } }
    [Header("���� ��")] [SerializeField] private string charName;  //ĳ���� �̸�
    [SerializeField] private short level;
    public int str;
    public int def;
    [SerializeField] private int exp;
    [Header("���� ��")] [SerializeField] private string resoName;  //Resources�������� ���� ���� ���� �̸�(�θ�)

    private Vector3 moveDir, worldDir;  //������ ����, ������ ���� ����
    [SerializeField] public int hp;
    [SerializeField] public float stamina;
    [HideInInspector] public bool isDie, isJumping;
    
    [HideInInspector] public bool isStamina0;  //���׹̳��� 0���� üũ
    public Transform center;  //�÷��̾� ������Ʈ������ �߽� �κ�
    public Transform playerModel;  //�÷��̾��� ���� ����(��)�� �ִ� ������Ʈ
    public Transform footCenter;
    public LayerMask whatIsGround, whatIsObj;
    public GameObject parent;
    private int speedFloat;  //������ �ִϸ��̼� ó���� �ִϸ��̼� �̸��� ���̵�
    private int jumpTrigger, landingTrigger;

    private float checkTime;

    public GameCharacter gameChar;
    [HideInInspector] public bool isMovable = true;
                                            
    private void Start() //������(2): ���� �ִϸ��̼��� ��ġ���� ���������鼭 �������� �����. ���� �ִϸ��̼� ������ �����ϴٰ� ������ �������� ������ ª�� ����ǰ� ����.
    {                               // --> �� üũ ���̰� �߿��� ������ ������ ���ذ�.             --> ���ڸ� ������ ���� �׷��� ���. => fixed duration�� �༭ ���ذ�
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

    void _Input()  //��ǻ�Ϳ�
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

    private void StaminaCheck()  //���׹̳� 0�������� üũ�� ���׹̳� ���� ó��
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

    private void StaminaRecovery()  //���׹̳� ȸ��
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

    private void GroundHit()  //���� ��� �ִ��� üũ
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

    private void CheckObj()  //�ֺ��� ��ȣ�ۿ� ������ ���� �ִ��� üũ�ϰ� ó��
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

    public void SetData(JoystickControl jc, Vector3 pos, Quaternion rot, Quaternion modelRot)  //�÷��̾� �����ǰų� ��ü�� ������ ����
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

    public void Save()  //�÷��̾� �ɷ�ġ ���� ����
    {
        gameChar = new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName);
        gameChar.exp = exp;
        gameChar.level = level;
        gameChar.stamina = stamina;
        gameChar.hp = hp;

        GameManager.Instance.savedData.userInfo.currentChar = gameChar;
    }

    public void AddInfo()  //�÷��̾� �߰�
      => GameManager.Instance.savedData.userInfo.characters.Add(new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName));
    
}
