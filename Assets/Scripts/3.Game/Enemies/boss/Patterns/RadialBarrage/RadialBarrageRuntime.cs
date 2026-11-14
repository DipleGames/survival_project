using System.Collections.Generic;
using UnityEngine;

// 실행 상태는 공유 패턴 에셋이 아닌 보스 인스턴스에서 관리합니다.
public class RadialBarrageRuntime : MonoBehaviour
{
    private RadialBarrageWarningVFX warning;
    private readonly List<RadialBarrageProjectile> projectiles = new List<RadialBarrageProjectile>();

    public void ShowWarning(Vector3 origin, float radius, float gapDirection, float gapAngle,
        Material material, Color dangerColor, Color safeColor)
    {
        HideWarning();
        GameObject area = new GameObject("Radial Barrage Warning");
        area.transform.position = origin;
        warning = area.AddComponent<RadialBarrageWarningVFX>();
        warning.Show(radius, gapDirection, gapAngle, material, dangerColor, safeColor);
    }

    public void HideWarning()
    {
        if (warning == null) return;

        warning.gameObject.SetActive(false);

        Destroy(warning.gameObject);
        warning = null;
    }

    public void SpawnProjectile(Vector3 position, Vector3 direction, float speed, float distance,
        float radius, int damage, LayerMask playerLayers, LayerMask obstacleLayers,
        Material material, Color color)
    {
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "Boss Barrage Projectile";

        // 근접 반사 대상에서 제외
        orb.layer = LayerMask.NameToLayer("Ignore Raycast");
        orb.transform.position = position;
        orb.transform.localScale = Vector3.one * (radius * 2f);
        RadialBarrageProjectile projectile = orb.AddComponent<RadialBarrageProjectile>();
        projectiles.Add(projectile);
        projectile.Initialize(transform, direction, speed, distance, radius, damage,
            playerLayers, obstacleLayers, material, color);
    }

    public void ClearAttack()
    {
        HideWarning();

        foreach (RadialBarrageProjectile projectile in projectiles)
        {
            if (projectile == null) continue;

            projectile.gameObject.SetActive(false);
            Destroy(projectile.gameObject);
        }
        projectiles.Clear();
    }

    private void OnDisable()
    {
        // 페이즈 전환, 보스 비활성화와 사망 시 잔여 탄막을 정리
        ClearAttack();
    }
}
