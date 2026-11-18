using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BP_Charge",
    menuName = "Boss/Patterns/Charge"
)]
public class ChargePattern : BossPattern
{
    [Header("돌진 설정")]
    [Min(0f)]
    public float chargeDistance = 9f;

    [Min(0.01f)]
    public float chargeDuration = 0.6f;

    [Header("돌진 마무리 시간")]
    [Min(0f)]
    public float attackDuration = 0.3f;

    [Header("공격 시간")]
    [Min(0f)]
    public float telegraphDuration = 0.7f;

    [Header("공격 판정")]
    public int damage = 2;
    public Vector3 hitboxSize = new Vector3(3f, 1f, 3f);

    [Header("돌진 경고 영역")]
    [Min(0.1f)]
    public float warningWidth = 3f;

    [Header("돌진 애니메이션")]
    [SerializeField] private AnimationClip chargeAnimationClip;

    private static readonly int ChargeState = Animator.StringToHash("Base Layer.ChargeAttack");
    private static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");
    private static readonly int ChargeSpeed = Animator.StringToHash("VargaChargeSpeed");

    public override bool CanExecute(BossContext context)
    {
        return context != null
            && context.Boss != null
            && context.Player != null
            && context.BossRigidbody != null
            && context.DamageHitbox != null;
    }

    public override void OnInterrupted(BossContext context)
    {
        context.ChargeWarning?.Hide();
    }

    public override IEnumerator Execute(BossContext context)
    {
        Transform boss = context.Boss;
        Transform player = context.Player;
        Rigidbody rb = context.BossRigidbody;

        // 1. 돌진 방향 계산
        Vector3 direction = player.position - boss.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            yield break;

        direction.Normalize();

        // 1. 돌진 경로 표시
        Vector3 startPosition = rb.position;
        Vector3 targetPosition = startPosition + direction * chargeDistance;

        // 돌진 경고 영역 표시
        if (context.ChargeWarning != null)
        {
            context.ChargeWarning.Show(
                startPosition,
                direction,
                chargeDistance,
                warningWidth,
                telegraphDuration
            );
        }

        context.TelegraphMotion?.PlayTelegraph(telegraphDuration);

        yield return new WaitForSeconds(telegraphDuration);

        context.ChargeWarning?.Hide();
        context.TelegraphMotion?.PlayRelease();

        Animator animator = context.BossAnimator;

        bool playChargeAnimation = animator != null
            && animator.isActiveAndEnabled
            && chargeAnimationClip != null
            && animator.HasState(0, ChargeState)
            && animator.HasState(0, IdleState);

        if (playChargeAnimation)
        {
            // ChargeAttack 상태에만 적용하며 전체 Animator.speed는 유지한다.
            animator.SetFloat(ChargeSpeed, chargeAnimationClip.length / Mathf.Max(0.01f, chargeDuration));
            animator.Play(ChargeState, 0, 0f);
            animator.Update(0f);
        }

        // 2. 돌진 시작
        float elapsed = 0f;
        bool hasHitPlayer = false;

        while (elapsed < chargeDuration)
        {
            yield return new WaitForFixedUpdate();

            elapsed += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsed / chargeDuration);

            Vector3 nextPosition = Vector3.Lerp(startPosition, targetPosition, t);

            rb.MovePosition(nextPosition);

            // 이동 예정 위치에서 공격 판정
            if (!hasHitPlayer)
            {
                hasHitPlayer = context.DamageHitbox.TryHitPlayer(nextPosition, hitboxSize * 0.5f, damage);
            }
        }

        if (playChargeAnimation && animator != null && animator.isActiveAndEnabled)
        {
            animator.Play(IdleState, 0, 0f);
            animator.Update(0f);
        }

        // 공격 마무리 동작을 위한 대기
        yield return new WaitForSeconds(attackDuration);
    }
}
