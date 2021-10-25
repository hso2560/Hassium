using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class DayAndNight : MonoBehaviour
{
    Light mainLight;
    private Color defaultLightColor;
    public List<Color> lightColors;
    //private bool doLightColorEffect = false;

    [SerializeField] private float secondPerRealTimeSecond;  //현실 1초 == 해당 변수 (초)  (게임 속 하루(24시간) = 360도 회전)
    [HideInInspector] public bool isNight = false;
    private float speed;

    [SerializeField] private float fogDensityCalc;

    [SerializeField] private float nightFogDensity;
    private float dayFogDensity;
    private float currentFogDensity;

    public Material[] skyMaterials;

    private void Awake()
    {
        mainLight = GetComponent<Light>();
        defaultLightColor = mainLight.color;
    }

    private void Start()
    {
        dayFogDensity = RenderSettings.fogDensity;
        speed = 1/240f*secondPerRealTimeSecond;  //1도 회전하는 시간 (86400/secondPerRealTimeSecond/360) (86400초=24시간) (360도=하루) (1도에 240초) (1초에 1/240도)
    }

    private void Update()
    {
        RotateSun();
        Fog();
    }

    private void RotateSun()
    {
        transform.Rotate(Vector3.right, speed * Time.deltaTime);

        if (transform.eulerAngles.x >= 170)
        {
            isNight = true;
        }
        else if (transform.eulerAngles.x <= 10)
        {
            isNight = false;
        }

        //transform.RotateAround(GameManager.Instance.PlayerSc.transform.position, Vector3.right, transform.eulerAngles.x);
    }

    private void Fog()
    {
        if (isNight)
        {
            if (currentFogDensity < nightFogDensity)
            {
                currentFogDensity += 0.1f * fogDensityCalc * Time.deltaTime;
                RenderSettings.fogDensity = currentFogDensity;
            }
        }
        else
        {
            if (currentFogDensity >= dayFogDensity)
            {
                currentFogDensity -= 0.1f * fogDensityCalc * Time.deltaTime;
                RenderSettings.fogDensity = currentFogDensity;
            }
        }
    }

    public void OnOffLightEffect(bool on, float time = 1f)
    {
        //doLightColorEffect = on;
        if (on)
        {
            int index = -1;

            TweenCallback tc2 = null;

            TweenCallback tc = () => mainLight.DOColor(lightColors[++index % lightColors.Count], time).OnComplete(tc2);
            tc2 = () => mainLight.DOColor(lightColors[++index % lightColors.Count], time).OnComplete(tc);

            tc();
        }
        else
        {
            mainLight.DOKill();
            mainLight.DOColor(defaultLightColor, 1.5f);
        }
    }
}
