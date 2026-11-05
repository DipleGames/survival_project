using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 characterSpawnPos = new Vector3(0, 0, -56f);

    [Header("카메라")]
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Scene Fade")]
    [SerializeField] private SceneFade sceneFade;


    public Player Player { get; private set; }

    public string currentScene;

    [HideInInspector] public bool isPause;

    private bool _isLoading;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        GamePause(false);
    }

    public void ToNextScene(string sceneName)
    {
        if (_isLoading)
            return;

        StartCoroutine(LoadSceneAndSpawnPlayer(sceneName));
    }

    private IEnumerator LoadSceneAndSpawnPlayer(string sceneName)
    {
        _isLoading = true;

        GamePause(false);

        // 씬 로드 전에 검은 화면 표시
        sceneFade.gameObject.SetActive(true);

        SceneManager.sceneLoaded += OnSceneLoaded;

        yield return SceneManager.LoadSceneAsync(sceneName);

        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 씬 초기화 및 카메라 갱신을 위한 프레임 확보
        yield return null;
        yield return new WaitForEndOfFrame();

        // 2초 동안 검은 화면에서 게임 화면으로 전환
        sceneFade.FadeIn();

        _isLoading = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;

        // 기존 플레이어 참조 초기화
        Player = null;

        GameObject playerObject = Instantiate(playerPrefab, characterSpawnPos, playerPrefab.transform.rotation);
        Player = playerObject.GetComponent<Player>();

        // 새 씬에 존재하는 CameraFollow 연결
        cameraFollow = FindObjectOfType<CameraFollow>();

        if (cameraFollow != null && Player != null)
            cameraFollow.SetPlayerTarget(Player.transform);
        else
            Debug.LogWarning("[GameManager] 플레이어 또는 CameraFollow 연결 실패");
    }

    protected virtual void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void GamePause(bool pause)
    {
        isPause = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    public void TogglePause()
    {
        GamePause(!isPause);
    }
}