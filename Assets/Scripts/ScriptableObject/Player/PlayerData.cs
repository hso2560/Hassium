using UnityEngine;

[CreateAssetMenu(fileName ="Player Data", menuName ="Scriptable Object/Player Data")]
public class PlayerData : ScriptableObject
{
    public float groundRayDist = 0.58f;  //�÷��̾ ������ ��� �ִ��� üũ�ϴ� ������ ����
    public float interactionRadius = 1.8f; //������Ʈ�� ��ȣ�ۿ� ������ ����
    public float DamagedByFallNeedHeight = 20f;  //���� ������ �������ɷ� ������ �ޱ� ���� �ּ� ���� (RigidBody�� Velocity�� y���� �̿��ҰŶ� ��Ȯ���� ���̴� �ƴ�)
}
