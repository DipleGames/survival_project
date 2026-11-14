using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BP_HoofSlam",
    menuName = "Boss/Patterns/Hoof Slam"
)]
public class HoofSlamPattern : BossPattern
{
    [Header("공격 조건")]
    [Min(0f)]
    public float activationRange = 3.5f;

    [Header("공격 설정")]
    [Min(0.1f)]
    public float attackRadius = 3f;

    [Min(0)]
    public int damage = 2;

    [Header("공격 시간")]
    [Min(0f)]
    public float telegraphDuration = 0.8f;

    [Header("공격 실행 시간")]
    [Min(0f)]
    public float attackDuration = 0.4f;

    [Header("애니메이션 설정")]
    [SerializeField]
    private AnimationClip slamAnimationClip;

    [SerializeField]
    private string slamAnimationState = "SlamAttack";

    [SerializeField]
    private string idleAnimationState = "Idle";

    private static readonly int SlamSpeed = Animator.StringToHash("VargaSlamSpeed");

    [Header("플레이어 판정")]
    public LayerMask playerLayer;

    public override bool CanExecute(BossContext context)
    {
        if (context == null ||
            context.Boss == null ||
            context.Player == null)
            return false;

        Vector3 offset = context.Player.position - context.Boss.position;

        offset.y = 0f;

        return offset.sqrMagnitude <= activationRange * activationRange;
    }

    public override void OnInterrupted(BossContext context)
    {
        context.HoofSlamWarning?.Hide();
    }

    public override IEnumerator Execute(BossContext context)
    {
        Transform boss = context.Boss;

        Animator animator = context.BossAnimator;

        bool playSlamAnimation = animator != null
            && animator.isActiveAndEnabled
            && slamAnimationClip != null
            && animator.HasState(0, Animator.StringToHash(slamAnimationState))
            && animator.HasState(0, Animator.StringToHash(idleAnimationState));

        Debug.Log($"[{boss.name}] 심판의 발굽 준비");

        // 1. 원형 경고 시작
        context.HoofSlamWarning?.Show(attackRadius, telegraphDuration);

        // 2. 공격 전조 시간
        yield return new WaitForSeconds(telegraphDuration);

        // 3. 공격 순간 경고 종료
        context.HoofSlamWarning?.Hide();

        // 전조 종료 후 실제 공격과 함께 애니메이션을 시작한다.
        if (playSlamAnimation)
        {
            animator.SetFloat(SlamSpeed, slamAnimationClip.length / Mathf.Max(0.01f, attackDuration));
            animator.Play(slamAnimationState, 0, 0f);
            animator.Update(0f);
        }

        // 4. 기존 원형 공격 판정
        Vector3 center = boss.position;

        Collider[] hits = Physics.OverlapSphere(
            center,
            attackRadius,
            playerLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            Vector3 closestPoint = hit.ClosestPoint(center);
            Vector3 offset = closestPoint - center;
            offset.y = 0f;

            if (offset.sqrMagnitude >
                attackRadius * attackRadius)
                continue;

            Debug.Log($"[{boss.name}] 심판의 발굽 적중: {hit.name}, Damage: {damage}");

            // 플레이어 체력 시스템은 추후 연결
            break;
        }

        // 공격 후 동작이 완료될 때까지 Attack 상태 유지
        yield return new WaitForSeconds(attackDuration);

        // 공격 애니메이션이 끝난 뒤 충격파 VFX 실행
        context.HoofSlamShockwave?.Play(attackRadius);

        // 공격 애니메이션 종료 후 Idle 복귀
        if (playSlamAnimation && animator != null && animator.isActiveAndEnabled)
        {
            animator.CrossFade(idleAnimationState, 0.1f);
        }

        // 이후 BossController에서 Recovery 상태로 전환
    }
}
