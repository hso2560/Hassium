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

        if (info.dead)  //죽은 애는 없어야하니까
        {
            enemy.CurrentHp = 0;
            enemy.isDie = true;
            enemy.enemyState = EnemyState.DIE;
            gameObject.SetActive(false);
        }

        if (info.bRunaway || info.isFighting) //공격 받은 상태 이어가기
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
        GameManager.Instance.KillNPC();
    }
}
