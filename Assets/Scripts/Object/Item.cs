using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ObjData
{
    public int count;  //���� count�� ������ ����
    public int droppedCount = 1;  //������ ������ ��
    public Sprite sprite;

    public override void Interaction()
    {
        Inventory.Instance.GetItem(this);
    }
}
