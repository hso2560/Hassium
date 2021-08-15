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
    public List<GameObject> ui;

    public SceneType ScType;
    public Transform ManagerGroup;
    public Transform poolTrm;

    public Button[] gameBtns;
    public Image[] gameImgs;
    public Ease[] gameEases;
    public Canvas[] cvses;
    public InteractionBtn[] itrBtns;

    public Slider camSlider;

    public JoystickControl joystickCtrl;
    //public CinemachineVirtualCamera thirdPCam;
    public CameraMove camMove;
    public SceneSaveObjects infoSaveObjs;

    [Header("Screen Resolution")]
    public int scrWidth = 1920, scrHeight = 1080;

    private void Awake()
    {
        Screen.SetResolution(scrWidth, scrHeight, true);

        GameManager.Instance.ManagerDataLoad(gameObject);
        UIManager.Instance.ManagerDataLoad(gameObject);
        SoundManager.Instance.ManagerDataLoad(gameObject);
        SkillManager.Instance.ManagerDataLoad(gameObject);
        MapManager.Instance.ManagerDataLoad(gameObject);
        Inventory.Instance.ManagerDataLoad(gameObject);

        managers.Add(GameManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(UIManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(SoundManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(SkillManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(MapManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(Inventory.Instance.GetComponent<ISceneDataLoad>());

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

    public void OnClickUIButton(int num)
    {
        UIManager.Instance.OnClickUIButton(num);
    }

    public void TestBtn(int i)  //Test Code
    {
        GameManager.Instance.ResetData((short)i);
    }
}
