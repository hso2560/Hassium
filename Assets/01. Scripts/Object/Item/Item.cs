using System.Collections.Generic;
using UnityEngine;

public class Item : ObjData  //아이템들 얘 상속 받고 할 수도 있겠지만 어차피 아이템 별로 없어서 걍 함
{
    public int index;
    public int droppedCount = 1;  //떨어진 아이템 수
    public ItemData itemData;

    private void Awake()
    {
        itemData.id = id;
        objName = string.Concat(itemData.name, "×", droppedCount);
    }

    public override void Interaction()
    {
        Inventory.Instance.GetItem(this);
    }
}
