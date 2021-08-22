using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Door : ObjData  //문 열릴 때 ScaleX값을 0.003으로 바꿔야하는데(저장 포함) 어떻게 해야 조금 더 효율적으로 코드를 짤 수 있는지 좀 더 생각이 필요 
{
    public enum DoorType
    {
        ETERNAL,
        ONLYONCE
    }

    public GameObject[] obj;  //타겟
    public bool bSimultaneously = true; //움직일 오브젝트가 여러개일 경우 동시에 움직일지
    public Ease ease = Ease.Linear;
    public Vector3[] targetPos;
    public float[] time;  //target의 변화 속도
    public DoorType doorType;
    public SaveObjInfoType soit = SaveObjInfoType.TRANSFORM;
     
    public bool isDOT;  //DOTween을 써서 움직이나
    public bool isOpen;  //열린 상태?
    public int[] objIndex;  
    public bool isChangeCamRange; //카메라 범위 변경 할지
    public Vector3 camMinPos, camMaxPos;

    [SerializeField] private Animator ani;
    private Sequence seq;
    private Vector3[] firstPos;

    private void Awake()
    {
        if(isDOT)
        {
            seq = DOTween.Sequence();
            firstPos = new Vector3[obj.Length];

            for(int i=0; i<firstPos.Length; i++)
            {
                firstPos[i] = obj[i].transform.position;
            }
        }
    }

    private void Start()
    {
        if (doorType == DoorType.ONLYONCE)
        {
            if (GameManager.Instance.savedData.objActiveInfo.objActiveKeys.Contains(saveActiveStateId))
            {
                active = GameManager.Instance.savedData.objActiveInfo[saveActiveStateId];
            }
        }
    }

    public override void Interaction()
    {
        if(isDOT)
        {
            if(bSimultaneously)
            {
                for(int i=0; i<obj.Length; i++)
                {
                    obj[i].transform.DOMove(targetPos[i], time[i]).SetEase(ease);
                }
            }
            else
            {
                for (int i = 0; i < obj.Length; i++)
                {
                    seq.Append(obj[i].transform.DOMove(targetPos[i], time[i]).SetEase(ease));
                }
                seq.Play();
            }
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
        if (doorType == DoorType.ONLYONCE)
        {
            active = false;

            for(int i=0; i<obj.Length; i++)
                GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(objIndex[i], soit, targetPos[i], obj[i].transform.rotation, obj[i].transform.localScale));

            base.Interaction();
        }
    }
}










/*seq.AppendCallback(() =>
            {
                
                if(isChangeCamRange)
                {
                    GameManager.Instance.camMove.camMinPos = camMinPos;
                    GameManager.Instance.camMove.camMaxPos = camMaxPos;
                }
            });*/
