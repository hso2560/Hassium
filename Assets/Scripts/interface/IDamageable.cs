using UnityEngine;

public interface IDamageable
{
    public void OnDamaged(int damage, Vector3 hitNormal, float force);
    public void Death();
}
