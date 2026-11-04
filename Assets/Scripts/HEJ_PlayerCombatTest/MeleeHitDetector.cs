using System.Collections.Generic;
using UnityEngine;

public class MeleeHitDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayers;
    private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

    public void BeginStep()
    {
        _hitTargets.Clear();
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
        Collider[] colliders = Physics.OverlapSphere(origin, step.MaxDistance, _targetLayers, QueryTriggerInteraction.Collide);
        bool hasHit = false;

        foreach (Collider hitCollider in colliders)
        {
            if (hitCollider.transform.IsChildOf(transform)) continue;
            
            IDamageable target = hitCollider.GetComponentInParent<IDamageable>();
            if (target == null || _hitTargets.Contains(target)) continue;
            
            Component targetComponent = target as Component;
            if (targetComponent == null) continue;
            Vector3 toTarget = targetComponent.transform.position - origin;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (distance < step.MinDistance || distance > step.MaxDistance) continue;
            if (checkAngle && Vector3.Angle(attackDirection, toTarget) > step.CenterAngle * 0.5f) continue;
            
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
}