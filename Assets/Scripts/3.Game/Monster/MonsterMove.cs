using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;
using static UnityEngine.Rendering.HableCurve;

public enum MonsterFocusObject
{
    House,
    Player
}

public class MonsterMove : MonoBehaviour
{
    float moveTime;
    float waitTime;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] BoxCollider weaponBoxCollider;
    [SerializeField] Animator anim;
    [SerializeField] GameObject weapon;
    [SerializeField] SphereCollider characterDetectCollider;

    Character character;
    GameManager gameManager;

    Vector3 dir;

    [HideInInspector] public NavMeshAgent agent;

    float initScaleX;
    float initColliderX;
    float initWeaponColliderX;

    [SerializeField] float initMoveTime;
    [SerializeField] float initWaitTime;
    [SerializeField] float initSpeed;

    Vector3 housePos;

    Vector3 destination;
    public MonsterFocusObject FocusObject { get; private set; }

    WaitForSeconds WFS_1s;

    LayerMask detactLayer;
    float impactRadius;

    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] int segments;

    private void Awake()
    {
        character = Character.Instance;
        gameManager = GameManager.Instance;
        agent = GetComponent<NavMeshAgent>();

        housePos = GameObject.Find("House").transform.position;

        agent.updateRotation = false;

        initScaleX = transform.localScale.x;
        initColliderX = boxCollider.size.x;

        if (weaponBoxCollider != null)
            initWeaponColliderX = weaponBoxCollider.size.x;

        /*initMoveTime = moveTime;
        initWaitTime = waitTime;*/

        moveTime = initMoveTime;
        waitTime = initWaitTime;

        destination = housePos;
        FocusObject = MonsterFocusObject.House;

        WFS_1s = new WaitForSeconds(1f);
        impactRadius = 1f;

        lineRenderer.enabled = true;
        segments = 60;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.widthMultiplier = 0.025f;
        lineRenderer.enabled = false;
    }

    private void OnEnable()
    {
        moveTime = initMoveTime;
        waitTime = initWaitTime;
    }

    private void Update()
    {
        anim.SetBool("isWalk", agent.enabled);

        if (!GetComponent<Monster>().CanMove || GetComponent<Monster>().IsDead)
        {
            agent.enabled = false;
            return;
        }

        Flip();

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Character"))
        {
            FocusObject = MonsterFocusObject.Player;
            destination = character.transform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Character"))
        {
            FocusObject = MonsterFocusObject.House;
            destination = housePos;
        }
    }

    public void InitSetting(float speed)
    {
        initSpeed = speed;
        agent.speed = initSpeed;
    }

    public void InitailizeCoolTime()
    {
        waitTime = initWaitTime;
        moveTime = initMoveTime;
    }

    Vector3[] corners;
    int count = 1;

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
        boxCollider.size = new Vector3(colliderX, boxCollider.size.y, boxCollider.size.z);

        if (weaponBoxCollider != null)
        {
            float weaponColliderX = dir.x > 0 ? -initWeaponColliderX : initWeaponColliderX;
            weaponBoxCollider.size = new Vector3(weaponColliderX, weaponBoxCollider.size.y, weaponBoxCollider.size.z);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + characterDetectCollider.center, characterDetectCollider.radius);
    }

    public IEnumerator FlyAway(Vector3 flyDirection, float power, float damage, bool isCri)
    {
        GetComponentInParent<Monster>().isblowed = true;

        Vector3 endPos = transform.position + (new Vector3(flyDirection.x, 0, flyDirection.z) * power);
        endPos.y = transform.position.y;

        float distance = 1 + power;             // 비거리 : [1 ~ 5]
        float flyDuration = 1f;
        float height = 2f;
        StartCoroutine(MoveParabolic(transform.position, endPos, height, flyDuration));
        yield return WFS_1s;    // 비행시간 동안 대기

        // float radius = 0.25f + distance * 0.25f;         // 비거리에 따른 공격판정 범위
        detactLayer = 1 << LayerMask.NameToLayer("MonsterAttaked");
        var detected = Physics.OverlapSphere(transform.position, impactRadius, detactLayer);

        if (detected != null)
        {
            foreach (Collider monster in detected)
            {
                if (monster == boxCollider) continue;
                if (monster.GetComponentInParent<Monster>().isblowed) continue;

                monster.GetComponent<IDamageable>().Attacked(damage * 0.7f, monster.gameObject);
                monster.GetComponent<IDamageable>().RendDamageUI(damage * 0.7f, monster.transform.position, true, isCri);
            }
        }

        StartCoroutine(Stunned());
        GetComponentInParent<Monster>().isblowed = false;
        GetComponentInChildren<IDamageable>().Attacked(damage * 0.7f, gameObject);
        GetComponentInChildren<IDamageable>().RendDamageUI(damage * 0.7f, transform.position, true, isCri);
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

    IEnumerator Stunned()
    {
        agent.enabled = false;
        GetComponent<Monster>().canAttack = false;
        float time = 0f;

        // WFS 없이 적용되는가?
        while (time < 1f)
        {
            time += Time.deltaTime;
            yield return null;
        } 

        GetComponent<Monster>().canAttack = true;
        agent.enabled = true;
    }

    IEnumerator DrawLandingPoint(Vector3 landingPoint, float duration)
    {
        float time = 0f;
        lineRenderer.enabled = true;

        // Debug.Log("impactRadiuis: " + impactRadius);
        for (int i = 0; i <= segments; i++)
        {
            float angle = (2 * Mathf.PI) * ((float)i / segments);
            // Debug.Log("i: " + i + " angle: " + angle);
            float x = landingPoint.x + Mathf.Cos(angle) * impactRadius;
            float z = landingPoint.z + Mathf.Sin(angle) * impactRadius;
            lineRenderer.SetPosition(i, new Vector3(x, 0.1f, z));
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }

        lineRenderer.enabled = false;       // 비행 종료시 비활성화
    }

    private void OnDisable()
    {
        GetComponent<Monster>().canAttack = true;
        agent.enabled = true;
        StopAllCoroutines();
    }
}
