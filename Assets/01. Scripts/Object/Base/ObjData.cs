using System;
using UnityEngine;

public class ObjData : MonoBehaviour
{
    public int id;
    public bool active = true;  //��ȣ�ۿ� ���� ��������
    public string objName;  //�������̸� ������ �̸�,  ������Ʈ�� ������Ʈ �ൿ �̸�
    public string explain;

    public int saveActiveStateId = -1;

    /*private void Awake()
    {
        if (saveActiveStateId > -1) Debug.Log(saveActiveStateId); //Ȯ�ο�
    }*/

    public virtual void Interaction() //��ư Ŭ�� ��
    {
        if (saveActiveStateId > -1)  //������Ʈ ��Ƽ����� �� ����
        {
            GameManager.Instance.savedData.objActiveInfo[saveActiveStateId] = false;
        }
    }

    public virtual void BaseStart(Action activeFunc=null, Action inactiveFunc=null)  //÷�� ������ �� ���� ��ư (��)Ȱ��ȭ ���� ���� �ƴ��� Ȯ���ϰ� ���¿� ���� �Լ� ����
    {
        if (GameManager.Instance.ContainKeyActiveId(saveActiveStateId))
        {
            active = GameManager.Instance.savedData.objActiveInfo[saveActiveStateId];
        }
        if (active)
        {
            activeFunc?.Invoke();
        }
        else
        {
            inactiveFunc?.Invoke();
        }
    }
}
