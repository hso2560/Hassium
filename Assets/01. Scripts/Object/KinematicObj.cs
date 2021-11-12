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

    private void OnCollisionEnter(Collision collision) //땅에 닿으면 옵젝이 못움직이게 막음
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
