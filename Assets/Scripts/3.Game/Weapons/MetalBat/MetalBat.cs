using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.UI.Image;

enum ChargeOptionNo
{
    None,
    SingleTarget,
    AttackRange
}

public class MetalBat : MonoBehaviour, IWeapon
{
    [SerializeField] MonsterFinder monsterFinder;
    [SerializeField] Transform chargeBar;
    [SerializeField] Transform landingPoint;
    [SerializeField] Transform EffectRange;
    List<Collider> detectedList;
    Collider detectedOne;
    Collider detectedPastOne;
    Vector3 targetPos;
    Vector3 dirToMouse;

    [Header("Stat")]
    [SerializeField] float damage;
    [SerializeField] float batDelay;
    [SerializeField] public float attackAngle;
    [SerializeField] float maxDistance;
    [SerializeField] float maxHeight;
    [SerializeField] float flyTime;
    [SerializeField] float stunDuration;

    [SerializeField] int optionNo;

    [Header("Time")]
    [Range(0f, 3f)]
    [SerializeField] float clickTime;
    float attackAnimTime;

    [Header("Sound")]
    [SerializeField] List<AudioClip> attackSounds;
    //[SerializeField] AudioClip chargeSound;
    //[SerializeField] AudioClip chargeAttackSound;

    [Header("Tracker")]
    [Inspectable] Transform monster;
    [Inspectable] Vector3 direction;
    [Inspectable] float diffX;
    [Inspectable] float diffZ;
    [Inspectable] Vector3 diffPos;
    [Inspectable] float flyDistance;
    MonsterOutline monsterOutline;

    Animator anim;
    Character character;
    GameManager gameManager;
    WeaponManager weaponManager;
    SoundManager soundManager;
    

    public RangeMode rangeMode;
    Color transparentYellow = new Color(1f, 0.92f, 0.016f, 0.05f);
    

    private void Awake()
    {
        anim = GetComponent<Animator>();
        attackAnimTime = 0.4f;
        
        character = Character.Instance;
        gameManager = GameManager.Instance;
        weaponManager = WeaponManager.Instance;
        soundManager = SoundManager.Instance;

        monsterFinder = GetComponent<MonsterFinder>();
        detectedList = new List<Collider>();
        detectedOne = new Collider();
        detectedPastOne = new Collider();
        optionNo = (int)ChargeOptionNo.SingleTarget;
        
        if (landingPoint.gameObject.activeSelf)
            landingPoint.gameObject.SetActive(false);

        if (EffectRange.gameObject.activeSelf)
            EffectRange.gameObject.SetActive(false);
    }

    public void Attack()
    {
        if (!weaponManager.canAttack || gameManager.isPause || character.isDead || !character.isCanControll)
            return;

        if (Input.GetMouseButtonDown(0) && character.isCanControll && weaponManager.canAttack)
        {
            clickTime = 0f;
        }
        
        if (Input.GetMouseButton(0) && character.isCanControll && weaponManager.canAttack)
        {
            if(clickTime < 3f && weaponManager.delayTime <= 0f)
                clickTime += Time.deltaTime;
            
            else if(clickTime > 3f)
                clickTime = 3f;

            if (1f <= clickTime)
            {
                chargeBar.gameObject.SetActive(true);
                weaponManager.chargeTime = clickTime - 1f;
                
                if(weaponManager.chargeTime > 0f)
                {
                    // 차지공격 단일타겟 옵션인 경우
                    if (optionNo == (int)ChargeOptionNo.SingleTarget)
                    {
                        ChargeAttack_Option1_Targetting();
                    }
                    // 차지공격 범위전체 옵션인 경우
                    if (optionNo == (int)ChargeOptionNo.AttackRange)
                    {
                        EffectRange.gameObject.SetActive(true);
                    }
                }
                
            }
        }
        
        if (Input.GetMouseButtonUp(0) && character.isCanControll && weaponManager.canAttack)
        {
            chargeBar.gameObject.SetActive(false);
            clickTime = 0f;

            AudioClip selectedSound = attackSounds[Random.Range(0, 3)];
            soundManager.PlaySFX(selectedSound);

            weaponManager.canWeaponChange = false;
            weaponManager.canAttack = false;
            StartCoroutine(ControlAnim());

            if(weaponManager.chargeTime <= 0f)
            {
                NormalAttack();
            }
            else if(weaponManager.chargeTime > 0f)
            {
                switch(optionNo)
                {
                    case 1:
                        Debug.Log("모드: 차지공격1");
                        ChargeAttack_Option1();
                        break;
                    case 2:
                        Debug.Log("모드: 차지공격2");
                        // ChargeAttack_Option2();
                        break;
                }
            }

            landingPoint.gameObject.SetActive(false);
            EffectRange.gameObject.SetActive(false);
            weaponManager.chargeTime = 0f;
        }
    }
    public void SetDelay()
    {
        weaponManager.delayTime = batDelay * character.attackSpeed;
        weaponManager.canWeaponChange = false;
        weaponManager.canAttack = false;
        character.canFlip = true;
    }
    
