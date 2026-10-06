using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SmallBazicEnemy : MonoBehaviour
{
    public enum EnemyState { Move, ReadyToDash, Dashing, Cooldown }
    private EnemyState currentState = EnemyState.Move;

    [Header("기본 능력치")]
    public float moveSpeed = 3.0f;
    public float detectAttackRange = 4.0f;

    public int hp = 5;

    [Header("돌진(집중) 설정")]
    public float chargeTime = 2.0f;
    public float maxDashRange = 6.0f;
    public float dashDuration = 1.0f;
    public float cooldownTime = 2.0f;

    [Header("DoTween 연출 이펙트")]
    public Transform warningIndicator; // 인스펙터에서 'WarningPivot' 부모를 할당하세요!

    private Transform player;
    private Vector3 dashTargetPosition;
    public SpriteRenderer childSpriteRenderer;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Character");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // 혹시 인스펙터에서 깜빡하고 할당하지 않았을 경우를 대비한 방어 코드
        if (childSpriteRenderer == null)
        {
            childSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (warningIndicator != null)
        {
            warningIndicator.localScale = new Vector3(1f, 1f, 0f);
            warningIndicator.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.Move:
                HandleMovement();
                break;
        }
    }

    void HandleMovement()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectAttackRange && currentState == EnemyState.Move)
        {
            currentState = EnemyState.ReadyToDash;
            StartCoroutine(PrepareDash());
        }
        else if (currentState == EnemyState.Move)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;

            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            if (childSpriteRenderer != null)
            {
                // 플레이어가 왼쪽에 있으면 뒤집기 (에셋 방향에 따라 부호 수정 가능)
                childSpriteRenderer.flipX = (player.position.x > transform.position.x);
            }
        }
    }

    IEnumerator PrepareDash()
    {
        Debug.Log("[일반 몹] 공격 준비");

        Vector3 dashDirection = (player.position - transform.position).normalized;
        dashDirection.y = 0;

        dashTargetPosition = transform.position + (dashDirection * maxDashRange);

        if (childSpriteRenderer != null)
        {
            childSpriteRenderer.flipX = (player.position.x > transform.position.x);
        }

        if (warningIndicator != null)
        {
            warningIndicator.DOKill();
            warningIndicator.gameObject.SetActive(true);

            warningIndicator.localPosition = Vector3.zero;

            if (dashDirection != Vector3.zero)
            {
                float angle = Mathf.Atan2(dashDirection.x, dashDirection.z) * Mathf.Rad2Deg;

                warningIndicator.localRotation = Quaternion.Euler(90f, angle - 90f, 0f);
            }

            float indicatorWidth = 1f;
            warningIndicator.localScale = new Vector3(0f, 1f, indicatorWidth);

            warningIndicator.DOScaleX(maxDashRange, chargeTime).SetEase(Ease.InQuad);
        }

        yield return new WaitForSeconds(chargeTime);
        StartDash();
    }

    void StartDash()
    {
        currentState = EnemyState.Dashing;
        Debug.Log("[일반 몹] 돌진 시작");

        if (warningIndicator != null)
        {
            warningIndicator.localScale = new Vector3(0f, 1f, 1f);
            warningIndicator.gameObject.SetActive(false);
        }

        transform.DOMove(dashTargetPosition, dashDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                StartCoroutine(CooldownState());
            });
    }

    IEnumerator CooldownState()
    {
        currentState = EnemyState.Cooldown;
        yield return new WaitForSeconds(cooldownTime);
        currentState = EnemyState.Move;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"[일반 몹] 피격! 남은 HP: {hp}");

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        transform.DOKill();

        if (warningIndicator != null)
        {
            warningIndicator.DOKill();
        }

        Destroy(gameObject);
    }
}




