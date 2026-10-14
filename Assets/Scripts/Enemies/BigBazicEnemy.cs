using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BigBazicEnemy : MonoBehaviour
{
    public enum EnemyState
    {
        Move,
        Telegraphing,
        Dashing,
        Dodging,
        Cooldown
    }

    private EnemyState currentState = EnemyState.Move;

    [Header("기본 능력치")]
    public float moveSpeed = 3.0f;
    public int hp = 8;

    [Header("공격 및 아종 설정")]
    public bool isVariant = false;
    public float attackRange = 4f;
    public float telegraphDuration = 0.5f;
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float cooldownTime = 1.5f;

    [Header("베기 판정 설정 (XZ 평면)")]
    public Vector3 attackBoxSize = new Vector3(1.8f, 1f, 1.8f);
    public LayerMask playerLayer;

    [SerializeField] private float slashForwardOffset = 1f;
    [SerializeField] private float slashDuration = 0.3f;

    [Header("베기 이펙트")]
    public Transform slashIndicator;

    [Header("구르기 회피 설정")]
    [Range(0f, 1f)]
    public float dodgeChance = 0.6f;

    public float dodgeCooldown = 3.0f;
    public float dodgeDistance = 2.5f;
    public float dodgeDuration = 0.25f;

    [Header("스프라이트")]
    public SpriteRenderer childSpriteRenderer;

    private Transform player;
    private Color originalColor;

    private bool isAttackCooldown = false;
    private bool isDodgeCooldown = false;
    private bool isInvincible = false;

    private int comboCount = 0;

    // 현재 공격에서 사용할 방향을 저장한다.
    private Vector3 lockedAttackDirection = Vector3.right;

    // 모든 이동 및 공격 코루틴의 진입점
    private Coroutine aiCoroutine;

    private void Start()
    {
        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Character");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (childSpriteRenderer != null)
        {
            originalColor = childSpriteRenderer.color;
        }

        if (slashIndicator != null)
        {
            slashIndicator.gameObject.SetActive(false);
        }

        aiCoroutine = StartCoroutine(AIExecutionLoop());
    }

    // =========================================================
    // AI 상태 루프
    // =========================================================

    private IEnumerator AIExecutionLoop()
    {
        while (true)
        {
            if (player == null)
            {
                yield return null;
                continue;
            }

            float distanceToPlayer =
                Vector3.Distance(transform.position, player.position);

            switch (currentState)
            {
                case EnemyState.Move:
                    HandleMovement(distanceToPlayer);
                    break;

                case EnemyState.Telegraphing:
                case EnemyState.Dashing:
                case EnemyState.Dodging:
                case EnemyState.Cooldown:
                    // 해당 상태는 별도 코루틴에서 처리한다.
                    break;
            }

            yield return null;
        }
    }

    // =========================================================
    // 방향 처리
    // =========================================================

    private void FaceDirection(Vector3 direction)
    {
        if (childSpriteRenderer == null)
            return;

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            bool faceRight = direction.x > 0f;

            childSpriteRenderer.flipX = faceRight;
        }
    }

    private float GetFacingSign()
    {
        if (childSpriteRenderer == null)
            return -1f;

        return childSpriteRenderer.flipX ? 1f : -1f;
    }

    // =========================================================
    // 플레이어 추적
    // =========================================================

    private void HandleMovement(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange && !isAttackCooldown)
        {
            comboCount = isVariant ? 2 : 1;

            StartCoroutine(AttackSequence());
            return;
        }

        Vector3 direction =
            player.position - transform.position;

        // 3D 월드의 XZ 평면으로 제한
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        FaceDirection(direction);

        transform.position +=
            direction * moveSpeed * Time.deltaTime;
    }

    // =========================================================
    // 공격 시퀀스
    // =========================================================

    private IEnumerator AttackSequence()
    {
        // 공격 시작 시 방향을 결정하고 고정한다.
        lockedAttackDirection =
            player.position - transform.position;

        lockedAttackDirection.y = 0f;

        if (lockedAttackDirection.sqrMagnitude < 0.001f)
        {
            lockedAttackDirection =
                new Vector3(GetFacingSign(), 0f, 0f);
        }

        lockedAttackDirection.Normalize();

        FaceDirection(lockedAttackDirection);

        while (comboCount > 0)
        {
            if (player != null)
            {
                Vector3 directionToPlayer =
                    player.position - transform.position;

                // XZ 평면 기준
                directionToPlayer.y = 0f;

                if (directionToPlayer.sqrMagnitude > 0.001f)
                {
                    lockedAttackDirection = directionToPlayer.normalized;
                }
            }

            // 이번 공격 방향으로 몸체 방향 갱신
            FaceDirection(lockedAttackDirection);


            currentState = EnemyState.Telegraphing;

            Vector3 dashDirection = lockedAttackDirection;

            // -----------------------------
            // 공격 전조
            // -----------------------------

            Vector3 originalScale = transform.localScale;

            if (childSpriteRenderer != null)
            {
                childSpriteRenderer.DOKill();

                childSpriteRenderer.color = originalColor;

                childSpriteRenderer
                    .DOColor(Color.red, telegraphDuration)
                    .SetEase(Ease.InQuad);
            }

            Vector3 pullBackPos =
                transform.position - dashDirection * 0.4f;

            Vector3 squashScale = new Vector3(
                originalScale.x * 1.2f,
                originalScale.y * 0.8f,
                originalScale.z
            );

            Tween pullBackTween = transform
                .DOMove(pullBackPos, telegraphDuration)
                .SetEase(Ease.OutQuad);

            Tween squashTween = transform
                .DOScale(squashScale, telegraphDuration)
                .SetEase(Ease.OutQuad);

            yield return pullBackTween.WaitForCompletion();
            yield return squashTween.WaitForCompletion();

            if (childSpriteRenderer != null)
            {
                childSpriteRenderer.color = originalColor;
            }

            transform.localScale = originalScale;

            // -----------------------------
            // 돌진
            // -----------------------------

            currentState = EnemyState.Dashing;

            Vector3 targetDashPos =
                transform.position +
                dashDirection * dashDistance;

            Tween dashTween = transform
                .DOMove(targetDashPos, dashDuration)
                .SetEase(Ease.OutQuad);

            yield return dashTween.WaitForCompletion();

            // 돌진 후 잠깐 멈춘다.
            yield return new WaitForSeconds(0.2f);

            // -----------------------------
            // 가로형 베기
            // -----------------------------

            ExecuteSlashAttack(dashDirection);

            comboCount--;

            if (comboCount > 0)
            {
                yield return new WaitForSeconds(0.25f);
            }
        }

        StartCoroutine(AttackCooldownRoutine());
    }

    // =========================================================
    // 가로형 베기 및 OverlapBox 판정
    // =========================================================

    private void ExecuteSlashAttack(Vector3 attackDir)
    {
        // 가로형 베기는 X축 기준으로 좌우 방향만 사용한다.
        // 돌진 방향이 Z축에 가까우면 마지막 좌우 방향을 유지한다.
        float horizontalSign;

        if (Mathf.Abs(attackDir.x) > 0.01f)
        {
            horizontalSign = attackDir.x < 0f ? -1f : 1f;
        }
        else
        {
            horizontalSign = GetFacingSign();
        }

        // 몸체도 동일한 좌우 방향을 바라보게 한다.
        FaceDirection(new Vector3(horizontalSign, 0f, 0f));

        // 이펙트와 공격 판정이 공유하는 중심
        Vector3 attackCenter = transform.position;

        attackCenter.x += horizontalSign * Mathf.Abs(slashForwardOffset);

        // -----------------------------
        // 베기 이펙트
        // -----------------------------

        if (slashIndicator != null)
        {
            slashIndicator.DOKill();

            slashIndicator.position = attackCenter;

            SpriteRenderer indicatorSR =
                slashIndicator.GetComponent<SpriteRenderer>();

            if (indicatorSR != null)
            {
                indicatorSR.DOKill();

                indicatorSR.flipX = horizontalSign > 0f;

                Color effectColor = indicatorSR.color;
                effectColor.a = 1f;
                indicatorSR.color = effectColor;

                slashIndicator.gameObject.SetActive(true);

                indicatorSR
                    .DOFade(0f, slashDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        if (slashIndicator != null)
                        {
                            slashIndicator.gameObject.SetActive(false);
                        }
                    });
            }
            else
            {
                slashIndicator.gameObject.SetActive(true);

                DOVirtual.DelayedCall(slashDuration, () =>
                {
                    if (slashIndicator != null)
                    {
                        slashIndicator.gameObject.SetActive(false);
                    }
                });
            }
        }

        // -----------------------------
        // 실제 공격 판정
        // -----------------------------

        Collider[] hitPlayers = Physics.OverlapBox(
            attackCenter,
            attackBoxSize * 0.5f,
            Quaternion.identity,
            playerLayer
        );

        foreach (Collider playerCollider in hitPlayers)
        {
            Debug.Log(
                $"[{gameObject.name}] 베기 적중: " +
                $"{playerCollider.name}"
            );

            // 실제 플레이어 체력 감소 함수가 있다면
            // 이곳에서 호출한다.
        }
    }

    // =========================================================
    // 공격 후 쿨타임
    // =========================================================

    private IEnumerator AttackCooldownRoutine()
    {
        currentState = EnemyState.Cooldown;
        isAttackCooldown = true;

        yield return new WaitForSeconds(cooldownTime);

        isAttackCooldown = false;
        currentState = EnemyState.Move;
    }

    // =========================================================
    // 플레이어 공격 감지 및 구르기 회피
    // =========================================================

    public bool TryDodgeFromPlayerAttack()
    {
        // 이미 회피 중이면 추가 회피 불가
        if (currentState == EnemyState.Dodging || isDodgeCooldown)
        {
            return false;
        }

        // 회피 확률 판정
        if (Random.value >= dodgeChance)
        {
            return false;
        }

        // 기존 공격 및 AI 코루틴 중단
        StopAllCoroutines();
        aiCoroutine = null;

        isAttackCooldown = false;
        comboCount = 0;

        // 진행 중인 DOTween 이동 중단
        transform.DOKill();

        if (childSpriteRenderer != null)
        {
            childSpriteRenderer.DOKill();
            childSpriteRenderer.color = originalColor;

            childSpriteRenderer.transform.DOKill();
            childSpriteRenderer.transform.localRotation = Quaternion.identity;
        }

        // 진행 중인 베기 이펙트 정리
        if (slashIndicator != null)
        {
            slashIndicator.DOKill();

            SpriteRenderer indicatorSR =
                slashIndicator.GetComponent<SpriteRenderer>();

            if (indicatorSR != null)
            {
                indicatorSR.DOKill();
            }

            slashIndicator.gameObject.SetActive(false);
        }

        // 회피 시작
        StartCoroutine(DodgeRoutine());

        return true;
    }

    private IEnumerator DodgeRoutine()
    {
        currentState = EnemyState.Dodging;
        isInvincible = true;
        isDodgeCooldown = true;

        if (player == null)
        {
            isInvincible = false;
            isDodgeCooldown = false;
            currentState = EnemyState.Move;

            StartAICoroutine();
            yield break;
        }

        Vector3 awayFromPlayer =
            transform.position - player.position;

        awayFromPlayer.y = 0f;

        if (awayFromPlayer.sqrMagnitude < 0.001f)
        {
            awayFromPlayer = new Vector3(-GetFacingSign(), 0f, 0f);
        }

        awayFromPlayer.Normalize();

        Vector3 dodgeTarget = transform.position + awayFromPlayer * dodgeDistance;

        bool dodgeCompleted = false;
        bool dodgeInterrupted = false;

        Tween dodgeTween = transform
        .DOMove(dodgeTarget, dodgeDuration)
        .SetEase(Ease.OutSine)
        .OnComplete(() => dodgeCompleted = true)
        .OnKill(() =>
        {
            if (!dodgeCompleted)
                dodgeInterrupted = true;
        });

        while (!dodgeCompleted && !dodgeInterrupted)
        {
            yield return null;
        }

        isInvincible = false;
        currentState = EnemyState.Move;

        // 회피가 끝나면 AI 재시작
        StartAICoroutine();

        // 회피 쿨타임 유지
        yield return new WaitForSeconds(dodgeCooldown);

        isDodgeCooldown = false;
    }

    private void StartAICoroutine()
    {
        if (aiCoroutine == null)
        {
            aiCoroutine = StartCoroutine(AIExecutionLoop());
        }
    }

    // =========================================================
    // 피격 처리
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (isInvincible)
        {
            Debug.Log(
                $"[{gameObject.name}] 구르기로 공격을 회피했습니다!"
            );

            return;
        }

        hp -= damage;

        Debug.Log(
            $"[{gameObject.name}] 피격! 남은 HP: {hp}"
        );

        if (childSpriteRenderer != null)
        {
            childSpriteRenderer.DOKill();

            childSpriteRenderer.color = Color.red;

            childSpriteRenderer
                .DOColor(originalColor, 0.2f);
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    // =========================================================
    // 사망 처리
    // =========================================================

    private void Die()
    {
        StopAllCoroutines();

        transform.DOKill();

        if (childSpriteRenderer != null)
        {
            childSpriteRenderer.DOKill();
            childSpriteRenderer.transform.DOKill();
        }

        if (slashIndicator != null)
        {
            slashIndicator.DOKill();

            SpriteRenderer indicatorSR =
                slashIndicator.GetComponent<SpriteRenderer>();

            if (indicatorSR != null)
            {
                indicatorSR.DOKill();
            }
        }

        Destroy(gameObject);
    }

    // =========================================================
    // 공격 범위 기즈모
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float horizontalSign = 1f;

        if (childSpriteRenderer != null &&
            childSpriteRenderer.flipX)
        {
            horizontalSign = -1f;
        }

        Vector3 attackCenter = transform.position +
            new Vector3(
                horizontalSign * slashForwardOffset,
                0f,
                0f
            );

        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.DrawWireCube(
            attackCenter,
            attackBoxSize
        );
    }
}
