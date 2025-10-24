using System.Collections;
using UnityEngine;

public partial class Monster
{
    [SerializeField] BoxCollider attackCollider;
    [SerializeField] AudioClip attackSound;

    [Header("Stat")]
    [SerializeField] public float damage;
    [SerializeField] public float attackDelay;
    
    public float initAttackDelay;
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool isAttack = false;

    [SerializeField] public int attackCount;
    int initAttackCount;
    public int InitAttackCount => initAttackCount;

    public void InitMonsterSetting(MonsterStat stat)
    {
        initAttackDelay = stat.attackDelay;
        attackDelay = stat.attackDelay;
        isAttack = false;
        canAttack = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Character"))
        {
            character.OnDamaged(damage, gameObject);
        }
    }

    void AttackSound()
    {
        soundManager.PlaySFX(attackSound);
    }

    public void AttackDelay()
    {
        if (isAttack || canAttack)
            return;

        attackDelay -= Time.deltaTime;

        if (attackDelay <= 0)
        {
            canAttack = true;
            attackDelay = initAttackDelay;
            attackCollider.enabled = true;
        }
    }
}
