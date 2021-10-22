using UnityEngine;

[CreateAssetMenu(fileName ="Player Data", menuName ="Scriptable Object/Player Data")]
public class PlayerData : ScriptableObject
{
    public float rotateSpeed = 10f;
    public float gravity = 0.5f; //�߷�
    public float groundRayDist = 0.58f;  //�÷��̾ ������ ��� �ִ��� üũ�ϴ� ������ ����
    public float interactionRadius = 1.8f; //������Ʈ�� ��ȣ�ۿ� ������ ����
    public float DamagedByFallNeedHeight = 20f;  //���� ������ �������ɷ� ������ �ޱ� ���� �ּ� ���� (RigidBody�� Velocity�� y���� �̿��ҰŶ� ��Ȯ���� ���̴� �ƴ�)
    public int defaultRespawnHp = 350; //��Ȱ �� ä������ Hp
    public short maxLevel = 10;

    public LayerMask whatIsGround;
    public LayerMask whatIsObj;

    public int addStr = 2;
    public int addDef = 4;
    public int addMaxHp = 50;
    public float addMaxStamina = 5;

    public int addStatPoint = 2;
    public int addMaxExp = 150;
}
