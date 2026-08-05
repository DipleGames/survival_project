using System.Collections;
using UnityEngine;

public class FireBullet : FireProjectile
{
    [SerializeField] AudioClip reloadingSound;
    [SerializeField] Transform bulletParent;

    int maxBulletCount;
    int currentBulletCount;

    Coroutine currentCoroutine;

    bool isDayChange = true; 

    private void OnEnable()
    {
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

    private void OnDisable()
    {
        if (isDayChange)
            isDayChange = false;

        if(!GamesceneManager.Instance.isNight)
            isDayChange = true;

        StopAllCoroutines();
    }

    void Update()
    {
        if (currentBulletCount > 0)
        {
            FireAppliedCoolTime();
        }

        else if (currentBulletCount <= 0)
            character.canWeaponChange = true;

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

    protected override void Fire()
    {
        if (Input.GetMouseButton(0) && !gameManager.isPause && canFire)
        {
            soundManager.PlaySFX(weaponSound);

            character.canWeaponChange = false;
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

        yield return CoroutineCaching.WaitForSeconds(1);

        character.isCanControll = true;
    }
}
