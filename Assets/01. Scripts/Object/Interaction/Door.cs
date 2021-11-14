using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Door : ObjData  //문 열릴 때 ScaleX값을 0.003으로 바꿔야하는데(저장 포함) 어떻게 해야 조금 더 효율적으로 코드를 짤 수 있는지 좀 더 생각이 필요 
{
    public enum DoorType
    {
        ETERNAL,
        ONLYONCE,
        ROTATION
    }

    public GameObject[] obj;  //타겟들
    public bool bSimultaneously = true; //움직일 오브젝트가 여러개일 경우 동시에 움직일지
    public Ease ease = Ease.Linear;
    public Vector3[] targetPos;  //타겟 위치들
    public float[] time;  //target의 변화 속도
    public DoorType doorType;  //문의 타입
    public SaveObjInfoType soit = SaveObjInfoType.TRANSFORM;  //문의 정보를 저장할 경우 오브젝트 저장 타입
     
    public bool isDOT;  //DOTween을 써서 움직이나
    public bool isOpen;  //열린 상태?
    public int[] objIndex;  
    //public bool isChangeCamRange; //카메라 범위 변경 할지
    //public Vector3 camMinPos, camMaxPos;  //x:-167.627 y:-135 z:-50  x:31.5 y:85 z:80

    [SerializeField] private Animator ani;
    private Sequence seq;
    private Vector3[] firstPos;  //원래의 위치값

    public bool useDoorBtnScr;  //DoorButton스크립트의 Interation함수 쓸지
    public DoorButton doorBtnData;  //DoorButton스크립트 담음

    public bool needKey = false;
    public int needKeyId;

    [SerializeField] private bool isNoTime = false;

    private void Awake()
    {
        if(isDOT)  //다트윈으로 움직일 경우 초기 위치들을 저장해둔다
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
        if (doorType == DoorType.ONLYONCE)  //ONLYONCE타입일 경우 시작할 때 이 값의 액티브 상태를 불러와서 대입한다
        {
            if (GameManager.Instance.ContainKeyActiveId(saveActiveStateId))
            {
                active = GameManager.Instance.savedData.objActiveInfo[saveActiveStateId];
            }
        }
    }

    public override void Interaction()
    {
        if(isNoTime)
        {
            PoolManager.GetItem<SystemTxt>().OnText("아직은 열 수 없습니다.");
            return;
        }

        if (needKey)
        {
            if (!Inventory.Instance.ExistItem(needKeyId))
            {
                PoolManager.GetItem<SystemTxt>().OnText("열쇠가 필요합니다.",2);
                return;
            }
            else
            {
                Inventory.Instance.UseItem(needKeyId);
            }
        }

        if (useDoorBtnScr)
        {
            doorBtnData.Interaction();  //DoorButton의 Interation함수 호출
        }

        if(isDOT)  //다트윈 사용의 경우
        {
            if(bSimultaneously)  //동시에 문이 움직일 때
            {
                for(int i=0; i<obj.Length; i++)
                {
                    obj[i].transform.DOMove(targetPos[i], time[i]).SetEase(ease);
                }
            }
            else  //문이 차례대로 움직일 때
            {
                for (int i = 0; i < obj.Length; i++)
                {
                    seq.Append(obj[i].transform.DOMove(targetPos[i], time[i]).SetEase(ease));
                }
                seq.Play();
            }
        }
        else  //애니메이션으로 움직일 떄
        {
            ani.Play(isOpen?"Close":"Open");
        }

        isOpen = !isOpen;
        SoundManager.Instance.PlaySoundEffect(SoundEffectType.MOVEDOOR, time[0]);

        /*if (isChangeCamRange)    //문이 열리고 카메라의 범위가 바뀔 때
        {
            GameManager.Instance.camMove.camMinPos = camMinPos;
            GameManager.Instance.camMove.camMaxPos = camMaxPos;
        }*/
        if (doorType == DoorType.ONLYONCE)  //ONLYONCE면
        {
            active = false;  //다시는 상호작용 못하게 해줌

            for(int i=0; i<obj.Length; i++)  //오브젝트 위치 정보 저장
                GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(objIndex[i], soit, targetPos[i], obj[i].transform.rotation, obj[i].transform.localScale));

            base.Interaction();

            /*switch (id)
            {
                case 20:

                    break;
            }*/
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
