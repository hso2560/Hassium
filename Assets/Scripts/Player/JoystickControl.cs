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
    private CanvasGroup jumpBtnCvsGroup;

    public PlayerScript player;

    private float radius;
    private Vector2 dragVec;
    [HideInInspector] public Vector2 dirVec;

    [HideInInspector] public bool isTouch = false;
    [HideInInspector] public bool isRun;

    private void Awake()
    {
        radius = rectBackground.rect.width * 0.5f;
        jumpBtn.onClick.AddListener(Jump);

        bgImg = GetComponent<Image>();
        joystickImg = transform.GetChild(0).GetComponent<Image>();
        bgBefore = bgImg.color;
        joystickBefore = joystickImg.color;

        jumpBtnCvsGroup = jumpBtn.GetComponent<CanvasGroup>();
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
}
