using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
        FocusObject = MonsterFocusObject.Player;
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
}
