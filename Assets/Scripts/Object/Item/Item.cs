using System.Collections.Generic;
using UnityEngine;

public class Item : ObjData
{
    public int index;
    public int droppedCount = 1;  //������ ������ ��
    public ItemData itemData;

    private void Awake()
    {
        itemData.id = id;
        objName = string.Concat(itemData.name, "��", droppedCount);
    }

    public override void Interaction()
    {
        Inventory.Instance.GetItem(this);
    }
}
