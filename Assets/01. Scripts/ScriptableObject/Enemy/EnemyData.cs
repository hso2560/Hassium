using UnityEngine;

[CreateAssetMenu(fileName ="Enemy Data", menuName ="Scriptable Object/Enemy Data", order =int.MaxValue)]
public class EnemyData : ScriptableObject
{
    public EnemyType eType;
    public LayerMask playerLayer;

    public string enemyName;

    public int maxHp;

    public float speed;  //��� �̵� �ӵ�
    public float traceSpeed;  //�߰� �ӵ�
    public float patrolRange;  //��� �̵� ����
    public float traceRange;  //�߰� ����(�÷��̾���� �Ÿ�)
    public float attackRange;  //���� ����(�÷��̾���� �Ÿ�)
    public float viewingAngle;  //�þ߰�
    public float movableRange;  //�����������κ��� �ش� ���� �Ѿ�� �߰� ����
    public float hpUIDistSquare = 169; //player���� �Ÿ��� ������ �� ������ ũ�� HP UI�Ⱥ��� 
}

public enum EnemyType
{
    TRMPOINTSPATROL_ORDER,  //Tramsform[]�� ���� ������� �̵�
    TRMPOINTSPATROL_RANDOM,  // ���� ���������� ������ ����Ʈ�� �̵�
    RANDOMLYMOVE, //�ƹ� �������� �����ϰ� �̵�
}
