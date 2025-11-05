using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public enum MonsterFocusObject
{
    House,
    Player
}

public partial class Monster
{   
    [SerializeField] LayerMask groundLayer;
    [SerializeField] GameObject weapon;

    float initScaleX;
    float initColliderX;
    float initWeaponColliderX;

    [HideInInspector] public NavMeshAgent agent;

    Vector3 dir;
    Vector3 playerPos;
    Vector3 destination;

    [Header("Stat")]
    [SerializeField] float moveDelay;
    [HideInInspector] float moveSpeed;

    [Header("Setting")]
    [SerializeField] float initMoveTime;
    [SerializeField] float initWaitTime;
    [SerializeField] float initSpeed = 0;

    public bool canMove = true;

    [SerializeField] float moveTime;
    [SerializeField] float waitTime;

    public MonsterFocusObject FocusObject { get; private set; }

    LayerMask detactLayer;
    float impactRadius;
    float flyDelay;

    Vector3[] corners;
    int count = 1;

    [SerializeField] GameObject StunObject;

    private void OnEnable()
    {
        moveTime = initMoveTime;
        waitTime = initWaitTime;
    }

    private void monsterMove()
    {
        anim.SetBool("isWalk", agent.enabled);

        if (!canMove || IsDead)
        {
            agent.enabled = false;
            return;
        }

        Flip();

        ExecuteMove();
    }

    private void ExecuteMove()
    {
        if (isStun)
            return;

        destination = character.transform.position;

        if (agent.enabled)
        {
            //destination = character.transform.position;

            agent.SetDestination(destination);

            if (initMoveTime != 0)
            {
                moveTime -= Time.deltaTime;

                if (moveTime <= 0)
                {
                    agent.enabled = false;
                    waitTime = initWaitTime;
                }
            }
        }
        else
        {
            if (initWaitTime != 0)
            {

                waitTime -= Time.deltaTime;

                if (waitTime <= 0)
                {
                    agent.enabled = true;
                    
                    moveTime = initMoveTime;
                }
            }
        }
    }

    public void InitailizeCoolTime()
    {
        waitTime = initWaitTime;
        moveTime = initMoveTime;
    }

    void MoveDelay()
    {
        if (canMove || isAttack)
            return;

        moveDelay -= Time.deltaTime;

        if (moveDelay <= 0)
        {
            InitailizeCoolTime();
            agent.enabled = true;
            canMove = true;
            moveDelay = initMoveDelay;
        }
    }

    void Flip()
    {
        if (corners == null || corners != agent.path.corners)
        {
            if (agent.path.corners.Length > 1)
            {
                corners = agent.path.corners;
                count = 1;
            }

            else
            {
                dir = (destination - transform.position).normalized;
            }
        }

        if (corners != null)
        {
            dir = (corners[count] - transform.position).normalized;

            if (transform.position == corners[count] && count < agent.path.corners.Length - 1)
                count++;
        }

        float scaleX = dir.x > 0 ? -initScaleX : initScaleX;
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);

        float colliderX = dir.x > 0 ? -initColliderX : initColliderX;

        hitCollider.size = new Vector3(colliderX, hitCollider.size.y, hitCollider.size.z);

        if (attackCollider != null)
        {
            float weaponColliderX = dir.x > 0 ? -initWeaponColliderX : initWeaponColliderX;
            attackCollider.size = new Vector3(weaponColliderX, attackCollider.size.y, attackCollider.size.z);
        }

        //rend.flipX = dir.x > 0;
    }

    public void RotateWeapon(Vector3 bulletDir)
    {
        float scaleX = transform.localScale.x > 0 ? 1 : -1;
        weapon.transform.localScale =  new Vector3(scaleX, weapon.transform.localScale.y, transform.localScale.z);

        float angle = Mathf.Atan2(bulletDir.z, bulletDir.x) * Mathf.Rad2Deg;

        weapon.transform.rotation = Quaternion.Euler(90, -angle + 180, 0);

        if (bulletDir.x < 0)
            weapon.transform.rotation *= Quaternion.Euler(0, 0, 0);

        else
            weapon.transform.rotation *= Quaternion.Euler(180, 0, 0);
    }

    public IEnumerator FlyAway(Vector3 flyDirection, float distance, float height, float duration, float damage, bool isCri)
    {
        Vector3 endPos = transform.position + (new Vector3(flyDirection.x, 0, flyDirection.z) * distance);
        endPos.y = transform.position.y;

        StartCoroutine(MoveParabolic(transform.position, endPos, height, duration));    // 포물선 이동
        
        flyDelay = 0f;
        while (flyDelay < duration)
        {
            flyDelay += Time.deltaTime;
            yield return null;
        }

        AreaDamage(distance, isCri);
    }

    IEnumerator MoveParabolic(Vector3 startPos, Vector3 endPos, float maxHeight, float duration)
    {
        agent.enabled = false;

        float timePassed = 0f;
        float time = 0f;

        StartCoroutine(DrawLandingPoint(endPos, duration));

        while (timePassed < duration)
        {
            time = timePassed / duration;

            Vector3 basePos = Vector3.Lerp(startPos, endPos, time);

            float height = 2f * maxHeight * time * (1 - time);

            Vector3 posNow = new Vector3(basePos.x, transform.position.y, basePos.z + height);

            transform.position = posNow;

            timePassed += Time.deltaTime;

            yield return null;
        }

        NavMeshHit hit;

        if (NavMesh.SamplePosition(transform.position, out hit, 1000, 1 << groundLayer))
        {
            transform.position = hit.position;
        }

        agent.enabled = true;
        
    }

    IEnumerator DrawLandingPoint(Vector3 landingPoint, float duration)
    {
        float time = 0f;
        
        // 착지지점 오브젝트 풀을 생성하여 관리할 것
        // ㅂㅈㄷㄱㅁㄴㅇㄹ

        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }
    }

}
