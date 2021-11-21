using UnityEngine;

public class FollowPlayer : MonoBehaviour  //플레이어 따라다니는 옵젝 (x,z만)
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
