using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform rectBackground;
    [SerializeField] private RectTransform rectJoystick;

    public Color bgAfter, joystickAfter;
    private Color bgBefore, joystickBefore;
    private Image bgImg, joystickImg;

    public Button jumpBtn;
    public Button skillBtn;
    private CanvasGroup jumpBtnCvsGroup;

    public PlayerScript player;

    private float radius;
    private Vector2 dragVec;
    [HideInInspector] public Vector2 dirVec;

    [HideInInspector] public bool isTouch = false;
    [HideInInspector] public bool isRun;

    public EventTrigger trigger;

    public EventTrigger.Entry entry1 = new EventTrigger.Entry();
    public EventTrigger.Entry entry2 = new EventTrigger.Entry();

    private void Awake()
    {
        radius = rectBackground.rect.width * 0.5f;
        jumpBtn.onClick.AddListener(Jump);

        bgImg = GetComponent<Image>();
        joystickImg = transform.GetChild(0).GetComponent<Image>();
        bgBefore = bgImg.color;
        joystickBefore = joystickImg.color;

        jumpBtnCvsGroup = jumpBtn.GetComponent<CanvasGroup>();

        /*trigger = skillBtn.GetComponent<EventTrigger>();
        entry1.eventID = EventTriggerType.PointerDown;
        entry1.callback = new EventTrigger.TriggerEvent();
        entry2.eventID = EventTriggerType.PointerUp;
        entry2.callback = new EventTrigger.TriggerEvent();*/
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragVec = eventData.position - (Vector2)rectBackground.position;
        dragVec = Vector2.ClampMagnitude(dragVec, radius);
        rectJoystick.localPosition = dragVec;

        dirVec = dragVec.normalized;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouch = true;

        dragVec = eventData.position - (Vector2)rectBackground.position;
        dragVec = Vector2.ClampMagnitude(dragVec, radius);
        rectJoystick.localPosition = dragVec;

        dirVec = dragVec.normalized;

        bgImg.color = bgAfter;
        joystickImg.color = joystickAfter;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouch = false;
        rectJoystick.localPosition = Vector3.zero;

        bgImg.color = bgBefore;
        joystickImg.color = joystickBefore;
    }

    public void OnPointerDownRunBtn() => isRun = !player.isStamina0;
    public void OnPointerUpRunBtn() => isRun = false;

    private void Update()
    {
        if (player != null)
        {
            CheckJoystickState();
        }
    }

    private void Jump()
    {
        player.Jump();
        //점프 했을 때 각종 처리 
    }

    public void CheckJoystickState()
    {
        bool jumpCheck = player.isJumping;
        jumpBtnCvsGroup.alpha = !jumpCheck ? 1 : 0.3f;
        jumpBtnCvsGroup.blocksRaycasts = !jumpCheck ? true : false;
        jumpBtnCvsGroup.interactable = !jumpCheck ? true : false;

    }

    public void ClearSkillBtn()
    {
        skillBtn.onClick.RemoveAllListeners();
        trigger.triggers.RemoveRange(0, 2);
    }

    public void SkillBtnTriggerAdd()
    {
        trigger.triggers.Add(entry1);
        trigger.triggers.Add(entry2);
    }
}
