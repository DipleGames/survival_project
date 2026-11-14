using System.Collections.Generic;
using UnityEngine;

public class MeleeHitDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private LayerMask _reflectableLayers;
    private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();
    private bool _hasReflectedArrowThisStep;
    private ArrowReflectionTrigger _reflectionTrigger;

    public void BeginReflectionWindow(AttackStepData step, System.Action onArrowNearby)
    {
        EndReflectionWindow();
        if (_hasReflectedArrowThisStep || step == null || step.MaxDistance <= 0f) return;

        if (_reflectionTrigger == null)
        {
            GameObject sensor = new GameObject("Arrow Reflection Trigger");
            // 일반적인 대상 선택과 레이캐스트에서 감지 영역을 제외합니다.
            sensor.layer = LayerMask.NameToLayer("Ignore Raycast");
            sensor.transform.SetParent(transform, false);
            _reflectionTrigger = sensor.AddComponent<ArrowReflectionTrigger>();
        }

        _reflectionTrigger.Open(step.MaxDistance, _reflectableLayers, onArrowNearby);
    }

    public void EndReflectionWindow()
    {
        if (_reflectionTrigger != null) _reflectionTrigger.Close();
    }

    private void OnDisable()
    {
        EndReflectionWindow();
    }

    public void BeginStep()
    {
        _hitTargets.Clear();
        _hasReflectedArrowThisStep = false;
    }

    public bool DetectHit(AttackStepData step, Vector3 attackDirection, float baseDamage)
    {
        if (step == null || step.MaxDistance <= 0f) return false;

        float damage = baseDamage * step.DamageMultiplier;
        if (damage <= 0f) return false;
    
        attackDirection.y = 0f;
        bool checkAngle = step.RangeType == AttackRangeType.Sector && step.CenterAngle < 360f;
        if (checkAngle && attackDirection.sqrMagnitude < 0.0001f) return false;
  
        Vector3 origin = transform.position;

        //
        int detectionLayers = _targetLayers.value | _reflectableLayers.value;
        Collider[] colliders = Physics.OverlapSphere(origin, step.MaxDistance, detectionLayers, QueryTriggerInteraction.Collide);
        bool hasHit = false;

        // 이번 타수에서 화살을 반사했다면 해당 타수의 적 타격 판정은 생략합니다.
        if (_hasReflectedArrowThisStep) return false;
        if (TryReflectNearestArrow(step, attackDirection)) return true;
        //

        foreach (Collider hitCollider in colliders)
        {
            if (!IsInLayerMask(hitCollider.gameObject.layer, _targetLayers)) continue;
            if (hitCollider.transform.IsChildOf(transform)) continue;
            
            IDamageable target = hitCollider.GetComponentInParent<IDamageable>();
            if (target == null || _hitTargets.Contains(target)) continue;
            
            Component targetComponent = target as Component;
            if (targetComponent == null) continue;
            Vector3 toTarget = targetComponent.transform.position - origin;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (!IsInsideAttackArea(step, attackDirection, toTarget, distance, checkAngle)) continue;
            
            // 같은 타수에서는 동일 대상에게 한 번만 데미지 전달
            _hitTargets.Add(target);
            target.Attacked(damage, gameObject);
            if (step.KnockbackPower > 0f && target is IKnockbackable knockbackTarget)
            {
                knockbackTarget.ApplyKnockback(attackDirection, step.KnockbackPower);
            }
            hasHit = true;
        }

        return hasHit;
    }

    public bool TryReflectNearestArrow(AttackStepData step, Vector3 attackDirection)
    {
        if (_hasReflectedArrowThisStep || step == null || step.MaxDistance <= 0f) return false;

        attackDirection.y = 0f;
        bool checkAngle = step.RangeType == AttackRangeType.Sector && step.CenterAngle < 360f;
        if (checkAngle && attackDirection.sqrMagnitude < 0.0001f) return false;

        Vector3 origin = transform.position;
        Collider[] colliders = Physics.OverlapSphere(
            origin,
            step.MaxDistance,
            _reflectableLayers,
            QueryTriggerInteraction.Collide);

        ArcherArrow nearestArrow = null;
        float nearestArrowDistance = float.MaxValue;

        foreach (Collider hitCollider in colliders)
        {
            ArcherArrow arrow = hitCollider.GetComponentInParent<ArcherArrow>();
            if (arrow == null) continue;

            Vector3 toArrow = arrow.transform.position - origin;
            toArrow.y = 0f;

            float distance = toArrow.magnitude;
            if (!IsInsideAttackArea(step, attackDirection, toArrow, distance, checkAngle)) continue;
            if (distance >= nearestArrowDistance) continue;

            nearestArrow = arrow;
            nearestArrowDistance = distance;
        }

        if (nearestArrow == null || !nearestArrow.TryReflect(transform)) return false;

        _hasReflectedArrowThisStep = true;
        EndReflectionWindow();
        return true;
    }

    private static bool IsInsideAttackArea(AttackStepData step, Vector3 attackDirection,
        Vector3 toTarget, float distance, bool checkAngle)
    {
        if (distance < step.MinDistance || distance > step.MaxDistance) return false;
        if (checkAngle && Vector3.Angle(attackDirection, toTarget) > step.CenterAngle * 0.5f) return false;

        return true;
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
