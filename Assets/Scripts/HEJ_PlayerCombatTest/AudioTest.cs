using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    public static AudioTest Instance;
    [SerializeField] private AudioSource _audioSource;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void PlaySFX(AudioClip sfx)
    {
        _audioSource.PlayOneShot(sfx);
    }
}
