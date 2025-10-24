using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public partial class Monster : MonoBehaviour, IDamageable
{
    [SerializeField] SpriteRenderer rend;
    [SerializeField] Animator anim;
    [SerializeField] BoxCollider hitCollider;
    
    [SerializeField] Vector2 attackRange;
    [SerializeField] AudioClip damagedSound;

    [HideInInspector] public bool isDead = false;
    public bool IsDead => isDead;
    public bool isStun = false;

    public float hp;
    public float maxHp;

    

    [HideInInspector] protected Vector3 initScale;
    [HideInInspector] public MonsterStat stat;

    int itemDropPercent;

    private IObjectPool<Monster> managedPool;

    GameManager gameManager;
    Character character;
    GamesceneManager gamesceneManager;
    SoundManager soundManager;

    IEnumerator runningCoroutine;

    Color initcolor;

    public float Speed => moveSpeed;

    float xDistance;
    float zDistance;

    float initMoveDelay;
    //public bool canMove = true;

    int initOrder;

    // public bool CanMove => canMove;

    public int monsterNum;

    public bool isblowed = false;

    [SerializeField] MonsterOutline monsterOutline;

    private void Awake()
    {
        gameManager = GameManager.Instance;
        gamesceneManager = GamesceneManager.Instance;
        character = Character.Instance;
        soundManager = SoundManager.Instance;
        damageUIManager = DamageUIManager.Instance;
        
        monsterOutline = GetComponent<MonsterOutline>();

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        initScaleX = transform.localScale.x;
        initColliderX = hitCollider.size.x;

        if (attackCollider != null)
            initWeaponColliderX = attackCollider.size.x;

        /*initMoveTime = moveTime;
        initWaitTime = waitTime;*/

        moveTime = initMoveTime;
        waitTime = initWaitTime;

        playerPos = character.transform.position;
        destination = playerPos;
        FocusObject = MonsterFocusObject.Player;

        impactRadius = 1f;
        flyDelay = 0f;

        StartSetting();
    }

    private void OnDisable()
    {
        canAttack = true;
        agent.enabled = true;
        StopAllCoroutines();
    }

    protected void StartSetting()
    {
        initScale = transform.localScale;
        initcolor = rend.color;
        initOrder = rend.sortingOrder;
    }

    void Update()
    {
        if (!gamesceneManager.isNight || gameManager.isClear)
        {
            OnDead();
        }

        if(isDead)
        {
            CheckDieAnimation();
        }

        Attack();
        AttackEnd();
        AttackDelay();
        MoveDelay();
        monsterMove();
    }

    public void InitMonsterSetting(bool isLeader)
    {
        /*hp = stat.monsterMaxHp * (2 + Mathf.Floor(gameManager.round / 5) * Mathf.Floor(gameManager.round / 5) * (1 + Mathf.Floor(gameManager.round / 20) * 0.5f)) * 0.5f;
        damage = stat.monsterDamage * (1 + Mathf.Floor(gameManager.round / 30)) + Mathf.Floor(gameManager.round / 5) * 2f;*/

        hp = stat.monsterMaxHp;
        maxHp = hp;
        damage = stat.monsterDamage;
        isDead = false;
        
        moveSpeed = stat.monsterSpeed;
        initSpeed = moveSpeed;
        moveDelay = stat.moveDelay;
        initMoveDelay = moveDelay;
        itemDropPercent = stat.itemDropPercent;
        canMove = true;

        initAttackCount = stat.attackCount;
        attackCount = initAttackCount;

        anim.speed = 1f;
        transform.localScale = initScale;
        rend.color = initcolor;
        rend.sortingOrder = initOrder;

        InitMonsterSetting(stat);
        InitSetting(moveSpeed);

        hitCollider.enabled = true;
    }

    protected IEnumerator MonsterColorBlink()
    {
        Color semiWhite = initcolor;
        semiWhite.a = 0.5f;

        for (int i = 0; i < 3; i++)
        {
            rend.color = initcolor;
            yield return CoroutineCaching.WaitForSeconds(0.1f);

            rend.color = semiWhite;
            yield return CoroutineCaching.WaitForSeconds(0.1f);
        }

        rend.color = initcolor;
    }   

    void Attack()
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
            if (xDistance <= attackRange.x + 0.5f && zDistance <= attackRange.y+0.5f)
            {
                isAttack = true;
                agent.enabled = false;
                canMove = false;
                canAttack = false;
            }
        }

        anim.SetBool("isAttack", isAttack);
    }

    void AttackEnd()
    {
        if (!isAttack)
            return;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                isAttack = false;
                anim.SetBool("isAttack", isAttack);
            }
        }
    }

    void MoveDelay()
    {
        if (canMove || isAttack)
            return;

        moveDelay -= Time.deltaTime;

        if(moveDelay <= 0)
        {
            InitailizeCoolTime();
            agent.enabled = true;
            canMove = true;
            moveDelay = initMoveDelay;
        }
    }

    public void OnDamaged(float damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            soundManager.PlaySFX(damagedSound);
            OnDead();

            if(monsterOutline != null)
                monsterOutline.SetOutLine(false);
        }

        else
        {
            if (runningCoroutine != null)
                StopCoroutine(runningCoroutine);

            runningCoroutine = MonsterColorBlink();
            StartCoroutine(runningCoroutine);
        }
    }

    public void OnDead()
    {
        if (isDead)
            return;

        isDead = true;
        agent.enabled = false;
        canMove = false;
        rend.sortingOrder = 0;
        anim.speed = 1f;
        rend.color = Color.white;

        if (itemDropPercent > 0 && hp <= 0)
        {
            int rand = Random.Range(0, 100);

            if (rand < itemDropPercent)
            {
                gameManager.totalBulletCount++;

                character.getItemUI.GetComponent<GetItemUI>().SetBulletGetImage();
                character.getItemUI.gameObject.SetActive(true);
            }
        }

        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);

        if (attackCollider != null)
            attackCollider.enabled = false;

        anim.SetTrigger("Die");
    }

    void CheckDieAnimation()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Die"))
        {
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                DestroyMonster();
            }
        }
    }

    public void ChangeOutline(Color color)
    {
        rend.material.SetColor("_SolidOutline", color);
    }

    public void SetManagedPool(IObjectPool<Monster> pool)
    {
        managedPool = pool;
    }

    public void DestroyMonster()
    {
        managedPool.Release(this);
        agent.enabled = false;
        hitCollider.enabled = false;
    }

    
}
