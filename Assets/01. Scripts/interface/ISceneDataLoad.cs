using UnityEngine;
//�Ŵ��� ������Ʈ�鿡 ���� �������̽�
//�� ó�� ������ SceneObjects�� ���ؼ� �Ʒ��� �Լ��� ����� ��� �Ŵ��� ������Ʈ���� ��ȯ��ų����
//�ٵ� ������ �̷������� ���ص� �ǰ� ���ϴ°� �� ������ ��. (�׳� �̷��� �� �� �غ��ǵ� ���� �� �����ϴ���)
public interface ISceneDataLoad
{
    public void ManagerDataLoad(GameObject sceneObjs); //ó���� �غ���

    public bool GetReadyState  //�غ�� �����ΰ�
    {
        get;
        set;
    }
}
