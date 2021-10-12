using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private PlayerScript ps;
    [SerializeField] private EnemyBase enemy;

    [SerializeField] private int force = 25;

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable e = collision.gameObject.GetComponent<IDamageable>();

        //e?.OnDamaged(1, Vector3.zero, 1);
        if (e != null)
        {
            GameObject o=null;
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

            if (o != collision.gameObject)
            {
                e.OnDamaged(damage, (o.transform.position-collision.transform.position).normalized, force, true);
            }
        }
    }
}
