using System.Collections.Generic;
using UnityEngine;

public class TimeSkill : Skill
{
    private List<PrevInfo> prevInfos;
    [HideInInspector] public bool isRewind = false;
    private int rewindCnt;
    private PrevInfo prInfo;
    

    private void Start()
    {
        player = GetComponent<PlayerScript>();
        skillManager = GameManager.Instance.skillManager;

        prevInfos = new List<PrevInfo>();
        rewindCnt = (int)skillContnTime * 60;
        player.joystickCtrl.entry1.callback.AddListener((data) => UseSkill());
        player.joystickCtrl.entry2.callback.AddListener((data) => OffSkill());

        base.Init(isFirstSkillUseTreat);

        skillManager.playerSkills.Add(this);
    }

    public override void UseSkill()
    {
        if(!isUsedSkill && !isUsingSkill)
        {
            isUsingSkill = true;
            skillOffTime = Time.time + skillContnTime;

            isRewind = true;
        }
    }

    public override void OffSkill()
    {
        if (isUsingSkill)
        {
            isRewind = false;

            isUsingSkill = false;
            prevInfos.Clear();

            isUsedSkill = true;
            skillRechargeTime = Time.time + coolTime;
        }
    }

    private void Record()
    {
        prevInfos.Add(new PrevInfo(player.transform.position,player.playerModel.rotation,player.transform.rotation,player.hp,player.stamina));

        if(prevInfos.Count>=rewindCnt)
        {
            prevInfos.RemoveAt(0);
        }
    }

    private void Rewind()
    {
        if(prevInfos.Count>0)
        {
            prInfo = prevInfos[prevInfos.Count - 1];

            player.transform.position = prInfo.position;
            player.playerModel.rotation = prInfo.modelRotation;
            player.transform.rotation = prInfo.rotation;
            player.hp = prInfo.hp;
            player.stamina = prInfo.stamina;

            prevInfos.RemoveAt(prevInfos.Count - 1);
        }
    }

    private void Update()
    {
        if (player.parent.activeSelf)
        {
            if (!isRewind)
            {
                Record();
            }
            else
            {
                Rewind();
            }
        }
    }

    public override void SetData()
    {
        prevInfos.Clear();

        player.joystickCtrl.ClearSkillBtn();

        player.joystickCtrl.SkillBtnTriggerAdd();
    }
}
