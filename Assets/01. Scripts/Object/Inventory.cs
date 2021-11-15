using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Inventory : MonoSingleton<Inventory>, ISceneDataLoad  //�� �޴� ���� �ٸ� UI�鵵 �����ϰ� ��
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
    private RectTransform btnSelectPanelPos;  //�κ� ��ư ������ ���/������ �ߴ� �װ�
    private float slotHalfWidth;

    [SerializeField] private InfoPanelData dumpPanelInfo;

    //ĳ���� â ���� ����
    public GameObject noRenderingZone;
    public GameObject[] playerModelsInUI; //ĳ���� â���� ���� ĳ���͵�
    private GameObject currentCharInUI;
    public GameObject deathMark;
    public TextMeshProUGUI charNameTxt, chestCountTxtTmp; 
    public Text charInfoTxt, expTxt, lvTxt;
    public Image expFill;

    public Button[] charChangeBtns;

    private PlayerScript ps;
    public event Action<int> acquisitionEvent;

    //���� â ���� ����
    public GameObject treasureUIPrefab;
    public Transform treasureUIParent;

    //ĳ���� ��ȭ â
    public Text reinfNameText, statPointText;
    public Text[] statTexts;
    public Text reinforceVerifyText, statPointTxtInBuyPanel;
    private int selectedReinfStatNumber;
    [SerializeField] private int statPointPrice = 500;
  

    private GameManager gameManager;
    private Dictionary<int, Action> itemUseAction = new Dictionary<int, Action>();

    private void Awake()
    {
        btnSelectPanelPos = btnSelectPanel.GetComponent<RectTransform>();
        slotHalfWidth = itemSlots[0].GetComponent<RectTransform>().rect.width * 0.5f;
        acquisitionEvent += (id) => { };
        throwBtn.onClick.AddListener(() => 
        {
            dumpPanelInfo.objImage.sprite = clickedSlot.Item_Data.sprite;
            dumpPanelInfo.mainInput.text = "1";
            dumpPanelInfo.explain.text = $"<b>[{clickedSlot.Item_Data.name}]</b> �� �� �����ðڽ��ϱ�?\n(���� ������ �ʰ��ؼ� �Է��� ���, �ڵ����� ���������� �Էµ˴ϴ�.)";
            UIManager.Instance.OnClickUIButton(7);
        });
        useBtn.onClick.AddListener(() =>
        {
            UseItem(clickedSlot.Item_Data.id);
        });
        SetDict();
    }

    private void SetDict()  //���� ��� ���� �� �������� Ŭ���� ���� �����ϸ� ���ϰ����� ���ϴϱ� �̷��� ��
    {
        itemUseAction.Add(50, () => GetGold(-1, 1000, 2000));
        itemUseAction.Add(60, () => GetGold(-1, 3000, 4500));
        itemUseAction.Add(70, () => GetGold(-1, 6000, 8000));
        itemUseAction.Add(80, () => GetGold(-1, 10000, 15000));

        int u = 200;
        for (int i = 100; i <= 200; i += 50)
        {
            int temp = u;  //�̰� ���ϸ� ���̵� ��� u�� ���������� ����Ǵϱ� �ؾ���
            itemUseAction.Add(i, () => gameManager.PlayerSc.RecoveryHp(temp));
            u *= 2;
        }

        itemUseAction.Add(250, () => gameManager.PlayerSc.RecoveryHp(1200));
        itemUseAction.Add(300, () => gameManager.PlayerSc.RecoveryHp(3000));

        itemUseAction.Add(590, () => gameManager.PlayerSc.StatPoint += 2);
        itemUseAction.Add(600, () => gameManager.PlayerSc.StatPoint += 5);
        itemUseAction.Add(610, () => gameManager.PlayerSc.StatPoint += 8);
    }

    public void GetGold(int g, int min=0, int max=0) //min��max���̷� ��� ȹ���ϰų� ���� ��� ȹ��
    {
        int i = g;
        if(i==-1)
        {
            i = UnityEngine.Random.Range(min, max);
        }
        gameManager.savedData.userInfo.money += i;
        PoolManager.GetItem<SystemTxt>().OnText(string.Format("<color=yellow>{0}���</color>�� ȹ���Ͽ����ϴ�.", i), 3, 40);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        items = gameManager.savedData.userInfo.itemList;

        for(int i=0; i<items.Count; i++)  //�κ��丮�� ������ ���� �ҷ���
        {
            items[i].sprite = Resources.Load<Sprite>("Sprites/Item/" + items[i].spritePath);
            idToItem.Add(items[i].id, items[i]);
            itemSlots[i].SetData(items[i]);
        }

        for(int i=1; i<=gameManager.savedData.userInfo.characters.Count; i++)  //ĳ ��ü ��ư Ȱ��ȭ
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

    public void GetItem(Item itemObj) //�� ȹ��
    {
        ItemData item = itemObj.itemData;
        if(!idToItem.ContainsKey(item.id))  //���� ���� ��
        {
            if(items.Count==maxItemSlotCnt)
            {
                PoolManager.GetItem<SystemTxt>().OnText("�κ��丮�� ���� á���ϴ�.");

                return;
            }

            items.Add(item);
            idToItem.Add(item.id, item);
            item.count = itemObj.droppedCount;

            for(int i=0; i<itemSlots.Count; i++)
            {
                if(!itemSlots[i].transform.GetChild(0).gameObject.activeSelf)
                {
                    itemSlots[i].SetData(item);
                    break;
                }
            }
        }
        else  //�̹� ������ �ִ��� --> ���� ����
        {
            idToItem[item.id].count += itemObj.droppedCount;
            itemSlots.Find(x => x.Item_Data.id == item.id).itemCountText.text = idToItem[item.id].count.ToString();
        }

        itemObj.gameObject.SetActive(false);
        if(itemObj.index != -1)
           gameManager.savedData.saveObjDatas.Add(new SaveObjData(itemObj.index, SaveObjInfoType.ACTIVE, false));
        acquisitionEvent(item.id);
    }

    public void GetItem(ItemData data) //�� ȹ��. �ٸ� Ÿ���� �Ű������� ���� (�Լ� �����ε�)
    {
        if (!idToItem.ContainsKey(data.id))  
        {
            if (items.Count == maxItemSlotCnt)
            {
                //�� ���� �Ƹ� �߻����� ��������
                PoolManager.GetItem<SystemTxt>().OnText("�κ��丮�� ���� ���� ȹ���� �������� �κ��丮�� ǥ�õ��� ���� ���� �ֽ��ϴ�.");
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

    public void UseItem(int id, int count=1) //�� ���
    {
        if (!ExistItem(id)) return;

        ItemData data = idToItem[id];
        if (data.count < count)
        {
            PoolManager.GetItem<SystemTxt>().OnText($"{data.name}�� ������ �����մϴ�", 2);
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

        if(itemUseAction.ContainsKey(id))
        {
            itemUseAction[id]();
        }
        ClickSetting(false);
    }

    #region �κ��丮 â
    public void BeginDrg(bool active, ItemSlot i = null)  //�� ���� �巡�� ���� OR �巡�� ���� �ٱ����� �巡�� ����
    {
        dragImage.gameObject.SetActive(active);
        isDragging = active;

        if (i != null)
        {
            dragImage.sprite = i.Item_Data.sprite;
            beginSlot = i;
            itemSlots.ForEach(x => x.GetComponent<Image>().raycastTarget = true);
        }
        else
        {
            itemSlots.ForEach(x => x.SetRaycastTarget());
        }
    }

    public void Exchange(ItemSlot i)  //�� ���� �ΰ��� ��ġ �ٲ�
    {
        if (i == beginSlot) return;

        ItemData data1 = new ItemData(i.Item_Data);
        ItemData data2 = new ItemData(beginSlot.Item_Data);

        beginSlot.SetData(data1);
        i.SetData(data2);

        ClickSetting(false);
        itemSlots.ForEach(x => x.SetRaycastTarget());
    }

    public void Change(ItemSlot i)  //�� ���� ��ü
    {
        i.SetData(beginSlot.Item_Data);
        beginSlot.ResetData();

        ClickSetting(false);
        itemSlots.ForEach(x => x.SetRaycastTarget());
    }

    public void SortItemList() //������ ����. �׼ӿ� �� ����� �ȳ־���
    {
        itemSlots.FindAll(x => x.itemCountText.gameObject.activeSelf).ForEach(y => y.ResetData());

        items.Sort(ItemSort1);

        for (int i = 0; i < items.Count; i++)
        {
            itemSlots[i].SetData(items[i]);
        }

        ClickSetting(false);
    }

    private int ItemSort1(ItemData x, ItemData y) //���� ���
    {
        int a = ((int)x.itemType).CompareTo((int)y.itemType);

        if (a != 0) return a;
        else
        {
            return x.name.CompareTo(y.name);
        }
    }

    public void ClickItemSlot(ItemSlot ist)  //�� ���� Ŭ�� �� ó��
    {
        if (clickedSlot == null || ist!=clickedSlot)
        {
            clickedSlot = ist;
            ClickSetting(true);

            ItemData idt = ist.Item_Data;

            infoPanelData.objImage.sprite = idt.sprite;
            infoPanelData.nameText.text = idt.name;
            infoPanelData.countText.text = $"���� ����: {idt.count}";
            infoPanelData.explain.text = idt.explain;
        }
        else
        {
            ClickSetting(false);
        }
    }

    private void ClickSetting(bool active) //������ Ŭ���� ���� �ߴ°�
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

    public void DumpItem()  //������ ������
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

    #region ĳ���� â
    public void ClickCharacterPanel(bool on)  //ĳ���� â �����ų� �� �� 
    {
        noRenderingZone.SetActive(on);
        
        if (on)
        {
            ViewCharacterInfo(ps.Id);
        }
    }

    public void ViewCharacterInfo(short id) //ĳ��â�� �� �÷��̾� �� ���
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

    public void UpdateCharInfoUI()//���� ĳ�� �ɷ�ġ ���� ���
    {
        if (currentCharInUI != null)
        {
            deathMark.SetActive(ps.isDie);
            charNameTxt.text = ps.CharName;

            charInfoTxt.text = $"���ݷ�: {ps.str}\n\n����: {ps.def}\n\n�����: {ps.hp}/{ps.MaxHp}\n\n���׹̳�: {Mathf.Round(ps.stamina)}/{ps.MaxStamina}\n\n " +
                $"�޸��� �ӵ�: {Mathf.Round(ps.runSpeed)}\n\n ������: {Mathf.Round(ps.jumpPower)}\n\n ���׹̳� ȸ�� �ӵ�: {Mathf.Round(ps.staminaRecoverySpeed)}";

            expTxt.text = string.Format("{0}/{1}", ps.Exp, ps.MaxExp);
            expFill.fillAmount = (float)ps.Exp / ps.MaxExp;
            lvTxt.text = "����: " + ps.Level.ToString();
        }
    }

    public void ChangeCharacter(int id)  //ĳ ��ü
    {
        gameManager.ChangeCharacter((short)id);
        ViewCharacterInfo((short)id);
        UIManager.Instance.OnClickUIButton(9);
    }

    //�ɷ�ġ ��ȭ
    public void AddStat()  //�ɷ�ġ ��ȭ
    {
        if (gameManager.PlayerSc.skill.isUsingSkill)
        {
            PoolManager.GetItem<SystemTxt>().OnText("��ų ����߿��� �ɷ�ġ�� �ø� �� �����ϴ�. ����Ŀ� �ٽ� �õ����ּ���.");
            return;
        }

        if(gameManager.PlayerSc.StatPoint==0)
        {
            PoolManager.GetItem<SystemTxt>().OnText("���� ����Ʈ�� �����մϴ�.");
            return;
        }

        switch (selectedReinfStatNumber)  //���ʷ� ��, ��, �ִ� ü, �ִ� ���׹̳�
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

    public void ShowStatUpVerify(int number) //�ɷ�ġ ��ȭ Ȯ�� �г� ���
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
                reinforceVerifyText.text = $"<b>[���ݷ�]</b>\n\n����: {gameManager.PlayerSc.str}\n����: <color=#27009A>{GameManager.Instance.PlayerSc.str+pData.addStr}</color>\n(�Ҹ� ����Ʈ: 1)";
                break;
            case 2:
                reinforceVerifyText.text = $"<b>[����]</b>\n\n����: {gameManager.PlayerSc.def}\n����: <color=#27009A>{GameManager.Instance.PlayerSc.def + pData.addDef}</color>\n(�Ҹ� ����Ʈ: 1)";
                break;
            case 3:
                reinforceVerifyText.text = $"<b>[�ִ� ü��]</b>\n\n����: {gameManager.PlayerSc.MaxHp}\n����: <color=#27009A>{GameManager.Instance.PlayerSc.MaxHp + pData.addMaxHp}</color>\n(�Ҹ� ����Ʈ: 1)";
                break;
            case 4:
                reinforceVerifyText.text = $"<b>[�ִ� ���׹̳�]</b>\n\n����: {gameManager.PlayerSc.MaxStamina}\n����: <color=#27009A>{GameManager.Instance.PlayerSc.MaxStamina + pData.addMaxStamina}</color>\n(�Ҹ� ����Ʈ: 1)";
                break;
        }
    }

    public void OnClickReinforceBtn()  //��ȭ ��ư Ŭ��
    {
        PlayerScript p = gameManager.PlayerSc;
        reinfNameText.text = "��ȭ ĳ����: " + p.CharName;
        statPointText.text = "���� ����Ʈ: " + p.StatPoint.ToString();

        statTexts[0].text = string.Concat("���ݷ�: ", p.str);
        statTexts[1].text = string.Concat("����: ", p.def);
        statTexts[2].text = string.Concat("�ִ� ü��: ", p.MaxHp);
        statTexts[3].text = string.Concat("�ִ� ���׹̳�: ", p.MaxStamina);
    }

    public void BuyStatPoint()  //��������Ʈ �����ϱ�
    {
        if(gameManager.savedData.userInfo.money<statPointPrice)
        {
            PoolManager.GetItem<SystemTxt>().OnText("��尡 �����մϴ�.");
            return;
        }
        gameManager.savedData.userInfo.money -= statPointPrice;
        gameManager.PlayerSc.StatPoint++;

        statPointText.text = "���� ����Ʈ: " + gameManager.PlayerSc.StatPoint.ToString();
        sceneObjs.gameTexts[0].text = gameManager.savedData.userInfo.money.ToString() + " ���";
        statPointTxtInBuyPanel.text = "���� " + statPointText.text;
    }

    #endregion

    #region ����â

    public void AddTreasure(ChestData data) //�� ������ ���� ���� �߰�
    {
        GameObject o = Instantiate(treasureUIPrefab, treasureUIParent);
        o.transform.GetChild(1).GetComponent<Text>().text = data.name;
        o.transform.GetChild(2).GetComponent<Text>().text = data.date;
    }

    public void LoadTreasure() //����� ���� ������ �ҷ���
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





//End�� Drop ������: 1. End�� ��ü �ۿ� ������ ����ص� ȣ��, Drop�� �ش� ��ü �ȿ����� ����ؾ� ȣ��
//                  2. End�� ���� ��ü�� ��ũ��Ʈ���� ȣ������� Drop�� ó���� ���� ��ü�� �ƴϴ��� ���� ��ũ��Ʈ �޸� ��ü�� ����ϸ� ȣ�� ������ �� ��ü���� ȣ��
//              ex) 2�� ������ ��� �巡�� �ϴٰ� 18�� ���Կ��� ����� �ϸ� End�� 2�� ���Կ��� ȣ��ǰ� Drop�� 18�� ���Կ��� ȣ���.
// Drop�� End���� ���� ȣ���.









//