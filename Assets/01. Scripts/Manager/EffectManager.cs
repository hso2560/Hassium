using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    APPEARANCE
}

public class EffectManager : MonoSingleton<EffectManager>
{
    //Hit관련
    public GameObject hitEffect;
    private List<GameObject> hitEffectList;

    public GameObject appearanceEffect;
    private List<GameObject> appearanceEffectList;

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        hitEffectList = FunctionGroup.CreatePoolList(hitEffect, transform, 5);
        appearanceEffectList = FunctionGroup.CreatePoolList(appearanceEffect, transform, 5);
    }

    public void OnHitEffect(Vector3 pos, Vector3 normal)  //쳐맞을 때의 이펙트
    {
        GameObject e = FunctionGroup.GetPoolItem(hitEffectList);
        e.transform.position = pos;
        if(normal!=Vector3.zero)
        {
            e.transform.rotation = Quaternion.LookRotation(normal);
        }
        StartCoroutine(InactiveEffectCo(e, 0.4f)); 
    }

    /*public void OnPlayerSkillEffect(PSkillType type,Vector3 pos, float time)
    {
        GameObject s = pSkillEffects[(int)type];
        s.gameObject.transform.position = pos;
        s.SetActive(true);

        StartCoroutine(InactiveEffectCo(s, time));
    }*/

    public void OnEffect(EffectType type, Vector3 pos, float time=.4f)  //다른 이펙트들 활성화해줌
    {
        GameObject ef = null;
        switch (type)
        {
            case EffectType.APPEARANCE:
                ef = FunctionGroup.GetPoolItem(appearanceEffectList);
                break;
        }
        ef.transform.position = pos;
        StartCoroutine(InactiveEffectCo(ef, time));
    }

    IEnumerator InactiveEffectCo(GameObject effect, float time)  //몇 초후에 해당 이펙트 꺼준다
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
