using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public enum EffectType
{
    APPEARANCE
}

public class EffectManager : MonoSingleton<EffectManager>
{
    //Hit����
    public GameObject hitEffect;
    private List<GameObject> hitEffectList;

    public GameObject appearanceEffect;
    private List<GameObject> appearanceEffectList;

    public GameObject lightningEffectPref;
    private List<GameObject> lightningEffectList;

    public PostProcessVolume postProcVolume;
    private ChromaticAberration chro;
    private Grain grain;
    private ColorGrading colorGra;
    private Bloom bloom;
    private Dictionary<PSkillType, bool> skillOnDict = new Dictionary<PSkillType, bool>();

    private bool reinfChromIncr;

    private void Awake()
    {
        CreatePool();
        postProcVolume.profile.TryGetSettings(out chro);
        postProcVolume.profile.TryGetSettings(out grain);
        postProcVolume.profile.TryGetSettings(out colorGra);
        postProcVolume.profile.TryGetSettings(out bloom);
        skillOnDict.Add(PSkillType.REINFORCE, false);
        skillOnDict.Add(PSkillType.TIME, false);
        skillOnDict.Add(PSkillType.ROPE, false);
    }

    private void CreatePool() //����Ʈ Ǯ ����
    {
        hitEffectList = FunctionGroup.CreatePoolList(hitEffect, transform, 5);
        appearanceEffectList = FunctionGroup.CreatePoolList(appearanceEffect, transform, 5);
        lightningEffectList = FunctionGroup.CreatePoolList(lightningEffectPref, transform, 7);

      
    }

    private void Update()
    {
        SkillEffect();
    }

    private void SkillEffect()  //�������� ������ ��ų ��� ����Ʈ
    {
        if(skillOnDict[PSkillType.REINFORCE])
        {
            chro.intensity.value += reinfChromIncr ? Time.deltaTime : -Time.deltaTime;
            if(chro.intensity.value<0.7f || chro.intensity.value>=1)
            {
                reinfChromIncr = !reinfChromIncr;
            }
        }
        else if(skillOnDict[PSkillType.TIME])
        {
            if(colorGra.temperature<40)
            {
                colorGra.temperature.value += Time.deltaTime * 2.5f;
            }
        }
    }

    public void OnHitEffect(Vector3 pos, Vector3 normal)  //�ĸ��� ���� ����Ʈ
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

    public void OnEffect(EffectType type, Vector3 pos, float time=.4f)  //�ٸ� ����Ʈ�� Ȱ��ȭ����
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

    public void OnLightningEffect(Vector3 pos, Vector3 start, Vector3 end, float time=0.35f)  //���� ����Ʈ (���� ���)
    {
        GameObject le = FunctionGroup.GetPoolItem(lightningEffectList);
        le.transform.position = pos;

        le.transform.GetChild(0).position = start;
        le.transform.GetChild(1).position = end;

        StartCoroutine(InactiveEffectCo(le, time));
    }

    IEnumerator InactiveEffectCo(GameObject effect, float time)  //�� ���Ŀ� �ش� ����Ʈ ���ش�
    {
        yield return new WaitForSeconds(time);
        effect.SetActive(false);
    }

    public void SkillEffectVolume(PSkillType pst, bool on)  //��ų ���� ��� ����Ʈ
    {
        skillOnDict[pst] = on;
        switch(pst)
        {
            case PSkillType.REINFORCE:
                chro.intensity.value = on ? 0.7f : 0;
                reinfChromIncr = on;
                break;
            case PSkillType.TIME:
                colorGra.temperature.value = 0;
                grain.enabled.value = on;
                break;
            case PSkillType.ROPE:
                chro.intensity.value = on ? 1 : 0;
                bloom.threshold.value = on ? 1f : 1.1f;
                break;
        }
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
