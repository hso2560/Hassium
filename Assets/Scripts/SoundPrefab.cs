
using UnityEngine;

public class SoundPrefab : MonoBehaviour
{
    [SerializeField] private AudioSource _audio;
    private bool soundStart;

    public void SoundPlay(AudioClip _clip)
    {
        _audio.clip = _clip;
        _audio.volume = GameManager.Instance.savedData.option.soundEffectSize;
        _audio.Play();
        soundStart = true;
    }

    private void OnEnable() => soundStart = false;

    private void Update()
    {
        if(!_audio.isPlaying && soundStart)
        {
            gameObject.SetActive(false);
        }
    }
}
