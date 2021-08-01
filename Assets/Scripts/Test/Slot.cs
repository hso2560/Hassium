using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//대충 짠 테스트용 코드
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

    //End와 Drop 차이점: 1. End는 객체 밖에 나가서 드랍해도 호출, Drop은 해당 객체 안에서만 드롭해야 호출
    //                  2. End는 잡은 객체의 스크립트에서 호출되지만 Drop은 처음에 잡은 객체가 아니더라도 같은 스크립트 달린 객체에 드롭하면 호출 되지만 그 객체에서 호출
    //              ex) 2번 슬롯을 잡고 드래그 하다가 18번 슬롯에서 드롭을 하면 End는 2번 슬롯에서 호출되고 Drop은 18번 슬롯에서 호출됨.
    // Drop이 End보다 먼저 호출됨.
}
