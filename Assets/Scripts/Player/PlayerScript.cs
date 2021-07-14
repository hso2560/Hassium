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
    [SerializeField] private float staminaDownSpeed=7f;  //���׹̳� ���� �ӵ�
    
    [SerializeField] private float groundRayDist=3f;  //�÷��̾ ������ ��� �ִ��� üũ�ϴ� ������ ����
    [SerializeField] private float rotateSpeed = 3.5f;
    [SerializeField] private float needStaminaMin;  //���׹̳� �ٴڳ� �Ŀ� �ٽ� ���ϱ� ���ؼ� �ʿ��� �ּ� ���׹̳�
    [SerializeField] private float staminaRecoverySpeed;  //���׹̳� ȸ�� �ӵ�
    [SerializeField] private float interactionRadius = 3.5f;  //������Ʈ�� ��ȣ�ۿ� ������ ����

    [SerializeField] private int id;

    private Vector3 moveDir, worldDir;  //������ ����, ������ ���� ����
    private int hp;
    private float stamina;
    private bool isDie, isJumping;

    public bool isStamina0;  //���׹̳��� 0���� üũ
    public Transform center;  //�÷��̾� ������Ʈ������ �߽� �κ�
    public Transform playerModel;  //�÷��̾��� ���� ����(��)�� �ִ� ������Ʈ
    public LayerMask whatIsGround;

    private int speedFloat;

    private void Start()
    {
        InitData();
        joystickCtrl = FindObjectOfType<JoystickControl>();
    }

    private void InitData()
    {
        gameObject.AddComponent<AudioListener>();
        hp = maxHp;
        stamina = maxStamina;

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
        if(stamina<maxStamina && !joystickCtrl.isRun)
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
            //���� �ִϸ��̼�
            rigid.velocity = Vector3.up * jumpPower;
        }
    }

    private void GroundHit()
    {
        Debug.DrawRay(center.position, Vector3.down * groundRayDist, Color.blue);
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
}
