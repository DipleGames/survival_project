using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BP_RingBurst", menuName = "Boss/Patterns/Ring Burst")]
public class RingBurstPattern : BossPattern
{
    public enum AttackOrder
    {
        OuterThenInner = 0,
        InnerThenOuter = 1
    }

    [Header("공격 순서")]
    public AttackOrder attackOrder = AttackOrder.OuterThenInner;

    [Header("공격 범위")]
    [Min(0.1f)] public float innerRadius = 3f;
    [Min(0.2f)] public float outerRadius = 7f;
    [Min(0.1f)] public float hitHeight = 3f;
    public LayerMask playerLayer = 1 << 6;
    [Min(0)] public int damage = 2;

    [Header("공격 시간")]
    [Min(0.05f)] public float outerTelegraphDuration = 1f;
    [Min(0.05f)] public float innerTelegraphDuration = 1.2f;
    [Min(0f)] public float impactDuration = 0.2f;

    [Header("표시 설정")]
    public Material displayMaterial;
    public Color warningColor = new Color(1f, 0.15f, 0.05f, 0.3f);
    public Color impactColor = new Color(1f, 0.65f, 0.1f, 0.8f);
    public float warningHeight = 0.05f;

    public override bool CanExecute(BossContext context)
    {
        return context != null && context.Boss != null && context.Player != null
            && displayMaterial != null && innerRadius > 0f && outerRadius > innerRadius;
    }

    public override IEnumerator Execute(BossContext context)
    {
        if (!CanExecute(context)) yield break;

        RingBurstRuntime runtime = context.Boss.GetComponent<RingBurstRuntime>();

        if (runtime == null) 
        {
            runtime = context.Boss.gameObject.AddComponent<RingBurstRuntime>();
        }

        runtime.ClearAttack();

        // 두 공격 모두 시작 위치를 사용하여 경고 이후 범위가 이동하지 않게 함
        Vector3 center = context.Boss.position;
        float inner = Mathf.Max(0.1f, innerRadius);
        float outer = Mathf.Max(inner + 0.1f, outerRadius);
        bool outerFirst = attackOrder == AttackOrder.OuterThenInner;

        yield return ExecuteBurst(context, runtime, center,
            outerFirst ? inner : 0f, outerFirst ? outer : inner,
            outerFirst ? outerTelegraphDuration : innerTelegraphDuration);

        if (context.Boss == null || context.Player == null) { runtime.ClearAttack(); yield break; }
        
        yield return ExecuteBurst(context, runtime, center,
            outerFirst ? 0f : inner, outerFirst ? inner : outer,
            outerFirst ? innerTelegraphDuration : outerTelegraphDuration);
            
        runtime.ClearAttack();
        // Recovery 대기는 BossController에서만 처리합니다.
    }

    private IEnumerator ExecuteBurst(BossContext context, RingBurstRuntime runtime,
        Vector3 center, float minRadius, float maxRadius, float warningDuration)
    {
        runtime.ShowArea(center + Vector3.up * warningHeight, minRadius, maxRadius,
            displayMaterial, warningColor);

        context.TelegraphMotion?.PlayTelegraph(Mathf.Max(0.05f, warningDuration));

        yield return new WaitForSeconds(Mathf.Max(0.05f, warningDuration));

        if (context.Boss == null || context.Player == null) { runtime.ClearAttack(); yield break; }

        runtime.ShowArea(center + Vector3.up * warningHeight, minRadius, maxRadius,
            displayMaterial, impactColor);

        runtime.ApplyDamage(center, minRadius, maxRadius, Mathf.Max(0.1f, hitHeight), damage, playerLayer);

        yield return new WaitForSeconds(Mathf.Max(0f, impactDuration));
        runtime.ClearAttack();
    }

    public override void OnInterrupted(BossContext context)
    {
        if (context != null && context.Boss != null)
            context.Boss.GetComponent<RingBurstRuntime>()?.ClearAttack();
    }
}
