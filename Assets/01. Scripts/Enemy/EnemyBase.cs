using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    public EnemyData enemyData;
    public Transform center;

    public bool bViewingAngle360, frontObstacle; //시야각이 360도인가, 플레이어와 자신 사이에 장애물 있으면 추격 안되는가
    protected bool isDie;
    [SerializeField] protected int currentHp;

    protected Vector3 startPoint;  //처음 위치
    [SerializeField] protected int str;
    public int Str { get { return str; } set { str = value; } }
    [SerializeField] protected int def;
    public int Def { get { return def; } set { def = value; } }

    [SerializeField] protected NPCAI npc;
    protected NavMeshAgent agent;
    protected Rigidbody rigid;
    protected HPBar hpBar;
    protected GameObject model;

    [SerializeField] protected Transform target;
    protected int speedFloat, hitTrigger, deathTrigger, atkTrigger;
    
    protected Animator ani;
    [SerializeField] protected EnemyState enemyState = EnemyState.PATROL;

    protected bool isTrace;
    protected float ableAtkTime;
    [SerializeField] protected float attackCoolTime = 2f;

    //protected bool isStop, isPatrolling, isRunaway, isAttacking;
    //protected bool addHandler;

    protected virtual void Awake()
    {
        ani = transform.GetChild(0).GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        model = transform.GetChild(1).gameObject;
        currentHp = enemyData.maxHp;
        speedFloat = Animator.StringToHash("speed");
        hitTrigger = Animator.StringToHash("hit");
        deathTrigger = Animator.StringToHash("death");
        atkTrigger = Animator.StringToHash("attack");
        startPoint = transform.position;
        SetSpeed(0);
    }

    public virtual void Trace()  //추격
    {
        SetSpeed(enemyData.traceSpeed);
        target = GameManager.Instance.PlayerSc.transform;
        agent.SetDestination(target.position);

       /* if (!addHandler)
        {
            GameManager.Instance.objActionHandle += Handler;
            addHandler = true;
        }*/

    }

    #region 주석
    /*public void Handler()
    {
        agent.SetDestination(GameManager.Instance.PlayerSc.transform.position);
    }*/

    /*public virtual void ResetData()  //리셋
    {
        *//*if(addHandler)
        {
            GameManager.Instance.objActionHandle -= Handler;
            addHandler = false;
        }*//*
        currentHp = enemyData.maxHp;
        CheckHp();   
    }*/
    #endregion

    public virtual void CheckHp()  //HP체크
    {
        if(hpBar != null) hpBar.SetHPFill(currentHp, enemyData.maxHp);
        if(currentHp<=0)
        {
            Death();
        }
    }

    public virtual void SetSpeed(float s, bool attack = false)
    {
        agent.speed = s;
        agent.isStopped = s == 0;
        if(agent.isStopped)
        {
            agent.velocity = Vector3.zero;
        }
        if(!attack) ani.SetFloat(speedFloat, agent.speed);
    }

    public virtual void Runaway()  //도망
    {
        //isRunaway = true;

        SetSpeed(enemyData.traceSpeed);
        Vector3 dir = -(GameManager.Instance.PlayerSc.transform.position - transform.position).normalized;
        RaycastHit hit;
        if(Physics.Raycast( ( dir * Random.Range(5f,15f) )+new Vector3(0,15,0), Vector3.down, out hit, 100f ))
        {
            if (hit.transform != null)
            {
                agent.destination = hit.point;
            }
        }
        else
        {
            do
            {
                Vector3 dir2 = FunctionGroup.GetRandomDir();
                dir2.y = 0;
                Physics.Raycast((dir2 * Random.Range(5f, 15f)) + new Vector3(0, 15, 0), Vector3.down, out hit, 100f);
            } while (hit.transform==null);
            agent.destination = hit.point;
        }
    }

    public virtual void Stop()
    {
        //isStop = true;

        SetSpeed(0);
    }

    public abstract void Patrol();  //보통 상태
    public abstract void Attack();  //공격

    public void OnDamaged(int damage, Vector3 hitNormal, float force, bool useDef)
    {
        if (isDie) return;

        if (!useDef) currentHp -= damage;
        else
        {
            currentHp -= FunctionGroup.GetDamageAmount(damage, def);
            EffectManager.Instance.OnHitEffect(center.position, hitNormal);
        }

        if(hpBar==null)
        {
            hpBar = PoolManager.GetItem<HPBar>();
            hpBar.SetHPFill(currentHp, enemyData.maxHp);
        }

        ani.SetTrigger(hitTrigger);

        Damaged();
        CheckHp();
    }

    public void Death()
    {
        if (npc != null)
        {
            npc.Death();
        }

        enemyState = EnemyState.DIE;
        currentHp = 0;
        isDie = true;
        SetSpeed(0, true);
        ani.SetTrigger(deathTrigger);
    }

    public virtual void Damaged()
    {
        
    }

    public virtual void StartEnemy()
    {

    }
}
