using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoSingleton<EffectManager>
{
    public GameObject[] pSkillEffects;

    //Hit°ü·Ã
    public GameObject hitEffect;
    private List<GameObject> hitEffectList;

    private void Awake()
    {
        for(int i=0; i<pSkillEffects.Length; i++)
        {
            GameObject o = Instantiate(pSkillEffects[i], transform);
            o.SetActive(false);
            pSkillEffects[i] = o;
        }
        CreatePool();
    }

    private void CreatePool()
    {
        hitEffectList = FunctionGroup.CreatePoolList(hitEffect, transform, 5);
    }

    public void OnHitEffect(Vector3 pos)
    {
        GameObject e = FunctionGroup.GetPoolItem(hitEffectList);
        e.transform.position = pos;
        StartCoroutine(InactiveEffectCo(e, 0.4f));
    }

    public void OnPlayerSkillEffect(PSkillType type,Vector3 pos, float time)
    {
        GameObject s = pSkillEffects[(int)type];
        s.gameObject.transform.position = pos;
        s.SetActive(true);

        StartCoroutine(InactiveEffectCo(s, time));
    }

    IEnumerator InactiveEffectCo(GameObject effect, float time)
    {
        yield return new WaitForSeconds(time);
        effect.SetActive(false);
    }

    /*public bool GetReadyState { get; set; }
    public void ManagerDataLoad(GameObject sceneObjs)
    {
        EffectManager[] managers = FindObjectsOfType<EffectManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();




        isReady = true;
    }*/
}
