using System.Collections.Generic;
using UnityEngine;

// 공유 ScriptableObject 대신 보스별로 이펙트와 피격 상태를 관리합니다.
public class RingBurstRuntime : MonoBehaviour
{
    private RingBurstWarningVFX area;
    private readonly HashSet<Player> hitPlayers = new HashSet<Player>();

    public void ShowArea(Vector3 position, float inner, float outer, Material material, Color color)
    {
        ClearAttack();
        GameObject display = new GameObject("Ring Burst Area");
        display.transform.position = position;
        area = display.AddComponent<RingBurstWarningVFX>();
        area.Show(inner, outer, material, color);
    }

    public void ApplyDamage(Vector3 center, float inner, float outer, float height,
        int damage, LayerMask playerLayer)
    {
        hitPlayers.Clear();
        Collider[] hits = Physics.OverlapBox(center + Vector3.up * height * 0.5f,
            new Vector3(outer, height * 0.5f, outer), Quaternion.identity,
            playerLayer, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            if (hit.transform.IsChildOf(transform)) continue;
            Player player = hit.GetComponentInParent<Player>();
            // 캐릭터의 지면 위치 기준. 경고와 같은 XZ 반지름으로 판정합니다.
            Vector3 position = player != null ? player.transform.position : hit.transform.position;
            Vector3 offset = position - center;
            offset.y = 0f;
            float distance = offset.sqrMagnitude;
            if (distance > outer * outer || (inner > 0f && distance <= inner * inner)) continue;
            if (player != null)
            {
                if (hitPlayers.Add(player) && player.Health != null)
                    player.Health.TakeDamage(Mathf.Max(0, damage));
            }
            else
                Debug.Log($"[대지 균열] 플레이어 판정 적중: {hit.name}, 피해량: {damage}");
        }
    }

    public void ClearAttack()
    {
        if (area == null) return;
        area.gameObject.SetActive(false);
        Destroy(area.gameObject);
        area = null;
    }

    private void OnDisable() { ClearAttack(); }
    private void OnDestroy() { ClearAttack(); }
}
