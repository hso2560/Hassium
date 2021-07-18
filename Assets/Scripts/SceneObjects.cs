using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
//using Cinemachine;

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
    //public CinemachineVirtualCamera thirdPCam;
    public CameraMove camMove;

    private void Awake()
    {
        //Screen.SetResolution(1920, 1080, true);

        GameManager.Instance.ManagerDataLoad(gameObject);
        UIManager.Instance.ManagerDataLoad(gameObject);
        SoundManager.Instance.ManagerDataLoad(gameObject);

        //Application.runInBackground = true;
        //Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void TestBtn(int i)  //Test Code
    {
        GameManager.Instance.ResetData((short)i);
    }
}
