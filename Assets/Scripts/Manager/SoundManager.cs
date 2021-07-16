using System.Collections.Generic;
using UnityEngine;

public enum SoundEffectType
{
    FADEIN=0,
    FADEOUT=1
}

public class SoundManager : MonoSingleton<SoundManager>, ISceneDataLoad
{
    public List<AudioClip> gameSoundEffectList; 

    public void PlaySoundEffect(SoundEffectType set)
    {
        Option op = GameManager.Instance.savedData.option;

        if (op.masterSoundSize <= 0 || op.soundEffectSize <= 0) return;

        SoundPrefab sp = PoolManager.GetItem<SoundPrefab>();
        sp.SoundPlay(gameSoundEffectList[(int)set]);
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        SoundManager[] managers = FindObjectsOfType<SoundManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();
    }

    
}
