using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayAndNight : MonoBehaviour
{
    [SerializeField] private float secondPerRealTimeSecond;  //현실 1초 == 해당 변수 (초)  (게임 속 하루(24시간) = 360도 회전)
    private bool isNight = false;
    private float speed;

    [SerializeField] private float fogDensityCalc;

    [SerializeField] private float nightFogDensity;
    private float dayFogDensity;
    private float currentFogDensity;

    private void Start()
    {
        dayFogDensity = RenderSettings.fogDensity;
        speed = 240/secondPerRealTimeSecond;  //1도 회전하는 시간 (86400/secondPerRealTimeSecond/360) (86400초=24시간)
    }

    private void Update()
    {
        RotateSun();
        Fog();
    }

    private void RotateSun()
    {
        transform.Rotate(Vector3.right, 0.1f * speed * Time.deltaTime);

        if (transform.eulerAngles.x >= 170)
        {
            isNight = true;
        }
        else if (transform.eulerAngles.x <= 10)
        {
            isNight = false;
        }
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
}
