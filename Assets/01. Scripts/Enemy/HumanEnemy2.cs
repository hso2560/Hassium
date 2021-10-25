using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanEnemy2 : EnemyBase
{
    public Transform wayPointsGroup;
    private List<Transform> wayPoints;
    private int pointIndex;

    public float judgeDelay = .6f;

    private float checkTime;

    //여기선 isTrace가 isRunaway처럼 쓰인다

    protected override void Awake()
    {
        base.Awake();

        wayPoints = new List<Transform>(wayPointsGroup.GetComponentsInChildren<Transform>());
        wayPoints.RemoveAt(0);

        pointIndex = Random.Range(0, wayPoints.Count);
        target = wayPoints[pointIndex];
    }

    public override void ResetData()
    {
        base.ResetData();
        //원래 가던 곳까진 가야지
    }

    void Sight()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, enemyData.traceRange, enemyData.playerLayer);

        if (cols.Length > 0)
        {
            Runaway();
        }
        else
        {
            Stop();
        }
    }

    private void Update()
    {
        if (npc.info.bRunaway && !isDie)
        {
            base.ShowHP();
            if (checkTime < Time.time)
            {
                checkTime = Time.time + judgeDelay;
                Sight();
            }
        }
    }

    public override void Attack()
    {
        //공격 없음
    }

    public override void Patrol()
    {
        //순찰 없음
    }

    public override void Stop()
    {
        if (IsArriveTarget())
        {
            isTrace = false;
            base.Stop();
        }
    }

    public override void Runaway()
    {
        isTrace = true;
        if(IsArriveTarget())
        {
            agent.velocity = Vector3.zero;

            int prevIdx = pointIndex;
            do
            {
                pointIndex = Random.Range(0, wayPoints.Count);
            } while (pointIndex==prevIdx);

            target = wayPoints[pointIndex];
            agent.SetDestination(target.position);
            base.SetSpeed(enemyData.traceSpeed);
        }
        else
        {
            agent.SetDestination(target.position);
            base.SetSpeed(enemyData.traceSpeed);
        }
    }

    bool IsArriveTarget() => (target.position - transform.position).sqrMagnitude < 1;

    public override void Damaged()
    {
        if (!npc.info.bRunaway)
        {
            StartEnemy();
        }
        if (!isTrace)
        {
            Runaway();
        }
    }

    public override void StartEnemy()
    {
        base.StartEnemy();
        npc.info.bRunaway = true;
    }
}
