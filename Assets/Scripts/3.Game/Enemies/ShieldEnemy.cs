using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShieldEnemy : Enemies
{
    [Header("방패 설정")]
    public float shieldHp = 3;

    [Header("방패 파괴 전환 설정")]
    public GameObject normalEnemyPrefab;
    public float knockbackDistance = 1.5f;
    public float knockbackDuration = 0.2f;

    private bool isAttacking = false;
    private bool isDestroyed = false;

    void Start()
    {
        InitializePlayerReference();

        defaultFacingRight = false;

        if (childSpriteRenderer != null)
        {
            originalColor = childSpriteRenderer.color;
        }
    }

    void Update()
    {
        if (player == null || isDestroyed)
        {
            return;
        }

        if (!IsPlayerInAttackRange())
        {
            isAttacking = false;

            Vector3 direction = GetDirectionToPlayer();

            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            UpdateFacingDirection(childSpriteRenderer);
        }
        else
        {
            if(!isAttacking)
            {
                isAttacking = true;
                PerformMeleeAttack();
            }
        }
    }

    void PerformMeleeAttack()
    {
        Debug.Log("[방패 몹] 근접 공격 중...");
    }

    // =========================================================
    // 피격
    // =========================================================
    protected override void TakeDamage(float damage)
    {
        shieldHp -= damage;

        hitFlashEffect?.PlayHitFlash();

        if (shieldHp <= 0)
        {
            BreakShield();
        }
    }

    // =========================================================
    // 사망 처리
    // =========================================================
    void BreakShield()
    {
        isDestroyed = true;

        Vector3 knockbackDirection = (transform.position - player.position).normalized;
        knockbackDirection.y = 0;
        Vector3 targetPosition = transform.position + (knockbackDirection * knockbackDistance);

        transform.DOMove(targetPosition, knockbackDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            if (normalEnemyPrefab != null)
            {
                Instantiate(normalEnemyPrefab, transform.position, Quaternion.Euler(90f,0f,0f));
            }
            Destroy(gameObject);
        });
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
