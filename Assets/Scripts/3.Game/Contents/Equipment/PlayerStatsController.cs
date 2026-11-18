
using System.Collections;
using UnityEngine;

//이벤트 구독, 스탯 조회, 버튼 입력 처리 담당
public class PlayerStatsController : MonoBehaviour
{
    [SerializeField] private PlayerStatsView view;

    private PlayerStatus status;
    private Coroutine connectCoroutine;
    private bool isOpen;

    private void OnEnable()
    {
        view.ToggleButton.onClick.AddListener(TogglePanel);

        isOpen = false;
        view.SetPanelVisible(false);

        connectCoroutine = StartCoroutine(ConnectPlayer());
    }

    private IEnumerator ConnectPlayer() //플레이 루프 진입 시 Player(Clone) 생성되므로 코루틴으로 연결 시도, PlayerStatus가 생성될 때까지 대기.
    {
        Player player = FindObjectOfType<Player>();

        while (player == null || player.Status == null)
        {
            yield return new WaitForSecondsRealtime(0.2f);
            player = FindObjectOfType<Player>();
        }

        status = player.Status;

        status.OnStatsChanged += RefreshUI;
        RefreshUI();
    }

    private void TogglePanel()
    {
        isOpen = !isOpen;
        view.SetPanelVisible(isOpen);
    }

    private void RefreshUI()
    {
        view.Render(status.Stat);
    }

    private void OnDisable()
    {
        view.ToggleButton.onClick.RemoveListener(TogglePanel);

        if (connectCoroutine != null)
            StopCoroutine(connectCoroutine);

        if (status != null)
            status.OnStatsChanged -= RefreshUI;

        status = null;
    }
}
