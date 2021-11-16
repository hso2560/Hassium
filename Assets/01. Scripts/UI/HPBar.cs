using UnityEngine.UI;
using UnityEngine;

public class HPBar : MonoBehaviour //적 전용
{
    public Image hpFill;

    public void SetHPFill(int current, int max)  //적 HP UI 채워줌
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