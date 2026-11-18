using System;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("체력 설정")]
    [SerializeField, Min(1f)]
    private float maxHp = 100f;

    [Header("피격 이펙트")]
    [SerializeField] private HitFlashEffect hitFlashEffect;

    private float currentHp;
    private bool isDead;
    private bool isInvincible;

    public float MaxHp => maxHp;
    public float CurrentHp => currentHp;
    public bool IsDead => isDead;
    public bool IsInvincible => isInvincible;

    public float HealthRatio =>
        currentHp / maxHp;

    // 체력 변경 알림
    public event Action<float, float> OnHealthChanged;

    // 사망 알림
    public event Action OnDied;

    private void Awake()
    {
        currentHp = maxHp;
        isDead = false;

        if (hitFlashEffect == null)
            hitFlashEffect = GetComponent<HitFlashEffect>();
    }

    public void Attacked(float damage, GameObject hitObject)
    {
        TakeDamage(damage);
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        // 추후 보스 데미지 UI 연결
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;

        if (value)
            hitFlashEffect?.ResetFlash();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible || damage <= 0f)
            return;

        currentHp = Mathf.Max(currentHp - damage, 0f);

        hitFlashEffect?.PlayHitFlash();

        Debug.Log($"[{gameObject.name}] 보스 피격! " + $"현재 HP: {currentHp}/{maxHp}");

        OnHealthChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log($"[{name}] 보스 사망");

        OnDied?.Invoke();
    }
}
