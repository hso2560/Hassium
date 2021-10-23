using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanEnemy : EnemyBase
{
    public Transform wayPointsGroup;
    private List<Transform> wayPoints;
    private int wayPointIndex = 0;
    private float uiUpdateTime;

    public float judgeDelay = 0.2f;
    public Vector3 offset;
    private Camera mainCam;
    private WaitForSeconds ws;

    private float checkTime;
    private float _atkRange;
    private float _movRange;

    protected override void Awake()
    {
        base.Awake();

        wayPoints = new List<Transform>(wayPointsGroup.GetComponentsInChildren<Transform>());
        wayPoints.RemoveAt(0);

        ws = new WaitForSeconds(judgeDelay);
        _movRange = enemyData.movableRange * enemyData.movableRange;
        _atkRange = enemyData.attackRange * enemyData.attackRange;
        target = wayPoints[0];
    }

    private void Start()
    {
        mainCam = GameManager.Instance.sceneObjs.camMove.GetComponent<Camera>();
    }

    private void Sight()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, enemyData.traceRange, enemyData.playerLayer);

        if (cols.Length > 0)
        {
            if ((startPoint - transform.position).sqrMagnitude > _movRange)
            {
                transform.position = startPoint;
                StopOrPatrol(3);
            }

            Transform tr = cols[0].transform;
            if(isTrace)
            {
                TraceOrAttack(tr);
                return;
            }
            if (bViewingAngle360)
            {
                TraceOrAttack(tr);
                /*if(hit)
                   TraceOrAttack(tr);
                else if (enemyState != EnemyState.STOP)
                    StopOrPatrol(3);*/
            }
            else
            {
                Vector3 dir = (tr.position - transform.position).normalized;

                bool hit = true;
                if (frontObstacle)
                {
                    if (!Physics.Raycast(transform.forward, dir, enemyData.traceRange, enemyData.playerLayer))
                        hit = false;
                }

                float angle = Vector3.Angle(dir, transform.forward);
                if (angle < enemyData.viewingAngle * 0.5f && hit)
                    TraceOrAttack(tr);
                else if (enemyState != EnemyState.STOP)
                    StopOrPatrol(3);
            }
        }
        else if(enemyState != EnemyState.STOP)
        {
            StopOrPatrol(3);
        }
    }  //end of Sight

    private void StopOrPatrol(int range)
    {
        if (isTrace)
        {
            isTrace = false;
            target = wayPoints[wayPointIndex%wayPoints.Count];
            agent.destination = target.position;
            enemyState = EnemyState.PATROL;
            return;
        }
        int random = Random.Range(0, 10);
        if (random < range && (target.position - transform.position).sqrMagnitude < 1f)
        {
            enemyState = EnemyState.STOP;
        }
        else
        {
            enemyState = EnemyState.PATROL;
        }
    }

    private void TraceOrAttack(Transform tr)
    {
        isTrace = true;
        if ((tr.position - transform.position).sqrMagnitude < _atkRange)
            enemyState = EnemyState.ATTACK;
        else
            enemyState = EnemyState.TRACE;
    }

    protected void Update()
    {
        if (npc.info.isFighting && !isDie)
        {
            if (hpBar != null)
            {
                hpBar.transform.position = mainCam.WorldToScreenPoint(transform.position + offset);

                if(uiUpdateTime<Time.time)
                {
                    uiUpdateTime = Time.time + 1;
                    if((GameManager.Instance.PlayerSc.transform.position-transform.position).sqrMagnitude>enemyData.hpUIDistSquare)
                    {
                        hpBar.gameObject.SetActive(false);
                        hpBar = null;
                    }
                }
            }

            if (checkTime < Time.time)
            {
                checkTime = Time.time + 0.35f;
                Sight();
            }
            //Look();
        }
    }

    private void FixedUpdate()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    public override void Attack()
    {
        if (ableAtkTime<Time.time)
        {
            ableAtkTime = Time.time + attackCoolTime;
            base.SetSpeed(0);
            FunctionGroup.Look(target, transform);
            ani.SetTrigger(atkTrigger);
            //공격처리
        }
    }

    public override void Patrol()
    {
        if ((transform.position - target.position).sqrMagnitude < 1f)
        {
            agent.velocity = Vector3.zero;
            target = wayPoints[++wayPointIndex % wayPoints.Count];
            base.SetSpeed(enemyData.speed);
            agent.SetDestination(target.position);
        }
    }

    public override void Damaged()
    {
        if (!npc.info.isFighting)
        {
            StartEnemy();
        }
        if (!isTrace)
        {
            TraceOrAttack(GameManager.Instance.PlayerSc.transform);
        }
    }

    public override void StartEnemy()
    {
        base.StartEnemy();
        npc.info.isFighting = true;
        base.SetSpeed(enemyData.speed);
        agent.destination = target.position;
        StartCoroutine(EnemyAI());
    }

    private IEnumerator EnemyAI()
    {
        while(!isDie)
        {
            //Sight();
            yield return ws;
            switch(enemyState)
            {
                case EnemyState.PATROL:
                    Patrol();
                    break;
                case EnemyState.TRACE:
                    Trace();
                    break;
                case EnemyState.ATTACK:
                    Attack();
                    break;
                case EnemyState.STOP:
                    Stop();
                    float t = 0;
                    float limit = Random.Range(1f, 2.5f);
                    while(enemyState==EnemyState.STOP && t<limit)
                    {
                        yield return null;
                        t += Time.deltaTime;
                        if (t >= limit)
                            enemyState = EnemyState.PATROL;
                    }
                    break;
            }
        }
    }
}
