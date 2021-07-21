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

    private void Start()
    {
        player = GetComponent<PlayerScript>();
        skillManager = GameManager.Instance.skillManager;

        base.Init(isFirstSkillUseTreat);

        skillManager.playerSkills.Add(this);
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
        player.joystickCtrl.ClearSkillBtn();
        player.joystickCtrl.skillBtn.onClick.AddListener(UseSkill);
    }
}
