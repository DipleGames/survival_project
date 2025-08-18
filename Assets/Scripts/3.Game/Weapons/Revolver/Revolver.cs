using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : ProjectileManager, IWeapon
{
    [SerializeField] float revolverDelay;
    [SerializeField] protected AudioClip reloadingSound;
    [SerializeField] protected Transform bulletParent;

    int maxBulletCount;
    int currentBulletCount;

    Coroutine currentCoroutine;

    bool isDayChange = true;

    public void Attack()
    {
        if (currentBulletCount > 0 && weaponManager.canAttack)
        {
            FireAppliedCoolTime();
        }

        else if (currentBulletCount <= 0)
            weaponManager.canWeaponChange = true;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentBulletCount = gameManager.totalBulletCount < maxBulletCount ? gameManager.totalBulletCount : maxBulletCount;

            for (int i = 0; i < currentBulletCount; ++i)
            {
                bulletParent.GetChild(i).gameObject.SetActive(true);
            }

            for (int i = currentBulletCount; i < bulletParent.childCount; ++i)
            {
                bulletParent.GetChild(i).gameObject.SetActive(false);
            }
        }
#endif
    }
    public void SetDelay()
    {
        weaponManager.delayTime = revolverDelay * character.attackSpeed;
        weaponManager.canAttack = false;
    }

    private void OnEnable()
    {
        weaponManager.SetChangeDelay();
        SetWeaponDelay(revolverDelay);

        if (!isDayChange)
            return;

        maxBulletCount = gameManager.specialStatus[SpecialStatus.AmmoPouch] ? 5 : 4;
        maxBulletCount = gameManager.totalBulletCount < maxBulletCount ? gameManager.totalBulletCount : maxBulletCount;
        currentBulletCount = maxBulletCount;

        for (int i = 0; i < maxBulletCount; ++i)
        {
            bulletParent.GetChild(i).gameObject.SetActive(true);
        }

        for (int i = maxBulletCount; i < bulletParent.childCount; ++i)
        {
            bulletParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        Attack();
    }

    private void OnDisable()
    {
        if (isDayChange)
            isDayChange = false;

        if (!GamesceneManager.Instance.isNight)
            isDayChange = true;

        StopAllCoroutines();
    }

    protected override void Fire()
    {
        if (Input.GetMouseButton(0) && !gameManager.isPause && weaponManager.canAttack)
        {
            soundManager.PlaySFX(weaponSound);

            weaponManager.canWeaponChange = false;
            SetFire();

            if (bulletParent.gameObject.activeSelf)
                bulletParent.GetChild(currentBulletCount - 1).gameObject.SetActive(false);

            currentBulletCount--;
            gameManager.totalBulletCount--;

            if (gameManager.specialStatus[SpecialStatus.SilverBullet])
            {
                if (currentBulletCount <= 0)
                    character.defence -= 5;
            }

            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(FreezeCharacter());
        }
    }

    protected override void SettingProjectile()
    {
        base.SettingProjectile();

        projectile.GetComponent<ProjectileMovement>().Shoot(dir.normalized, normalFirePos.position);
    }

    IEnumerator FreezeCharacter()
    {
        soundManager.PlaySFX(reloadingSound);
        character.isCanControll = false;

        yield return CoroutineCaching.WaitForSeconds(0.5f);

        character.isCanControll = true;
    }
}
