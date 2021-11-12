using UnityEngine;

public class PlayerFoot : MonoBehaviour  
{
    private PlayerData pData;

    private void Start()
    {
        pData = GameManager.Instance.PlayerSc.pData;
    }

    private void OnTriggerEnter(Collider other) //�߼Ҹ� ���� ��
    {
        if (Physics.Raycast(transform.position, Vector3.down, 0.5f, pData.whatIsGround))
        {
            SoundManager.Instance.PlaySoundEffect(SoundEffectType.WALK);
        }
    }
}
