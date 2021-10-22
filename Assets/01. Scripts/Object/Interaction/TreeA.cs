using UnityEngine;

public class TreeA : Tree
{
    private TreeAPuzzle rule;
    private Material before;
    public MeshRenderer mainMesh;

    private int hp;
    public bool active = false;

    private void Awake()
    {
        rule = transform.parent.GetChild(0).GetComponent<TreeAPuzzle>();
        hp = rule.MaxHp;
        before = mainMesh.material;
    }

    public override void AddWork()
    {
        if (rule.IsStart && active)
        {
            hp--;
            if (hp <= 0)
            {
                rule.CheckCount();
                ResetData();
            }
        }
    }

    public void ResetData()
    {
        active = false;
        hp = rule.MaxHp;
        mainMesh.material = before;
    }
}
