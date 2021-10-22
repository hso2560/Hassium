using UnityEngine;

[CreateAssetMenu(fileName ="Player Data", menuName ="Scriptable Object/Player Data")]
public class PlayerData : ScriptableObject
{
    public float rotateSpeed = 10f;
    public float gravity = 0.5f; //중력
    public float groundRayDist = 0.58f;  //플레이어가 땅위를 밟고 있는지 체크하는 레이의 길이
    public float interactionRadius = 1.8f; //오브젝트와 상호작용 가능한 범위
    public float DamagedByFallNeedHeight = 20f;  //높은 곳에서 떨어진걸로 데미지 받기 위한 최소 높이 (RigidBody의 Velocity의 y값을 이용할거라 정확히는 높이는 아님)
    public int defaultRespawnHp = 350; //부활 시 채워지는 Hp
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
