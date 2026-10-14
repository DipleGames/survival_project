using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 2.0f;
    public LayerMask enemyLayer;

    [Header("화살 반사")]
    public LayerMask arrowLayer;

    [Header("차지 공격 설정")]
    public float minChargeTime = 0.5f;
    private float chargeTimer = 0f;
    private bool isCharging = false;

    [Header("공격 데미지 설정")]
    public int normalDamage = 1;
    public int heavyDamage = 3;

    [Header("기즈모 스타일 설정")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    public bool showSolidSphere = true;

    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeTimer = 0f;
        }

        if (isCharging && Input.GetMouseButton(0))
            chargeTimer += Time.deltaTime;

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;
            bool isHeavyAttack = chargeTimer >= minChargeTime;
            TryAttack(isHeavyAttack);
        }
    }

    private void TryAttack(bool isHeavyAttack)
    {
        int finalDamage = isHeavyAttack ? heavyDamage : normalDamage;

        Debug.Log(isHeavyAttack
            ? $"[플레이어] 강공격! 데미지: {finalDamage}"
            : $"[플레이어] 일반공격! 데미지: {finalDamage}");

        // 적과 화살을 모두 모은 뒤, 가장 가까운 대상을 선택합니다.
        // 거리가 거의 같으면 화살이 우선됩니다.
        Collider[] enemyHits = Physics.OverlapSphere(
            cachedTransform.position, attackRange, enemyLayer);

        Collider[] arrowHits = arrowLayer.value != 0
            ? Physics.OverlapSphere(cachedTransform.position, attackRange, arrowLayer)
            : new Collider[0];

        float nearestEnemyDistance = float.MaxValue;
        Collider nearestEnemyCollider = null;

        foreach (Collider hit in enemyHits)
        {
            if (hit == null) continue;
            float distance = (hit.ClosestPoint(cachedTransform.position) - cachedTransform.position).sqrMagnitude;
            if (distance < nearestEnemyDistance)
            {
                nearestEnemyDistance = distance;
                nearestEnemyCollider = hit;
            }
        }

        float nearestArrowDistance = float.MaxValue;
        ArcherArrow nearestArrow = null;

        foreach (Collider hit in arrowHits)
        {
            if (hit == null) continue;
            ArcherArrow arrow = hit.GetComponentInParent<ArcherArrow>();
            if (arrow == null) continue;

            float distance = (hit.ClosestPoint(cachedTransform.position) - cachedTransform.position).sqrMagnitude;
            if (distance < nearestArrowDistance)
            {
                nearestArrowDistance = distance;
                nearestArrow = arrow;
            }
        }

        // 동거리 또는 사실상 같은 거리(0.05 유닛 이내)면 화살 우선.
        if (nearestArrow != null &&
            nearestArrowDistance <= nearestEnemyDistance + 0.05f * 0.05f)
        {
            if (nearestArrow.TryReflect(cachedTransform))
                Debug.Log("[플레이어] 화살을 쳐냈습니다.");
            return;
        }

        if (nearestEnemyCollider == null)
            return;

        BigBazicEnemy bigEnemy = nearestEnemyCollider.GetComponentInParent<BigBazicEnemy>();
        if (bigEnemy != null)
        {
            bool dodged = bigEnemy.TryDodgeFromPlayerAttack();
            if (dodged)
            {
                Debug.Log("[플레이어] 적이 공격을 회피했습니다.");
                return;
            }

            bigEnemy.TakeDamage(finalDamage);
            return;
        }

        ArcherEnemy archer = nearestEnemyCollider.GetComponentInParent<ArcherEnemy>();
        if (archer != null)
        {
            bool dodged = archer.TryDodgeFromPlayerAttack();
            if (dodged)
            {
                Debug.Log("[플레이어] 궁수가 공격을 회피했습니다.");
                return;
            }

            archer.TakeDamage(finalDamage);
            return;
        }

        ShieldEnemy shieldEnemy = nearestEnemyCollider.GetComponentInParent<ShieldEnemy>();
        if (shieldEnemy != null)
        {
            shieldEnemy.TakeDamage(finalDamage, isHeavyAttack);
            return;
        }

        SmallBazicEnemy normalEnemy = nearestEnemyCollider.GetComponentInParent<SmallBazicEnemy>();
        if (normalEnemy != null)
        {
            normalEnemy.TakeDamage(finalDamage);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = gizmoColor;

        if (showSolidSphere)
            Gizmos.DrawSphere(Vector3.zero, attackRange);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireSphere(Vector3.zero, attackRange);
    }
}
