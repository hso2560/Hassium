using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    public EnemyData enemyData;

    public bool bViewingAngle360;

    protected Vector3 startPoint;
    [SerializeField] protected int str;
    public int Str { get { return str; } set { str = value; } }
    [SerializeField] protected int def;
    public int Def { get { return def; } set { def = value; } }

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

    public void OnDamaged(int damage, Vector3 hitNormal, float force, bool useDef)
    {
        
    }

    public void Death()
    {
        
    }
}
