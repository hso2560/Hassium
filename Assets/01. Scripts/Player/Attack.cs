using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private PlayerScript ps;
    [SerializeField] private EnemyBase enemy;

    [SerializeField] private int force = 25;

    private void OnTriggerEnter(Collider other)
    {
        IDamageable e = other.GetComponent<IDamageable>();

        //e?.OnDamaged(1, Vector3.zero, 1);
        if (e != null)
        {
            GameObject o = null;
            int damage;
            if (ps != null)
            {
                o = ps.gameObject;
                damage = ps.str;
            }
            else
            {
                o = enemy.gameObject;
                damage = enemy.Str;
            }

            if (o != other.gameObject)
            {
                e.OnDamaged(damage, (o.transform.position - other.transform.position).normalized, force, true);
                EffectManager.Instance.OnHitEffect(other.transform.position, Vector3.zero);
                /*if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 10))
                {
                    if(hit.transform!=null)
                       EffectManager.Instance.OnHitEffect(hit.point);
                }*/
            }
        }
    }
}
