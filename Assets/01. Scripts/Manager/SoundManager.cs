using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public enum SoundEffectType
{
    WALK,
    JUMP,
    ATTACK,
    FADEOUT,  //FADEIN은 보류
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

    public void PlaySoundEffect(SoundEffectType set, float time = -1f)  //효과음 출력
    {
        Option op = GameManager.Instance.savedData.option;

        if (op.masterSoundSize <= 0 || op.soundEffectSize <= 0) return;
        
        PoolManager.GetItem<SoundPrefab>().SoundPlay(gameSoundEffectList[(int)set], time,op.soundEffectSize);
    }
    
    public void PlayBGM(BGMSound bgm)  //BGM 출력 or 비활     
    {
        if ((bgm == BGMSound.NULL && _audio.clip == null) || (bgm!=BGMSound.NULL && _audio.clip == gameSoundBGMList[(int)bgm])) return;  //(bgm!=BGMSound.NULL && _audio.clip == gameSoundBGMList[(int)bgm]) 여기에서 &&기준으로 이렇게 순서 맞춰야함. 순서 다르면 오류발생하니 오류발생하지 않는 조건을 먼저 왼쪽에 배치해서 먼저 계산하도록 한다

        isBGM = bgm != BGMSound.NULL;

        if (isBGM) _audio.clip = gameSoundBGMList[(int)bgm];
        if (bgmCo != null) StopCoroutine(bgmCo);
        bgmCo = StartCoroutine(GradualBGMCo());
    }

    private IEnumerator GradualBGMCo()  //배경음과 라이트의 세기를 점진적으로 키우거나 줄인다
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

    public void OnChangeMasterSound(UnityEngine.UI.Slider slider)  //Master 사운드 크기 조절
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
