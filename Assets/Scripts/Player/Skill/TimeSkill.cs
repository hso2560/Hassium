using System.Collections.Generic;
using UnityEngine;

public class TimeSkill : Skill
{
    private List<PrevInfo> prevInfos;
    [HideInInspector] public bool isRewind = false;
    private int rewindCnt;
    private PrevInfo prInfo;

    private void Awake()
    {
        player = GetComponent<PlayerScript>();
        prevInfos = new List<PrevInfo>();
        rewindCnt = (int)skillContnTime * 60;

        base.Init(isFirstSkillUseTreat);
    }

    public override void UseSkill()
    {
        if(!isUsedSkill && !isUsingSkill)
        {
            player.isMovable = false;

            isUsingSkill = true;
            skillOffTime = Time.time + skillContnTime;

            isRewind = true;
        }
    }

    public override void OffSkill()
    {
        if (isUsingSkill)
        {
            player.isMovable = true;
            isRewind = false;

            isUsingSkill = false;
            prevInfos.Clear();

            isUsedSkill = true;
            skillRechargeTime = Time.time + coolTime;
        }
    }

    private void Record()
    {
        prevInfos.Add(new PrevInfo(transform.position,player.playerModel.rotation,transform.rotation,player.hp,player.stamina));

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

            transform.position = prInfo.position;
            player.playerModel.position = transform.position;
            player.playerModel.rotation = prInfo.modelRotation;
            transform.rotation = prInfo.rotation;
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

        //player.joystickCtrl.ClearSkillBtn();

        //player.joystickCtrl.entry1.callback.AddListener((data) => UseSkill());
        //player.joystickCtrl.entry2.callback.AddListener((data) => OffSkill());

        //player.joystickCtrl.SkillBtnTriggerAdd();
    }
}
