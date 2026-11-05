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

    public void SetPlayerTarget(Transform target)
    {
        if (target == null)
            return;

        vcam.Follow = target;
        vcam.LookAt = target;

        vcam.PreviousStateIsValid = false;
    }
}