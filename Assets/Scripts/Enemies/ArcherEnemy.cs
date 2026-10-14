using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArcherEnemy : MonoBehaviour
{
    private enum ArcherState
    {
        Move,
        Aiming,
        DaggerWindup,
        DaggerRecovery,
        Retreating,
        Dodging,
        Stunned,
        Dead
    }

    [Header("기본 능력치")]
    [SerializeField] private int hp = 8;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float fastRetreatSpeed = 6f;
    [SerializeField] private float targetShootingDistance = 5f;
    [SerializeField] private float shootingDistanceTolerance = 0.5f;
    [SerializeField] private float daggerDistance = 2.5f;

    [Header("활 공격")]
    [SerializeField] private ArcherArrow arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private float aimDuration = 0.5f;
    [SerializeField] private float shotInterval = 2f;
    [SerializeField] private float arrowSpeed = 12f;
    [SerializeField] private int arrowDamage = 2;

    [Header("단검 공격")]
    [SerializeField] private Vector3 daggerBoxSize = new Vector3(1.8f, 1f, 1.8f);
    [SerializeField] private float daggerForwardOffset = 1f;
    [SerializeField] private float daggerHitDuration = 0.15f;
    [SerializeField] private float daggerRecoveryDuration = 0.5f;
    [SerializeField] private float postDaggerRetreatDistance = 5f;
    [SerializeField] private int daggerDamage = 1;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool showDaggerDebug = true;

    [Header("단검 공격 경고")]
    [SerializeField] private float daggerWarningDuration = 0.4f;
    [SerializeField] private GameObject daggerWarningPrefab;
    [SerializeField] private Transform daggerWarningPoint;

    private Transform daggerEffectTransform;
    private Vector3 daggerEffectBaseLocalPosition;
    private Quaternion daggerEffectBaseLocalRotation;
    private bool daggerEffectInitialized;

    private GameObject activeDaggerWarning;

    [Header("회피")]
    [Range(0f, 1f)]
    [SerializeField] private float dodgeChance = 0.6f;
    [SerializeField] private float dodgeDistance = 3f;
    [SerializeField] private float dodgeDuration = 0.5f;
    [SerializeField] private float dodgeCooldown = 1.5f;

    [Header("기절")]
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private Color stunColor = Color.yellow;

    [Header("스프라이트 및 탐색")]
    [SerializeField] private SpriteRenderer childSpriteRenderer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleCheckDistance = 0.6f;
    [SerializeField] private float sideStepAngle = 55f;

    private Rigidbody rb;
    private Transform player;
    private Color originalColor = Color.white;
    private ArcherState state = ArcherState.Move;

    private Vector3 desiredMove;
    private Vector3 lastMoveDirection = Vector3.right;
    private Vector3 lastDaggerCenter;

    private bool isDead;
    private bool isInvincible;
    private bool isDodgeOnCooldown;
    private bool isStunned;

    private bool isDaggerWindowOpen;
    private bool hasDaggerHitPlayer;

    private float nextShotTime;
    private float dodgeCooldownEndsAt;

    private Coroutine behaviorCoroutine;
    private Coroutine colorCoroutine;
    private Coroutine daggerCoroutine;
    private Coroutine dodgeCooldownCoroutine;

    private Tween flashTween;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints |= RigidbodyConstraints.FreezePositionY;
        rb.constraints |= RigidbodyConstraints.FreezeRotation;

        if (daggerWarningPrefab != null)
        {
            daggerEffectTransform = daggerWarningPrefab.transform;

            daggerEffectBaseLocalPosition =
                daggerEffectTransform.localPosition;

            daggerEffectBaseLocalRotation =
                daggerEffectTransform.localRotation;

            daggerEffectInitialized = true;

            daggerWarningPrefab.SetActive(false);
        }

        if (childSpriteRenderer != null)
            originalColor = childSpriteRenderer.color;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Character");
        if (playerObject != null)
            player = playerObject.transform;

        nextShotTime = Time.time;
        behaviorCoroutine = StartCoroutine(BehaviorLoop());
    }

    private void FixedUpdate()
    {
        if (isDead || state == ArcherState.Stunned || state == ArcherState.Aiming ||
            state == ArcherState.DaggerWindup || state == ArcherState.DaggerRecovery)
        {
            desiredMove = Vector3.zero;
            return;
        }

        Vector3 move = desiredMove;
        move.y = 0f;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        if (move.sqrMagnitude > 0.0001f)
        {
            Vector3 step = move.normalized * GetCurrentMoveSpeed() * Time.fixedDeltaTime;
            if (!IsMovementBlocked(step))
            {
                rb.MovePosition(rb.position + step);
                lastMoveDirection = move.normalized;
            }
            else
            {
                // 장애물에 막히면 좌우 대체 방향을 시도
                Vector3 sideA = Quaternion.Euler(0f, sideStepAngle, 0f) * move.normalized;
                Vector3 sideB = Quaternion.Euler(0f, -sideStepAngle, 0f) * move.normalized;

                if (!IsMovementBlocked(sideA * obstacleCheckDistance))
                {
                    rb.MovePosition(rb.position + sideA * GetCurrentMoveSpeed() * Time.fixedDeltaTime);
                    lastMoveDirection = sideA;
                }
                else if (!IsMovementBlocked(sideB * obstacleCheckDistance))
                {
                    rb.MovePosition(rb.position + sideB * GetCurrentMoveSpeed() * Time.fixedDeltaTime);
                    lastMoveDirection = sideB;
                }
            }
        }
    }

    private float GetCurrentMoveSpeed()
    {
        return state == ArcherState.Retreating || state == ArcherState.Dodging
            ? fastRetreatSpeed
            : moveSpeed;
    }

    private IEnumerator BehaviorLoop()
    {
        while (!isDead)
        {
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Character");
                if (playerObject != null) player = playerObject.transform;
                desiredMove = Vector3.zero;
                yield return null;
                continue;
            }

            if (state == ArcherState.Move)
                EvaluateMovementAndAttack();

            yield return null;
        }
    }

    private void EvaluateMovementAndAttack()
    {
        Vector3 toPlayer = GetPlayerOffset();
        float distance = toPlayer.magnitude;

        if (toPlayer.sqrMagnitude > 0.0001f)
            FaceDirection(toPlayer);

        if (distance <= daggerDistance)
        {
            StartBehavior(
                DaggerAttackRoutine(),
                ArcherState.DaggerWindup
            );
            return;
        }

        float maxShootDistance =
            targetShootingDistance + shootingDistanceTolerance;

        if (distance > maxShootDistance)
        {
            desiredMove = toPlayer.normalized;
            return;
        }

        desiredMove = Vector3.zero;

        if (Time.time >= nextShotTime)
        {
            StartBehavior(
                AimAndShootRoutine(),
                ArcherState.Aiming
            );
        }
    }

    private IEnumerator AimAndShootRoutine()
    {
        state = ArcherState.Aiming;
        desiredMove = Vector3.zero;

        if (player != null)
            FaceDirection(GetPlayerOffset());

        yield return new WaitForSeconds(aimDuration);

        if (isDead || isStunned || player == null)
        {
            if (!isDead && !isStunned)
                ResumeBehavior();

            yield break;
        }

        if (GetPlayerOffset().magnitude <= daggerDistance)
        {
            ResumeBehavior();
            yield break;
        }

        Vector3 spawnPosition = arrowSpawnPoint != null
            ? arrowSpawnPoint.position
            : transform.position;

        Vector3 aimPoint = player.position;
        aimPoint.y = spawnPosition.y;

        Vector3 direction = aimPoint - spawnPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            direction = new Vector3(GetFacingSign(), 0f, 0f);

        direction.Normalize();

        if (arrowPrefab != null)
        {
            ArcherArrow arrow = Instantiate(
                arrowPrefab,
                spawnPosition,
                arrowPrefab.transform.rotation
            );

            // 비활성 프리팹이라면 활성화한 다음 초기화
            if (!arrow.gameObject.activeSelf)
                arrow.gameObject.SetActive(true);

            arrow.Initialize(
                direction,
                arrowSpeed,
                arrowDamage,
                gameObject,
                playerLayer
            );
        }
        else
        {
            Debug.LogWarning(
                $"[{gameObject.name}] Arrow Prefab이 지정되지 않았습니다."
            );
        }

        nextShotTime = Time.time + shotInterval;
        ResumeBehavior();
    }

    private IEnumerator DaggerAttackRoutine()
    {
        state = ArcherState.DaggerWindup;
        desiredMove = Vector3.zero;
        hasDaggerHitPlayer = false;

        Vector3 toPlayer = GetPlayerOffset();

        if (toPlayer.sqrMagnitude > 0.0001f)
            FaceDirection(toPlayer);

        // 1. 공격 경고 효과 표시
        Vector3 warningPosition = daggerWarningPoint != null
            ? daggerWarningPoint.position
            : transform.position;

        Color previousColor = originalColor;

        if (daggerWarningPrefab != null)
        {
            activeDaggerWarning = Instantiate(
                daggerWarningPrefab,
                warningPosition,
                Quaternion.identity
            );

            activeDaggerWarning.SetActive(true);
        }
        else if (childSpriteRenderer != null)
        {
            previousColor = childSpriteRenderer.color;
            childSpriteRenderer.color = Color.yellow;
        }

        // 2. 경고 시간 동안 대기
        yield return new WaitForSeconds(daggerWarningDuration);

        if (activeDaggerWarning != null)
        {
            Destroy(activeDaggerWarning);
            activeDaggerWarning = null;
        }

        if (childSpriteRenderer != null && !isStunned && !isDead)
            childSpriteRenderer.color = previousColor;

        if (isDead || isStunned)
            yield break;

        // 3. 실제 단검 공격 시작
        state = ArcherState.DaggerWindup;
        isDaggerWindowOpen = true;

        UpdateDaggerEffectDirection();

        if (daggerWarningPrefab != null)
            daggerWarningPrefab.SetActive(true);

        float elapsed = 0f;

        while (elapsed < daggerHitDuration && !isDead && !isStunned)
        {
            PerformDaggerHitCheck();

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. 실제 공격 종료 및 이펙트 비활성화
        isDaggerWindowOpen = false;

        if (daggerWarningPrefab != null)
            daggerWarningPrefab.SetActive(false);

        if (isDead || isStunned)
            yield break;

        // 5. 공격 후 회복
        state = ArcherState.DaggerRecovery;
        yield return new WaitForSeconds(daggerRecoveryDuration);

        if (isDead || isStunned)
            yield break;

        // 6. 공격 후 플레이어 반대 방향으로 후퇴
        Vector3 away = -GetPlayerOffset();
        away.y = 0f;

        if (away.sqrMagnitude < 0.0001f)
            away = new Vector3(-GetFacingSign(), 0f, 0f);

        away.Normalize();

        state = ArcherState.Retreating;

        float duration = postDaggerRetreatDistance
            / Mathf.Max(0.01f, fastRetreatSpeed);

        float t = 0f;

        while (t < duration && !isDead && !isStunned)
        {
            desiredMove = away;

            t += Time.deltaTime;
            yield return null;
        }

        desiredMove = Vector3.zero;

        if (!isDead && !isStunned)
            ResumeBehavior();
    }

    private void PerformDaggerHitCheck()
    {
        Vector3 center = transform.position;
        center.x += GetFacingSign() * daggerForwardOffset;
        lastDaggerCenter = center;

        Collider[] hits = Physics.OverlapBox(
            center,
            daggerBoxSize * 0.5f,
            Quaternion.identity,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform.root)
                continue;

            if (hasDaggerHitPlayer)
                break;

            hasDaggerHitPlayer = true;
            Debug.Log($"[{gameObject.name}] 단검 적중: {hit.name}, 예상 피해: {daggerDamage}");
            break;
        }
    }

    private void StartBehavior(IEnumerator routine, ArcherState initialState)
    {
        if (behaviorCoroutine != null)
        {
            StopCoroutine(behaviorCoroutine);
            behaviorCoroutine = null;
        }

        if (daggerCoroutine != null)
        {
            StopCoroutine(daggerCoroutine);
            daggerCoroutine = null;
        }

        // 상태를 먼저 변경한 뒤 행동 코루틴을 실행
        state = initialState;
        desiredMove = Vector3.zero;

        daggerCoroutine = StartCoroutine(routine);
    }

    private void ResumeBehavior()
    {
        if (isDead || isStunned || state == ArcherState.Dead)
            return;

        state = ArcherState.Move;

        if (behaviorCoroutine == null)
            behaviorCoroutine = StartCoroutine(BehaviorLoop());
    }

    private Vector3 GetPlayerOffset()
    {
        if (player == null) return Vector3.zero;
        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        return offset;
    }

    private void FaceDirection(Vector3 direction)
    {
        if (childSpriteRenderer == null || Mathf.Abs(direction.x) < 0.01f)
            return;

        childSpriteRenderer.flipX = direction.x < 0f;
    }

    private void UpdateDaggerEffectDirection()
    {
        if (!daggerEffectInitialized ||
            daggerEffectTransform == null)
            return;

        bool facingLeft =
            childSpriteRenderer != null &&
            childSpriteRenderer.flipX;

        // 궁수가 바라보는 방향에 맞춰 효과의 위치를 좌우 반전
        Vector3 localPosition = daggerEffectBaseLocalPosition;
        localPosition.x =
            Mathf.Abs(daggerEffectBaseLocalPosition.x) *
            (facingLeft ? -1f : 1f);

        daggerEffectTransform.localPosition = localPosition;

        // 효과가 향하는 방향도 함께 반전
        Quaternion directionRotation = facingLeft
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.identity;

        daggerEffectTransform.localRotation =
            directionRotation * daggerEffectBaseLocalRotation;
    }

    private float GetFacingSign()
    {
        if (childSpriteRenderer == null) return 1f;
        return childSpriteRenderer.flipX ? -1f : 1f;
    }

    private bool IsMovementBlocked(Vector3 displacement)
    {
        if (obstacleLayer.value == 0 || displacement.sqrMagnitude < 0.0001f)
            return false;

        Collider ownCollider = GetComponent<Collider>();
        Vector3 origin = ownCollider != null ? ownCollider.bounds.center : transform.position;
        float distance = displacement.magnitude;
        Vector3 direction = displacement.normalized;
        float radius = ownCollider != null
            ? Mathf.Max(0.1f, Mathf.Min(ownCollider.bounds.extents.x, ownCollider.bounds.extents.z) * 0.8f)
            : 0.3f;

        return Physics.SphereCast(
            origin,
            radius,
            direction,
            out _,
            Mathf.Min(distance, obstacleCheckDistance),
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    /// <summary>
    /// PlayerAttack에서 호출, 회피 성공하면 공격 피해 무시.
    /// </summary>
    public bool TryDodgeFromPlayerAttack()
    {
        if (isDead || isStunned ||
            state == ArcherState.Dodging ||
            isDodgeOnCooldown)
        {
            return false;
        }

        float roll = Random.value;

        if (roll >= dodgeChance)
        {
            return false;
        }

        InterruptCurrentBehavior();

        daggerCoroutine = StartCoroutine(DodgeRoutine());
        return true;
    }

    private IEnumerator DodgeRoutine()
    {
        state = ArcherState.Dodging;
        isInvincible = true;
        desiredMove = Vector3.zero;

        Vector3 away = -GetPlayerOffset();
        away.y = 0f;

        if (away.sqrMagnitude < 0.0001f)
            away = new Vector3(-GetFacingSign(), 0f, 0f);

        away.Normalize();

        float elapsed = 0f;

        while (elapsed < dodgeDuration && !isDead && !isStunned)
        {
            desiredMove = away;
            elapsed += Time.deltaTime;
            yield return null;
        }

        desiredMove = Vector3.zero;
        isInvincible = false;

        if (isDead || isStunned)
            yield break;

        isDodgeOnCooldown = true;
        dodgeCooldownEndsAt = Time.time + dodgeCooldown;

        if (dodgeCooldownCoroutine != null)
            StopCoroutine(dodgeCooldownCoroutine);

        dodgeCooldownCoroutine = StartCoroutine(DodgeCooldownRoutine());

        // 회피 직후에는 전투 행동을 재개
        ResumeBehavior();
    }

    private IEnumerator DodgeCooldownRoutine()
    {
        while (Time.time < dodgeCooldownEndsAt && !isDead)
        {
            yield return null;
        }

        isDodgeOnCooldown = false;
        dodgeCooldownCoroutine = null;

        Debug.Log($"[{gameObject.name}] 회피 쿨다운 종료");
    }

    private void InterruptCurrentBehavior()
    {
        if (behaviorCoroutine != null)
        {
            StopCoroutine(behaviorCoroutine);
            behaviorCoroutine = null;
        }

        if (daggerCoroutine != null)
        {
            StopCoroutine(daggerCoroutine);
            daggerCoroutine = null;
        }

        if (activeDaggerWarning != null)
        {
            Destroy(activeDaggerWarning);
            activeDaggerWarning = null;
        }

        if (daggerWarningPrefab != null)
        {
            daggerWarningPrefab.SetActive(false);
        }

        isDaggerWindowOpen = false;

        StopCoroutineIfRunning(colorCoroutine);
        colorCoroutine = null;

        flashTween?.Kill();
        flashTween = null;

        desiredMove = Vector3.zero;
        isDaggerWindowOpen = false;
    }

    private void StopCoroutineIfRunning(Coroutine coroutine)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
            return;

        hp -= damage;
        Debug.Log($"[{gameObject.name}] 피격! 남은 HP: {hp}");

        if (childSpriteRenderer != null)
        {
            flashTween?.Kill();
            childSpriteRenderer.color = Color.red;
            flashTween = childSpriteRenderer.DOColor(originalColor, 0.2f);
        }

        if (hp <= 0)
            Die();
    }

    public void ApplyReflectedArrowStun()
    {
        if (isDead || isStunned)
            return;

        InterruptCurrentBehavior();
        isStunned = true;
        isInvincible = false;
        state = ArcherState.Stunned;
        desiredMove = Vector3.zero;

        if (childSpriteRenderer != null)
        {
            childSpriteRenderer.color = stunColor;
        }

        StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        yield return new WaitForSeconds(stunDuration);

        if (isDead) yield break;

        isStunned = false;
        if (childSpriteRenderer != null)
            childSpriteRenderer.color = originalColor;

        ResumeBehavior();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        state = ArcherState.Dead;

        InterruptCurrentBehavior();
        StopAllCoroutines();

        flashTween?.Kill();
        transform.DOKill();

        if (childSpriteRenderer != null)
        {
            childSpriteRenderer.DOKill();
            childSpriteRenderer.color = originalColor;
        }

        Debug.Log("[옛 신앙 궁수] 사망했습니다.");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float sign = childSpriteRenderer != null && childSpriteRenderer.flipX ? -1f : 1f;
        Vector3 center = transform.position + new Vector3(sign * daggerForwardOffset, 0f, 0f);
        Gizmos.DrawWireCube(center, daggerBoxSize);
    }
}
