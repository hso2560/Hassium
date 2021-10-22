using UnityEngine;

[CreateAssetMenu(fileName ="Enemy Data", menuName ="Scriptable Object/Enemy Data", order =int.MaxValue)]
public class EnemyData : ScriptableObject
{
    public EnemyType eType;
    public LayerMask playerLayer;

    public string enemyName;

    public int maxHp;

    public float speed;  //평소 이동 속도
    public float traceSpeed;  //추격 속도
    public float patrolRange;  //평소 이동 범위
    public float traceRange;  //추격 범위(플레이어와의 거리)
    public float attackRange;  //공격 범위(플레이어와의 거리)
    public float viewingAngle;  //시야각
    public float movableRange;  //시작지점으로부터 해당 범위 넘어서면 추격 중지
    public float hpUIDistSquare = 169; //player와의 거리의 제곱이 이 값보다 크면 HP UI안보임 
}

public enum EnemyType
{
    TRMPOINTSPATROL_ORDER,  //Tramsform[]을 따라 순서대로 이동
    TRMPOINTSPATROL_RANDOM,  // 위와 동일하지만 랜덤한 포인트로 이동
    RANDOMLYMOVE, //아무 방향으로 랜덤하게 이동
}
