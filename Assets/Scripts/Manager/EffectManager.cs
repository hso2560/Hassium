using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoSingleton<EffectManager>, ISceneDataLoad
{
    public bool GetReadyState { get; set; }



    public void ManagerDataLoad(GameObject sceneObjs)
    {
        EffectManager[] managers = FindObjectsOfType<EffectManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();




        isReady = true;
    }
}
