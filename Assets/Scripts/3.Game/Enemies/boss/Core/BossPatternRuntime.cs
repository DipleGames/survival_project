using System.Collections.Generic;
using UnityEngine;

public class BossPatternRuntime
{
    // 패턴별 다음 사용 가능 시각
    private readonly Dictionary<BossPattern, float> cooldownEndTimes = new Dictionary<BossPattern, float>();

    // 패턴 사용 가능 여부
    public bool IsReady(BossPattern pattern)
    {
        if (pattern == null)
            return false;

        if (!cooldownEndTimes.TryGetValue(pattern, out float endTime))
            return true;

        return Time.time >= endTime;
    }

    // 패턴 쿨다운 시작
    public void StartCooldown(BossPattern pattern)
    {
        if (pattern == null)
            return;

        cooldownEndTimes[pattern] = Time.time + pattern.cooldown;
    }

    // 남은 쿨다운 조회
    public float GetRemainingCooldown(BossPattern pattern)
    {
        if (pattern == null)
            return 0f;

        if (!cooldownEndTimes.TryGetValue(pattern, out float endTime))
            return 0f;

        return Mathf.Max(0f, endTime - Time.time);
    }

    // 모든 쿨다운 초기화
    public void ResetCooldowns()
    {
        cooldownEndTimes.Clear();
    }
}
