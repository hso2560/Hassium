using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour  //타이틀에서 검은 화면(로딩 화면)에만 뜸
{
    private Image loadingImg;

    private bool wait;

    private float fillSpeed;
    public float fillSpeedCalc = 0.2f;

    private void Awake()
    {
        loadingImg = GetComponent<Image>();
        fillSpeed = Time.deltaTime * fillSpeedCalc;
    }

    private void Update()
    {
        if (!wait)
        {
            loadingImg.fillAmount += fillSpeed;
            if (loadingImg.fillAmount == 1)
            {
                wait = true;
            }
        }
        else
        {
            Color c = loadingImg.color;
            c.a -= Time.deltaTime;
            loadingImg.color = c;

            if(loadingImg.color.a<=0)
            {
                loadingImg.fillAmount = 0;
                loadingImg.color = Color.white;
                wait = false;
            }
        }
    }
}
