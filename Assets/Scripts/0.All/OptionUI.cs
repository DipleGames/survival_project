using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionUI : MonoBehaviour
{
    [SerializeField] protected GameObject panel;
    [SerializeField] OptionSettingsUI optionSettings;
    [SerializeField] GameObject CheckHomePanel;
    [SerializeField] GameObject statPanel;
    [SerializeField] protected GameObject backPanel;

    [SerializeField]protected GameManager gameManager;
    SoundManager soundManager;
    GamesceneManager gamesceneManager;

    private void Start()
    {
        //optionPannel
        panel.SetActive(false);
        CheckHomePanel.SetActive(false);
        if(gameManager == null)
            gameManager = GameManager.Instance;
        soundManager = SoundManager.Instance;
        gamesceneManager = GamesceneManager.Instance;

        optionSettings.Initialize(panel);
    }

    private void Update()
    {
        // 키 변경 중 ESC는 공통 컴포넌트가 취소로 처리합니다.
        if (optionSettings.IsWaitingForKey)
            return;

        if (!TutorialManager.Instance.IsTutoProgressing && !gameManager.isClear)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !gameManager.isPause)
                PauseGame();

            else if (Input.GetKeyDown(KeyCode.Escape) && gameManager.isPause)
                ReturnToGame();
        }
    }

    virtual public void PauseGame()
    {
        if (!gameManager.isPause)
        {
            Cursor.lockState = CursorLockMode.None;

            if (gameManager.currentScene == "Game")
            {
                Vector2 cursorHotSpot = new Vector3(gameManager.useCursorNormal.width * 0.5f, gameManager.useCursorNormal.height * 0.5f);
                Cursor.SetCursor(gameManager.useCursorNormal, cursorHotSpot, CursorMode.ForceSoftware);
            }

            if (backPanel != null)
                backPanel.SetActive(true);

            if (statPanel != null)
                statPanel.SetActive(true);

            panel.SetActive(true);
            gameManager.GamePause(true);
        }
    }
    virtual public void ReturnToGame()
    {
        if (gameManager.isPause)
        {
            Cursor.lockState = CursorLockMode.Confined;

            if (gameManager.currentScene == "Game")
            {
                if (!gamesceneManager.isNight)
                {
                    Vector2 cursorHotSpot = new Vector3(gameManager.useCursorNormal.width * 0.5f, gameManager.useCursorNormal.height * 0.5f);
                    Cursor.SetCursor(gameManager.useCursorNormal, cursorHotSpot, CursorMode.ForceSoftware);
                }

                else
                {
                    Vector2 cursorHotSpot = new Vector3(gameManager.useCursorAttack.width * 0.5f, gameManager.useCursorAttack.height * 0.5f);
                    Cursor.SetCursor(gameManager.useCursorAttack, cursorHotSpot, CursorMode.ForceSoftware);
                }
            }

            if (backPanel != null)
                backPanel.SetActive(false);

            if (statPanel != null)
                statPanel.SetActive(false);

            optionSettings.CancelKeyChange();
            panel.SetActive(false);
            Invoke("UnPauseGame", Time.deltaTime);

            Cursor.visible = gameManager.isCursorVisible;
        }
    }

    void UnPauseGame()
    {
        gameManager.GamePause(false);
    }

    public void OpenCheckHomePanel()
    {
        CheckHomePanel.SetActive(true);
    }

    public void CloseCheckHomePanel()
    {
        CheckHomePanel.SetActive(false);
    }

    public void TitleScene()
    {
        optionSettings.SaveMuteSettings();
        optionSettings.CancelKeyChange();
        if (soundManager.gameObject != null)
            Destroy(soundManager.gameObject);

        if (gameManager.gameObject != null)
            Destroy(gameManager.gameObject);

        if (Character.Instance.gameObject != null)
            Destroy(Character.Instance.gameObject);

        SceneManager.LoadScene("StartTitle");
    }

}
