using System;
using UnityEngine;

public class Bow : ProjectileManager, IWeapon
{
    [SerializeField] float bowDelay;

    public void Attack()
    {
        FireAppliedCoolTime();
    }
    public void SetDelay()
    {
        weaponManager.delayTime = bowDelay * character.attackSpeed;
        weaponManager.canAttack = false;
    }

    private void OnEnable()
    {
        weaponManager.SetChangeDelay();
        SetWeaponDelay(bowDelay);
    }

    void Update()
    {
        Attack();
    }

    protected override void SettingProjectile()
    {
        base.SettingProjectile();

        bool isCatch = GetComponent<BowCatchBar>().IsCatch();
        projectile.GetComponent<ArrowTypeChange>().ChangeArrowType(isCatch);

        Transform activeArrow = projectile.transform.GetChild(Convert.ToInt32(isCatch));

        projectile.GetComponent<ProjectileObjectPool>().isPenetrate = isCatch ? true : false;
        projectile.GetComponent<ProjectileObjectPool>().canCri = isCatch ? true : false;

        float damage = activeArrow.GetComponent<ArrowStatus>().Damage;
        projectile.GetComponent<ProjectileObjectPool>().SetDamage(damage);

        float speed = activeArrow.GetComponent<ArrowStatus>().Speed;
        projectile.GetComponent<ProjectileMovement>().Shoot(dir.normalized, normalFirePos.position, speed);
    }

}
