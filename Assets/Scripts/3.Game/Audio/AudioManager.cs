using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource _audioSource;


    public void PlaySFX(AudioClip sfx)
    {
        _audioSource.PlayOneShot(sfx);
    }
}
