using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer Singleton { get; private set; }

    public AudioSource AudioSourceInstance;
    public AudioClip CardSound;

    private void Awake()
    {
        Singleton = this;
    }

    public void PlaySound(AudioClip sound, float pitchBase = 1f)
    {
        StartCoroutine(PlayThenDisableSound(sound, pitchBase));
    }

    IEnumerator PlayThenDisableSound(AudioClip sound, float pitchBase)
    {
        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
        newAudioSource.pitch = pitchBase;
        newAudioSource.PlayOneShot(sound);

        while (newAudioSource.isPlaying)
        {
            yield return new WaitForEndOfFrame();
        }

        Destroy(newAudioSource);
    }

    public static void PlayCardSound()
    {
        Singleton.PlaySound(Singleton.CardSound);
    }
}
