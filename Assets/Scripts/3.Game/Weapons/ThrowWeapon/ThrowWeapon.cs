public class ThrowWeapon : FireProjectile
{
    void Start()
    {
        initCoolTime = 2f;
    }

    private void Update()
    {
        FireAppliedCoolTime();
    }

    protected override void SettingProjectile()
    {
        normalFirePos = character.transform;
        base.SettingProjectile();
        projectile.GetComponent<ThrowObjecMovement>().Fire(normalFirePos.position);
        projectile.GetComponent<ThrowObject>().FlipRotate(dir.x >= 0 ? true : false);
    }

    protected override void SetFire()
    {
        base.SetFire();
        GetComponent<ParabolaLineRenderer>().ChangeLineColor(canFire);
    }

    protected override void SetInitFire()
    {
        base.SetInitFire();
        GetComponent<ParabolaLineRenderer>().ChangeLineColor(canFire);
    }
}
