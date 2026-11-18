using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BP_Spike",
    menuName = "Boss/Patterns/Spike"
)]
public class SpikePattern : BossPattern
{
    [Header("가시 공격 설정")]
    [SerializeField, Min(1)]
    private int spikeCount = 3;

    [SerializeField, Min(0.1f)]
    private float attackRadius = 1.2f;

    [SerializeField, Min(0)]
    private int damage = 1;

    [SerializeField]
    private LayerMask playerLayer;

    [Header("공격 시간")]
    [SerializeField, Min(0f)]
    private float telegraphDuration = 0.5f;

    [SerializeField, Min(0f)]
    private float spikeInterval = 0.2f;

    [SerializeField, Min(0f)]
    private float attackDuration = 0.4f;

    [Header("경고 VFX")]
    [SerializeField]
    private SpikeWarningVFX warningPrefab;

    [Header("가시 공격 이펙트")]
    [SerializeField]
    private SpikeImpactVFX impactPrefab;

    public override bool CanExecute(BossContext context)
    {
        return context != null &&
               context.Boss != null &&
               context.Player != null;
    }

    public override IEnumerator Execute(BossContext context)
    {
        Transform boss = context.Boss;

        for (int i = 0; i < spikeCount; i++)
        {
            if (context.Player == null)
                yield break;

            // 1. 현재 플레이어 위치 기록
            Vector3 targetPosition = context.Player.position;

            targetPosition.y = boss.position.y;

            Debug.Log($"[{boss.name}] 가시 {i + 1}번째 위치 기록: {targetPosition}");

            // 2. 경고 프리팹 생성
            SpikeWarningVFX warning = null;

            if (warningPrefab != null)
            {
                warning = Instantiate(warningPrefab, targetPosition, Quaternion.Euler(90f, 0f, 0f));

                context.RegisterSpikeWarning(warning);

                warning.Show(targetPosition, attackRadius, telegraphDuration);
            }

            // 3. 경고 시간 대기
            yield return new WaitForSeconds(telegraphDuration);

            // 4. 경고 종료
            context.ClearSpikeWarning();

            // 가시 이펙트 생성
            if (impactPrefab != null)
            {
                SpikeImpactVFX impact = Instantiate(impactPrefab, targetPosition, Quaternion.identity);

                impact.Play(targetPosition);
            }

            // 5. 기록된 위치에 공격 판정
            Collider[] hits = Physics.OverlapSphere(
                targetPosition,
                attackRadius,
                playerLayer,
                QueryTriggerInteraction.Collide
            );

            foreach (Collider hit in hits)
            {
                Vector3 offset = hit.transform.position - targetPosition;

                offset.y = 0f;

                if (offset.sqrMagnitude > attackRadius * attackRadius)
                    continue;

                Debug.Log($"[{boss.name}] 가시 공격 적중: {hit.name}, Damage: {damage}");

                // 실제 플레이어 HP 감소는 추후 연결
                break;
            }

            // 마지막 가시 이후에는 간격 대기 생략
            if (i < spikeCount - 1)
            {
                yield return new WaitForSeconds(spikeInterval);
            }
        }

        // 모든 가시 공격 종료 후 마무리
        yield return new WaitForSeconds(attackDuration);

        // 이후 BossController에서 Recovery 처리
    }

    public override void OnInterrupted(BossContext context)
    {
        context?.ClearSpikeWarning();
    }
}
