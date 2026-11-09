using System;
using System.Collections.Generic;

public enum StatType
{
    MaxHp,
    AttackPower,
    MoveSpeed,
    CriticalProb,
    CriticalValue,
}

// 스탯 타입과 값을 묶어 Inspector에서 설정하기 위한 구조체
[Serializable]
public struct Stat
{
    public StatType type;
    public float value;
}

public class PlayerStatus
{
    // 플레이어의 기본 능력치 (ScriptableObject에서 초기화)
    private readonly Dictionary<StatType, float> _baseStat = new();

    // 장비, 버프 등의 효과가 적용된 최종 능력치
    private readonly Dictionary<StatType, float> _stat = new();

    // 외부에서 스탯을 조회할 수 있도록 읽기 전용으로 제공
    public IReadOnlyDictionary<StatType, float> Stat => _stat;
    public IReadOnlyDictionary<StatType, float> BaseStat => _baseStat;

    // 스탯 변경 시 외부에 알리는 이벤트
    public event Action OnStatsChanged;

    /// <summary>
    /// 초기 스탯 데이터를 전달받아 기본 스탯과 최종 스탯을 초기화한다.
    /// </summary>
    public PlayerStatus(IEnumerable<Stat> initialStats)
    {
        foreach (Stat stat in initialStats)
        {
            _baseStat[stat.type] = stat.value;
            _stat[stat.type] = stat.value;
        }
    }

    /// <summary>
    /// 지정한 스탯 타입의 최종 능력치를 반환한다.
    /// 해당 스탯이 존재하지 않으면 0을 반환한다.
    /// </summary>
    public float GetStat(StatType type)
    {
        return _stat.TryGetValue(type, out float value) ? value : 0f;
    }

    /// <summary>
    /// 기본 스탯에 장비 보너스를 합산하여 최종 스탯을 재계산한다.
    /// </summary>
    public void RecalculateStats(IEnumerable<Stat> equipmentBonuses)
    {
        // 1. 최종 스탯 초기화
        _stat.Clear();

        // 2. 기본 스탯 복사
        foreach (var pair in _baseStat)
        {
            _stat[pair.Key] = pair.Value;
        }

        // 3. 장비 보너스 적용
        if (equipmentBonuses != null)
        {
            foreach (Stat bonus in equipmentBonuses)
            {
                if (_stat.ContainsKey(bonus.type))
                    _stat[bonus.type] += bonus.value;
                else
                    _stat[bonus.type] = bonus.value;
            }
        }

        // 4. 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 장비 보너스를 제거하고 최종 스탯을 기본값으로 복원한다.
    /// </summary>
    public void ResetStat()
    {
        RecalculateStats(Array.Empty<Stat>());
    }
}