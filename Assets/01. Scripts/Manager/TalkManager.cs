using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

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
    [Range(0.05f,1f)]
    public float talkTextTime = 0.1f;  //한 글자당 나오는 시간
    private NPCInfo currentNpc;
    private bool isTalking; //텍스트가 다 나왔는지
    Sequence seq;
    private int closeHash;

    private Dictionary<string, Action> talkEndAction = new Dictionary<string, Action>();
    private string key = "";

    private void Awake()
    {
        closeHash = Animator.StringToHash("close");

        talkEndAction.Add("22,0", () =>
        {
            GameManager.Instance.AddCharacter("DefaultPlayer2");
            GameManager.Instance.SaveObjActiveInfo(28, false);
            GameManager.Instance.infoSaveObjs.objs[28].SetActive(false);
        });
        talkEndAction.Add("23,0", () =>
        {
            GameManager.Instance.AddCharacter("DefaultPlayer3");
            GameManager.Instance.SaveObjActiveInfo(44, false);
            GameManager.Instance.infoSaveObjs.objs[44].SetActive(false);
        });
    }

    public void CvsgFade(CanvasGroup cg, int target, float time)  //UI처리
    {
        bool b = (target != 0);
        cg.DOFade(target, time);
        cg.blocksRaycasts = b;
        cg.interactable = b;
    }

    private void SetPlayerState(bool talk)  //플레이어 움직임 방지
    {
        GameManager.Instance.PlayerSc.NoControl = talk;
        GameManager.Instance.PlayerSc.IsInvincible = talk;
        //interBtnsPanel.SetActive(!talk);
    }

    public void StartTalk(NPCInfo info)  //대화 시작
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

    public void DoTalkWithNPC()  //대화 함
    {
        if (currentNpc == null) return;

        if(isTalking)  //텍스트 효과 안끝났으면 강제로 끝내준다
        {
            isTalking = false;
            seq.Kill();
            talkText.DOKill();
            talkText.text = currentNpc.talkList[currentNpc.talkId].stringList[index];
            talkEndMark.SetActive(true);
        }
        else  // 다음 대화 ㄱ
        {
            index++;
            talkEndMark.SetActive(false);

            if (index == talkCount)  //마지막 말까지 들음
            {
                if (currentNpc.talkList[currentNpc.talkId].endAction)
                {
                    key = string.Concat(currentNpc.id, ",", currentNpc.talkId);
                }
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

    private void OffTalkPanel() //대화 종료
    {
        talkPanelAni.gameObject.SetActive(false);
        interBtnsPanel.SetActive(true);

        if(key!="")  
        {
            talkEndAction[key]();  //해당 대화가 끝난 후에 무슨 일을 처리할게 있으면 해준다
            key = "";
        }
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        Inventory[] managers = FindObjectsOfType<Inventory>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();


        isReady = true;
    }
}
