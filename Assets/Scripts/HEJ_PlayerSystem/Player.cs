using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerStatConfig _statConfig;

    [Header("현재 스탯 (디버그용)")]
    [SerializeField] private List<Stat> _currentStats = new();

    public PlayerStatus Status { get; private set; }
    public PlayerHealth Health { get; private set; }

    private void Awake()
    {
        // 기본 스탯 초기화
        Status = new PlayerStatus(_statConfig.initialStats);

        // 체력 초기화
        Health = new PlayerHealth(Status);

        // 이벤트 등록
        Health.OnDied += PlayerDie;
        Status.OnStatsChanged += RefreshStatInspector;

        RefreshStatInspector();
    }

    /// <summary>
    /// 현재 장착 중인 장비 보너스를 전달받아 최종 스탯을 재계산한다.
    /// </summary>
    public void ApplyEquipmentStats(IEnumerable<Stat> equipmentBonuses)
    {
        Status.RecalculateStats(equipmentBonuses);
    }

    /// <summary>
    /// 현재 최종 스탯을 Inspector 디버그용 List에 반영한다.
    /// </summary>
    private void RefreshStatInspector()
    {
        _currentStats.Clear();

        foreach (var pair in Status.Stat)
        {
            _currentStats.Add(new Stat
            {
                type = pair.Key,
                value = pair.Value
            });
        }
    }

    /// <summary>
    /// 플레이어 사망 시 호출된다.
    /// </summary>
    private void PlayerDie()
    {
        Debug.Log("플레이어 사망");
    }

    private void OnDestroy()
    {
        if (Health != null)
        {
            Health.OnDied -= PlayerDie;
            Health.Dispose();
        }

        if (Status != null)
            Status.OnStatsChanged -= RefreshStatInspector;
    }
}