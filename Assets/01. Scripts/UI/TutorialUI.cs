using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialUI : MonoBehaviour
{
    private List<string> tutorialStrs;
    private List<TutoArrowUI> tutoArrowStateList;
    private int index;
    private RectTransform arrowBackRectTr;

    public Text tutoText;
    public Text page;
    public Text title;

    public Button prevBtn, nextBtn, confirm;

    public Animator arrowAni;

    private void Awake()
    {
        arrowBackRectTr = arrowAni.transform.parent.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        title.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 3).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    private void Start()
    {
        prevBtn.onClick.AddListener(() => SoundManager.Instance.PlaySoundEffect(SoundEffectType.MENUCLICK));
        nextBtn.onClick.AddListener(() => SoundManager.Instance.PlaySoundEffect(SoundEffectType.MENUCLICK));
        confirm.onClick.AddListener(() => SoundManager.Instance.PlaySoundEffect(SoundEffectType.MENUCLICK));
    }

    public void SetData(List<string> tutorial, List<TutoArrowUI> arrowStates, bool loading=false)
    {
        if (!loading)
        {
            GameManager.Instance.PlayerSc.isMovable = false;
        }

        tutorialStrs = tutorial;
        tutoArrowStateList = arrowStates;
        index = 0;

        tutoText.text = tutorialStrs[0];
        page.text = $"{index+1}/{tutorialStrs.Count}";
        CheckNextButton();
        CheckArrowImage();
    }

    public void OnClickNext(bool next) //false면 previous
    {
        index += (next ? 1 : -1);
        tutoText.text = tutorialStrs[index];
        page.text = $"{index + 1}/{tutorialStrs.Count}";
        CheckNextButton();
        CheckArrowImage();
    }

    public void OnClickClose() //닫기
    {
        GameManager.Instance.PlayerSc.isMovable = true;
        gameObject.SetActive(false);
    }

    private void CheckNextButton() //다음 버튼
    {
        prevBtn.gameObject.SetActive(index != 0);
        nextBtn.gameObject.SetActive(index != tutorialStrs.Count-1);

        confirm.gameObject.SetActive(index == tutorialStrs.Count - 1);
    }

    private void CheckArrowImage() //튜토 창 화살표
    {
        for(int i = 0; i<tutoArrowStateList.Count; i++)
        {
            if(index==tutoArrowStateList[i].index)
            {
                arrowAni.gameObject.SetActive(true);
                arrowAni.SetTrigger("trg");
                arrowAni.SetBool("vertical", Mathf.Abs(tutoArrowStateList[i].rotationZ) != 90f);
                arrowAni.transform.rotation = Quaternion.Euler(0, 0, tutoArrowStateList[i].rotationZ);
                arrowBackRectTr.anchoredPosition = tutoArrowStateList[i].rectArrowPos;

                return;
            }
        }
        arrowAni.gameObject.SetActive(false);
    }
}
