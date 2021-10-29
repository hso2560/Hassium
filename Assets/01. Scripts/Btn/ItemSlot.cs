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

    public void SetData(ItemData iData) //��ư�� �̹����� �ؽ�Ʈ�� �ְ� ������ ������ �ش�
    {
        itemImg.gameObject.SetActive(true);
        itemCountText.gameObject.SetActive(true);
        itemData = iData;
        itemImg.sprite = iData.sprite;
        itemCountText.text = iData.count.ToString();
    }
    public void ResetData()  //��ư�� ���ư���� �ϰ� ������ ���ش�
    {
        itemImg.gameObject.SetActive(false);
        itemCountText.gameObject.SetActive(false);
        itemData = null;
    }

    private void Start()
    {
        dragImg = Inventory.Instance.dragImage;
    }

    public void OnBeginDrag(PointerEventData eventData) //��ư ���� ��
    {
        if (itemImg.gameObject.activeSelf)
        {
            Inventory.Instance.BeginDrg(true, this);
        }
    }

    public void OnDrag(PointerEventData eventData)  //��ư ���� ���¸� ������ ��
    {
        if (Inventory.Instance.isDragging)
            dragImg.transform.position = eventData.position;
    }

    public void OnDrop(PointerEventData eventData)  //��ư�� �� ��. (�ش� �������̽� �����ϴ� ��ư����)
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

    public void OnEndDrag(PointerEventData eventData)  //��ư�� �� �� (ȭ�鿡�� ��� ��ġ���� ���� ȣ��)
    {
        if (Inventory.Instance.isDragging)
            Inventory.Instance.BeginDrg(false);
    }

    public void OnPointerClick(PointerEventData eventData)  //��ư Ŭ�� ��
    {
        if(itemImg.gameObject.activeSelf)
           Inventory.Instance.ClickItemSlot(this);
    }
}
