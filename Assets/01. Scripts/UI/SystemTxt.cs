using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System;

public class SystemTxt : MonoBehaviour
{
    public Ease[] ease;
    
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    public void OnText(string msg,float time=3f ,int fontSize=50, Action EndAction=null)  //화면 상단의 시스템 메시지
    {
        text.text = msg;
        text.fontSize = fontSize;

        Sequence seq = DOTween.Sequence();
        seq.Append( transform.DOScale(Vector3.one, 0.35f).SetEase(ease[0]) );
        seq.AppendInterval(time);
        seq.Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(ease[1]));
        seq.AppendCallback(() => { EndAction?.Invoke(); gameObject.SetActive(false); });
        seq.Play().SetUpdate(true);

        SoundManager.Instance.PlaySoundEffect(SoundEffectType.SYSTEMINFOMSG);
    }
}
