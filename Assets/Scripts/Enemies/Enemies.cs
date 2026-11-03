using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Enemies : EnemyStats, IDamageable, IKnockbackable
{
    protected Transform player;
    public SpriteRenderer childSpriteRenderer;
    protected bool defaultFacingRight;
    protected Color originalColor;

    protected void InitializePlayerReference()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Character");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        childSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (childSpriteRenderer != null)
        {
            originalColor = childSpriteRenderer.color;
        }
    }

    protected bool ApplyDamageToHealth(float damage)
    {
        hp -= damage;

        return hp <= 0;
    }

    protected void PlayHitFlash(SpriteRenderer spriteRenderer, Color originalColor, float duration = 0.2f)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.DOKill();
        spriteRenderer.color = Color.red;
        spriteRenderer.DOColor(originalColor, duration);
    }

    protected void UpdateFacingDirection(SpriteRenderer spriteRenderer)
    {
        if (player == null || spriteRenderer == null)
            return;

        bool playerIsRight = player.position.x > transform.position.x;

        spriteRenderer.flipX = playerIsRight != defaultFacingRight;
    }

    protected Vector3 GetDirectionToPlayer()
    {
        if (player == null)
            return Vector3.zero;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        return direction.normalized;
    }

    protected bool IsPlayerInAttackRange()
    {
        if (player == null)
            return false;

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;

        return offset.sqrMagnitude <= attackRange * attackRange;
    }

    // =========================================================
    // 피격 처리
    // =========================================================

    public void Attacked(float damage, GameObject hitObject)
    {
        TakeDamage(damage);

    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        
    }


    protected virtual void TakeDamage(float damage)
    {
        bool isDead = ApplyDamageToHealth(damage);

        Debug.Log($"[{gameObject.name}] 피격! 남은 HP: {hp}");

        PlayHitFlash(childSpriteRenderer, originalColor);

        if (isDead)
        {
            Die();
        }
    }

    // =========================================================
    // 사망 처리
    // =========================================================

    protected virtual void Die()
    {
        StopAllCoroutines();

        transform.DOKill();

        if (childSpriteRenderer != null)
        {
            childSpriteRenderer.DOKill();
            childSpriteRenderer.transform.DOKill();
        }

        Destroy(gameObject);
    }

    private Coroutine _knockbackRoutine;
    public void ApplyKnockback(Vector3 direction, float power)
    {
        if (!isActiveAndEnabled || power <= 0f) return;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        // 연속 피격 시 기존 넉백을 멈추고 현재 위치에서 다시 시작
        if (_knockbackRoutine != null) StopCoroutine(_knockbackRoutine);
        _knockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized, power));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float distance)
    {
        const float duration = 0.15f;
        Vector3 start = transform.position;
        Vector3 end = start + direction * distance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = 1f - (1f - t) * (1f - t); // 처음엔 빠르게, 끝에서는 느리게
            transform.position = Vector3.Lerp(start, end, easedT);
            yield return null;
        }

        _knockbackRoutine = null;
    }

    private void OnDisable()
    {
        if (_knockbackRoutine != null) StopCoroutine(_knockbackRoutine);
        _knockbackRoutine = null;
    }

}