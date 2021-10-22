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

    protected override void Awake()
    {
        base.Awake();

        wayPoints = new List<Transform>(wayPointsGroup.GetComponentsInChildren<Transform>());
        wayPoints.RemoveAt(0);

        ws = new WaitForSeconds(judgeDelay);
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
            Transform tr = cols[0].transform;
            Vector3 dir = (tr.position - transform.position).normalized;
            float angle = Vector3.Angle(dir, transform.forward);

            if (angle < enemyData.viewingAngle * 0.5f)
            {
                if ((transform.position - tr.position).sqrMagnitude < enemyData.attackRange)
                    enemyState = EnemyState.ATTACK;
                else
                    enemyState = EnemyState.TRACE;
            }
            else if(enemyState!=EnemyState.STOP) 
                enemyState = EnemyState.PATROL;
        }
        else if(enemyState != EnemyState.STOP)
        {
            int random = Random.Range(0, 10);
            if(random<3 && (transform.position - target.position).sqrMagnitude < 1f)
            {
                enemyState = EnemyState.STOP;
            }
            else
            {
                enemyState = EnemyState.PATROL;
            }
        }
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
        }
    }

    public override void Attack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            base.SetSpeed(0, true);
            ani.SetTrigger(atkTrigger);
        }
    }

    public override void Patrol()
    {
        if((transform.position - target.position).sqrMagnitude < 1f)
        {
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
    }

    public override void StartEnemy()
    {
        npc.info.isFighting = true;
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
                    }
                    break;
            }
        }
    }
}
