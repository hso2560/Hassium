using UnityEngine;
using UnityEngine.UI;

public class InteractionBtn : MonoBehaviour
{
    public ObjData data;
    public Button btn;
    public CanvasGroup cvs;

    private void Start()
    {
        btn.onClick.AddListener(() =>
        {
            if(data!=null && data.gameObject.activeSelf && data.active)
            {
                data.Interaction();
            }
        });
    }
}
