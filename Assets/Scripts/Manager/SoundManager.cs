using UnityEngine;

public enum SoundEffectType
{
    FADEIN=0,
    FADEOUT=1
}

public class SoundManager : MonoSingleton<SoundManager>
{
    public AudioClip[] gameSoundEffects;  //SoundEffectType�� int���� ���缭 �迭�� �־��ش�.
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
