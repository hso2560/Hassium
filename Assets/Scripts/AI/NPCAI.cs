using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAI : ObjData
{
    public NPCInfo info;

    private void Start()
    {
        if(GameManager.Instance.savedData.npcInfo.objActiveKeys.Contains(info.id))
        {
            info = GameManager.Instance.savedData.npcInfo[info.id];
        }
        else
        {
            GameManager.Instance.savedData.npcInfo[info.id] = info;
        }
    }

    public override void Interaction()
    {
        base.Interaction();

        TalkManager.Instance.StartTalk(info);
    }
}
