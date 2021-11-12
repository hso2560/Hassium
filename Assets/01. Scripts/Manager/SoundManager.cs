using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public enum SoundEffectType
{
    WALK,
    JUMP,
    ATTACK,
    FADEOUT,  //FADEIN�� ����
    MENUCLICK,
    INTERACTION,
    SYSTEMINFOMSG,
    MOVEDOOR
}

public enum BGMSound
{
    RAIN,
    NULL
}

public class SoundManager : MonoSingleton<SoundManager>, ISceneDataLoad
{
    [HideInInspector] public AudioSource _audio;

    public List<AudioClip> gameSoundEffectList;
    public List<AudioClip> gameSoundBGMList;
    public Light _light;

    private Coroutine bgmCo = null;

    public float gradualBgmSpeedRevision = 2.5f;
    private float bgmSpeed;
    private bool isBGM;

    public AudioMixer _audioMixer;
    

    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public void PlaySoundEffect(SoundEffectType set, float time = -1f)  //ȿ���� ���
    {
        Option op = GameManager.Instance.savedData.option;

        if (op.masterSoundSize <= 0 || op.soundEffectSize <= 0) return;
        
        PoolManager.GetItem<SoundPrefab>().SoundPlay(gameSoundEffectList[(int)set], time,op.soundEffectSize);
    }
    
    public void PlayBGM(BGMSound bgm)  //BGM ��� or ��Ȱ     
    {
        if ((bgm == BGMSound.NULL && _audio.clip == null) || (bgm!=BGMSound.NULL && _audio.clip == gameSoundBGMList[(int)bgm])) return;  //(bgm!=BGMSound.NULL && _audio.clip == gameSoundBGMList[(int)bgm]) ���⿡�� &&�������� �̷��� ���� �������. ���� �ٸ��� �����߻��ϴ� �����߻����� �ʴ� ������ ���� ���ʿ� ��ġ�ؼ� ���� ����ϵ��� �Ѵ�

        isBGM = bgm != BGMSound.NULL;

        if (isBGM) _audio.clip = gameSoundBGMList[(int)bgm];
        if (bgmCo != null) StopCoroutine(bgmCo);
        bgmCo = StartCoroutine(GradualBGMCo());
    }

    private IEnumerator GradualBGMCo()  //������� ����Ʈ�� ���⸦ ���������� Ű��ų� ���δ�
    {
        _audio.Play();

        _audioMixer.SetFloat("bgm", isBGM ? -40 : 0);
        float time = 0f;
        float curIntensity = _light.intensity;
        
        for(;time<2.5f;)
        {
            if(isBGM)
            {
                _audioMixer.SetFloat("bgm", -40 - ( -40*time/2.5f) );
                _light.intensity = Mathf.Clamp(curIntensity + ( -0.7f * time / 2.5f),0.6f,1.3f);
            }
            else
            {
                _audioMixer.SetFloat("bgm", -40 * time / 2.5f);
                _light.intensity = Mathf.Clamp(curIntensity + (0.7f * time / 2.5f), 0.6f, 1.3f);
            }
            time += Time.deltaTime;
            yield return null;
        }

        _audioMixer.SetFloat("bgm", isBGM ? 0 : -40);
        _light.intensity = isBGM ? 0.6f : 1.3f;
        bgmCo = null;
        if(!isBGM)
        {
            _audio.clip = null;
            _audio.Stop();
        }
    }

    public void OnChangeMasterSound(UnityEngine.UI.Slider slider)  //Master ���� ũ�� ����
    {
        _audioMixer.SetFloat("master", slider.value);
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        SoundManager[] managers = FindObjectsOfType<SoundManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        _audio = GetComponent<AudioSource>();
        bgmSpeed = Time.deltaTime / gradualBgmSpeedRevision;

        isReady = true;
    }
}
