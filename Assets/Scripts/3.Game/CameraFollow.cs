using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraFollow : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;

    private void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    private void Start()
    {
        SetPlayerTarget();
    }

    /// <summary>
    /// 생성된 플레이어를 찾아 카메라 추적 대상으로 설정한다.
    /// </summary>
    public void SetPlayerTarget()
    {
        Player player = GameManager.Instance.Player;

        if (player == null)
            return;

        vcam.Follow = player.transform;
        vcam.LookAt = player.transform;
    }
}