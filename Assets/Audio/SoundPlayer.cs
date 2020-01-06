using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer Singleton { get; private set; }

    public AudioSource AudioSourceInstance;
    public List<AudioClip> CardSounds;

    private void Awake()
    {
        Singleton = this;
    }

    public IEnumerator PersonalPlayCardSound(AudioClip sound)
    {
        AudioSourceInstance.PlayOneShot(sound);
        yield return new WaitForSeconds(sound.length);
    }

    public static IEnumerator PlayCardSound(PlayingCard fromCard)
    {
        int cardPersonalIndex = Mathf.Abs(Mathf.Abs(fromCard.RepresentingCard.FlavorCode) % Singleton.CardSounds.Count);
        yield return Singleton.PersonalPlayCardSound(Singleton.CardSounds[cardPersonalIndex]);
    }
}
