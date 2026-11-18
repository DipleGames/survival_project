using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 2f;

    [Tooltip("실제로 멈춘 뒤 Idle로 전환하기까지의 시간, 이동 재개는 지연하지 않는다")]
    [SerializeField, Min(0f)] private float idleAnimationDelay = 0.12f;

    private Transform player;
    private Rigidbody rb;

    private bool canMove = true;
    private Animator animator;
    private bool isWalking;
    private float lastMovementTime;
    private static readonly int WalkState = Animator.StringToHash("Base Layer.Walk");
    private static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");

    public void Initialize(Transform target, Rigidbody rigidbody, Animator bossAnimator = null)
    {
        player = target;
        rb = rigidbody;
        // FixedUpdate의 물리 이동을 렌더 프레임 사이에서 부드럽게 표시한다.
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            // 씬의 이전 오버라이드가 남아 있어도 XZ 이동을 허용한다.
            rb.constraints = (rb.constraints & ~(RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezePositionZ)) | RigidbodyConstraints.FreezePositionY;
        }

        animator = bossAnimator != null ? bossAnimator : GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        // 공격 중에는 ChargePattern의 Animator 제어를 덮어쓰지 않는다.
        if (!canMove)
            return;

        if (player == null || rb == null)
        {
            SetWalking(false);
            return;
        }

        Vector3 direction = player.position - rb.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= stoppingDistance || moveSpeed <= 0f)
        {
            // 애니메이션 전환에만 여유를 두고 추적 위치에는 재개 지연을 적용하지 않는다.
            if (Time.time - lastMovementTime >= idleAnimationDelay)
                SetWalking(false);
            return;
        }

        direction.Normalize();

        float moveDistance = Mathf.Min(moveSpeed * Time.fixedDeltaTime, distance - stoppingDistance);

        Vector3 nextPosition = rb.position + direction * moveDistance;

        rb.MovePosition(nextPosition);
        lastMovementTime = Time.time;

        SetWalking(moveDistance > 0f);
    }

    public void StopMovement()
    {
        canMove = false;
        SetWalking(false);
    }

    public void ResumeMovement()
    {
        canMove = true;
    }

    public bool IsWithinStoppingDistance()
    {
        if (player == null || rb == null)
            return false;

        Vector3 offset = player.position - rb.position;
        offset.y = 0f;

        return offset.sqrMagnitude <= stoppingDistance * stoppingDistance;
    }

    private void SetWalking(bool walking)
    {
        if (isWalking == walking)
            return;

        isWalking = walking;
        if (animator == null || !animator.isActiveAndEnabled)
            return;

        int state = walking ? WalkState : IdleState;
        if (!animator.HasState(0, state))
            return;

        // 상태가 바뀔 때만 재생해 이동 중 프레임이 매번 초기화되지 않도록 한다.
        animator.Play(state, 0, 0f);
        animator.Update(0f);
    }
}
