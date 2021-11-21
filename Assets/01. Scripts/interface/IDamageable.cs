using UnityEngine;

public interface IDamageable //딜 들가는 것들
{
    public void OnDamaged(int damage, Vector3 hitNormal, float force, bool useDef);
    public void Death();
}
