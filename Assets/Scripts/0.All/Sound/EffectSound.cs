using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class EffectSound : MonoBehaviour
{
    [SerializeField] public AudioSource source;

    private IObjectPool<EffectSound> managedPool;

    public void PlaySFX(AudioClip clip)
    {
        source.clip = clip;
        source.loop = false;
        source.Play();

        StartCoroutine(CheckPlay());
    }

    public void PlaySFX(AudioClip clip, bool isLoop)
    {
        source.clip = clip;
        source.loop = isLoop;
        source.Play();

        StartCoroutine(CheckPlay());
    }

    IEnumerator CheckPlay()
    {
        while (source.isPlaying)        // 만약 플레잉 중이라면
        {
            yield return null;          // 한프레임 대기

            if (GameManager.Instance.isPause)
                source.Pause();

            yield return CoroutineCaching.WaitWhile(() => GameManager.Instance.isPause);

            source.UnPause();
        }

        DestroyPool();
    }

    public void SetManagedPool(IObjectPool<EffectSound> pool)
    {
        managedPool = pool;
    }

    public void DestroyPool()
    {
        if (gameObject.activeSelf)
        {
            managedPool.Release(this);
        }
    }
}
