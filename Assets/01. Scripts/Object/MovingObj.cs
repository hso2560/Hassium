using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingObj : MonoBehaviour
{
    [SerializeField] short id;
    [SerializeField] Ease ease;

    public float rotateSpeed;
    public float[] time;

    public Vector3 target;

    private Vector3 orgPos;

    private void Start()
    {
        orgPos = transform.position;
        Sequence seq = DOTween.Sequence();

        switch (id)
        {
            case 50:
                seq.AppendInterval(time[1]);
                seq.Append(transform.DOMove(target, time[0]).SetEase(ease));
                seq.AppendInterval(time[1]);
                seq.Append(transform.DOMove(orgPos, time[0]).SetEase(ease));
                seq.Play().SetLoops(-1,LoopType.Restart);
                break;

            case 100:
                target = FunctionGroup.GetRandomDir() * Random.Range(3f, 7.7f) + orgPos;
                transform.DOMove(target, Random.Range(4.7f, 8f)).SetEase(ease).SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }

    private void Update()
    {
        switch (id)
        {
            case 10:
                transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
                break;

            case 150:
                transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
                break;
        }
    }
}
