using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BP_RadialBarrage", menuName = "Boss/Patterns/Radial Barrage")]
public class RadialBarragePattern : BossPattern
{
    [Header("연속 발사 설정")]
    [Min(1)] public int volleyCount = 3;
    [Min(0.05f)] public float telegraphDuration = 1.2f;
    [Min(0f)] public float volleyInterval = 0.5f;

    [Header("탄막과 안전지대")]
    [Range(8, 120)] public int projectileCount = 32;
    [Range(20f, 180f)] public float safeAngle = 80f;
    [Range(0f, 180f)] public float minimumGapRotation = 45f;
    [Range(0f, 180f)] public float maximumGapRotation = 90f;

    [Header("투사체 설정")]
    [Min(0.1f)] public float projectileSpeed = 8f;
    [Min(0.01f)] public float projectileRadius = 0.15f;
    [Min(0f)] public float spawnRadius = 1.2f;
    [Min(0.1f)] public float travelDistance = 12f;
    public float projectileHeight = 0.5f;
    [Min(0)] public int damage = 1;
    public LayerMask playerLayer = 1 << 6;
    public LayerMask obstacleLayers = (1 << 10) | (1 << 14) | (1 << 16) | (1 << 21);

    [Header("표시 설정")]
    public Material displayMaterial;
    public Color dangerColor = new Color(1f, 0.15f, 0.05f, 0.25f);
    public Color safeColor = new Color(0.1f, 1f, 0.35f, 0.35f);
    public Color projectileColor = new Color(1f, 0.3f, 0.05f, 1f);
    public float warningHeight = 0.05f;

    public override bool CanExecute(BossContext context)
    {
        return context != null && context.Boss != null && context.Player != null
            && displayMaterial != null && projectileSpeed > 0f && travelDistance > 0f;
    }

    public override IEnumerator Execute(BossContext context)
    {
        if (!CanExecute(context)) yield break;

        RadialBarrageRuntime runtime = context.Boss.GetComponent<RadialBarrageRuntime>();

        if (runtime == null) runtime = context.Boss.gameObject.AddComponent<RadialBarrageRuntime>();

        runtime.ClearAttack();

        float gapDirection = Random.Range(0f, 360f);
        float speed = Mathf.Max(0.1f, projectileSpeed);
        float distance = Mathf.Max(0.1f, travelDistance);
        int count = Mathf.Clamp(projectileCount, 8, 120);
        float gap = Mathf.Clamp(safeAngle, 20f, 180f);

        for (int volley = 0; volley < Mathf.Max(1, volleyCount); volley++)
        {
            if (context.Boss == null || context.Player == null)
            {
                runtime.ClearAttack();
                yield break;
            }

            if (volley > 0)
            {
                float minRotation = Mathf.Clamp(minimumGapRotation, 0f, 180f);
                float maxRotation = Mathf.Clamp(maximumGapRotation, minRotation, 180f);

                gapDirection = Mathf.Repeat(gapDirection + Random.Range(minRotation, maxRotation) * (Random.value < 0.5f ? -1f : 1f), 360f);
            }

            // 전조와 발사는 같은 월드 위치와 방향을 사용
            Vector3 origin = context.Boss.position;
            runtime.ShowWarning(origin + Vector3.up * warningHeight,
                spawnRadius + distance, gapDirection, gap, displayMaterial, dangerColor, safeColor);

            context.TelegraphMotion?.PlayTelegraph(telegraphDuration);
            yield return new WaitForSeconds(Mathf.Max(0.05f, telegraphDuration));

            runtime.HideWarning();

            if (context.Boss == null || context.Player == null)
            {
                runtime.ClearAttack();
                yield break;
            }

            // 안전지대 경계 바깥에 투사체 반지름만큼 여유를 두어 표시 영역을 침범하지 않음
            float radius = Mathf.Max(0.01f, projectileRadius);
            float launchRadius = Mathf.Max(spawnRadius, radius * 2f);
            float edgePadding = Mathf.Asin(Mathf.Clamp01(radius / launchRadius)) * Mathf.Rad2Deg;

            for (int i = 0; i < count; i++)
            {
                float angle = gapDirection + i * 360f / count;

                if (Mathf.Abs(Mathf.DeltaAngle(gapDirection, angle)) <= gap * 0.5f + edgePadding)
                    continue;

                Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));

                runtime.SpawnProjectile(origin + Vector3.up * projectileHeight + direction * launchRadius,
                    direction, speed, distance, radius, damage, playerLayer, obstacleLayers,
                    displayMaterial, projectileColor);
            }

            if (volley < Mathf.Max(1, volleyCount) - 1)
                yield return new WaitForSeconds(Mathf.Max(0f, volleyInterval));
        }

        // 마지막 탄막이 사라진 뒤 공격을 종료
        yield return new WaitForSeconds(distance / speed);
        runtime.ClearAttack();
    }

    public override void OnInterrupted(BossContext context)
    {
        if (context != null && context.Boss != null)
            context.Boss.GetComponent<RadialBarrageRuntime>()?.ClearAttack();
    }
}
