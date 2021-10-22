using UnityEngine;

public class ReinforceSkill : Skill
{
    //값을 더함
    public float runSpeed; 
    public float jumpPower;
    public float staminaRecoverySpeed;

    //값을 곱함
    public float str;
    public float def;

    private void Awake()
    {
        player = GetComponent<PlayerScript>();
        base.Init(isFirstSkillUseTreat);
        skillExplain = $"{skillContnTime}초 동안 캐릭터의 일부 능력치들을 상승시킨다. (재사용 대기시간: {coolTime}초)\n달리기 속도 {runSpeed} 상승, 점프력 {jumpPower} 상승\n스테미나 회복 속도 {staminaRecoverySpeed} 상승\n공격력 {str}배 상승, 방어력 {def}배 상승";
    }


    public override void UseSkill()
    {
        if (!isUsedSkill && !isUsingSkill)
        {
            isUsingSkill = true;
            skillOffTime = Time.time + skillContnTime;

            player.runSpeed += runSpeed;
            player.jumpPower += jumpPower;
            player.staminaRecoverySpeed += staminaRecoverySpeed;

            player.str = AbilUp_Multiple((float)player.str, str);
            player.def = AbilUp_Multiple((float)player.def, def);
            EffectManager.Instance.OnPlayerSkillEffect(player.skillType, player.center.position, 0.5f);
        }
    }

    public int AbilUp_Multiple(float abil, float value)
        => Mathf.RoundToInt(abil * value);
    

    public override void OffSkill()
    {
        if (isUsingSkill)
        {
            player.runSpeed -= runSpeed;
            player.jumpPower -= jumpPower;
            player.staminaRecoverySpeed -= staminaRecoverySpeed;

            player.str = AbilUp_Multiple((float)player.str, 1 / str);
            player.def = AbilUp_Multiple((float)player.def, 1 / def);

            isUsingSkill = false;

            isUsedSkill = true;
            skillRechargeTime = Time.time + coolTime;
        }
    }

    public override void SetData()
    {
        //player.joystickCtrl.ClearSkillBtn();
        //player.joystickCtrl.skillBtn.onClick.AddListener(UseSkill);
    }
}
