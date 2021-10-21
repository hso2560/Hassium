using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Inventory : MonoSingleton<Inventory>, ISceneDataLoad  //걍 메뉴 안의 다른 UI들도 관리하게 됨
{
    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public PlayerData pData;
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
    public event Action acquisitionEvent;

    //보물 창 관련 변수
    public GameObject treasureUIPrefab;
    public Transform treasureUIParent;

    //캐릭터 강화 창
    public Text reinfNameText, statPointText;
    public Text[] statTexts;
    public Text reinforceVerifyText, statPointTxtInBuyPanel;
    private int selectedReinfStatNumber;
    [SerializeField] private int statPointPrice = 500;
  

    private GameManager gameManager;

    private void Awake()
    {
        btnSelectPanelPos = btnSelectPanel.GetComponent<RectTransform>();
        slotHalfWidth = itemSlots[0].GetComponent<RectTransform>().rect.width * 0.5f;
        acquisitionEvent += () => { };
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
        gameManager = GameManager.Instance;
        items = gameManager.savedData.userInfo.itemList;

        for(int i=0; i<items.Count; i++)
        {
            items[i].sprite = Resources.Load<Sprite>("Sprites/Item/" + items[i].spritePath);
            idToItem.Add(items[i].id, items[i]);
            itemSlots[i].SetData(items[i]);
        }

        for(int i=1; i<=gameManager.savedData.userInfo.characters.Count; i++)
        {
            short key = (short)(i * 10);
            if (gameManager.IsExistCharac(key))
            {
                charChangeBtns[i - 1].gameObject.SetActive(true);
                charChangeBtns[i - 1].transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.idToMyPlayer[key].CharName;
            }
        }

        LoadTreasure();
        ps = gameManager.PlayerSc;
    }

    public bool ExistItem(int id) => idToItem.ContainsKey(id);

    public void GetItem(Item itemObj) //템 획득
    {
        ItemData item = itemObj.itemData;
        if(!idToItem.ContainsKey(item.id))  //없는 템일 때
        {
            if(items.Count==maxItemSlotCnt)
            {
                PoolManager.GetItem<SystemTxt>().OnText("인벤토리가 가득 찼습니다.");

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
        gameManager.savedData.saveObjDatas.Add(new SaveObjData(itemObj.index, SaveObjInfoType.ACTIVE, false));
        acquisitionEvent();
    }

    public void GetItem(ItemData data)
    {
        if (!idToItem.ContainsKey(data.id))  
        {
            if (items.Count == maxItemSlotCnt)
            {
                //이 경우는 아마 발생하지 않을거임
                PoolManager.GetItem<SystemTxt>().OnText("인벤토리가 가득 차서 획득한 아이템이 인벤토리에 표시되지 않을 수도 있습니다.");
            }

            items.Add(data);
            idToItem.Add(data.id, data);

            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (!itemSlots[i].transform.GetChild(0).gameObject.activeSelf)
                {
                    itemSlots[i].SetData(data);
                    break;
                }
            }
        }
        else  
        {
            idToItem[data.id].count += data.count;
            itemSlots.Find(x => x.Item_Data.id == data.id).itemCountText.text = idToItem[data.id].count.ToString();
        }
    }

    public void UseItem(int id, int count=1)
    {
        if (!ExistItem(id)) return;

        ItemData data = idToItem[id];
        if (data.count < count)
        {
            PoolManager.GetItem<SystemTxt>().OnText($"{data.name}의 개수가 부족합니다", 2);
            return;
        }

        data.count -= count;

        if (data.count == 0)
        {
            items.Remove(data);
            idToItem.Remove(id);
            GetItemSlot(id).ResetData();
        }
        else
        {
            GetItemSlot(id).itemCountText.text = data.count.ToString();
        }
    }

    #region 인벤토리 창
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

    private void ClickSetting(bool active) //아이템 클릭시 정보 뜨는거
    {
        if (!active) clickedSlot = null;
        infoPanelData.infoPanel.gameObject.SetActive(active);
        btnSelectPanel.SetActive(active);

        if (active)
        {
            Vector3 pos = clickedSlot.GetComponent<RectTransform>().position;
            btnSelectPanelPos.position = new Vector3(pos.x+slotHalfWidth,pos.y) + (new Vector3(btnSelectPanelPos.rect.width, btnSelectPanelPos.rect.height) * 0.5f);
            useBtn.gameObject.SetActive(clickedSlot.Item_Data.itemType != ItemType.ETC);
            throwBtn.gameObject.SetActive(!clickedSlot.Item_Data.cannotDump);
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

    public ItemSlot GetItemSlot(int id) => itemSlots.Find(x => x.gameObject.activeSelf && x.Item_Data.id == id);
    
    #endregion

    #region 캐릭터 창
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
        if (gameManager.IsExistCharac(id))
        {
            if (currentCharInUI != null) currentCharInUI.SetActive(false);

            currentCharInUI = playerModelsInUI[(id / 10) - 1];
            currentCharInUI.SetActive(true);
            ps = gameManager.idToMyPlayer[id];
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
                $"달리기 속도: {Mathf.Round(ps.runSpeed)}\n\n 점프력: {Mathf.Round(ps.jumpPower)}\n\n 스테미나 회복 속도: {Mathf.Round(ps.staminaRecoverySpeed)}";

            expTxt.text = string.Format("{0}/{1}", ps.Exp, ps.MaxExp);
            expFill.fillAmount = (float)ps.Exp / ps.MaxExp;
            lvTxt.text = "레벨: " + ps.Level.ToString();
        }
    }

    public void ChangeCharacter(int id)
    {
        gameManager.ChangeCharacter((short)id);
        ViewCharacterInfo((short)id);
        UIManager.Instance.OnClickUIButton(9);
    }

    //능력치 강화
    public void AddStat()
    {
        if(gameManager.PlayerSc.StatPoint==0)
        {
            PoolManager.GetItem<SystemTxt>().OnText("스탯 포인트가 부족합니다.");
            return;
        }

        switch (selectedReinfStatNumber)  //차례로 공, 방, 최대 체, 최대 스테미나
        {
            case 1:
                gameManager.PlayerSc.str += pData.addStr;
                break;
            case 2:
                gameManager.PlayerSc.def += pData.addDef;
                break;
            case 3:
                gameManager.PlayerSc.MaxHp += pData.addMaxHp;
                break;
            case 4:
                PlayerScript _p = gameManager.PlayerSc;
                _p.MaxStamina += pData.addMaxStamina;
                UIManager.Instance.AdjustFillAmound(UIType.HPFILL, _p.hp, _p.MaxHp);
                break;
        }
        gameManager.PlayerSc.StatPoint--;
        ShowStatUpVerify(-1);
        OnClickReinforceBtn();
    }

    public void ShowStatUpVerify(int number)
    {
        if (number != -1)
        {
            UIManager.Instance.OnClickUIButton(11);
            selectedReinfStatNumber = number;
        }
        else number = selectedReinfStatNumber;
        switch (number)  
        {
            case 1:
                reinforceVerifyText.text = $"<b>[공격력]</b>\n\n현재: {gameManager.PlayerSc.str}\n다음: <color=#27009A>{GameManager.Instance.PlayerSc.str+pData.addStr}</color>\n(소모 포인트: 1)";
                break;
            case 2:
                reinforceVerifyText.text = $"<b>[방어력]</b>\n\n현재: {gameManager.PlayerSc.def}\n다음: <color=#27009A>{GameManager.Instance.PlayerSc.def + pData.addDef}</color>\n(소모 포인트: 1)";
                break;
            case 3:
                reinforceVerifyText.text = $"<b>[최대 체력]</b>\n\n현재: {gameManager.PlayerSc.MaxHp}\n다음: <color=#27009A>{GameManager.Instance.PlayerSc.MaxHp + pData.addMaxHp}</color>\n(소모 포인트: 1)";
                break;
            case 4:
                reinforceVerifyText.text = $"<b>[최대 스테미나]</b>\n\n현재: {gameManager.PlayerSc.MaxStamina}\n다음: <color=#27009A>{GameManager.Instance.PlayerSc.MaxStamina + pData.addMaxStamina}</color>\n(소모 포인트: 1)";
                break;
        }
    }

    public void OnClickReinforceBtn()
    {
        PlayerScript p = gameManager.PlayerSc;
        reinfNameText.text = "강화 캐릭터: " + p.CharName;
        statPointText.text = "스탯 포인트: " + p.StatPoint.ToString();

        statTexts[0].text = string.Concat("공격력: ", p.str);
        statTexts[1].text = string.Concat("방어력: ", p.def);
        statTexts[2].text = string.Concat("최대 체력: ", p.MaxHp);
        statTexts[3].text = string.Concat("최대 스테미나: ", p.MaxStamina);
    }

    public void BuyStatPoint()
    {
        if(gameManager.savedData.userInfo.money<statPointPrice)
        {
            PoolManager.GetItem<SystemTxt>().OnText("골드가 부족합니다.");
            return;
        }
        gameManager.savedData.userInfo.money -= statPointPrice;
        gameManager.PlayerSc.StatPoint++;

        statPointText.text = "스탯 포인트: " + gameManager.PlayerSc.StatPoint.ToString();
        sceneObjs.gameTexts[0].text = gameManager.savedData.userInfo.money.ToString();
        statPointTxtInBuyPanel.text = "현재 " + statPointText.text;
    }

    #endregion

    #region 보물창

    public void AddTreasure(ChestData data)
    {
        GameObject o = Instantiate(treasureUIPrefab, treasureUIParent);
        o.transform.GetChild(1).GetComponent<Text>().text = data.name;
        o.transform.GetChild(2).GetComponent<Text>().text = data.date;
    }

    public void LoadTreasure()
    {
        List<ChestData> list = GameManager.Instance.savedData.userInfo.myChestList;
        for(int i=0; i < list.Count; ++i)
        {
            GameObject o = Instantiate(treasureUIPrefab, treasureUIParent);
            o.transform.GetChild(1).GetComponent<Text>().text = list[i].name;
            o.transform.GetChild(2).GetComponent<Text>().text = list[i].date;
        }
    }

    #endregion

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





//End와 Drop 차이점: 1. End는 객체 밖에 나가서 드랍해도 호출, Drop은 해당 객체 안에서만 드롭해야 호출
//                  2. End는 잡은 객체의 스크립트에서 호출되지만 Drop은 처음에 잡은 객체가 아니더라도 같은 스크립트 달린 객체에 드롭하면 호출 되지만 그 객체에서 호출
//              ex) 2번 슬롯을 잡고 드래그 하다가 18번 슬롯에서 드롭을 하면 End는 2번 슬롯에서 호출되고 Drop은 18번 슬롯에서 호출됨.
// Drop이 End보다 먼저 호출됨.









//