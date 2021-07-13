using UnityEngine;

public enum SoundEffectType
{
    FADEIN=0,
    FADEOUT=1
}

public class SoundManager : MonoSingleton<SoundManager>
{
    public AudioClip[] gameSoundEffects;  //SoundEffectType의 int값에 맞춰서 배열에 넣어준다.
    private GameManager manager;

    private void Start()
    {
        manager = GameManager.Instance;
    }

    public void PlaySoundEffect(SoundEffectType set)
    {
        Option op = manager.savedData.option;

        if (op.masterSoundSize <= 0 || op.soundEffectSize <= 0) return;

        SoundPrefab sp = PoolManager.GetItem<SoundPrefab>();
        sp.SoundPlay(gameSoundEffects[(int)set]);
    }
}
