using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum SceneType
{
    START,
    LOBBY,
    MAIN
}

public class SceneObjects : MonoBehaviour  //�ش� ������ �ʿ��� ������Ʈ���� ��Ƶд�. (������ �ִ� ��ũ��Ʈ)
{
    public SceneType ScType;
    public Transform ManagerGroup;
    public Transform poolTrm;

    public Image[] gameImgs;
    public Ease[] gameEases;

    public JoystickControl joystickCtrl;

    private void Awake()
    {
        GameManager.Instance.ManagerDataLoad(gameObject);
        UIManager.Instance.ManagerDataLoad(gameObject);
        SoundManager.Instance.ManagerDataLoad(gameObject);
    }
}
