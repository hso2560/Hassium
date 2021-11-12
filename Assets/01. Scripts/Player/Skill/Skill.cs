using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    public string skillExplain;

    public bool isFirstSkillUseTreat;
    public bool isResetIfChangeChar;
    public bool isHoldSkill;

    public Sprite skillBtnImg;

    [HideInInspector] public PlayerScript player;

    public bool isUsedSkill, isUsingSkill;

    public float skillContnTime;
    public float coolTime;

    [HideInInspector] public float skillOffTime, skillRechargeTime;

    public GameObject skillEffect;
    public GameObject trailEffect;
    //protected SkillManager skillManager;

    public abstract void UseSkill(); //��ų ���
    public abstract void OffSkill(); //��ų ��
    public abstract void SetData();
    public virtual void Init(bool isFirstSkill)
    {
        if(isFirstSkill)
        {
            skillRechargeTime = Time.time + coolTime;
            isUsedSkill = true;
        }
    }

    public virtual void Change()  //ĳ���Ͱ� �ٲ� ��
    {
          
    }
}
