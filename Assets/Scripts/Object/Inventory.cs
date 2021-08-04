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
                //획득 불가능 메시지 띄우기

                return;
            }

            items.Add(item);
            idToItem.Add(item.id, item);

            //인벤토리에 넣기
        }
        else
        {
            idToItem[item.id].count += item.droppedCount;
            //인벤토리 뷰 업데이트
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
