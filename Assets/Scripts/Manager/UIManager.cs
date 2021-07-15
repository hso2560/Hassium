using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoSingleton<UIManager>, ISceneDataLoad
{
    public List<Ease> gameEases;
    public Image LoadingImg;

    private Color noColor;

    private void Awake()
    {
        noColor = new Color(0, 0, 0, 0);
    }

    public void LoadingFade(bool isFadeIn, int index=0)  //페이드인 페이드아웃
    {
        Sequence seq = DOTween.Sequence();
        LoadingImg.gameObject.SetActive(true);

        seq.Append(LoadingImg.DOFade(isFadeIn ? 0 : 1, 0.6f).SetEase(gameEases[index]));
        seq.AppendCallback(() =>
        {
            if(isFadeIn)
               LoadingImg.gameObject.SetActive(false);
            else
            {
                GameManager.Instance.Loading();
            }
        });
        seq.Play();
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        UIManager[] managers = FindObjectsOfType<UIManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        if(this.sceneObjs.ScType==SceneType.MAIN)
        {
            LoadingImg = this.sceneObjs.gameImgs[0];
            gameEases = new List<Ease>(this.sceneObjs.gameEases);
        }
        LoadingFade(true, 0);
    }
}
