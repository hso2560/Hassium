using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameUI : MonoBehaviour
{
    public short id;
    public bool timeStop;  //UI가 켜지면 시간을 멈출지 (일시정지)
    public bool bSameBtnOff = false;  //같은 버튼으로 해당 UI를 끌 수 있는가?  ==> false면 같은 버튼 눌러도 해당 UI꺼지지않고 아무 변화 없음
    public bool noStackUI = false;  //UIManager의 UI List에 쌓이지 않음. --> ESC로 끌 수 없는 UI --> 이런건 대부분 ESC를 누르면 그 UI를 담고 있는 상위 UI가 꺼질 것

    public Image img;
    public CanvasGroup cvsGroup;

    [SerializeField] private Vector3 targetVec = new Vector3(1,1,1);
    [SerializeField] private float time1 = 0.5f, time2 = 0.3f;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private int idx = -1;  //스태미나 같은 UI는 그저 보이기만 하는 것이기때문에 -2로 해놓고 따로 UIManager까지 접근할 필요는 없다

    [SerializeField] private bool startAutoInactive = false;  //시작하면 자동으로 오브젝트 꺼줄까?  
    [SerializeField] private bool useUIQueue = true;  //UIQueue를 사용할까  (스테미나 UI같은거는 사용X)
    public PRS prs;
    Sequence seq;

    private void Awake()
    {
        prs = new PRS(transform.position, transform.rotation, transform.localScale);
        if (startAutoInactive) gameObject.SetActive(false);
    }

    private void OnEnable() //활성화 되면 애니 실행
    {
        UIQueue(true);

        if (idx == -1)
            idx = UIManager.Instance.sceneObjs.ui.IndexOf(gameObject);

        switch (id)
        {
            case 0:
                UIAnimation(targetVec, () => { Time.timeScale = 0; }, 1, time2, time1);
                break;
            case 10:
                if (UIManager.Instance.curMenuPanel != gameObject)
                {
                    GameObject o = UIManager.Instance.curMenuPanel;
                    o.GetComponent<GameUI>().img.gameObject.SetActive(false);
                    o.SetActive(false);

                    img.gameObject.SetActive(true);
                    UIManager.Instance.curMenuPanel = gameObject;

                    if (idx == 3)  //캐릭터 창
                    {
                        Inventory.Instance.ClickCharacterPanel(true);
                    }
                    else
                    {
                        Inventory.Instance.ClickCharacterPanel(false);
                    }
                }
                UIQueue();
                break;
            case 20:  //스태미나 바
                UIAnimation(targetVec, () => { }, time1);
                break;
            case 30:
                UIAnimation(targetVec, () => { }, time1);
                break;

            default:
                break;
        }
    }

    private void OnDisable()
    {
        UIManager.Instance.stackUI.Remove(gameObject);
    }

    public void ResetData(int num=-3)
    {
        if (!bSameBtnOff && idx == num)
        {
            UIQueue();
            return;
        }

        switch (id)
        {
            case 0:
                Time.timeScale = 1;
                UIAnimation(Vector3.zero, () => { gameObject.SetActive(false); }, 0, time2, time1);
                break;
            case 20:
                UIAnimation(prs.rotation.eulerAngles, () => { gameObject.SetActive(false); }, time1);
                break;
            case 30:
                UIAnimation(prs.scale, () => { gameObject.SetActive(false); }, time1);
                break;
        }
    }

    private void UIAnimation(Vector3 vValue, TweenCallback tc ,params float[] fValues)
    {
        seq = DOTween.Sequence();

        tc += () => UIQueue();

        switch (id)
        {
            case 0:
                cvsGroup.DOFade(fValues[0], fValues[1]);
                seq.Append(transform.DOScale(vValue, fValues[2]).SetEase(ease));
                seq.AppendCallback(tc);
                seq.Play();
                break;
            case 20:
                seq.Append ( transform.DORotate(vValue, fValues[0]) )
                    .AppendCallback(tc).Play();
                break;
            case 30:
                seq.Append(transform.DOScale(vValue, fValues[0]))
                    .AppendCallback(tc).SetUpdate(true).Play();  //.Play() 안 붙여도 실행은 되더라 (일단 명시적으로 붙여야겠다)  메뉴 안에서는 Time.timeScale이 0이라서 실행안되는거 있으므로 그거 고쳐 
                break;
        }
    }

    private void UIQueue(bool enqueue=false) 
    {
        if(useUIQueue)
           UIManager.Instance.UIQueue(enqueue);
    }
}
