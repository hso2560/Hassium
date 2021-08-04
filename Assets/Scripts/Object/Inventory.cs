using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoSingleton<Inventory>, ISceneDataLoad
{
    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public List<Item> items = new List<Item>();
    public Dictionary<int, Item> idToItem = new Dictionary<int, Item>(); 
    private int maxItemSlotCnt = 28; // 4 * 7

    public void GetItem(Item item)
    {
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
            idToItem[item.id].count += item.droppedCount;
            //�κ��丮 �� ������Ʈ
        }

        item.gameObject.SetActive(false);
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        Inventory[] managers = FindObjectsOfType<Inventory>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();



        isReady = true;
    }
}
