using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MetalBat : MonoBehaviour, IWeapon
{
    [SerializeField] GameObject chargeBar;
    [SerializeField] List<Collider> detectedList;

    [Header("Stat")]
    [SerializeField] float damage;
    [SerializeField] float batDelay;
    [SerializeField] float pushPower;

    [Header("Time")]
    [Range(0f, 3f)]
    [SerializeField] float clickTime;

    [Header("Sound")]
    [SerializeField] List<AudioClip> attackSounds;
    //[SerializeField] AudioClip chargeSound;
    //[SerializeField] AudioClip chargeAttackSound;

    WaitForSeconds WFS_400ms;

    Animator anim;
    Character character;
    GameManager gameManager;
    WeaponManager weaponManager;
    SoundManager soundManager;
    MonsterFinder monsterFinder;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        character = Character.Instance;
        gameManager = GameManager.Instance;
        weaponManager = WeaponManager.Instance;
        soundManager = SoundManager.Instance;
        
        monsterFinder = GetComponent<MonsterFinder>();
        detectedList = new List<Collider>();
        WFS_400ms = new WaitForSeconds(0.4f);
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
            clickTime += Time.deltaTime;

            if(clickTime > 3f)
            {
                clickTime = 3f;
            }
            else if (1f <= clickTime)
            {
                chargeBar.SetActive(true);
                weaponManager.chargeTime = clickTime - 1f;
            }
        }
        
        if (Input.GetMouseButtonUp(0) && character.isCanControll && weaponManager.canAttack)
        {
            chargeBar.SetActive(false);
            clickTime = 0f;

            AudioClip selectedSound = attackSounds[Random.Range(0, 3)];
            soundManager.PlaySFX(selectedSound);

            weaponManager.canWeaponChange = false;
            weaponManager.canAttack = false;
            StartCoroutine(ControlAnim());

            detectedList = monsterFinder.FindMonster();
            if (detectedList.Count > 0)
            {
                //Debug.Log(string.Join(',', detectedList.Select(x => x.gameObject.transform.parent.name)));

                foreach (Collider moncol in detectedList)
                {
                    Transform monster = moncol.transform.parent;

                    Vector3 direction = (monster.position - transform.position).normalized;

                    if (moncol.GetComponent<MonsterHit>() != null)
                    {
                        bool isCri = gameManager.status[Status.Critical] >= Random.Range(0f, 100f);
                        float finalDamage = (damage + gameManager.status[Status.Damage] + gameManager.status[Status.CloseDamage] + gameManager.bloodDamage) * (100 + character.percentDamage) * 0.01f;

                        finalDamage *= (isCri ? 2 : 1);
                        
                        float hpNow = moncol.gameObject.GetComponentInParent<Monster>().hp;
                        float realDamage = (weaponManager.chargeTime > 0f && hpNow <= finalDamage) ? hpNow - 1 : finalDamage;

                        moncol.GetComponent<IDamageable>().Attacked(realDamage, moncol.gameObject);
                        moncol.GetComponent<IDamageable>().RendDamageUI(realDamage, moncol.transform.position, true, isCri);

                        if (weaponManager.chargeTime > 0f)
                        {
                            float finalPower = pushPower * (weaponManager.chargeTime / 2f);

                            if (monster.GetComponent<MonsterMove>() != null)
                            {
                                StartCoroutine(monster.gameObject.GetComponent<MonsterMove>().FlyAway(direction, finalPower, finalDamage, isCri));
                            }
                        }
                    }
                }
            }
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
        chargeBar.SetActive(false);
    }
    private void Update()
    {
        Attack();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator ControlAnim()
    {
        weaponManager.delayTime = 0.4f;
        character.canFlip = false;
        character.anim.SetTrigger("isAttack");

        do
        {
            yield return WFS_400ms;
        } while (false);

        SetDelay();
    }

}
