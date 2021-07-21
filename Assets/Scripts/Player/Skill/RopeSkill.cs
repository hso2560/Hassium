using UnityEngine;

public class RopeSkill : Skill
{
    [HideInInspector] public short ropeStatePhase = 0;

    private void Start()
    {
        player = GetComponent<PlayerScript>();
        skillManager = GameManager.Instance.skillManager;

        base.Init(isFirstSkillUseTreat);

        skillManager.playerSkills.Add(this);
    }

    public override void UseSkill()
    {
        if(!isUsingSkill && !isUsedSkill)
        {

        }
    }

    public override void OffSkill()
    {
        if(isUsingSkill)
        {

        }
    }

    private void Update()
    {
        
    }

    public override void SetData()
    {
        player.joystickCtrl.ClearSkillBtn();
        player.joystickCtrl.skillBtn.onClick.AddListener(UseSkill);
    }
}
