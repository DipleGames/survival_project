using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MetalBat : MonoBehaviour, IWeapon
{
    [SerializeField] BoxCollider collder;
    [SerializeField] GameObject chargeBar;

    [Header("Stat")]
    [SerializeField] float damage;
    [SerializeField] float batDelay;
    [SerializeField] float pushPower;

    [Header("Time")]
    [Range(0f, 2.5f)]
    float clickTime;

    [Header("Sound")]
    [SerializeField] List<AudioClip> attackSounds;
    //[SerializeField] AudioClip chargeSound;
    //[SerializeField] AudioClip chargeAttackSound;

    Animator anim;
    Character character;
    GameManager gameManager;
    WeaponManager weaponManager;
    SoundManager soundManager;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        character = Character.Instance;
        gameManager = GameManager.Instance;
        weaponManager = WeaponManager.Instance;
        soundManager = SoundManager.Instance;
    }

    public void Attack()
    {
        if (!weaponManager.canAttack || gameManager.isPause || character.isDead || !character.isCanControll)
            return;

        if (Input.GetMouseButtonDown(0) && character.isCanControll && weaponManager.canAttack)
        {
            weaponManager.chargeTime = 0f;
            clickTime = 0f;
        }
        
        if (Input.GetMouseButton(0) && character.isCanControll && weaponManager.canAttack)
        {
            clickTime += Time.deltaTime;

            if (0.5f <= clickTime)
            {
                chargeBar.SetActive(true);
                weaponManager.chargeTime = clickTime - 1f;
            }
        }
        
        if (Input.GetMouseButtonUp(0) && character.isCanControll && weaponManager.canAttack)
        {
            AudioClip selectedSound = attackSounds[Random.Range(0, 3)];
            soundManager.PlaySFX(selectedSound);

            float angle1 = GetComponent<MonsterFinder>().rangeAngleA;
            float angle2 = GetComponent<MonsterFinder>().rangeAngleB;
            gameObject.GetComponent<ClipController>().SetKeyframesRotationZ(angle1, angle2);

            weaponManager.canWeaponChange = false;
            character.canFlip = false;
            character.anim.SetTrigger("isAttack");
            collder.enabled = true;

            anim.SetBool("canAttack", weaponManager.canAttack);
            
            if (character.IsFlip)
                anim.SetBool("RightAttack", true);
            else
                anim.SetBool("RightAttack", false);

            transform.rotation = Quaternion.identity;
            chargeBar.SetActive(false);
            SetDelay();
        }

    }
    public void SetDelay()
    {
        weaponManager.delayTime = batDelay * character.attackSpeed;
        weaponManager.canAttack = false;
        weaponManager.canWeaponChange = false;
    }

    public void ChargeAttack()
    {
        /// ???
    }

    
    private void OnEnable()
    {
        weaponManager.SetChangeDelay();
        collder.enabled = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null)
        {
            bool isCri = gameManager.status[Status.Critical] >= Random.Range(0f, 100f);

            float realDamage = (damage + gameManager.status[Status.Damage] + gameManager.status[Status.CloseDamage] + gameManager.bloodDamage) * (100 + character.percentDamage) * 0.01f;

            realDamage *= isCri ? 2 : 1;

            other.GetComponent<IDamageable>().Attacked(realDamage, this.gameObject);
            other.GetComponent<IDamageable>().RendDamageUI(realDamage, other.transform.position, true, isCri);

            if (weaponManager.chargeTime > 0f)
            {
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                pushDir.y = other.gameObject.transform.position.y;
                float finalPower = pushPower * (weaponManager.chargeTime / 2f);
                
            }
        }
    }

    void EndBatAttack()
    {
        collder.enabled = false;
        character.canFlip = true;
        anim.SetBool("canAttack", weaponManager.canAttack);
        SetDelay();
    }

}
