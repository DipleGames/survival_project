using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject skipButton;

    AsyncOperation async;

    int canSkip;

    void Start()
    {
        SoundManager.Instance.StopBGM();
        canSkip = PlayerPrefs.GetInt("LoadScene", 0);
        skipButton.SetActive(false);
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        async = SceneManager.LoadSceneAsync("Game");

        async.allowSceneActivation = false;

        while(!async.isDone)
        {
            if (async.progress >= 0.9f)
            {
                if (canSkip != 0 && !skipButton.activeSelf)
                    skipButton.SetActive(true);

                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
                {
                    PlayerPrefs.SetInt("LoadScene", 1);
                    async.allowSceneActivation = true;
                    GameManager.Instance.ToNextScene("Game");
                }
            }

            yield return null;
        }
    }

    public void SkipScene()
    {
        StopAllCoroutines();
        
        async.allowSceneActivation = true;
        GameManager.Instance.ToNextScene("Game");
    }
}
