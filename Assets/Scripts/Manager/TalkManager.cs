using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TalkManager : MonoSingleton<TalkManager>, ISceneDataLoad
{
    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public Animator talkPanelAni;
    [SerializeField] private Text nameText;
    [SerializeField] private Text talkText;

    [SerializeField] private CanvasGroup mainCvsg, infoCvsg;
    [SerializeField] private Button screenTouchPanel;
    public GameObject interBtnsPanel, talkEndMark;

    private int index, talkCount;
    public float talkTextTime = 0.1f;  //�� ���ڴ� ������ �ð�
    private NPCInfo currentNpc;
    private bool isTalking; //�ؽ�Ʈ�� �� ���Դ���
    Sequence seq;
    private int closeHash;

    private void Awake()
    {
        closeHash = Animator.StringToHash("close");
    }

    public void CvsgFade(CanvasGroup cg, int target, float time)
    {
        bool b = (target != 0);
        cg.DOFade(target, time);
        cg.blocksRaycasts = b;
        cg.interactable = b;
    }

    private void SetPlayerState(bool talk)
    {
        GameManager.Instance.PlayerSc.NoControl = talk;
        GameManager.Instance.PlayerSc.IsInvincible = talk;
        //interBtnsPanel.SetActive(!talk);
    }

    public void StartTalk(NPCInfo info)
    {
        index = 0;
        seq = DOTween.Sequence();
        talkText.text = "";
        SetPlayerState(true);
        interBtnsPanel.SetActive(false);

        {
            CvsgFade(mainCvsg, 0, 0.6f);
            CvsgFade(infoCvsg, 0, 0.5f);
            CancelInvoke("OffTalkPanel");
            talkPanelAni.gameObject.SetActive(true);
        }

        nameText.text = info.name;
        isTalking = true;
        {
            string s = info.talkList[info.talkId].stringList[0];
            seq.Append(talkText.DOText(s, s.Length * talkTextTime));
            seq.AppendCallback(() => { isTalking = false; talkEndMark.SetActive(true); }).Play();
        }
        screenTouchPanel.gameObject.SetActive(true);
        talkCount = info.talkList[info.talkId].stringList.Count;

        {
            currentNpc = info;
            screenTouchPanel.onClick.RemoveAllListeners();
            screenTouchPanel.onClick.AddListener(DoTalkWithNPC);
        }
    }

    public void DoTalkWithNPC()
    {
        if (currentNpc == null) return;

        if(isTalking)
        {
            isTalking = false;
            seq.Kill();
            talkText.DOKill();
            talkText.text = currentNpc.talkList[currentNpc.talkId].stringList[index];
            talkEndMark.SetActive(true);
        }
        else
        {
            index++;
            talkEndMark.SetActive(false);

            if (index == talkCount)
            {
                currentNpc = null;
                talkPanelAni.SetTrigger(closeHash);
                screenTouchPanel.gameObject.SetActive(false);
                CvsgFade(mainCvsg, 1, 0.6f);
                CvsgFade(infoCvsg, 1, 0.5f);
                Invoke("OffTalkPanel", 0.6f);
                SetPlayerState(false);
                talkEndMark.SetActive(false);

                return;
            }

            seq = DOTween.Sequence();
            talkText.text = "";
            string s = currentNpc.talkList[currentNpc.talkId].stringList[index];
            isTalking = true;
            seq.Append(talkText.DOText(s,s.Length*talkTextTime)).AppendCallback(()=> { isTalking = false; talkEndMark.SetActive(true); }).Play();
        }
    }

    private void OffTalkPanel()
    {
        talkPanelAni.gameObject.SetActive(false);
        interBtnsPanel.SetActive(true);
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        Inventory[] managers = FindObjectsOfType<Inventory>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();


        isReady = true;
    }
}
