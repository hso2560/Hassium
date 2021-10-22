using UnityEngine;

[CreateAssetMenu(fileName = "MeteorData", menuName = "Scriptable Object/MeteorData", order =int.MaxValue)]
public class MeteorData : ScriptableObject
{
    public Color[] colors;
    public Flare flare;

    public Vector3[] endPoints;
    public Vector3[] startPoints;

    public float minSpeed;
    public float maxSpeed;
    public float minBrightNess;
    public float maxBrightNess;
}
