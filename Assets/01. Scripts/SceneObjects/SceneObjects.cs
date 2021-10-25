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

public class SceneObjects : MonoBehaviour  //�ش� ������ �ʿ��� ������Ʈ���� ��Ƶд�. (������ �ִ� ��ũ��Ʈ)
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
        Screen.SetResolution(scrWidth, scrHeight, true);  //���ϸ� �ػ� ����
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

    public bool IsAllReadyManager() //��� �ų����� �غ�ƴ���
    {
        for(int i=0; i<managers.Count; i++)
        {
            if (!managers[i].GetReadyState) return false;
        }

        return true;
    }

    public void AllReadyFalse()  //��� �Ŵ����� �غ� ���¸� ����
    {
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].GetReadyState = false;
        }
    }

    public void OnClickUIButton(int num)  //UIManager���� ������ onClick.AddListener�� ��ư�� �߰��ϱ⿣ ��ũ��Ʈ�� �������� �� ���� �̰����� �ذ��ϰڴ�
    {
        UIManager.Instance.OnClickUIButton(num);
    }

    public void TestBtn(int i)  //Test Code
    {
        GameManager.Instance.ResetData((short)i);
    }
}
