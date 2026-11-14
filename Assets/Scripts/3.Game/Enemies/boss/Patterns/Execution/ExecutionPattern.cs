using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BP_Execution",
    menuName = "Boss/Patterns/Execution"
)]
public class ExecutionPattern : BossPattern
{
    [Header("공격 조건")]
    [Min(0f)]
    public float activationRange = 3f;

    [Header("부채꼴 공격 설정")]
    [Min(0.1f)]
    public float attackRadius = 3f;

    [Range(1f, 360f)]
    public float attackAngle = 120f;

    [Min(0)]
    public int damage = 2;

    public LayerMask playerLayer;

    [Header("공격 시간")]
    [Min(0f)]
    public float telegraphDuration = 0.7f;

    [Min(0f)]
    public float attackDuration = 0.4f;

    [Header("애니메이션 설정")]
    [SerializeField]
    private AnimationClip executionAnimationClip;

    [SerializeField]
    private string executionAnimationState = "ExecutionAttack";

    [SerializeField]
    private string idleAnimationState = "Idle";

    public override bool CanExecute(BossContext context)
    {
        if (context == null || context.Boss == null || context.Player == null)
            return false;

        Vector3 offset = context.Player.position - context.Boss.position;

        offset.y = 0f;

        return offset.sqrMagnitude <= activationRange * activationRange;
    }

    public override void OnInterrupted(BossContext context)
    {
        context.ExecutionWarning?.Hide();
        context.ExecutionSlash?.Stop();
    }

    public override IEnumerator Execute(BossContext context)
    {
        Transform boss = context.Boss;

        // 1. 공격 시작 시 플레이어 방향 고정
        Vector3 attackDirection = context.Player.position - boss.position;

        attackDirection.y = 0f;

        if (attackDirection.sqrMagnitude < 0.0001f)
            attackDirection = Vector3.left;
        else
            attackDirection.Normalize();

        Animator animator = context.BossAnimator;

        // 부채꼴 경고 표시
        context.ExecutionWarning?.Show(
            attackDirection,
            attackRadius,
            attackAngle,
            telegraphDuration
        );

        // 2. 전조 시간
        yield return new WaitForSeconds(telegraphDuration);

        // 공격 순간 경고 종료
        context.ExecutionWarning?.Hide();

        bool playExecutionAnimation = animator != null
            && animator.isActiveAndEnabled
            && animator.HasState(0, Animator.StringToHash(executionAnimationState));

        // 전조 종료 후 첫 프레임부터 공격 애니메이션 시작
        if (playExecutionAnimation)
        {
            animator.Play(executionAnimationState, 0, 0f);
            animator.Update(0f);
        }

        // 베기 이펙트
        context.ExecutionSlash?.Play(attackDirection, attackRadius, attackAngle);

        // 3. 부채꼴 공격 판정
        Collider[] hits = Physics.OverlapSphere(
            boss.position,
            attackRadius,
            playerLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            Vector3 targetPosition = hit.ClosestPoint(boss.position);

            Vector3 offset = targetPosition - boss.position;
            offset.y = 0f;

            // 수평 거리 검사
            if (offset.sqrMagnitude > attackRadius * attackRadius)
                continue;

            // 부채꼴 각도 검사
            if (offset.sqrMagnitude > 0.0001f)
            {
                float angle = Vector3.Angle(attackDirection, offset);

                if (angle > attackAngle * 0.5f)
                    continue;
            }

            Debug.Log($"[{boss.name}] 사도의 처형 적중: {hit.name}, Damage: {damage}");

            // 플레이어 체력 시스템은 추후 연결
            break;
        }

        // 4. 공격 마무리와 클립 재생이 모두 끝난 뒤 복귀한다.
        float finishDuration = attackDuration;
        
        if (playExecutionAnimation && executionAnimationClip != null)
            finishDuration = Mathf.Max(finishDuration, executionAnimationClip.length);

        yield return new WaitForSeconds(finishDuration);

        // Idle 복귀
        if (playExecutionAnimation && animator != null && animator.isActiveAndEnabled)
        {
            animator.CrossFade(idleAnimationState, 0.1f);
        }

        // 이후 BossController가 Recovery 처리
    }
}
