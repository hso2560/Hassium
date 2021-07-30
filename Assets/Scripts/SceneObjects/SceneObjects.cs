using System.Collections.Generic;
using System.Collections;
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
    private List<ISceneDataLoad> managers = new List<ISceneDataLoad>();

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
        SkillManager.Instance.ManagerDataLoad(gameObject);
        MapManager.Instance.ManagerDataLoad(gameObject);

        managers.Add(GameManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(UIManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(SoundManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(SkillManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(MapManager.Instance.GetComponent<ISceneDataLoad>());

        StartCoroutine(StartGame());

        //UIManager.Instance.LoadingFade(true, 0);

        //Application.runInBackground = true;
        //Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public IEnumerator StartGame()
    {
        while (!IsAllReadyManager()) yield return null;

        UIManager.Instance.LoadingFade(true, 0);
    }

    public bool IsAllReadyManager()
    {
        for(int i=0; i<managers.Count; i++)
        {
            if (!managers[i].GetReadyState) return false;
        }

        return true;
    }

    public void AllReadyFalse()
    {
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].GetReadyState = false;
        }
    }

    public void TestBtn(int i)  //Test Code
    {
        GameManager.Instance.ResetData((short)i);
    }
}
