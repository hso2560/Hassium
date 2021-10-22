using UnityEngine;

public interface IDamageable
{
    public void OnDamaged(int damage, Vector3 hitNormal, float force, bool useDef);
    public void Death();
}
