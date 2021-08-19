using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    //스크립터블 오브젝트 사용

    public virtual void Trace()
    {

    }

    public abstract void Patrol();
    public abstract void Attack();
}
