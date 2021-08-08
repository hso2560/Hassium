using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Door : ObjData
{
    public enum DoorType
    {
        ETERNAL,
        ONLYONCE
    }

    public GameObject obj;
    public Ease ease = Ease.Linear;
    public Vector3 targetPos;
    public float time;
    public DoorType doorType;

    public bool isDOT;
    public bool isOpen;
    public int objIndex;
    public bool isChangeCamRange;
    public Vector3 camMinPos, camMaxPos;

    private Animator ani;
    private Sequence seq;
    private Vector3 firstPos;

    private void Awake()
    {
        if(!isDOT)
        {
            ani = obj.GetComponent<Animator>();
        }
        else
        {
            seq = DOTween.Sequence();
            firstPos = transform.position;
        }
    }

    public override void Interaction()
    {
        if(isDOT)
        {
            seq.Append(obj.transform.DOMove(targetPos, time).SetEase(ease));
            /*seq.AppendCallback(() =>
            {
                
                if(isChangeCamRange)
                {
                    GameManager.Instance.camMove.camMinPos = camMinPos;
                    GameManager.Instance.camMove.camMaxPos = camMaxPos;
                }
            });*/
            seq.Play();
        }
        else
        {
            ani.Play(isOpen?"Close":"Open");
        }

        isOpen = !isOpen;

        if (isChangeCamRange)
        {
            GameManager.Instance.camMove.camMinPos = camMinPos;
            GameManager.Instance.camMove.camMaxPos = camMaxPos;
        }
        if (doorType==DoorType.ONLYONCE)
        {
            active = false;
            GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(objIndex, SaveObjInfoType.TRANSFORM, targetPos, obj.transform.rotation, obj.transform.localScale));
        }
    }
}
