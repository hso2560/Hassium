using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayAndNight : MonoBehaviour
{
    [SerializeField] private float secondPerRealTimeSecond;  //���� 1�� == �ش� ���� (��)  (���� �� �Ϸ�(24�ð�) = 360�� ȸ��)
    private bool isNight = false;
    private float speed;

    [SerializeField] private float fogDensityCalc;

    [SerializeField] private float nightFogDensity;
    private float dayFogDensity;
    private float currentFogDensity;

    private void Start()
    {
        dayFogDensity = RenderSettings.fogDensity;
        speed = 240/secondPerRealTimeSecond;  //1�� ȸ���ϴ� �ð� (86400/secondPerRealTimeSecond/360) (86400��=24�ð�)
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
