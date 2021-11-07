using System;
using UnityEngine;

public class ObjData : MonoBehaviour
{
    public int id;
    public bool active = true;  //상호작용 가능 상태인지
    public string objName;  //아이템이면 아이템 이름,  오브젝트면 오브젝트 행동 이름
    public string explain;

    public int saveActiveStateId = -1;

    /*private void Awake()
    {
        if (saveActiveStateId > -1) Debug.Log(saveActiveStateId); //확인용
    }*/

    public virtual void Interaction() //버튼 클릭 시
    {
        if (saveActiveStateId > -1)  //오브젝트 액티브상태 값 저장
        {
            GameManager.Instance.savedData.objActiveInfo[saveActiveStateId] = false;
        }
    }

    public virtual void BaseStart(Action activeFunc=null, Action inactiveFunc=null)  //첨에 시작할 때 옵젝 버튼 (비)활성화 여부 저장 됐는지 확인하고 상태에 따라서 함수 실행
    {
        if (GameManager.Instance.ContainKeyActiveId(saveActiveStateId))
        {
            active = GameManager.Instance.savedData.objActiveInfo[saveActiveStateId];
        }
        if (active)
        {
            activeFunc?.Invoke();
        }
        else
        {
            inactiveFunc?.Invoke();
        }
    }
}
