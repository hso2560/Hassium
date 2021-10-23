using UnityEngine;

[System.Serializable]
public class PrevInfo
{
    public Vector3 position;
    public Quaternion modelRotation;
    public Quaternion rotation;

    public int hp;
    public float stamina;

    public PrevInfo(Vector3 pos, Quaternion modelRot, Quaternion rot, int _hp, float stam)
    {
        position = pos;
        modelRotation = modelRot;
        rotation = rot;

        hp = _hp;
        stamina = stam;
    }
}

public enum PSkillType
{
    REINFORCE,
    TIME,
    ROPE
}

public enum EnemyState
{
    STOP,
    PATROL,
    RUNAWAY,
    TRACE,
    ATTACK,
    DIE
}

[System.Serializable]
public class NPCHPLowMsg
{
    public int hp;
    public string message;
    public float time = 3f;
    public int fontSize = 50;
}