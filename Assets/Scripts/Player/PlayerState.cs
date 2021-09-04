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