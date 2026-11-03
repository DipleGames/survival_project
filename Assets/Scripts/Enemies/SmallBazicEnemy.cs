using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SmallBazicEnemy : Enemies
{
    public enum EnemyState { Move, ReadyToDash, Dashing, Cooldown }
    private EnemyState currentState = EnemyState.Move;

    [Header("돌진(집중) 설정")]
    public float chargeTime = 2.0f;
    public float dashHoldTime = 0.3f;
    public float maxDashRange = 6.0f;
    public float dashDuration = 1.0f;
    public float cooldownTime = 2.0f;

    [Header("아종 설정")]
    public bool isVariant = false;
    public float variantDashDuration = 1.5f;
    public float zigzagWidth = 1.0f;

    [Header("DoTween 연출 이펙트")]
    public Transform warningIndicator;

    private Vector3 dashTargetPosition;

    void Start()
    {
        defaultFacingRight = false;

        InitializePlayerReference();

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

        if (childSpriteRenderer != null)
        {
            originalColor = childSpriteRenderer.color;
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
        if (IsPlayerInAttackRange() && currentState == EnemyState.Move)
        {
            currentState = EnemyState.ReadyToDash;
            StartCoroutine(PrepareDash());
        }
        else if (currentState == EnemyState.Move)
        {
            Vector3 direction = GetDirectionToPlayer();

            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            UpdateFacingDirection(childSpriteRenderer);
        }
    }

    IEnumerator PrepareDash()
    {
        Debug.Log("[일반 몹] 공격 준비");

        Vector3 dashDirection = GetDirectionToPlayer();

        dashTargetPosition = transform.position + (dashDirection * maxDashRange);

        UpdateFacingDirection(childSpriteRenderer);

        if (warningIndicator != null)
        {
            warningIndicator.DOKill();
            warningIndicator.gameObject.SetActive(true);

            warningIndicator.localPosition = Vector3.zero;

            if (dashDirection != Vector3.zero)
            {
                Quaternion worldLookRotation = Quaternion.LookRotation(dashDirection, Vector3.up);

                warningIndicator.rotation = worldLookRotation * Quaternion.Euler(90f, -90f, 0f);
            }

            float indicatorWidth = 1f;
            warningIndicator.localScale = new Vector3(0f, 1f, indicatorWidth);

            warningIndicator.DOScaleX(maxDashRange, chargeTime).SetEase(Ease.InQuad);
        }

        yield return new WaitForSeconds(chargeTime);

        yield return new WaitForSeconds(dashHoldTime);

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

        if (isVariant)
        {
            StartVariantDash();
            return;
        }

        transform.DOMove(dashTargetPosition, dashDuration).SetEase(Ease.OutQuad).OnComplete(() => {StartCoroutine(CooldownState());});
    }

    void StartVariantDash()
    {
        Vector3 startPosition = transform.position;

        // 돌진 시작 시 결정한 방향을 고정
        Vector3 forwardDirection = dashTargetPosition - startPosition;
        forwardDirection.y = 0f;

        if (forwardDirection.sqrMagnitude < 0.0001f)
        {
            StartCoroutine(CooldownState());
            return;
        }

        forwardDirection.Normalize();

        // 전진 방향에 수직인 좌우 방향
        Vector3 sideDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // 불규칙한 간격으로 방향 전환 지점 결정
        float firstTurnProgress = Random.Range(0.20f, 0.40f);
        float secondTurnProgress = Random.Range(0.60f, 0.80f);

        // 최초 좌우 방향도 무작위
        float firstSide = Random.value < 0.5f ? -1f : 1f;

        Vector3 firstTurnPosition = startPosition + forwardDirection * (maxDashRange * firstTurnProgress) + sideDirection * (zigzagWidth * firstSide);

        Vector3 secondTurnPosition = startPosition + forwardDirection * (maxDashRange * secondTurnProgress) - sideDirection * (zigzagWidth * firstSide);

        // 마지막 지점은 기존 돌진 종료 위치
        Vector3[] path = {firstTurnPosition, secondTurnPosition, dashTargetPosition};

        transform.DOPath(path,variantDashDuration, PathType.Linear, PathMode.Full3D).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                StartCoroutine(CooldownState());
            });
    }

    IEnumerator CooldownState()
    {
        currentState = EnemyState.Cooldown;
        yield return new WaitForSeconds(cooldownTime);
        currentState = EnemyState.Move;
    }

    // =========================================================
    // 피격
    // =========================================================

    protected override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
    }

    // =========================================================
    // 사망 처리
    // =========================================================
    protected override void Die()
    {
        base.Die();

        transform.DOKill();

        if (warningIndicator != null)
        {
            warningIndicator.DOKill();
        }

    }
}