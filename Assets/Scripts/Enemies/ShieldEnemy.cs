using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShieldEnemy : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 1.5f;
    public float attackRange = 1.0f;

    [Header("체력 및 방패 설정")]
    public int shieldHp = 3;

    [Header("방패 파괴 전환 설정")]
    public GameObject normalEnemyPrefab;
    public float knockbackDistance = 1.5f;
    public float knockbackDuration = 0.2f;

    private Transform player;
    private bool isAttacking = false;
    private bool isDestroyed = false;
    public SpriteRenderer childSpriteRenderer;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Character");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null || isDestroyed)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            isAttacking = false;

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;

            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            if (childSpriteRenderer != null)
            {
                childSpriteRenderer.flipX = (player.position.x > transform.position.x);
            }
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

    public void TakeDamage(int damage, bool isHeavyAttack)
    {
        if (isDestroyed)
        {
            return;
        }

        if (isHeavyAttack)
        {
            shieldHp -= damage;

            if (shieldHp <= 0)
            {
                BreakShield();
            }
        }
        else
        {
            Debug.Log("플레이어의 공격이 방패에 막혔습니다!");
        }
    }

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


