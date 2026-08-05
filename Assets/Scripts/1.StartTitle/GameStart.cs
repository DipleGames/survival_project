using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    [SerializeField] GameObject damageUI;
    [SerializeField] GameObject optionPanel;
    [SerializeField] GameObject gameInfoPanel;
    [SerializeField] GameObject etcInfoPanel;
    SoundManager soundManager;

    private void Start()
    {
        optionPanel.SetActive(false);
        gameInfoPanel.SetActive(false);
        etcInfoPanel.SetActive(false);

        soundManager = SoundManager.Instance;
        soundManager.PlayBGM(0, true);

        Cursor.lockState = CursorLockMode.None;
    }

    public void ClickStart(string scene)
    {
        //soundManager.PlaySFX("StartButton");
        //GameManager.Instance.ToNextScene("Loading");
        SceneManager.LoadScene("Loading");
    }

    public void ClickOption()
    {
        //soundManager.PlaySFX("SelectButton");
        optionPanel.SetActive(true);
    }

    public void ClickExit()
    {
        //soundManager.PlaySFX("SelectButton");
        Application.Quit();
    }

    public void OnSelectSound()
    {
        //soundManager.PlaySFX("SelectButton");
    }
}
