using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Door : ObjData  //�� ���� �� ScaleX���� 0.003���� �ٲ���ϴµ�(���� ����) ��� �ؾ� ���� �� ȿ�������� �ڵ带 © �� �ִ��� �� �� ������ �ʿ� 
{
    public enum DoorType
    {
        ETERNAL,
        ONLYONCE,
        ROTATION
    }

    public GameObject[] obj;  //Ÿ�ٵ�
    public bool bSimultaneously = true; //������ ������Ʈ�� �������� ��� ���ÿ� ��������
    public Ease ease = Ease.Linear;
    public Vector3[] targetPos;  //Ÿ�� ��ġ��
    public float[] time;  //target�� ��ȭ �ӵ�
    public DoorType doorType;  //���� Ÿ��
    public SaveObjInfoType soit = SaveObjInfoType.TRANSFORM;  //���� ������ ������ ��� ������Ʈ ���� Ÿ��
     
    public bool isDOT;  //DOTween�� �Ἥ �����̳�
    public bool isOpen;  //���� ����?
    public int[] objIndex;  
    //public bool isChangeCamRange; //ī�޶� ���� ���� ����
    //public Vector3 camMinPos, camMaxPos;  //x:-167.627 y:-135 z:-50  x:31.5 y:85 z:80

    [SerializeField] private Animator ani;
    private Sequence seq;
    private Vector3[] firstPos;  //������ ��ġ��

    public bool useDoorBtnScr;  //DoorButton��ũ��Ʈ�� Interation�Լ� ����
    public DoorButton doorBtnData;  //DoorButton��ũ��Ʈ ����

    public bool needKey = false;
    public int needKeyId;

    [SerializeField] private bool isNoTime = false;

    private void Awake()
    {
        if(isDOT)  //��Ʈ������ ������ ��� �ʱ� ��ġ���� �����صд�
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
        if (doorType == DoorType.ONLYONCE)  //ONLYONCEŸ���� ��� ������ �� �� ���� ��Ƽ�� ���¸� �ҷ��ͼ� �����Ѵ�
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
            PoolManager.GetItem<SystemTxt>().OnText("������ �� �� �����ϴ�.");
            return;
        }

        if (needKey)
        {
            if (!Inventory.Instance.ExistItem(needKeyId))
            {
                PoolManager.GetItem<SystemTxt>().OnText("���谡 �ʿ��մϴ�.",2);
                return;
            }
            else
            {
                Inventory.Instance.UseItem(needKeyId);
            }
        }

        if (useDoorBtnScr)
        {
            doorBtnData.Interaction();  //DoorButton�� Interation�Լ� ȣ��
        }

        if(isDOT)  //��Ʈ�� ����� ���
        {
            if(bSimultaneously)  //���ÿ� ���� ������ ��
            {
                for(int i=0; i<obj.Length; i++)
                {
                    obj[i].transform.DOMove(targetPos[i], time[i]).SetEase(ease);
                }
            }
            else  //���� ���ʴ�� ������ ��
            {
                for (int i = 0; i < obj.Length; i++)
                {
                    seq.Append(obj[i].transform.DOMove(targetPos[i], time[i]).SetEase(ease));
                }
                seq.Play();
            }
        }
        else  //�ִϸ��̼����� ������ ��
        {
            ani.Play(isOpen?"Close":"Open");
        }

        isOpen = !isOpen;
        SoundManager.Instance.PlaySoundEffect(SoundEffectType.MOVEDOOR, time[0]);

        /*if (isChangeCamRange)    //���� ������ ī�޶��� ������ �ٲ� ��
        {
            GameManager.Instance.camMove.camMinPos = camMinPos;
            GameManager.Instance.camMove.camMaxPos = camMaxPos;
        }*/
        if (doorType == DoorType.ONLYONCE)  //ONLYONCE��
        {
            active = false;  //�ٽô� ��ȣ�ۿ� ���ϰ� ����

            for(int i=0; i<obj.Length; i++)  //������Ʈ ��ġ ���� ����
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
