using UnityEngine;

public class ReinforceSkill : Skill
{
    //���� ����
    public float runSpeed; 
    public float jumpPower;
    public float staminaRecoverySpeed;

    //���� ����
    public float str;
    public float def;

    private void Awake()
    {
        player = GetComponent<PlayerScript>();
        base.Init(isFirstSkillUseTreat);
        skillExplain = $"{skillContnTime}�� ���� ĳ������ �Ϻ� �ɷ�ġ���� ��½�Ų��. (���� ���ð�: {coolTime}��)\n�޸��� �ӵ� {runSpeed} ���, ������ {jumpPower} ���\n���׹̳� ȸ�� �ӵ� {staminaRecoverySpeed} ���\n���ݷ� {str}�� ���, ���� {def}�� ���";
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
