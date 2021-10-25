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
    public GameObject[] prefabs;

    public SceneType ScType;
    public Transform ManagerGroup, environMentGroup, systemMsgParent, enemyHPParent;
    public Transform poolTrm;

    public Button[] gameBtns;
    public Image[] gameImgs;
    public Ease[] gameEases;
    public Color[] gameColors;
    public Canvas[] cvses;
    public InteractionBtn[] itrBtns;
    public Text[] gameTexts;
    public GameObject[] gameObjs;
    public Transform[] trms;

    public Slider camSlider;

    public JoystickControl joystickCtrl;
    //public CinemachineVirtualCamera thirdPCam;
    public CameraMove camMove;
    public SceneSaveObjects infoSaveObjs;

    [Header("Screen Resolution")]
    public int scrWidth = 1920, scrHeight = 1080;

    private void Awake()
    {
        Screen.SetResolution(scrWidth, scrHeight, true);  //안하면 해상도 깨짐
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        GameManager.Instance.ManagerDataLoad(gameObject);
        UIManager.Instance.ManagerDataLoad(gameObject);
        SoundManager.Instance.ManagerDataLoad(gameObject);
        SkillManager.Instance.ManagerDataLoad(gameObject);
        MapManager.Instance.ManagerDataLoad(gameObject);
        Inventory.Instance.ManagerDataLoad(gameObject);
        TalkManager.Instance.ManagerDataLoad(gameObject);
        //EffectManager.Instance.ManagerDataLoad(gameObject);

        managers.Add(GameManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(UIManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(SoundManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(SkillManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(MapManager.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(Inventory.Instance.GetComponent<ISceneDataLoad>());
        managers.Add(TalkManager.Instance.GetComponent<ISceneDataLoad>());
        //managers.Add(EffectManager.Instance.GetComponent<ISceneDataLoad>());

        StartCoroutine(StartGame());
    }

    public IEnumerator StartGame()
    {
        while (!IsAllReadyManager()) yield return null; 

        UIManager.Instance.LoadingFade(true, 0);
    }

    public bool IsAllReadyManager() //모든 매너지는 준비됐는지
    {
        for(int i=0; i<managers.Count; i++)
        {
            if (!managers[i].GetReadyState) return false;
        }

        return true;
    }

    public void AllReadyFalse()  //모든 매니저의 준비 상태를 해제
    {
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].GetReadyState = false;
        }
    }

    public void OnClickUIButton(int num)  //UIManager에서 일일히 onClick.AddListener로 버튼에 추가하기엔 스크립트가 복잡해질 것 같아 이것으로 해결하겠다
    {
        UIManager.Instance.OnClickUIButton(num);
    }

    public void TestBtn(int i)  //Test Code
    {
        GameManager.Instance.ResetData((short)i);
    }
}
