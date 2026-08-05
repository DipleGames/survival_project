using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class SwingCutlass : MonoBehaviour
{
    [SerializeField] BoxCollider collder;
    [SerializeField] float initCoolTime;
    [SerializeField] AudioClip attackSound;

    Animator anim;
    Character character;
    SoundManager soundManager;
    GameManager gameManager;

    bool canAttack;

    float coolTime;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        character = Character.Instance;
        soundManager = SoundManager.Instance;
        gameManager = GameManager.Instance;
    }

    private void OnEnable()
    {
        coolTime = initCoolTime;

        collder.enabled = false;

        canAttack = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        Attack();
    }

    void Attack()
    {
        if (!canAttack || gameManager.isPause || character.isDead || !character.isCanControll)
            return;

        if (Input.GetMouseButton(0) && character.isCanControll)
        {
            soundManager.PlaySFX(attackSound);

            character.canWeaponChange = false;
            character.canFlip = false;
            character.anim.SetTrigger("isAttack");
            collder.enabled = true;

            anim.SetBool("canAttack", canAttack);

            if (character.IsFlip)
                anim.SetBool("RightAttack", true);

            else
                anim.SetBool("RightAttack", false);

            canAttack = false;
        }
    }

    void EndAttack()
    {
        collder.enabled = false;
        character.canFlip = true;
        anim.SetBool("canAttack", canAttack);
        StartCoroutine(AttackDelay());
    }

    IEnumerator AttackDelay()
    {
        if (character == null)
            character = Character.Instance;

        coolTime = initCoolTime * character.attackSpeed;
        yield return CoroutineCaching.WaitForSeconds(coolTime);

        canAttack = true;
        character.canWeaponChange = true;
    }
}