    private void OnEnable()
    {
        weaponManager.SetChangeDelay();
        chargeBar.gameObject.SetActive(false);
        landingPoint.gameObject.SetActive(false);
        weaponManager.rangeMode = (int)(RangeMode.Arc);
    }
    private void Update()
    {
        Attack();
    }

    // 공격모션(anim) 제어
    IEnumerator ControlAnim()
    {
        weaponManager.delayTime = 0.4f;
        character.canFlip = false;
        character.anim.SetTrigger("isAttack");

        float timeNow = 0f;
        while (timeNow < attackAnimTime)
        {
            timeNow += Time.deltaTime;
            yield return null;
        }

        SetDelay();
    }

    // 공격처리 함수
    protected void AttackProcess(Collider moncol)
    {
        // Transform monster = moncol.transform.parent;
        // Vector3 direction = (monster.position - transform.position).normalized;

        if (moncol.transform.parent.GetComponent<Monster>() != null)
        {
            bool isCri = gameManager.status[Status.Critical] >= Random.Range(0f, 100f);
            float finalDamage = (damage + gameManager.status[Status.Damage] + gameManager.status[Status.CloseDamage] + gameManager.bloodDamage) * (100 + character.percentDamage) * 0.01f;

            finalDamage *= (isCri ? 2 : 1);

            float hpNow = moncol.gameObject.GetComponentInParent<Monster>().hp;
            float realDamage = (weaponManager.chargeTime > 0f && hpNow <= finalDamage) ? hpNow - 1 : finalDamage;

            moncol.transform.parent.GetComponent<IDamageable>().Attacked(realDamage, moncol.gameObject);
            moncol.transform.parent.GetComponent<IDamageable>().RendDamageUI(realDamage, moncol.transform.position, true, isCri);
        }
    }

    // 0. 기본공격(+스턴) 실행처리
    protected void NormalAttack()
    {
        detectedList = monsterFinder.FindMonster();
        if (detectedList.Count > 0)
        {
            //Debug.Log(string.Join(',', detectedList.Select(col => col.gameObject.transform.parent.name)));

            foreach (Collider moncol in detectedList)
            {
                AttackProcess(moncol);

                if (moncol.gameObject.activeSelf)
                    moncol.transform.parent.GetComponent<Monster>().GetStunned(stunDuration);
            }
        }
    }

    // 1-1. 차지공격 타겟팅
    void ChargeAttack_Option1_Targetting()
    {
        detectedOne = monsterFinder.FindNearest();
            
        if (detectedPastOne != null && detectedPastOne != detectedOne)
            detectedPastOne.transform.parent.gameObject.GetComponentInChildren<MonsterOutline>().SetOutLine(false);

        if (detectedOne != null)
        {
            detectedPastOne = detectedOne;
            detectedOne.transform.parent.gameObject.GetComponentInChildren<MonsterOutline>().SetOutLine(true);

            monster = detectedOne.transform.parent;
            diffPos = monster.position - character.transform.position;
            direction = diffPos.normalized;
            flyDistance = 0.5f + (maxDistance * (weaponManager.chargeTime / 2f));

            landingPoint.gameObject.SetActive(true);
            targetPos = diffPos + (new Vector3(direction.x, 0, direction.z) * flyDistance);
            landingPoint.GetComponent<LandingPoint>().UpdateLandingPoint(targetPos);
        }
        else
        {
            landingPoint.gameObject.SetActive(false);
        }
    }

