using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour, IDamageable, IAttackable   //부모 스크립트 만들고 그걸 상속받았어야 했다..
{
    public PSkillType skillType;
    public Rigidbody rigid;
    [SerializeField] Animator ani;
    public Collider col;
    public Skill skill;
    public PlayerData pData;
    public Attack attack;

    [HideInInspector] public JoystickControl joystickCtrl;

    #region 정보
    //저장/로드 할 정보들(능력치)은 차라리 저장할 때의 클래스로 가져와서 클래스 접근을 통해 값을 쓰고 바꾸고 하는게 나았을듯.....아니면 가독성이라도 높이게 저것들 담는 클래스 뭔가 만들거나
    [SerializeField] private float speed = 8.5f;
    public float runSpeed = 17.8f;
    public float jumpPower = 10f;
    [SerializeField] private int maxHp;
    public int MaxHp { get { return maxHp; } set { maxHp = value; } }
    [SerializeField] private float maxStamina;
    public float MaxStamina { get { return maxStamina; } set { maxStamina = value; } }
    [SerializeField] private float staminaDownSpeed=7f;  //스테미나 감소 속도
    [SerializeField] private float staminaDecJAR = 10f;  //달리는 중에 점프할 때의 스테미나 감소 수치
    [SerializeField] private int statPoint = 0;  //스탯 포인트 (능력치 올리는 포인트)
    public int StatPoint { get { return statPoint; } set { statPoint = value; } }

    //[SerializeField] private float groundRayDist=3f;  //플레이어가 땅위를 밟고 있는지 체크하는 레이의 길이
    public float rotateSpeed = 3.5f;
    [SerializeField] private float needStaminaMin;  //스테미나 바닥난 후에 다시 런하기 위해서 필요한 최소 스테미나
    public float staminaRecoverySpeed;  //스테미나 회복 속도
    //[SerializeField] private float interactionRadius = 3.5f;  //오브젝트와 상호작용 가능한 범위


    [Header("고유 값")] [SerializeField] private short id;
    public short Id { get { return id; } }
    [Header("고유 값")] [SerializeField] private string charName;  //캐릭터 이름
    public string CharName { get { return charName; } }
    [SerializeField] private short level;
    public short Level { get { return level;  } set { level = value; } }
    public int str;
    public int def;
    [SerializeField] private int exp;
    [SerializeField] private int currentMaxExp;
    public int Exp { get { return exp; } set { exp = value; } }
    public int MaxExp { get { return currentMaxExp; } set { currentMaxExp = value; } }
    [Header("고유 값")] [SerializeField] private string resoName;  //Resources폴더에서 꺼낼 때의 파일 이름(부모)
    #endregion


    private Vector3 moveDir, worldDir;  //움직임 방향, 움직임 월드 방향
    public int hp;
    public float stamina;

    [SerializeField] private int fallDamage; //해당 변수 * 높이에 따른 값
    private float rigidVelY; //점프로 데미지 입을 때 rigid.velocity의 Y의 절댓값이 가장 큰 값을 저장
    public bool isDie, isJumping;

    [SerializeField] private float atkDelay=.9f;
    [SerializeField] private float existAtkTime = .5f;
    private float lastAtkTime;
    private bool isAttacking;
    private int attackStatePhase = 0;
    
    [HideInInspector] public bool isStamina0;  //스테미나가 0인지 체크
    public Transform center;  //플레이어 오브젝트에서의 중심 부분
    public Transform playerModel;  //플레이어의 실제 형태(모델)가 있는 오브젝트
    public Transform footCenter, footCenter2;  //하나로 하려했더니 여러 문제가 생겨서 갑작스럽게 2를 추가함
    //public LayerMask whatIsGround, whatIsObj;
    public GameObject parent;

    private int speedFloat;  //움직임 애니메이션 처리할 애니메이션 이름의 아이디
    private int jumpTrigger, landingTrigger;
    private int attackInt;
    private int attackTrigger;

    private float checkTime;  //너무 빨리 CheckObj함수 호출하는 것을 방지해줌
    private bool isDamageableByFall;  //높은 곳에서 떨어져서 데미지를 받을 수 있는 상태인지
    public bool IsDamageableByFall { set { isDamageableByFall = value; } get { return isDamageableByFall; } }
    private bool isInvincible = false;  //무적상태인가
    public bool IsInvincible { get { return isInvincible; } set { isInvincible = value; } }
    private bool isStart; //한 번이라도 시작했나

    public GameCharacter gameChar;
    [HideInInspector] public bool isMovable = true;  //움직일 수 없는 상태  (중력도 사라짐)
    private bool noControl = false; //제어할 수 없는 상태 (중력은 적용 됨)
    public bool NoControl { get { return noControl; } set { noControl = value; } }


    private IEnumerator IEhit, IEdeath;
    private WaitForSeconds hitWs = new WaitForSeconds(.3f);
    private bool jumpTry;
    private Transform joinTr;
                                            
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

    private void StaminaCheck()  //스테미나 0이하인지 체크와 스테미나 감소 처리
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

    private void StaminaRecovery()  //스테미나 회복
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

    private void GroundHit()  //땅을 밟고 있는지 체크
    {
        //Debug.DrawRay(footCenter.position, Vector3.down * groundRayDist, Color.blue);
        //Debug.DrawRay(footCenter2.position, Vector3.down * groundRayDist, Color.blue);
        RaycastHit hit;
        if(Physics.Raycast(footCenter.position, Vector3.down,out hit ,pData.groundRayDist,pData.whatIsGround) 
            || Physics.Raycast(footCenter2.position, Vector3.down,out hit ,pData.groundRayDist,pData.whatIsGround))  //점프 애니메이션에서 위치까지 강제 이동돼서 코드도 이상해지고 점프 모션도 좀 어색함 (임포트한 에셋의 점프가 위치까지 가져와서 매우 귀찮아짐)
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

    private void CheckObj()  //주변에 상호작용 가능한 옵젝 있는지 체크하고 처리
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
            //부활처리
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

    public void SetData(JoystickControl jc, Vector3 pos, Quaternion rot, Quaternion modelRot)  //플레이어 스폰되거나 교체될 때마다 실행
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

    public void Save()  //플레이어 능력치 정보 저장
    {
        gameChar = new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName);  //str~staminaRecoverySpeed까진 강화로 증가할 경우엔 gameChar.으로 접근해서 하고 플레이어값에도 대입한다.
        //이 안에서 값을 쓸 때는 str, def로 쓰지만 값을 저장하고 받을 땐 gameChar로 접근. 버프 아이템 쓸 경우를 위해서 이렇게  ---> 이렇게 하려 했음
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

    public void AddInfo()  //플레이어 추가
      => GameManager.Instance.savedData.userInfo.characters.Add(new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName));
}
