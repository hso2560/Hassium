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

public class SceneObjects : MonoBehaviour  //해당 씬마다 필요한 오브젝트들을 모아둔다. (씬마다 있는 스크립트)
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
