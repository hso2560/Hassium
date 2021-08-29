using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoorButton : ObjData
{
    public bool s_active;
    public int objIdx;
    public float time;

    public Vector3 targetVector;

    private Sequence seq;

    private void Awake()
    {
        active = s_active;
    }

    public override void Interaction()
    {
        seq = DOTween.Sequence();

        switch (id)
        {
            case 10:
                seq.Append(transform.DOScale(targetVector, time));
                break;

            default:
                break;
        }

        seq.AppendCallback(() =>
        {
            GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(objIdx, SaveObjInfoType.TRANSFORM,
                transform.position, transform.rotation, transform.localScale));
        }).Play();
    }
}
