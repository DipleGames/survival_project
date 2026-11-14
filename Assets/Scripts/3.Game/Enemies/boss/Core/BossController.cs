using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    Chase,
    Attack,
    Recovery,
    Transforming,
    Dead
}

public class BossController : MonoBehaviour
{
    [Header("페이즈 설정")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private string transformAnimationName = "Transform";

    [SerializeField]
    private List<BossPhaseData> phases = new List<BossPhaseData>();

    private BossPhaseData currentPhaseData;
    private int currentPhaseIndex = 0;
    private bool isTransforming;

    [Header("패턴 실행 설정")]
    [SerializeField, Min(0f)]
    private float patternInterval = 0.5f;

    [Header("참조 설정")]
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Rigidbody bossRigidbody;

    [SerializeField]
    private Animator bossAnimator;

    [SerializeField]
    private BossMovement bossMovement;

    [SerializeField]
    private BossTelegraphMotion telegraphMotion;

    [Header("공격 시작 조건")]
    [SerializeField, Min(0f)]
    private float attackStartDistance = 4f;

    private BossContext context;
    private BossPatternRuntime patternRuntime;
    private Coroutine bossCoroutine;
    private Coroutine patternCoroutine;
    private BossPattern currentPattern;

    public bool IsAttacking { get; private set; }
    public BossState CurrentState { get; private set; } = BossState.Chase;

    private void Awake()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<BossHealth>();

        if (bossRigidbody == null)
            bossRigidbody = GetComponent<Rigidbody>();

        if (bossAnimator == null)
            bossAnimator = GetComponentInChildren<Animator>();

        if (bossMovement == null)
            bossMovement = GetComponent<BossMovement>();

        if (telegraphMotion == null)
            telegraphMotion = GetComponent<BossTelegraphMotion>();

        patternRuntime = new BossPatternRuntime();
    }

