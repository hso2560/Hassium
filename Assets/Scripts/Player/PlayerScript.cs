using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] Rigidbody rigid;
    [SerializeField] Animator ani;
    public Collider col;

    private JoystickControl joystickCtrl;

    [SerializeField] private float speed = 8.5f;
    [SerializeField] private float runSpeed = 17.8f;
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private int maxHp;
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaDownSpeed=7f;  //스테미나 감소 속도
    
    [SerializeField] private float groundRayDist=3f;  //플레이어가 땅위를 밟고 있는지 체크하는 레이의 길이
    [SerializeField] private float rotateSpeed = 3.5f;
    [SerializeField] private float needStaminaMin;  //스테미나 바닥난 후에 다시 런하기 위해서 필요한 최소 스테미나
    [SerializeField] private float staminaRecoverySpeed;  //스테미나 회복 속도
    [SerializeField] private float interactionRadius = 3.5f;  //오브젝트와 상호작용 가능한 범위

    [SerializeField] private short id;
    public short Id { get { return id; } }
    [SerializeField] private string charName;
    [SerializeField] private short level;
    [SerializeField] private int str;
    [SerializeField] private int def;
    [SerializeField] private int exp;
    [SerializeField] private string resoName;

    private Vector3 moveDir, worldDir;  //움직임 방향, 움직임 월드 방향
    private int hp;
    private float stamina;
    private bool isDie, isJumping;

    public bool isStamina0;  //스테미나가 0인지 체크
    public Transform center;  //플레이어 오브젝트에서의 중심 부분
    public Transform playerModel;  //플레이어의 실제 형태(모델)가 있는 오브젝트
    public LayerMask whatIsGround;
    public GameObject parent;
    private int speedFloat;

    private GameCharacter gameChar;

    private void Start()
    {
        InitData();
    }

    private void InitData()
    {
        gameObject.AddComponent<AudioListener>();
        speedFloat = Animator.StringToHash("moveSpeed");
    }

    private void Update()
    {
        StaminaRecovery();
        Rotate();

        _Input();
    }

    void _Input()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        Move();
        GroundHit();
        CheckObj();
        rigid.angularVelocity = Vector3.zero;
    }

    private void Move()
    {
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
        if (!joystickCtrl.isTouch) return;

        float angle = Mathf.Atan2(worldDir.x, worldDir.z) * Mathf.Rad2Deg;

        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * rotateSpeed);
    }

    private void StaminaCheck()
    {
        if (ani.GetFloat(speedFloat) >= runSpeed)
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

    private void StaminaRecovery()
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
        if(!isJumping)
        {
            //점프 애니메이션
            rigid.velocity = Vector3.up * jumpPower;
        }
    }

    private void GroundHit()
    {
        //Debug.DrawRay(center.position, Vector3.down * groundRayDist, Color.blue);
        if(Physics.Raycast(center.position, Vector3.down, groundRayDist, whatIsGround))
        {
            isJumping = false;
        }
        else
        {
            isJumping = true;
        }
    }

    private void CheckObj()
    {

    }

    public void SetData(JoystickControl jc, Vector3 pos, Quaternion rot)
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

        transform.position = pos;
        transform.rotation = rot;
    }

    public void Save()
    {
        gameChar = new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName);
        gameChar.exp = exp;
        gameChar.level = level;
        gameChar.stamina = stamina;
        gameChar.hp = hp;
        GameManager.Instance.savedData.userInfo.currentChar = gameChar;
        GameManager.Instance.idToMyPlayer[id] = this;
    }

    public void AddInfo()
    {
        GameManager.Instance.savedData.userInfo.characters.Add(new GameCharacter(id, str, def, maxHp, maxStamina, runSpeed, jumpPower, staminaRecoverySpeed, charName, resoName));
    }
}
