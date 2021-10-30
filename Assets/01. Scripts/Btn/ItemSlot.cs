using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [SerializeField] private Image dragImg;
    [SerializeField] private Image itemImg;
    public Text itemCountText;

    [SerializeField] private ItemData itemData;
    public ItemData Item_Data { get { return itemData; } set { itemData = value; } }

    [HideInInspector] public Image slotImage;

    public void SetData(ItemData iData) //버튼에 이미지와 텍스트를 주고 아이템 정보를 준다
    {
        itemImg.gameObject.SetActive(true);
        itemCountText.gameObject.SetActive(true);
        itemData = iData;
        itemImg.sprite = iData.sprite;
        itemCountText.text = iData.count.ToString();
        try
        {
            slotImage.raycastTarget = true;
        }
        catch
        {
            slotImage = GetComponent<Image>();
            slotImage.raycastTarget = true;
        }
    }
    public void ResetData()  //버튼을 빈버튼으로 하고 데이터 없앤다
    {
        itemImg.gameObject.SetActive(false);
        itemCountText.gameObject.SetActive(false);
        itemData = null;
        slotImage.raycastTarget = false;
    }

    public void SetRaycastTarget()
    {
        if(itemImg.gameObject.activeSelf)
        {
            slotImage.raycastTarget = true;
        }
        else
        {
            slotImage.raycastTarget = false;
        }
    }

    private void Start()
    {
        dragImg = Inventory.Instance.dragImage;
    }

    public void OnBeginDrag(PointerEventData eventData) //버튼 누를 때
    {
        if (itemImg.gameObject.activeSelf)
        {
            Inventory.Instance.BeginDrg(true, this);
        }
    }

    public void OnDrag(PointerEventData eventData)  //버튼 누른 상태를 지속할 때
    {
        if (Inventory.Instance.isDragging)
            dragImg.transform.position = eventData.position;
    }

    public void OnDrop(PointerEventData eventData)  //버튼을 뗄 때. (해당 인터페이스 존재하는 버튼에서)
    {
        if (!Inventory.Instance.isDragging) return;

        if (Inventory.Instance.isDragging && itemImg.gameObject.activeSelf)
        {
            Inventory.Instance.Exchange(this);
        }
        else
        {
            Inventory.Instance.Change(this);
        }
    }

    public void OnEndDrag(PointerEventData eventData)  //버튼을 뗄 때 (화면에서 어느 위치에서 떼든 호출)
    {
        if (Inventory.Instance.isDragging)
            Inventory.Instance.BeginDrg(false);
    }

    public void OnPointerClick(PointerEventData eventData)  //버튼 클릭 시
    {
        if(itemImg.gameObject.activeSelf)
           Inventory.Instance.ClickItemSlot(this);
    }
}
