using UnityEngine.UI;
using UnityEngine;

public class HPBar : MonoBehaviour //�� ����
{
    public Image hpFill;

    public void SetHPFill(int current, int max)  //�� HP UI ä����
    {
        hpFill.fillAmount = (float)current / max;
        if (current < 0)
        {
            hpFill.fillAmount = 0;
            Invoke("DisableObj", 0.4f);
        }
    }

    private void DisableObj() => gameObject.SetActive(false);
}