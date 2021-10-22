
using UnityEngine;

public class SoundPrefab : MonoBehaviour
{
    [SerializeField] private AudioSource _audio;
    private bool soundStart;

    public void SoundPlay(AudioClip _clip, float time=-1f)
    {
        _audio.clip = _clip;
        _audio.volume = GameManager.Instance.savedData.option.soundEffectSize;
        _audio.Play();
        soundStart = true;
        if (time > 0)
        {
            Invoke("Inactive", time);
        }
    }

    private void OnEnable() => soundStart = false;

    private void Update()
    {
        if(!_audio.isPlaying && soundStart)
        {
            gameObject.SetActive(false);
        }
    }

    private void Inactive()
    {
        _audio.Stop();
        _audio.clip = null;
        gameObject.SetActive(false);
    }
}
