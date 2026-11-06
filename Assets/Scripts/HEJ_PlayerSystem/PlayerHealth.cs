using System;
using UnityEngine;

public class PlayerHealth
{
    private readonly PlayerStatus _status;

    public float CurrentHp { get; private set; }
    public float MaxHp => _status.GetStat(StatType.MaxHp);

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    /// <summary>
    /// PlayerStatus를 전달받아 체력을 초기화하고
    /// 스탯 변경 이벤트를 구독한다.
    /// </summary>
    public PlayerHealth(PlayerStatus status)
    {
        _status = status;
        CurrentHp = MaxHp;

        _status.OnStatsChanged += HandleStatsChanged;
    }

    /// <summary>
    /// 피해를 받아 현재 체력을 감소시킨다.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (damage <= 0 || CurrentHp <= 0)
            return;

        CurrentHp = Mathf.Max(0f, CurrentHp - damage);

        OnHealthChanged?.Invoke(CurrentHp, MaxHp);

        if (CurrentHp <= 0)
            OnDied?.Invoke();
    }

    /// <summary>
    /// 현재 체력을 회복한다.
    /// </summary>
    public void Heal(float amount)
    {
        if (amount <= 0 || CurrentHp <= 0)
            return;

        CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);

        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
    }

    /// <summary>
    /// 저장된 현재 체력을 복원한다.
    /// </summary>
    public void LoadCurrentHp(float hp)
    {
        CurrentHp = Mathf.Clamp(hp, 0f, MaxHp);

        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
    }

    /// <summary>
    /// 스탯 변경 시 현재 체력이 최대 체력을 초과하지 않도록 보정한다.
    /// </summary>
    private void HandleStatsChanged()
    {
        CurrentHp = Mathf.Clamp(CurrentHp, 0f, MaxHp);

        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
    }

    /// <summary>
    /// 이벤트 구독을 해제한다.
    /// </summary>
    public void Dispose()
    {
        _status.OnStatsChanged -= HandleStatsChanged;
    }
}