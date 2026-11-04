using System;
using UnityEngine;

// Sector: 전방 부채꼴 범위, Circle: 주변 원형 범위
public enum AttackRangeType { Sector, Circle }

[Serializable]
public class AttackStepData
{
    // 이번 타격에 사용할 Animator 상태 이름. 예: "Attack_1"
    [SerializeField] private string animationName;

    // 무기 기본 공격력에 곱하는 배율. 기본 공격력 10 × 배율 1.5 = 피해량 15
    [SerializeField, Min(0f)] private float damageMultiplier = 1f;

    // 이번 타격의 범위 모양. 실행기가 이 값을 보고 판정 방식을 선택
    [SerializeField] private AttackRangeType rangeType = AttackRangeType.Sector;

    // 타격 가능한 최소 거리. 이 값보다 가까운 대상은 제외하며, 0이면 최소 거리 제한 없음
    [SerializeField, Min(0f)] private float minDistance = 0f;

    // 타격 가능한 최대 거리. 이 값보다 멀리 있는 대상은 제외
    [SerializeField, Min(0f)] private float maxDistance = 1.5f;

    // 부채꼴의 전체 각도. 90도라면 공격 방향을 중심으로 좌우 45도씩 판정
    // Circle에서는 사용하지 않음
    [SerializeField, Range(0f, 360f)] private float centerAngle = 90f;

    // 피격 대상을 밀어내는 강도. 0이면 넉백 없음
    // 실제 이동 거리나 속도는 대상의 넉백 처리 방식에 따라 결정
    [SerializeField, Min(0f)] private float knockbackPower = 0f;

    // 타수 별 효과음
    [SerializeField] private AudioClip _AttackSFX;

    // 외부에서 설정값을 읽을 수 있도록 제공하는 읽기 전용 프로퍼티
    // 예: step.DamageMultiplier로 읽을 수 있지만 직접 대입할 수는 없음
    public string AnimationName => animationName;
    public float DamageMultiplier => damageMultiplier;
    public AttackRangeType RangeType => rangeType;
    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;
    public float CenterAngle => centerAngle;
    public float KnockbackPower => knockbackPower;
    public AudioClip AttackSFX => _AttackSFX;
}