using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    
    public bool isFirstSkillUseTreat;
    public bool isResetIfChangeChar;
    public bool isHoldSkill;

    public Sprite skillBtnImg;

    [HideInInspector] public PlayerScript player;

    public bool isUsedSkill, isUsingSkill;

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

    public virtual void Change()  //캐릭터가 바뀔 때
    {
          
    }
}
