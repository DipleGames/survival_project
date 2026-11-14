using UnityEngine;

public class BossDamageHitbox : MonoBehaviour
{
    [Header("플레이어 판정")]
    [SerializeField] private LayerMask playerLayer;

    public bool TryHitPlayer(Vector3 center, Vector3 halfExtents, int damage)
    {
        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            Quaternion.identity,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            // 현재는 공격 판정 확인용
            Debug.Log($"[{name}] 플레이어 피격! Damage: {damage}");
            return true;
        }

        return false;
    }
}
