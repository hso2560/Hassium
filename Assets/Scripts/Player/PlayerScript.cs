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
    public Transform body;  //�÷��̾��� ���� ����(��)�� �ִ� ������Ʈ

    private void Start()
    {
        InitData();
    }

    private void InitData()
    {
        gameObject.AddComponent<AudioListener>();
        hp = maxHp;
        stamina = maxStamina;
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    private void Move()
    {
        
    }
    private void Rotate()
    {
        
    }

    private void StaminaCheck()
    {

    }

    private void StaminaRecovery()
    {

    }

    public void Jump()
    {

    }

    private void GroundHit()
    {

    }

    private void CheckObj()
    {

    }
}
