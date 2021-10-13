using System.Collections.Generic;
using UnityEngine;

public class TreeAPuzzle : ObjData
{
    public bool IsStart { get; set; }
    [SerializeField] private float limitTime;

    public string startObjName = "시작";
    public string resetObjName = "초기화";

    private void Start()
    {
        if (GameManager.Instance.ContainKeyActiveId(saveActiveStateId))
        {
            active = GameManager.Instance.savedData.objActiveInfo[saveActiveStateId];
        }
    }

    public override void Interaction()
    {
        if (IsStart)
        {
            UIManager.Instance.TimeAttackMission(false);

            return;
        }

        IsStart = true;
        objName = resetObjName;

        UIManager.Instance.clearEvent += () =>
        {
            IsStart = false;
            active = false;
            base.Interaction();
        };
        UIManager.Instance.timeOverEvent += () =>
        {
            objName = startObjName;
            IsStart = false;
        };

        UIManager.Instance.OnTimer((int)limitTime);
    }
}
