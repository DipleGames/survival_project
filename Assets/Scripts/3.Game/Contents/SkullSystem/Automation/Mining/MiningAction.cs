using MineTest;
using UnityEngine;

/// <summary>
/// 해골이 광물을 한 번 타격하는 행동을 담당한다.
/// </summary>
public class MiningAction
{
    private readonly int _damagePerHit;

    public MiningAction(int damagePerHit)
    {
        _damagePerHit = Mathf.Max(1, damagePerHit);
    }

    public bool CanExecute(SkullController skull, MiningNode target)
    {
        if (skull == null || target == null)
            return false;

        return target.CanBeMinedBy(skull);
    }

    public bool Execute(SkullController skull, MiningNode target)
    {
        if (!CanExecute(skull, target))
            return false;

        target.TakeDamage(_damagePerHit);

        return true;
    }
}