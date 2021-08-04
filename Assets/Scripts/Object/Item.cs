using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ObjData
{
    public int count;  //현재 count개 가지고 있음
    public int droppedCount = 1;  //떨어진 아이템 수
    public Sprite sprite;

    public override void Interaction()
    {
        Inventory.Instance.GetItem(this);
    }
}
