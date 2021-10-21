using UnityEngine;

public class KinematicObj : MonoBehaviour
{
    private Rigidbody rigid;
    [HideInInspector] public bool colDisable = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.isKinematic = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Ground"))
        {
            rigid.isKinematic = true;
            if (colDisable)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
