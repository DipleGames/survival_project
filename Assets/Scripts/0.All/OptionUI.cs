using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionUI : MonoBehaviour
{
    [Header("Option Panels")]
    [SerializeField] protected GameObject panel;
    [SerializeField] protected GameObject KeySoundSettingPanel;
    [SerializeField] private OptionSettingsUI optionSettings;
    [SerializeField] private GameObject CheckHomePanel;
    [SerializeField] private GameObject statPanel;
    [SerializeField] protected GameObject backPanel;

    [Header("Compenent")]
    [SerializeField] protected GameManager gameManager;
    [SerializeField] private SaveLoad saveLoad;

    private SoundManager soundManager;

    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (KeySoundSettingPanel != null)
            KeySoundSettingPanel.SetActive(false);

        if (CheckHomePanel != null)
            CheckHomePanel.SetActive(false);

        if (gameManager == null)
            gameManager = GameManager.Instance;

        soundManager = SoundManager.Instance;

        if (optionSettings != null)
            optionSettings.Initialize(KeySoundSettingPanel);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        // 키 설정 변경 중에는 ESC로 패널을 닫지 않음
        if (optionSettings != null && optionSettings.IsWaitingForKey)
            return;

        // 1순위: 세이브/로드 패널이 열려 있다면 해당 패널만 닫기
        if (saveLoad.SaveLoadPanel != null && saveLoad.SaveLoadPanel.activeSelf)
        {
            saveLoad.CloseSaveLoadPanel();
            return;
        }

        // 2순위: 옵션 세부 설정 패널이 열려 있다면 해당 패널만 닫기
        if (KeySoundSettingPanel != null && KeySoundSettingPanel.activeSelf)
        {
            KeySoundSettingPanel.SetActive(false);
            panel.SetActive(true);
            return;
        }

        // 3순위: 타이틀 이동 확인 패널 닫기
        if (CheckHomePanel != null && CheckHomePanel.activeSelf)
        {
            CloseCheckHomePanel();
            return;
        }

        if (gameManager == null)
            return;

        // 4순위: 메인 옵션 패널 열기 / 닫기
        if (gameManager.isPause)
            ReturnToGame();
        else
            PauseGame();
    }

    /// <summary>
    /// 게임을 일시정지하고 옵션 UI를 표시한다.
    /// </summary>
    public virtual void PauseGame()
    {
        if (gameManager == null || gameManager.isPause)
            return;

        // 기존 커서 상태 저장
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        // 옵션 UI 조작을 위해 커서 활성화
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (backPanel != null)
            backPanel.SetActive(true);

        if (statPanel != null)
            statPanel.SetActive(true);

        if (panel != null)
            panel.SetActive(true);

        gameManager.GamePause(true);
    }

    /// <summary>
    /// 옵션 UI를 닫고 게임을 재개한다.
    /// </summary>
    public virtual void ReturnToGame()
    {
        if (gameManager == null || !gameManager.isPause)
            return;

        if (backPanel != null)
            backPanel.SetActive(false);

        if (statPanel != null)
            statPanel.SetActive(false);

        if (optionSettings != null)
            optionSettings.CancelKeyChange();

        if (panel != null)
            panel.SetActive(false);

        // 게임 재개
        gameManager.GamePause(false);

        // 기존 커서 상태 복원
        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;
    }

    /// <summary>
    /// 타이틀 이동 확인 패널을 연다.
    /// </summary>
    public void OpenCheckHomePanel()
    {
        if (CheckHomePanel != null)
            CheckHomePanel.SetActive(true);
    }

    /// <summary>
    /// 타이틀 이동 확인 패널을 닫는다.
    /// </summary>
    public void CloseCheckHomePanel()
    {
        if (CheckHomePanel != null)
            CheckHomePanel.SetActive(false);
    }

    /// <summary>
    /// 게임을 종료하고 타이틀 씬으로 이동한다.
    /// </summary>
    public void TitleScene()
    {
        // 설정 저장 및 키 변경 종료
        if (optionSettings != null)
        {
            optionSettings.SaveMuteSettings();
            optionSettings.CancelKeyChange();
        }

        // 일시정지 해제
        if (gameManager != null)
            gameManager.GamePause(false);

        // 타이틀에서 커서 사용 가능하도록 설정
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 플레이어 제거
        if (gameManager != null && gameManager.Player != null)
            Destroy(gameManager.Player.gameObject);

        // 사운드 매니저 제거
        if (soundManager != null)
            Destroy(soundManager.gameObject);

        // 게임 매니저 제거
        if (gameManager != null)
            Destroy(gameManager.gameObject);

        // 타이틀 씬 이동
        SceneManager.LoadScene("StartTitle");
    }
}