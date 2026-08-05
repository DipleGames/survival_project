using System;
using UnityEngine;
using UnityEngine.Pool;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] GameObject soundPrefab;
    [SerializeField] Transform soundStorage;
    [SerializeField] AudioClip[] effects;
    [SerializeField] AudioClip[] bgms;
    [SerializeField] int poolCount = 10;

    [HideInInspector] AudioSource audioSource;
    float AllSoundVolume;
    float bgmSoundVolume;
    float sfxSoundVolume;


    bool muteBgm;
    bool muteSfx;

    public bool isPlaying;

    private IObjectPool<EffectSound> objectPool;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        objectPool = new ObjectPool<EffectSound>(CreatePool, OnGetPool, OnReleasePool, OnDestroyPool, maxSize: poolCount);
    } 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        AllSoundVolume = PlayerPrefs.GetFloat("Sound_All", 0.5f);
        bgmSoundVolume = PlayerPrefs.GetFloat("Sound_Bgm", 0.5f);
        sfxSoundVolume = PlayerPrefs.GetFloat("Sound_Sfx", 0.5f);
        PlayerPrefs.SetFloat("Sound_All", AllSoundVolume);
        PlayerPrefs.SetFloat("Sound_Bgm", bgmSoundVolume);
        PlayerPrefs.SetFloat("Sound_Sfx", sfxSoundVolume);
        muteBgm = Convert.ToBoolean(PlayerPrefs.GetInt("Mute_Bgm", 0));
        muteSfx = Convert.ToBoolean(PlayerPrefs.GetInt("Mute_Sfx", 0));
    }

    private void Update()
    {
        audioSource.volume = AllSoundVolume * bgmSoundVolume;
        audioSource.mute = muteBgm;
        isPlaying = audioSource.isPlaying;
    }

    public void PlayBGM(int num, bool isLoop)
    {
        audioSource.clip = bgms[num];

        audioSource.loop = isLoop;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void WholeVolume(float num, bool isBgmMute, bool isSfxMute)
    {
        AllSoundVolume = num;
        muteBgm = isBgmMute;
        muteSfx = isSfxMute;
        PlayerPrefs.SetFloat("Sound_All", AllSoundVolume);
    }

    public void WholeVolumeOnOff(bool isMute)
    {
        muteBgm = isMute;
        muteSfx = isMute;
    }

    public void BgmVolume(float num, bool isMute)
    {
        bgmSoundVolume = num;
        muteBgm = isMute;

        PlayerPrefs.SetFloat("Sound_Bgm", bgmSoundVolume);
    }

    public void BgmOnOff(bool isMute)
    {
        muteBgm = isMute;
    }

    public void SfxVolume(float num, bool isMute)
    {
        sfxSoundVolume = num ;
        muteSfx = isMute;

        PlayerPrefs.SetFloat("Sound_Sfx", sfxSoundVolume);
    }

    public void SfxOnOff(bool isMute)
    {
        muteSfx = isMute;
    }

    public void StopBGM()
    {
        audioSource.Stop();
    }

    public void PauseBGM()
    {
        audioSource.Pause();
    }

    public void PlaySFX(string name)
    {
        if (!muteSfx)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].name == name)
                {
                    EffectSound effect = objectPool.Get();
                    effect.PlaySFX(effects[i]);
                    effect.source.volume = sfxSoundVolume * AllSoundVolume;
                    break;
                }
            }
        }
    }

    public void PlaySFX(AudioClip audioClip)
    {
        if (!muteSfx)
        {
            EffectSound effect = objectPool.Get();
            effect.PlaySFX(audioClip);
            effect.source.volume = sfxSoundVolume * AllSoundVolume;
        }
    }

    public void PlaySFX(AudioClip audioClip, bool isLoop)
    {
        if (!muteSfx)
        {
            EffectSound effect = objectPool.Get();
            effect.PlaySFX(audioClip, isLoop);
            effect.source.volume = sfxSoundVolume * AllSoundVolume;
        }
    }

    public EffectSound PlaySFXAndReturn(AudioClip audioClip, bool isLoop)
    {
        if (!muteSfx)
        {
            EffectSound effect = objectPool.Get();
            effect.PlaySFX(audioClip, isLoop);
            effect.source.volume = sfxSoundVolume * AllSoundVolume;

            return effect;
        }

        return null;
    }

    public void StopLoopSFX(EffectSound sfx)
    {
        sfx.DestroyPool();
    }

    private EffectSound CreatePool()
    {
        EffectSound pool = Instantiate(soundPrefab).GetComponent<EffectSound>();
        pool.SetManagedPool(objectPool);
        pool.transform.SetParent(soundStorage);

        return pool;
    }

    private void OnGetPool(EffectSound pool)
    {
        pool.gameObject.SetActive(true);
    }

    private void OnReleasePool(EffectSound pool)
    {
        if (pool.gameObject.activeSelf)
            pool.gameObject.SetActive(false);
    }

    private void OnDestroyPool(EffectSound pool)
    {
        Destroy(pool.gameObject);
    }
}
