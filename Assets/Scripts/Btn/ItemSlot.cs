using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    private Image dragImg;
    private Image itemImg;
    public Text itemCountText;

    [SerializeField] private ItemData itemData;
    public ItemData Item_Data { get { return itemData; } set { itemData = value; } }

    public void SetData(ItemData iData)
    {
        itemImg.gameObject.SetActive(true);
        itemCountText.gameObject.SetActive(true);
        itemData = iData;
        itemImg.sprite = iData.sprite;
        itemCountText.text = iData.count.ToString();
    }
    public void ResetData()
    {
        itemImg.gameObject.SetActive(false);
        itemCountText.gameObject.SetActive(false);
        itemData = null;
    }

    private void Awake()
    {
        itemImg = transform.GetChild(0).GetComponent<Image>();
        itemCountText = transform.GetChild(1).GetComponent<Text>();
    }

    private void Start()
    {
        dragImg = Inventory.Instance.dragImage;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemImg.gameObject.activeSelf)
        {
            Inventory.Instance.BeginDrg(true, this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Inventory.Instance.isDragging)
            dragImg.transform.position = eventData.position;
    }

    public void OnDrop(PointerEventData eventData)
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

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Inventory.Instance.isDragging)
            Inventory.Instance.BeginDrg(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //정보를 띄운다
    }
}
