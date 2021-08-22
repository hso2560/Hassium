using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Door : ObjData  //�� ���� �� ScaleX���� 0.003���� �ٲ���ϴµ�(���� ����) ��� �ؾ� ���� �� ȿ�������� �ڵ带 © �� �ִ��� �� �� ������ �ʿ� 
{
    public enum DoorType
    {
        ETERNAL,
        ONLYONCE
    }

    public GameObject[] obj;  //Ÿ��
    public bool bSimultaneously = true; //������ ������Ʈ�� �������� ��� ���ÿ� ��������
    public Ease ease = Ease.Linear;
    public Vector3[] targetPos;
    public float[] time;  //target�� ��ȭ �ӵ�
    public DoorType doorType;
    public SaveObjInfoType soit = SaveObjInfoType.TRANSFORM;
     
    public bool isDOT;  //DOTween�� �Ἥ �����̳�
    public bool isOpen;  //���� ����?
    public int[] objIndex;  
    public bool isChangeCamRange; //ī�޶� ���� ���� ����
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
