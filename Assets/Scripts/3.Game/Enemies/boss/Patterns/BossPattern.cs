using System.Collections;
using UnityEngine;

public abstract class BossPattern : ScriptableObject
{
    [Header("패턴 기본 설정")]
    public string patternName;

    [Min(0f)]
    public float cooldown = 3f;

    [Min(0f)]
    public float selectionWeight = 1f;

    [Header("반격 가능 시간")]
    [Min(0f)]
    public float recoveryDuration = 1.2f;

    // 현재 상황에서 패턴 실행 가능 여부
    public abstract bool CanExecute(BossContext context);

    // 전조부터 공격 종료까지 실행한다. Recovery 대기는 BossController가 담당한다.
    public abstract IEnumerator Execute(BossContext context);

    // 페이즈 전환 등으로 공격이 중단될 때 호출
    public virtual void OnInterrupted(BossContext context)
    {
        
    }
}
