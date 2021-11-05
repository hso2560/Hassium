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
        skillExplain = $"스킬 버튼을 누르고 있으면 최대 {skillContnTime}초 전으로까지 체력, 스테미나, 캐릭터의 위치를 되돌릴 수 있다. (재사용 대기시간: {coolTime}초)";
    }

    public override void UseSkill()
    {
        if(!isUsedSkill && !isUsingSkill)
        {
            player.isMovable = false;

            isUsingSkill = true;
            skillOffTime = Time.time + skillContnTime;

            isRewind = true;
            EffectManager.Instance.SkillEffectVolume(PSkillType.TIME, true);
            //MapManager.Instance.dayAndNight.OnOffLightEffect(true);
        }
    }

    public override void OffSkill()
    {
        if (isUsingSkill)
        {
            //MapManager.Instance.dayAndNight.OnOffLightEffect(false);
            EffectManager.Instance.SkillEffectVolume(PSkillType.TIME, false);
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
            UIManager.Instance.AdjustFillAmound(UIType.HPFILL, player.hp, player.MaxHp);

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
