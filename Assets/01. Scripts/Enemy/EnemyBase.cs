using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    public EnemyData enemyData;
    public Transform center;

    public bool bViewingAngle360, frontObstacle; //시야각이 360도인가, 플레이어와 자신 사이에 장애물 있으면 추격 안되는가
    [HideInInspector] public bool isDie;
    [SerializeField] protected int currentHp;
    public int CurrentHp { set { currentHp = value; } }

    protected Vector3 startPoint;  //처음 위치
    [SerializeField] protected int str;
    public int Str { get { return str; } set { str = value; } }
    [SerializeField] protected int def;
    public int Def { get { return def; } set { def = value; } }

    [SerializeField] protected NPCAI npc;
    protected NavMeshAgent agent;
    protected Rigidbody rigid;
    protected HPBar hpBar;
    [SerializeField] protected Attack attack;

    [SerializeField] protected Transform target;
    protected int speedFloat, hitTrigger, deathTrigger, atkTrigger;
    [SerializeField] protected float existAtkTime = .5f;
    protected float disableAtkTime;

    protected Animator ani;
    public EnemyState enemyState = EnemyState.PATROL;

    protected bool isTrace;
    protected float ableAtkTime;
    [SerializeField] protected float attackCoolTime = 2f;

    public List<NPCHPLowMsg> npcHPLowMsg;
    public Vector3 msgOffset;

    public Vector3 hpOffset;
    protected Camera mainCam;
    protected float uiUpdateTime;
    //protected bool isStop, isPatrolling, isRunaway, isAttacking;
    //protected bool addHandler;
    public bool usedSkill;
    public SoundEffectType skillSound;

    protected virtual void Awake()
    {
        ani = transform.GetChild(0).GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        
        currentHp = enemyData.maxHp;
        speedFloat = Animator.StringToHash("speed");
        hitTrigger = Animator.StringToHash("hit");
        deathTrigger = Animator.StringToHash("death");
        atkTrigger = Animator.StringToHash("attack");
        startPoint = transform.position;
        SetSpeed(0);
    }

    protected virtual void Start()
    {
        mainCam = UIManager.Instance.mainCam;
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

    /*public void Handler()
    {
        agent.SetDestination(GameManager.Instance.PlayerSc.transform.position);
    }*/

    public virtual void ResetData()  //리셋
    {
        currentHp = enemyData.maxHp;
        CheckHp();
    }

    public virtual bool NeedReset()
    {
        if(npc!=null)
        {
            if (!isDie && (npc.info.bRunaway || npc.info.isFighting)) return true;
            else return false;
        }
        else
        {
            return !isDie;
        }
    }

    public virtual void CheckHp()  //HP체크
    {
        if(hpBar != null) hpBar.SetHPFill(currentHp, enemyData.maxHp);
        if(currentHp<=0)
        {
            Death();
        }
    }

    public virtual void SetSpeed(float s)
    {
        agent.speed = s;
        agent.isStopped = s == 0;
        if(agent.isStopped)
        {
            agent.velocity = Vector3.zero;
        }
        ani.SetFloat(speedFloat, agent.speed);
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

    protected virtual void FixedUpdate()
    {
        rigid.angularVelocity = Vector3.zero;
    }

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

        if(npcHPLowMsg.Count>0 && currentHp < npcHPLowMsg[0].hp)
        {
            //PoolManager.GetItem<SystemTxt>().OnText(npcHPLowMsg[0].message, npcHPLowMsg[0].time, npcHPLowMsg[0].fontSize);
            UIManager.Instance.OnNPCMessage(npcHPLowMsg[0], transform, msgOffset);
            npcHPLowMsg.RemoveAt(0);
        }

        SoundManager.Instance.PlaySoundHitEffect();
        Damaged();
        CheckHp();
    }

    public void Death() //뒤짐
    {
        if (npc != null)
        {
            npc.Death();
        }

        currentHp = 0;
        enemyState = EnemyState.DIE;
        isDie = true;
        SetSpeed(0);
        ani.SetTrigger(deathTrigger);
        StartCoroutine(DeathCo());
    }

    public virtual void Damaged() //데미지받고 자식들이 추가로 작업할 게 있으면 이걸로
    {
        
    }

    public virtual void StartEnemy() 
    {
        if (npc != null)
        {
            npc.active = false;
        }
    }

    protected IEnumerator DeathCo() //사망후 사라질 때까지의 딜레이
    {
        agent.enabled = false;
        yield return new WaitForSeconds(3);
        GetComponent<Collider>().enabled = false;
        rigid.isKinematic = false;
        yield return new WaitForSeconds(3);
        //ResetData();
        gameObject.SetActive(false);
    }

    protected virtual void ShowHP() //HP보여줌. 멀어지면 꺼주고
    {
        if (hpBar != null)
        {
            hpBar.transform.position = mainCam.WorldToScreenPoint(transform.position + hpOffset);

            if (uiUpdateTime < Time.time)
            {
                uiUpdateTime = Time.time + 1;
                if ((GameManager.Instance.PlayerSc.transform.position - transform.position).sqrMagnitude > enemyData.hpUIDistSquare)
                {
                    hpBar.gameObject.SetActive(false);
                    hpBar = null;
                }
            }
        }
    }
}
