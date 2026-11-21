
using UnityEngine;

public class IncreaseMaxHpEffect : ConsumableEffect
{
    [Header("최대 체력 증가")]
    [SerializeField] private float maxHpBonus = 10f;

    public override bool CanApply(Player player)
    {
        return player != null && player.Status != null && player.Health != null && player.Health.CurrentHp > 0f;
    }

    public override void Apply(Player player)
    {
        // 최대 체력 증가 전 값 저장
        float previousMaxHp = player.Health.MaxHp;

        // 영구 최대 체력 증가
        player.Status.AddPermanentBonus(StatType.MaxHp, maxHpBonus);

        // 실제로 증가한 최대 체력 계산
        float increasedHp = player.Health.MaxHp - previousMaxHp;

        // 증가한 수치만큼 현재 체력 회복
        player.Health.Heal(increasedHp);
    }
}