    // 1. 가장 가까운 적의 착지점 UI 표시 & 대상 넉백 처리
    protected void ChargeAttack_Option1()
    {
        // 범위 안에서 가장 가까운 몬스터
        detectedOne = monsterFinder.FindNearest();
        
        if (detectedOne != null)
        {
            AttackProcess(detectedOne);

            if (weaponManager.chargeTime > 0f)
            {
                Transform monster = detectedOne.transform.parent;
                Vector3 direction = (monster.position - transform.position).normalized;

                float finalDistance = 1 + (maxDistance * (weaponManager.chargeTime / 2f));
                bool isCri = gameManager.status[Status.Critical] >= Random.Range(0f, 100f);

                float finalDamage = (damage + gameManager.status[Status.Damage] + gameManager.status[Status.CloseDamage] + gameManager.bloodDamage) * (100 + character.percentDamage) * 0.01f;
                finalDamage *= (isCri ? 2 : 1);

                if (monster.GetComponent<Monster>() != null)
                {
                    StartCoroutine(monster.gameObject.GetComponent<Monster>().FlyAway(direction, finalDistance, maxHeight, flyTime, finalDamage, isCri));
                }
            }

            landingPoint.gameObject.SetActive(false);
            detectedOne.transform.parent.gameObject.GetComponentInChildren<MonsterOutline>().SetOutLine(false);
        }
    }

    // 2. 범위내 몬스터 전체 넉백처리       // 구현 중
    /*
    protected void ChargeAttack_Option2()
    {
        detectedList = monsterFinder.FindMonster();
        if (detectedList.Count > 0)
        {
            foreach (Collider moncol in detectedList)
            {
                AttackProcess(moncol);

                Transform monster = moncol.transform.parent;
                Vector3 direction = (monster.position - transform.position).normalized;

                float finalDistance = 1 + (maxDistance * (weaponManager.chargeTime / 2f));
                bool isCri = gameManager.status[Status.Critical] >= Random.Range(0f, 100f);

                float finalDamage = (damage + gameManager.status[Status.Damage] + gameManager.status[Status.CloseDamage] + gameManager.bloodDamage) * (100 + character.percentDamage) * 0.01f;
                finalDamage *= (isCri ? 2 : 1);

                if (monster.GetComponent<MonsterMove>() != null)
                {
                    StartCoroutine(monster.gameObject.GetComponent<MonsterMove>().FlyAway(direction, finalDistance, maxHeight, flyTime, finalDamage, isCri));
                }
            }
        }
    }
    */

    private void OnDrawGizmos()
    {
        DrawEffectRange();
    }

    void DrawEffectRange()
    {
        if (weaponManager.chargeTime > 0f && optionNo == 1)
        {
            dirToMouse = monsterFinder.dirToMouse;
            float currentRadius = 3f + Mathf.Lerp(0f, maxDistance, weaponManager.chargeTime / 2f);

            Vector3 viewAngleA = monsterFinder.DirFromAngle(-attackAngle / 2);
            Vector3 viewAngleB = monsterFinder.DirFromAngle(attackAngle / 2);

            Handles.color = Color.yellow;
            Handles.DrawLine(transform.position + viewAngleA * 3f, transform.position + viewAngleA * currentRadius);
            Handles.DrawLine(transform.position + viewAngleB * 3f, transform.position + viewAngleB * currentRadius);
            Handles.DrawWireArc(transform.position, Vector3.up, viewAngleB, attackAngle, 3);
            Handles.DrawWireArc(transform.position, Vector3.up, viewAngleB, attackAngle, currentRadius);

        }
    }
}
