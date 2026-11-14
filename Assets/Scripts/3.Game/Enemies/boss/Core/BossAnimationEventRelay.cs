using UnityEngine;

public class BossAnimationEventRelay : MonoBehaviour
{
    [SerializeField]
    private BossController bossController;

    public void OnTransformAnimationFinished()
    {
        bossController?.OnTransformAnimationFinished();
    }
}