    private void OnEnable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged += HandleHealthChanged;
            bossHealth.OnDied += HandleBossDied;
        }
    }

    private void HandleBossDied()
    {
        if (CurrentState == BossState.Dead)
            return;

        CurrentState = BossState.Dead;

        if (bossCoroutine != null)
        {
            StopCoroutine(bossCoroutine);
            bossCoroutine = null;
        }

        InterruptCurrentAttack();
        context?.ClearSpikeWarning();
        isTransforming = false;

        Debug.Log($"[{name}] 보스 사망 - 전투 종료");

        Destroy(gameObject);
    }

    private void HandleHealthChanged(float currentHp, float maxHp)
    {
        if (isTransforming || maxHp <= 0f)
            return;

        int nextPhaseIndex = currentPhaseIndex + 1;

        if (nextPhaseIndex >= phases.Count)
            return;

        BossPhaseData nextPhase = phases[nextPhaseIndex];

        if (nextPhase == null)
            return;

        if (currentHp <= 0f)
            return;

        float healthRatio = currentHp / maxHp;

        if (healthRatio <= nextPhase.EnterHealthRatio)
        {
            StartPhaseTransition();
        }
    }

    private void StartPhaseTransition()
    {
        if (isTransforming)
            return;

        isTransforming = true;

        // 기존 전투 루프 중단
        if (bossCoroutine != null)
        {
            StopCoroutine(bossCoroutine);
            bossCoroutine = null;
        }

        // 실행 중인 공격 및 이펙트 정리
        InterruptCurrentAttack();

        CurrentState = BossState.Transforming;
        bossHealth?.SetInvincible(true);

        if (bossAnimator != null)
        {
            bossAnimator.CrossFade(transformAnimationName, 0.1f);
        }

        Debug.Log($"[{name}] 페이즈 전환 시작");
    }

    public void OnTransformAnimationFinished()
    {
        if (!isTransforming)
            return;

        int nextPhaseIndex = currentPhaseIndex + 1;

        if (nextPhaseIndex >= phases.Count)
            return;

        BossPhaseData nextPhase = phases[nextPhaseIndex];

        if (nextPhase == null)
            return;

        // 다음 페이즈 적용
        currentPhaseIndex = nextPhaseIndex;
        currentPhaseData = nextPhase;

        // 변신 상태 종료
        isTransforming = false;
        bossHealth?.SetInvincible(false);

        if (bossAnimator != null && currentPhaseData.AnimatorController != null)
        {
            bossAnimator.runtimeAnimatorController = currentPhaseData.AnimatorController;

            bossAnimator.Play("Base Layer.Idle", 0, 0f);
        }

        CurrentState = BossState.Chase;
        IsAttacking = false;
        bossMovement?.ResumeMovement();

        if (bossCoroutine == null)
            bossCoroutine = StartCoroutine(BossLoop());

        Debug.Log($"[{name}] 페이즈 전환 완료: {currentPhaseData.phaseName}");
    }

    private void InterruptCurrentAttack()
    {
        // 현재 공격 코루틴 중단
        if (patternCoroutine != null)
        {
            StopCoroutine(patternCoroutine);
            patternCoroutine = null;
        }

        // 현재 패턴의 이펙트 정리
        currentPattern?.OnInterrupted(context);

        telegraphMotion?.ResetMotion();
        bossMovement?.StopMovement();

        IsAttacking = false;
        currentPattern = null;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Character");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player == null)
        {
            Debug.LogWarning($"[{name}] 플레이어 참조가 없습니다.");
            return;
        }

        context = new BossContext(
            transform,
            player,
            bossRigidbody,
            bossAnimator,
            GetComponent<BossDamageHitbox>(),
            GetComponentInChildren<ChargeWarningVFX>(true),
            telegraphMotion,
            GetComponentInChildren<HoofSlamWarningVFX>(true),
            GetComponentInChildren<HoofSlamShockwaveVFX>(true),
            GetComponentInChildren<ExecutionWarningVFX>(true),
            GetComponentInChildren<ExecutionSlashVFX>(true)
        );

        if (bossMovement != null)
            bossMovement.Initialize(player, bossRigidbody, bossAnimator);

        if (phases.Count == 0 || phases[0] == null)
        {
            Debug.LogError($"[{name}] 시작 페이즈가 설정되지 않았습니다.");
            return;
        }

        currentPhaseData = phases[0];

        Debug.Log($"[{name}] 시작 페이즈: {currentPhaseData.phaseName}");

        bossCoroutine = StartCoroutine(BossLoop());
    }

    private IEnumerator BossLoop()
    {
        while (true)
        {
            if (player == null)
            {
                yield return null;
                continue;
            }

            Vector3 offset = player.position - transform.position;
            offset.y = 0f;

            // 공격 가능 거리까지 기본 이동 유지
            if (offset.sqrMagnitude > attackStartDistance * attackStartDistance)
            {
                yield return null;
                continue;
            }

            BossPattern selectedPattern = SelectPattern();
            if (selectedPattern == null)
            {
                yield return null;
                continue;
            }

            currentPattern = selectedPattern;
            CurrentState = BossState.Attack;
            IsAttacking = true;

            // 공격 시작 시 기본 이동 중단
            bossMovement?.StopMovement();

            patternRuntime.StartCooldown(currentPattern);

            patternCoroutine = StartCoroutine(currentPattern.Execute(context));

            yield return patternCoroutine;

            patternCoroutine = null;

            IsAttacking = false;
            CurrentState = BossState.Recovery;

            // Recovery 시작
            telegraphMotion?.PlayRecovery();

            float recoveryTime = currentPattern.recoveryDuration;

            // 복귀 애니메이션에 사용할 시간
            float exitTime = Mathf.Min(
                recoveryTime,
                telegraphMotion != null
                    ? telegraphMotion.RecoveryExitDuration
                    : 0f
            );

            // Recovery 자세 유지
            yield return new WaitForSeconds(Mathf.Max(0f, recoveryTime - exitTime));

            // 원래 자세로 복귀
            telegraphMotion?.EndRecovery();

            // 복귀 모션이 완료될 때까지 대기
            if (exitTime > 0f)
                yield return new WaitForSeconds(exitTime);

            currentPattern = null;
            CurrentState = BossState.Chase;

            // Recovery 종료 후 기본 이동 재개
            bossMovement?.ResumeMovement();

            yield return new WaitForSeconds(patternInterval);
        }
    }

    private BossPattern SelectPattern()
    {
        if (currentPhaseData == null)
            return null;

        List<BossPattern> availablePatterns = new List<BossPattern>();
        float totalWeight = 0f;

        foreach (BossPattern pattern in currentPhaseData.Patterns)
        {
            if (pattern == null)
                continue;
            if (!patternRuntime.IsReady(pattern))
                continue;
            if (!pattern.CanExecute(context))
                continue;
            if (pattern.selectionWeight <= 0f)
                continue;

            availablePatterns.Add(pattern);
            totalWeight += pattern.selectionWeight;
        }

        if (availablePatterns.Count == 0)
            return null;

        float randomWeight = Random.Range(0f, totalWeight);
        
        foreach (BossPattern pattern in availablePatterns)
        {
            randomWeight -= pattern.selectionWeight;
            if (randomWeight <= 0f)
                return pattern;
        }

        return availablePatterns[availablePatterns.Count - 1];
    }

    private void OnDisable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= HandleHealthChanged;
            bossHealth.OnDied -= HandleBossDied;
        }

        if (bossCoroutine != null)
        {
            StopCoroutine(bossCoroutine);
            bossCoroutine = null;
        }

        InterruptCurrentAttack();

        context?.ClearSpikeWarning();

        patternRuntime?.ResetCooldowns();
        telegraphMotion?.ResetMotion();

        // 사망 상태는 유지
        if (CurrentState != BossState.Dead)
        {
            CurrentState = BossState.Chase;
        }
    }
}
