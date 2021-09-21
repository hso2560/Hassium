using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoSingleton<Inventory>, ISceneDataLoad
{
    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public List<ItemData> items;
    public Dictionary<int, ItemData> idToItem = new Dictionary<int, ItemData>(); 
    private int maxItemSlotCnt = 28; // 4 * 7

    private void Start()
    {
        items = GameManager.Instance.savedData.userInfo.itemList;

        for(int i=0; i<items.Count; i++)
        {
            idToItem.Add(items[0].id, items[i]);
        }
    }

    public void GetItem(Item itemObj)
    {
        ItemData item = itemObj.itemData;
        if(!idToItem.ContainsKey(item.id))
        {
            if(items.Count==maxItemSlotCnt)
            {
                //ȹ�� �Ұ��� �޽��� ����

                return;
            }

            items.Add(item);
            idToItem.Add(item.id, item);

            //�κ��丮�� �ֱ�
        }
        else
        {
            idToItem[item.id].count += itemObj.droppedCount;
            //�κ��丮 �� ������Ʈ
        }

        itemObj.gameObject.SetActive(false);
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        Inventory[] managers = FindObjectsOfType<Inventory>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();



        isReady = true;
    }
}
