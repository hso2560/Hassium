using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    //��ũ���ͺ� ������Ʈ ���

    public virtual void Trace()
    {

    }

    public abstract void Patrol();
    public abstract void Attack();
}
