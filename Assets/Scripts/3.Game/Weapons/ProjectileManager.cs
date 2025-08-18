using UnityEngine;
using UnityEngine.Pool;

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected Transform normalFirePos;
    [SerializeField] protected int poolCount;
    [SerializeField] protected AudioClip weaponSound;
    [SerializeField] public float weaponDelay;

    protected GameManager gameManager;
    protected SoundManager soundManager;
    protected WeaponManager weaponManager;

    protected IObjectPool<ProjectileObjectPool> projectilePool;

    protected Vector3 dir, mouse;

    protected ProjectileObjectPool projectile;
    protected Character character;

    protected virtual void Awake()
    {
        projectilePool = new ObjectPool<ProjectileObjectPool>(CreatePool, OnGetPool, OnReleasePool, OnDestroyPool, maxSize: poolCount);

        gameManager = GameManager.Instance;
        character = Character.Instance;
        weaponManager = WeaponManager.Instance;
        soundManager = SoundManager.Instance;
    }

    protected virtual ProjectileObjectPool CreatePool()
    {
        ProjectileObjectPool pool = Instantiate(projectilePrefab, normalFirePos.position, projectilePrefab.transform.rotation).GetComponent<ProjectileObjectPool>();
        pool.SetManagedPool(projectilePool);
        pool.transform.SetParent(gameManager.bulletStorage);
        return pool;
    }

    protected virtual void OnGetPool(ProjectileObjectPool bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    protected virtual void OnReleasePool(ProjectileObjectPool bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    protected virtual void OnDestroyPool(ProjectileObjectPool bullet)
    {
        Destroy(bullet.gameObject);
    }

    protected virtual void Fire()
    {
        if(Input.GetMouseButton(0))
        {
            weaponManager.canWeaponChange = false;
        }

        if (Input.GetMouseButtonUp(0) && !gameManager.isPause && weaponManager.canAttack)
        {
            soundManager.PlaySFX(weaponSound);
            SetFire();
        }
    }
    protected virtual void SetFire()
    {
        SettingProjectile();
        weaponManager.delayTime = weaponDelay * character.attackSpeed;
        weaponManager.canAttack = false;
    }
    protected virtual void SettingProjectile()
    {
        projectile = projectilePool.Get();
        projectile.transform.position = new Vector3(normalFirePos.position.x, 0f, normalFirePos.position.z);
        FireDirection();
    }
    protected void FireDirection()
    {
        mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.y = 0;

        dir = mouse - transform.position;
    }

    protected virtual void SetInitFire()
    {
        weaponManager.canAttack = true;
        weaponManager.canWeaponChange = true;
    }

    protected virtual void FireAppliedCoolTime()
    {
        if (gameManager.isPause || character.isDead || !character.isCanControll)
            return;

        if (weaponManager.canAttack)
            Fire();

        else if (weaponManager.delayTime <= 0)
            SetInitFire();
    }

    public void SetWeaponDelay(float delay)
    {
        weaponDelay = delay;
    }

}
