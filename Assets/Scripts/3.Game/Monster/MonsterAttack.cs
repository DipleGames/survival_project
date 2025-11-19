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

    void Attack()
    {
        StartAttack();
        EndAttack();
        TickAttackDelay();
    }

    void StartAttack()
    {
        if (isDead || !canAttack)
            return;

        if (FocusObject == MonsterFocusObject.Player)
        {
            xDistance = Mathf.Abs(character.transform.position.x - transform.position.x);
            zDistance = Mathf.Abs(character.transform.position.z - transform.position.z);
        }

        if (!isAttack && !gameManager.isClear)
        {
            //if (xDistance <= attackRange.x && zDistance <= attackRange.y)
            if (xDistance <= attackRange.x + 0.45f && zDistance <= attackRange.y + 0.45f)
            {
                isAttack = true;
                agent.enabled = false;
                canMove = false;
                canAttack = false;
            }

            anim.SetBool("isAttack", isAttack);
        }
    }

    void EndAttack()
    {
        if (!isAttack)
            return;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                isAttack = false;
                anim.SetBool("isAttack", isAttack);

                attackCollider.enabled = false;
            }
        }
    }

    void AttackSound()
    {
        soundManager.PlaySFX(attackSound);
    }

    public void TickAttackDelay()
    {
        if (isAttack || canAttack || isStun)
            return;

        attackDelay -= Time.deltaTime;

        if (attackDelay <= 0)
        {
            canAttack = true;
            attackDelay = initAttackDelay;
        }
    }
}
