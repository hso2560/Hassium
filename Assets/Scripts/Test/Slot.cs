using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//���� § �׽�Ʈ�� �ڵ�
public class Slot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    float lastClickTime=-1;
    private Image mImg;

    private void Start()
    {
        mImg = SlotManager.instance.mImg;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(transform.GetChild(0).gameObject.activeSelf)
        {
            SlotManager.instance.Begin(true, transform.GetChild(0).GetComponent<Image>());
            //mImg.transform.position = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(SlotManager.instance.isDragging)
           mImg.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("EndDrag"+gameObject.name);
        if (SlotManager.instance.isDragging)
            SlotManager.instance.Begin(false);   
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!SlotManager.instance.isDragging) return;

       // Debug.Log("Drop"+gameObject.name);
        if(SlotManager.instance.isDragging && transform.GetChild(0).gameObject.activeSelf)
        {
            SlotManager.instance.Exchange(transform.GetChild(0).GetComponent<Image>());
        }
        else
        {
            SlotManager.instance.Change(transform.GetChild(0).GetComponent<Image>());
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Debug.Log("Click");
        if (lastClickTime == -1)
        {
            lastClickTime = Time.time;
        }
        else
        {
            if(Time.time-lastClickTime<0.7f)
            {
                Debug.Log("Double Click");
            }
            else
            {
                lastClickTime = Time.time;
            }
        }
    }

    //End�� Drop ������: 1. End�� ��ü �ۿ� ������ ����ص� ȣ��, Drop�� �ش� ��ü �ȿ����� ����ؾ� ȣ��
    //                  2. End�� ���� ��ü�� ��ũ��Ʈ���� ȣ������� Drop�� ó���� ���� ��ü�� �ƴϴ��� ���� ��ũ��Ʈ �޸� ��ü�� ����ϸ� ȣ�� ������ �� ��ü���� ȣ��
    //              ex) 2�� ������ ��� �巡�� �ϴٰ� 18�� ���Կ��� ����� �ϸ� End�� 2�� ���Կ��� ȣ��ǰ� Drop�� 18�� ���Կ��� ȣ���.
    // Drop�� End���� ���� ȣ���.
}
