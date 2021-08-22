using System.Collections.Generic;
using UnityEngine;

public class ObjData : MonoBehaviour
{
    public int id;
    public bool active = true;  //��ȣ�ۿ� ���� ��������
    public string objName;  //�������̸� ������ �̸�,  ������Ʈ�� ������Ʈ �ൿ �̸�
    public string explain;

    public int saveActiveStateId = -1;

    public virtual void Interaction() //��ư Ŭ�� ��
    {
        if (saveActiveStateId > -1)
        {
            GameManager.Instance.savedData.objActiveInfo[saveActiveStateId] = false;
        }
    }
}
