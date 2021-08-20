using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public EnemyData enemyData;

    public bool bViewingAngle360;

    protected Vector3 startPoint;
    
    public virtual void Trace()
    {

    }
    public virtual void Die()
    {

    }
    public virtual void ResetData()
    {

    }

    public abstract void Patrol();
    public abstract void Attack();
}
