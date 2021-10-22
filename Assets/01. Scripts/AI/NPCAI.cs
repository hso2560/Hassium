using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAI : ObjData
{
    public NPCInfo info;
    public GameObject puzzleRuleObj;
    public EnemyBase enemy;

    private void Start()
    {
        objName = info.name;

        if(GameManager.Instance.savedData.npcInfo.objActiveKeys.Contains(info.id))
        {
            info = GameManager.Instance.savedData.npcInfo[info.id];
        }
        else
        {
            GameManager.Instance.savedData.npcInfo[info.id] = info;
        }

        if (info.dead) gameObject.SetActive(false);

        if (info.bRunaway || info.isFighting)
        {
            active = false;
            enemy.StartEnemy();
        }
    }

    public override void Interaction()
    {
        base.Interaction();

        TalkManager.Instance.StartTalk(info);
    }

    public void Death()
    {
        puzzleRuleObj.GetComponent<IReward>().GetReward();
        info.bRunaway = false;
        info.isFighting = false;
        info.dead = true;
    }
}
