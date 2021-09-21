using System.Collections.Generic;
using UnityEngine;

public class Item : ObjData
{
    public int droppedCount = 1;  //떨어진 아이템 수
    public ItemData itemData;

    public override void Interaction()
    {
        Inventory.Instance.GetItem(this);
    }
}
