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

    /// <summary>
    /// 지정한 씬으로 이동한 후 플레이어를 생성한다.
    /// </summary>
    public void ToNextScene(string sceneName)
    {
        if (_isLoading)
            return;

        StartCoroutine(LoadSceneAndSpawnPlayer(sceneName));
    }

    /// <summary>
    /// 씬 로딩이 완료되면 플레이어 프리팹을 생성한다.
    /// </summary>
    private IEnumerator LoadSceneAndSpawnPlayer(string sceneName)
    {
        _isLoading = true;

        GamePause(false);

        yield return SceneManager.LoadSceneAsync(sceneName);

        GameObject playerObject = Instantiate(playerPrefab, characterSpawnPos, playerPrefab.transform.rotation);
        Player = playerObject.GetComponent<Player>();

        if (cameraFollow != null)
            cameraFollow.SetPlayerTarget();

        currentScene = sceneName;
        _isLoading = false;
    }

    /// <summary>
    /// 게임의 일시정지 상태를 설정한다.
    /// </summary>
    public void GamePause(bool pause)
    {
        isPause = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    /// <summary>
    /// 현재 일시정지 상태를 반전한다.
    /// </summary>
    public void TogglePause()
    {
        GamePause(!isPause);
    }
}