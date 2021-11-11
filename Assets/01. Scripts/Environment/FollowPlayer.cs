using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;
    }

    private void LateUpdate()
    {
        if (gm.PlayerSc != null) 
        {
            Vector3 target = gm.PlayerSc.transform.position;
            transform.position = new Vector3(target.x, transform.position.y, target.z);
        }
    }
}
