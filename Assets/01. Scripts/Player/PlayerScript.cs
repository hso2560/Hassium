using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour, IDamageable, IAttackable   //�θ� ��ũ��Ʈ ����� �װ� ��ӹ޾Ҿ�� �ߴ�..
{
    public PSkillType skillType;
    public Rigidbody rigid;
    [SerializeField] Animator ani;
    public Collider col;
    public Skill skill;
    public PlayerData pData;
    public Attack attack;

    [HideInInspector] public JoystickControl joystickCtrl;

    #region ����
    //����/�ε� �� ������(�ɷ�ġ)�� ���� ������ ���� Ŭ������ �����ͼ� Ŭ���� ������ ���� ���� ���� �ٲٰ� �ϴ°� ��������.....�ƴϸ� �������̶� ���̰� ���͵� ��� Ŭ���� ���� ����ų�
    [SerializeField] private float speed = 8.5f;
    public float runSpeed = 17.8f;
    public float jumpPower = 10f;
    [SerializeField] private int maxHp;
    public int MaxHp { get { return maxHp; } set { maxHp = value; } }
    [SerializeField] private float maxStamina;
    public float MaxStamina { get { return maxStamina; } set { maxStamina = value; } }
    [SerializeField] private float staminaDownSpeed=7f;  //���׹̳� ���� �ӵ�
    [SerializeField] private float staminaDecJAR = 10f;  //�޸��� �߿� ������ ���� ���׹̳� ���� ��ġ
    [SerializeField] private int statPoint = 0;  //���� ����Ʈ (�ɷ�ġ �ø��� ����Ʈ)
    public int StatPoint { get { return statPoint; } set { statPoint = value; } }

    //[SerializeField] private float groundRayDist=3f;  //�÷��̾ ������ ��� �ִ��� üũ�ϴ� ������ ����
    public float rotateSpeed = 3.5f;
    [SerializeField] private float needStaminaMin;  //���׹̳� �ٴڳ� �Ŀ� �ٽ� ���ϱ� ���ؼ� �ʿ��� �ּ� ���׹̳�
    public float staminaRecoverySpeed;  //���׹̳� ȸ�� �ӵ�
    //[SerializeField] private float interactionRadius = 3.5f;  //������Ʈ�� ��ȣ�ۿ� ������ ����


    [Header("���� ��")] [SerializeField] private short id;
    public short Id { get { return id; } }
    [Header("���� ��")] [SerializeField] private string charName;  //ĳ���� �̸�
    public string CharName { get { return charName; } }
    [SerializeField] private short level;
    public short Level { get { return level;  } set { level = value; } }
    public int str;
    public int def;
    [SerializeField] private int exp;
    [SerializeField] private int currentMaxExp;
    public int Exp { get { return exp; } set { exp = value; } }
    public int MaxExp { get { return currentMaxExp; } set { currentMaxExp = value; } }
    [Header("���� ��")] [SerializeField] private string resoName;  //Resources�������� ���� ���� ���� �̸�(�θ�)
    #endregion


    private Vector3 moveDir, worldDir;  //������ ����, ������ ���� ����
    public int hp;
    public float stamina;

    [SerializeField] private int fallDamage; //�ش� ���� * ���̿� ���� ��
    private float rigidVelY; //������ ������ ���� �� rigid.velocity�� Y�� ������ ���� ū ���� ����
    public bool isDie, isJumping;

    [SerializeField] private float atkDelay=.9f;
    [SerializeField] private float existAtkTime = .5f;
    private float lastAtkTime;
    private bool isAttacking;
    private int attackStatePhase = 0;
    
    [HideInInspector] public bool isStamina0;  //���׹̳��� 0���� üũ
    public Transform center;  //�÷��̾� ������Ʈ������ �߽� �κ�
    public Transform playerModel;  //�÷��̾��� ���� ����(��)�� �ִ� ������Ʈ
    public Transform footCenter, footCenter2;  //�ϳ��� �Ϸ��ߴ��� ���� ������ ���ܼ� ���۽����� 2�� �߰���
    //public LayerMask whatIsGround, whatIsObj;
    public GameObject parent;

    private int speedFloat;  //������ �ִϸ��̼� ó���� �ִϸ��̼� �̸��� ���̵�
    private int jumpTrigger, landingTrigger;
    private int attackInt;
    private int attackTrigger;

    private float checkTime;  //�ʹ� ���� CheckObj�Լ� ȣ���ϴ� ���� ��������
    private bool isDamageableByFall;  //���� ������ �������� �������� ���� �� �ִ� ��������
    public bool IsDamageableByFall { set { isDamageableByFall = value; } get { return isDamageableByFall; } }
    private bool isInvincible = false;  //���������ΰ�
    public bool IsInvincible { get { return isInvincible; } set { isInvincible = value; } }
    private bool isStart; //�� ���̶� �����߳�

    public GameCharacter gameChar;
    [HideInInspector] public bool isMovable = true;  //������ �� ���� ����  (�߷µ� �����)
    private bool noControl = false; //������ �� ���� ���� (�߷��� ���� ��)
    public bool NoControl { get { return noControl; } set { noControl = value; } }


    private IEnumerator IEhit, IEdeath;
    private WaitForSeconds hitWs = new WaitForSeconds(.3f);
    private bool jumpTry;
    private Transform joinTr;
                                            
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
        attackInt = Animator.StringToHash("attack");
        attackTrigger = Animator.StringToHash("atk");

        gameChar = new GameCharacter();
        //IEhit = HitCo();
        //IEdeath = DeathCo();
        CheckHp();
        isStart = true;
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
        CheckAttack();
        joystickCtrl?.CheckStamina(stamina,maxStamina,needStaminaMin);

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
        else if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            joystickCtrl.OnPointerDownRunBtn();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            joystickCtrl.OnPointerUpRunBtn();
        }
    }

    private void FixedUpdate()
    {
        Move();
        GroundHit();
        RigidHandle();
    }

    private void RigidHandle()
    {
        rigid.angularVelocity = Vector3.zero;

        if(ani.GetFloat(speedFloat)==0 && !isJumping && !jumpTry)   //ani.GetCurrentAnimatorStateInfo(0).nameHash==Animator.StringToHash("Base Layer.Run")
        {
            if(rigid.drag<10)
               rigid.drag = 55;
        }
        else
        {
            if (rigid.drag > 10)
                rigid.drag = 0.5f;
        }
    }

    private void Move()
    {
        if (!isMovable || isDie) return;

        if(!isJumping)
        {
            moveDir.x = joystickCtrl.isTouch ? (joystickCtrl.dirVec.x * (joystickCtrl.isRun ? runSpeed : speed)) : 0;
            moveDir.z = joystickCtrl.isTouch ? (joystickCtrl.dirVec.y * (joystickCtrl.isRun ? runSpeed : speed)) : 0;
        }

        if(joystickCtrl.PC_MoveDir!=Vector3.zero)
        {
            moveDir.x = joystickCtrl.PC_MoveDir.x * (joystickCtrl.isRun ? runSpeed : speed);
            moveDir.z = joystickCtrl.PC_MoveDir.z * (joystickCtrl.isRun ? runSpeed : speed);
        }

        if (noControl)
        {
            moveDir.x = 0;
            moveDir.z = 0;
        }

        worldDir = transform.TransformDirection(moveDir);
        Vector3 force = new Vector3(worldDir.x - rigid.velocity.x, -pData.gravity, worldDir.z - rigid.velocity.z);
        rigid.AddForce(force, ForceMode.VelocityChange);

        if (joystickCtrl.PC_MoveDir == Vector3.zero)
            ani.SetFloat(speedFloat, joystickCtrl.isTouch ? (joystickCtrl.isRun ? runSpeed : speed) : 0);
        else
            ani.SetFloat(speedFloat, joystickCtrl.isRun ? runSpeed : speed);
        playerModel.position = transform.position;

        StaminaCheck();
    }
    private void Rotate()
    {
        if ((!joystickCtrl.isTouch && joystickCtrl.PC_MoveDir==Vector3.zero) || isJumping || !isMovable || noControl) return;

        float angle = Mathf.Atan2(worldDir.x, worldDir.z) * Mathf.Rad2Deg;

        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * pData.rotateSpeed);
        
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
                joystickCtrl.CheckRunBtnState(true);
            }
        }
    }

    private void StaminaRecovery()  //���׹̳� ȸ��
    {
        if(stamina<maxStamina && (!joystickCtrl.isRun || moveDir==Vector3.zero))
        {
            stamina += staminaRecoverySpeed * Time.deltaTime;

            if (isStamina0 && stamina >= needStaminaMin)
            {
                isStamina0 = false;
                joystickCtrl.CheckRunBtnState(false);
            }
            if (stamina >= maxStamina) stamina = maxStamina;
        }
    }

    public void Jump()
    {
        if(!isJumping && isMovable && !noControl)
        {
            rigid.drag = 0.5f;
            jumpTry = true;
            Invoke("DelayHandling", 0.3f);

            ani.SetTrigger(jumpTrigger);
            rigid.velocity = Vector3.up * jumpPower;
            SoundManager.Instance.PlaySoundEffect(SoundEffectType.JUMP);
            
            if (joystickCtrl.isRun) stamina -= staminaDecJAR;
        }
    }

    private void GroundHit()  //���� ��� �ִ��� üũ
    {
        //Debug.DrawRay(footCenter.position, Vector3.down * groundRayDist, Color.blue);
        //Debug.DrawRay(footCenter2.position, Vector3.down * groundRayDist, Color.blue);
        RaycastHit hit;
        if(Physics.Raycast(footCenter.position, Vector3.down,out hit ,pData.groundRayDist,pData.whatIsGround) 
            || Physics.Raycast(footCenter2.position, Vector3.down,out hit ,pData.groundRayDist,pData.whatIsGround))  //���� �ִϸ��̼ǿ��� ��ġ���� ���� �̵��ż� �ڵ嵵 �̻������� ���� ��ǵ� �� ����� (����Ʈ�� ������ ������ ��ġ���� �����ͼ� �ſ� ��������)
        {
            if (isJumping)
            {
                isJumping = false;
                ani.SetTrigger(landingTrigger);
                if (isDamageableByFall)
                {
                    isDamageableByFall = false;
                    OnDamaged(fallDamage * (int)-rigidVelY / 20, Vector3.zero, 0, false);
                }
                rigidVelY = 0;
            }
            if (hit.transform.CompareTag("JoinObj")) 
            {
                if (joinTr != hit.transform)
                {
                    joinTr = hit.transform;
                    transform.parent = joinTr;

                    GameManager.Instance.ActionFuncHandle();
                    GameManager.Instance.objActionHandle += () =>
                    {
                        joinTr = null;
                        transform.parent = parent.transform;
                    };
                }
            }
            else
            {
                if (joinTr != null)
                {
                    GameManager.Instance.ActionFuncHandle();
                }
            }
        }
        else
        {
            isJumping = true;

            if (rigid.velocity.y < -pData.DamagedByFallNeedHeight)
            {
                isDamageableByFall = true;
                if(rigid.velocity.y<rigidVelY)
                {
                    rigidVelY = rigid.velocity.y;
                }
            }
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

        Collider[] cols = Physics.OverlapSphere(center.position, pData.interactionRadius, pData.whatIsObj);
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

    private void DelayHandling()
    {
        jumpTry = false;
    }

    private void CheckHp()
    {
        hp = Mathf.Clamp(hp, 0, maxHp);
        UIManager.Instance.AdjustFillAmound(UIType.HPFILL, hp, maxHp);
        if (hp <= 0) Death();
    }

    private void CheckAttack()
    {
        if (isAttacking)
        {
            if (lastAtkTime + existAtkTime < Time.time)
            {
                if(attack.gameObject.activeSelf)
                   attack.gameObject.SetActive(false);
            }
            if(lastAtkTime+atkDelay < Time.time)
            {
                attackStatePhase = 0;
                isAttacking = false;
            }
        }
    }

    public void OnDamaged(int damage, Vector3 hitNormal, float force, bool useDef)
    {
        if (isDie || isInvincible) return;

        if (!useDef) hp -= damage;
        else
        {
            hp -= FunctionGroup.GetDamageAmount(damage, def);
            EffectManager.Instance.OnHitEffect(center.position, hitNormal);
        }
        //rigid.AddForce(hitNormal * force, ForceMode.VelocityChange);
        //rigid.velocity = hitNormal * force;
        CheckHp();

        ani.SetTrigger("hit" + Random.Range(1, 3).ToString());
        //StopCoroutine(IEhit);
        if (IEhit == null)
        {
            IEhit = HitCo();
            StartCoroutine(IEhit);
        }
        //ani.SetInteger(hitInt, Random.Range(1, 3));
    }

    public void Attack()
    {
        if (joystickCtrl.isTouch || isJumping || noControl || !isMovable) return;

        if (!isAttacking)
        {
            isAttacking = true;
        }
        else
        {
            if(attackStatePhase!=1 || attack.gameObject.activeSelf)
            {
                return;
            }
        }

        attackStatePhase++;
        ani.SetTrigger(attackTrigger);
        ani.SetInteger(attackInt, attackStatePhase);
        attack.gameObject.SetActive(true);
        lastAtkTime = Time.time;
        SoundManager.Instance.PlaySoundEffect(SoundEffectType.ATTACK);
    }

    public void Death()
    {
        if (isDie) return;

        //StopCoroutine(IEhit);
        if (IEdeath == null)
        {
            IEdeath = DeathCo();
            StartCoroutine(IEdeath);
        }
    }

    public void RecoveryHp(int value)
    {
        hp += value;
        CheckHp();
        if (isDie)
        {
            isDie = false;
            //��Ȱó��
        }
    }

    private IEnumerator HitCo()
    {
        isInvincible = true;
        noControl = true;

        yield return hitWs;
        noControl = false;
        yield return hitWs;
        isInvincible = false;

        IEhit = null;
    }

    private IEnumerator DeathCo()
    {
        yield return hitWs;

        hp = 0;
        isDie = true;
        isInvincible = false;
        noControl = true;
        ani.SetTrigger("death" + Random.Range(1, 3).ToString());
        //ani.SetInteger(deathInt, Random.Range(1, 3));

        System.Action handler = null; 
        handler = () =>
        {
            IEdeath = null;
            noControl = false;
            GameManager.Instance.DeathEvent -= handler;
        };
        GameManager.Instance.DeathEvent += handler;
        GameManager.Instance.keyToVoidFunction[LoadingType.PLAYERDEATH]();
    }

    public void SetData(JoystickControl jc, Vector3 pos, Quaternion rot, Quaternion modelRot)  //�÷��̾� �����ǰų� ��ü�� ������ ����
    {
        gameChar = GameManager.Instance.savedData.userInfo.currentChar;

        isDie = gameChar.isDie;
        maxHp = gameChar.maxHp;
        hp = gameChar.hp;
        maxStamina = gameChar.maxStamina;
        stamina = gameChar.stamina;
        level = gameChar.level;
        exp = gameChar.exp;
        currentMaxExp = gameChar.currentMaxExp;
        str = gameChar.str;
        def = gameChar.def;
        runSpeed = gameChar.runSpeed;
        jumpPower = gameChar.jumpPower;
        staminaRecoverySpeed = gameChar.staminaRecoverySpeed;
        statPoint = gameChar.statPoint;

        joystickCtrl = jc;
        joystickCtrl.player = this;

        transform.position = pos;
        transform.rotation = rot;
        playerModel.rotation = modelRot;

        skill.SetData();
        joystickCtrl.skillBtn.image.sprite = skill.skillBtnImg;
        joystickCtrl.isHoldSkill = skill.isHoldSkill;

        if (isStart) CheckHp();
    }

    public void Save()  //�÷��̾� �ɷ�ġ ���� ����
    {
        gameChar = new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName);  //str~staminaRecoverySpeed���� ��ȭ�� ������ ��쿣 gameChar.���� �����ؼ� �ϰ� �÷��̾���� �����Ѵ�.
        //�� �ȿ��� ���� �� ���� str, def�� ������ ���� �����ϰ� ���� �� gameChar�� ����. ���� ������ �� ��츦 ���ؼ� �̷���  ---> �̷��� �Ϸ� ����
        gameChar.exp = exp;
        gameChar.level = level;
        gameChar.stamina = stamina;
        gameChar.hp = hp;
        gameChar.isDie = isDie;
        gameChar.currentMaxExp = currentMaxExp;
        gameChar.statPoint = statPoint;

        GameManager.Instance.savedData.userInfo.currentChar = gameChar;
    }

    public void GetExp(int exp)
    {
        this.exp += exp;

        if (this.exp >= currentMaxExp)
        {
            if (level == pData.maxLevel)
            {
                this.exp = currentMaxExp;
                return;
            }

            level++;
            int remainder = this.exp - currentMaxExp;
            this.exp = 0;

            statPoint += pData.addStatPoint;
            currentMaxExp += pData.addMaxExp;

            GetExp(remainder);
        }
    }

    public void AddInfo()  //�÷��̾� �߰�
      => GameManager.Instance.savedData.userInfo.characters.Add(new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName));
}
