using System.Collections.Generic;
using UnityEngine;

public class ObjData : MonoBehaviour
{
    public int id;
    public bool active = true;  //상호작용 가능 상태인지
    public string objName;  //아이템이면 아이템 이름,  오브젝트면 오브젝트 행동 이름
    public string explain;

    public int saveActiveStateId = -1;

    public virtual void Interaction() //버튼 클릭 시
    {
        if (saveActiveStateId > -1)
        {
            GameManager.Instance.savedData.objActiveInfo[saveActiveStateId] = false;
        }
    }
}
