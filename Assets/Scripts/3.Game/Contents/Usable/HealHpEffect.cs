using UnityEngine;

public class HealHpEffect : ConsumableEffect
{
    [Header("체력 회복")]
    [SerializeField, Min(0f)] private float healAmount = 20f;

    public override bool CanApply(Player player)
    {
        return player != null && player.Health != null && healAmount > 0f && player.Health.CurrentHp > 0f && player.Health.CurrentHp < player.Health.MaxHp;
    }
    //Player의 체력을 회복시키는 효과를 적용하는 메서드. 기존 스크립트에 정의된 Apply 메서드를 오버라이드하여 구현
    public override void Apply(Player player)
    {
        player.Health.Heal(healAmount);
    }
}