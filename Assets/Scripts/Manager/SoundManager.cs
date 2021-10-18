using System.Collections.Generic;
using UnityEngine;

public enum SoundEffectType
{
    WALK,
    JUMP,
    ATTACK,
    FADEOUT,  //FADEINÀº º¸·ù
    MENUCLICK,
    INTERACTION,
    SYSTEMINFOMSG
}

public class SoundManager : MonoSingleton<SoundManager>, ISceneDataLoad
{
    public List<AudioClip> gameSoundEffectList;

    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public void PlaySoundEffect(SoundEffectType set, float time = -1f)
    {
        Option op = GameManager.Instance.savedData.option;

        if (op.masterSoundSize <= 0 || op.soundEffectSize <= 0) return;

        PoolManager.GetItem<SoundPrefab>().SoundPlay(gameSoundEffectList[(int)set], time);
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        SoundManager[] managers = FindObjectsOfType<SoundManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        isReady = true;
    }

    
}
