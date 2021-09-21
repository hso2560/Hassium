using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAI : ObjData
{
    public NPCInfo info;

    public override void Interaction()
    {
        base.Interaction();

        TalkManager.Instance.StartTalk(info);
    }
}
