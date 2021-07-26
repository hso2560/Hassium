using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    public PSkillType skillType;
    public bool isFirstSkillUseTreat;

    [HideInInspector] public PlayerScript player;

    [HideInInspector] public bool isUsedSkill, isUsingSkill;

    public float skillContnTime;
    public float coolTime;

    [HideInInspector] public float skillOffTime, skillRechargeTime;

    //protected SkillManager skillManager;

    public abstract void UseSkill();
    public abstract void OffSkill();
    public abstract void SetData();
    public virtual void Init(bool isFirstSkill)
    {
        if(isFirstSkill)
        {
            skillRechargeTime = Time.time + coolTime;
            isUsedSkill = true;
        }
    }
}
