using System.Collections.Generic;
using UnityEngine;

public class ObjData : MonoBehaviour
{
    public int id;
    public bool active = true;  //��ȣ�ۿ� ���� ��������
    public string objName;  //�������̸� ������ �̸�,  ������Ʈ�� ������Ʈ �ൿ �̸�
    public string explain;

    public virtual void Interaction() //��ư Ŭ�� ��
    {
    }
}
