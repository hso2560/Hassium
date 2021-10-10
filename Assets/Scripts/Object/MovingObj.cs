using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingObj : MonoBehaviour
{
    [SerializeField] short id;
    [SerializeField] Ease ease;

    public float rotateSpeed;

    private void Update()
    {
        switch (id)
        {
            case 10:
                transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
                break;
        }
    }
}
