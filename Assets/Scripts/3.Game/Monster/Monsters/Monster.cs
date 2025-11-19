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
    IEnumerator stunCoroutine;

    Color initcolor;

    public float Speed => moveSpeed;

    float xDistance;
    float zDistance;

    float initMoveDelay;
    int initOrder;

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
        isStun = false;
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

        if (isStun) return;

        Attack();
        Move();
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
        agent.speed = initSpeed;
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

        isStun = false;
        hitCollider.enabled = true;

        attackCollider.enabled = false;

        if (StunObject.activeSelf)
            StunObject.SetActive(false);
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

    public void ChangeOutline(Color color)
    {
        rend.material.SetColor("_SolidOutline", color);
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
        isStun = false;

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

        if (attackCollider != null)
            attackCollider.enabled = false;

        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        // StunObject.SetActive(false);

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

    public void Stunned(float duration)
    {
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StunCorutine(duration);
        StartCoroutine(stunCoroutine);
    }

    // §ÌÑ¥ÅÌÉú ÅÏö©
    IEnumerator StunCorutine(float duration)
    {
        isStun = true;
        canMove = false;
        agent.enabled = false;
        canAttack = false;
        isAttack = false;

        anim.SetBool("isStun", isStun);
        anim.SetBool("isWalk", canMove);
        anim.SetBool("isAttack", isAttack);
        attackCollider.enabled = false;

        if (StunObject)
            StunObject.SetActive(true);

        Debug.Log("initWaitTime:" + initWaitTime);
        yield return CoroutineCaching.WaitForSeconds(duration);

        if (StunObject)
            StunObject.SetActive(false);

        isStun = false;
        canMove = true;
        agent.enabled = true;
        canAttack = true;

        anim.SetBool("isStun", isStun);
        anim.SetBool("isWalk", agent.enabled);
        anim.SetBool("isWalk", canAttack);
    }

}
