using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private RectTransform btnSelectPanelPos;  //인벤 버튼 누르면 사용/버리기 뜨는 그거
    private float slotHalfWidth;

    [SerializeField] private InfoPanelData dumpPanelInfo;

    //캐릭터 창 관련 변수
    public GameObject noRenderingZone;
    public GameObject[] playerModelsInUI; //캐릭터 창에서 보일 캐릭터들
    private GameObject currentCharInUI;
    public GameObject deathMark;
    public TextMeshProUGUI charNameTxt; 
    public Text charInfoTxt, expTxt, lvTxt;
    public Image expFill;

    public Button[] charChangeBtns;

    private PlayerScript ps;

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
            items[i].sprite = Resources.Load<Sprite>("Sprites/Item/" + items[i].spritePath);
            idToItem.Add(items[i].id, items[i]);
            itemSlots[i].SetData(items[i]);
        }

        for(int i=1; i<=GameManager.Instance.savedData.userInfo.characters.Count; i++)
        {
            short key = (short)(i * 10);
            if (GameManager.Instance.IsExistCharac(key))
            {
                charChangeBtns[i - 1].gameObject.SetActive(true);
                charChangeBtns[i - 1].transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.idToMyPlayer[key].CharName;
            }
        }

        ps = GameManager.Instance.PlayerSc;
    }

    public void GetItem(Item itemObj) //템 획득
    {
        ItemData item = itemObj.itemData;
        if(!idToItem.ContainsKey(item.id))  //없는 템일 때
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
        else  //이미 가지고 있는템 --> 개수 증가
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
        if (i == beginSlot) return;

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

    public void SortItemList() //아이템 정렬
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

    public void DumpItem()  //아이템 버리기
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

    public void ClickCharacterPanel(bool on)
    {
        noRenderingZone.SetActive(on);
        
        if (on)
        {
            ViewCharacterInfo(ps.Id);
        }
    }

    public void ViewCharacterInfo(short id)
    {
        if (GameManager.Instance.IsExistCharac(id))
        {
            if (currentCharInUI != null) currentCharInUI.SetActive(false);

            currentCharInUI = playerModelsInUI[(id / 10) - 1];
            currentCharInUI.SetActive(true);
            ps = GameManager.Instance.idToMyPlayer[id];
            UpdateCharInfoUI();
        }
    }

    public void UpdateCharInfoUI()
    {
        if (currentCharInUI != null)
        {
            deathMark.SetActive(ps.isDie);
            charNameTxt.text = ps.CharName;

            charInfoTxt.text = $"공격력: {ps.str}\n\n방어력: {ps.def}\n\n생명력: {ps.hp}/{ps.MaxHp}\n\n스테미나: {Mathf.Round(ps.stamina)}/{ps.MaxStamina}\n\n " +
                $"달리기 속도: {ps.runSpeed}\n\n 점프력: {ps.jumpPower}\n\n 스테미나 회복 속도: {ps.staminaRecoverySpeed}";

            expTxt.text = string.Format("{0}/{1}", ps.Exp, ps.MaxExp);
            expFill.fillAmount = (float)ps.Exp / ps.MaxExp;
            lvTxt.text = "레벨: " + ps.Level.ToString();
        }
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        Inventory[] managers = FindObjectsOfType<Inventory>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        isReady = true;
    }

    public void ChangeCharacter(int id)
    {
        GameManager.Instance.ChangeCharacter((short)id);
        ViewCharacterInfo((short)id);
        UIManager.Instance.OnClickUIButton(9);
    }
}

public enum ItemType
{
    EQUIP,
    CONSUME,
    ETC
}





//End와 Drop 차이점: 1. End는 객체 밖에 나가서 드랍해도 호출, Drop은 해당 객체 안에서만 드롭해야 호출
//                  2. End는 잡은 객체의 스크립트에서 호출되지만 Drop은 처음에 잡은 객체가 아니더라도 같은 스크립트 달린 객체에 드롭하면 호출 되지만 그 객체에서 호출
//              ex) 2번 슬롯을 잡고 드래그 하다가 18번 슬롯에서 드롭을 하면 End는 2번 슬롯에서 호출되고 Drop은 18번 슬롯에서 호출됨.
// Drop이 End보다 먼저 호출됨.









//