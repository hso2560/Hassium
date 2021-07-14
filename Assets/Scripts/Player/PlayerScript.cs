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

    [SerializeField] private int id;

    private Vector3 moveDir, worldDir;  //움직임 방향, 움직임 월드 방향
    private int hp;
    private float stamina;
    private bool isDie, isJumping;

    public bool isStamina0;  //스테미나가 0인지 체크
    public Transform center;  //플레이어 오브젝트에서의 중심 부분
    public Transform body;  //플레이어의 실제 형태(모델)가 있는 오브젝트

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
