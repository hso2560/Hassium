using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoSingleton<Inventory>, ISceneDataLoad
{
    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public List<ItemData> items;
    public Dictionary<int, ItemData> idToItem = new Dictionary<int, ItemData>(); 
    private int maxItemSlotCnt = 40; // 5 * 8

    public List<ItemSlot> itemSlots = new List<ItemSlot>();
    public Image dragImage;
    public ItemSlot beginSlot;
    public bool isDragging;

    public InfoPanelData infoPanelData;
    private ItemSlot clickedSlot;

    public GameObject btnSelectPanel;
    public Button useBtn, throwBtn;
    private RectTransform btnSelectPanelPos;
    private float slotHalfWidth;

    [SerializeField] private InfoPanelData dumpPanelInfo;

    private void Awake()
    {
        btnSelectPanelPos = btnSelectPanel.GetComponent<RectTransform>();
        slotHalfWidth = itemSlots[0].GetComponent<RectTransform>().rect.width * 0.5f;
        throwBtn.onClick.AddListener(() => 
        {
            dumpPanelInfo.objImage.sprite = clickedSlot.Item_Data.sprite;
            dumpPanelInfo.mainInput.text = "1";
            dumpPanelInfo.explain.text = $"<b>[{clickedSlot.Item_Data.name}]</b> 몇 개 버리시겠습니까?\n(보유 개수를 초과해서 입력할 경우, 자동으로 보유개수로 입력됩니다.)";
            UIManager.Instance.OnClickUIButton(7);
        });
    }

    private void Start()
    {
        items = GameManager.Instance.savedData.userInfo.itemList;

        for(int i=0; i<items.Count; i++)
        {
            idToItem.Add(items[i].id, items[i]);
            itemSlots[i].SetData(items[i]);
        }
    }

    public void GetItem(Item itemObj)
    {
        ItemData item = itemObj.itemData;
        if(!idToItem.ContainsKey(item.id))
        {
            if(items.Count==maxItemSlotCnt)
            {
                //획득 불가능 메시지 띄우기

                return;
            }

            items.Add(item);
            idToItem.Add(item.id, item);
            item.count += itemObj.droppedCount;

            for(int i=0; i<itemSlots.Count; i++)
            {
                if(!itemSlots[i].transform.GetChild(0).gameObject.activeSelf)
                {
                    itemSlots[i].SetData(item);
                    break;
                }
            }
        }
        else
        {
            idToItem[item.id].count += itemObj.droppedCount;
            itemSlots.Find(x => x.Item_Data.id == item.id).itemCountText.text = idToItem[item.id].count.ToString();
        }

        itemObj.gameObject.SetActive(false);
        GameManager.Instance.savedData.saveObjDatas.Add(new SaveObjData(itemObj.index, SaveObjInfoType.ACTIVE, false));
    }

    public void BeginDrg(bool active, ItemSlot i = null)
    {
        dragImage.gameObject.SetActive(active);
        isDragging = active;

        if (i != null)
        {
            dragImage.sprite = i.Item_Data.sprite;
            beginSlot = i;
        }
    }

    public void Exchange(ItemSlot i)  
    {
        ItemData data1 = new ItemData(i.Item_Data);
        ItemData data2 = new ItemData(beginSlot.Item_Data);

        beginSlot.SetData(data1);
        i.SetData(data2);

        ClickSetting(false);
    }

    public void Change(ItemSlot i)
    {
        i.SetData(beginSlot.Item_Data);
        beginSlot.ResetData();

        ClickSetting(false);
    }

    public void SortItemList()
    {
        itemSlots.FindAll(x => x.itemCountText.gameObject.activeSelf).ForEach(y => y.ResetData());

        items.Sort(ItemSort1);

        for (int i = 0; i < items.Count; i++)
        {
            itemSlots[i].SetData(items[i]);
        }

        ClickSetting(false);
    }

    private int ItemSort1(ItemData x, ItemData y)
    {
        int a = ((int)x.itemType).CompareTo((int)y.itemType);

        if (a != 0) return a;
        else
        {
            return x.name.CompareTo(y.name);
        }
    }

    public void ClickItemSlot(ItemSlot ist)
    {
        if (clickedSlot == null || ist!=clickedSlot)
        {
            clickedSlot = ist;
            ClickSetting(true);

            ItemData idt = ist.Item_Data;

            infoPanelData.objImage.sprite = idt.sprite;
            infoPanelData.nameText.text = idt.name;
            infoPanelData.countText.text = $"보유 개수: {idt.count}";
            infoPanelData.explain.text = idt.explain;
        }
        else
        {
            ClickSetting(false);
        }
    }

    private void ClickSetting(bool active)
    {
        if (!active) clickedSlot = null;
        infoPanelData.infoPanel.gameObject.SetActive(active);
        btnSelectPanel.SetActive(active);

        if (active)
        {
            Vector3 pos = clickedSlot.GetComponent<RectTransform>().position;
            btnSelectPanelPos.position = new Vector3(pos.x+slotHalfWidth,pos.y) + (new Vector3(btnSelectPanelPos.rect.width, btnSelectPanelPos.rect.height) * 0.5f);
            useBtn.gameObject.SetActive(clickedSlot.Item_Data.itemType != ItemType.ETC); 
        }
    }

    public void DumpItem()
    {
        ItemData idt = idToItem[clickedSlot.Item_Data.id];
        idt.count -= Mathf.Clamp(int.Parse(dumpPanelInfo.mainInput.text), 0, idt.count);

        if (idt.count == 0)
        {
            items.Remove(clickedSlot.Item_Data);
            idToItem.Remove(clickedSlot.Item_Data.id);
            clickedSlot.ResetData();
        }
        else
        {
            clickedSlot.itemCountText.text = idt.count.ToString();
        }
        ClickSetting(false);
        UIManager.Instance.OnClickUIButton(7);
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        Inventory[] managers = FindObjectsOfType<Inventory>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        isReady = true;
    }
}

public enum ItemType
{
    EQUIP,
    CONSUME,
    ETC
}