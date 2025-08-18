using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.WSA;

public class Cutlass : MonoBehaviour, IWeapon
{
    [SerializeField] BoxCollider collder;
    [SerializeField] AudioClip attackSound;

    [Header("Stat")]
    [SerializeField] float damage;
    [SerializeField] float cutlassDelay;

    Animator anim;
    GameManager gameManager;
    Character character;
    WeaponManager weaponManager;
    SoundManager soundManager;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        character = Character.Instance;
        gameManager = GameManager.Instance;
        soundManager = SoundManager.Instance;
        weaponManager = WeaponManager.Instance;
    }

    public void Attack()
    {
        if (!weaponManager.canAttack || gameManager.isPause || character.isDead || !character.isCanControll)
            return;

        if (Input.GetMouseButton(0) && character.isCanControll && weaponManager.canAttack)
        {
            soundManager.PlaySFX(attackSound);

            weaponManager.canWeaponChange = false;
            character.canFlip = false;
            character.anim.SetTrigger("isAttack");
            collder.enabled = true;

            anim.SetBool("canAttack", weaponManager.canAttack);

            if (character.IsFlip)
                anim.SetBool("RightAttack", true);
            else
                anim.SetBool("RightAttack", false);

            SetDelay();
        }
    }
    public void SetDelay()
    {
        weaponManager.delayTime = cutlassDelay * character.attackSpeed;
        weaponManager.canAttack = false;
        weaponManager.canWeaponChange = false;
    }

    private void OnEnable()
    {
        weaponManager.SetChangeDelay();

        collder.enabled = false;
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
        }
    }

    void EndAttack()
    {
        collder.enabled = false;
        character.canFlip = true;
        anim.SetBool("canAttack", weaponManager.canAttack);
        SetDelay();
    }
    
}
