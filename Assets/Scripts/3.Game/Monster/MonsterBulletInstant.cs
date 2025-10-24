using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class MonsterBulletInstant : MonoBehaviour
{
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletPos;
    [SerializeField] AudioClip attackSound;

    private IObjectPool<MonsterBullet> pool;

    Character character;
    SoundManager soundManager;

    Vector3 dir;

    private void Awake()
    {
        pool = new ObjectPool<MonsterBullet>(CreateBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet, maxSize: 5);

        character = Character.Instance;
        soundManager = SoundManager.Instance;
    }

    public void Shoot()
    {
        if (transform.parent.GetComponent<Monster>().attackCount <= 0)
            return;

        soundManager.PlaySFX(attackSound);

        transform.parent.GetComponent<Monster>().attackCount--;

        MonsterBullet bullet = pool.Get();

        dir = (character.transform.position - transform.position).normalized;

        if (weapon != null)
        {
            transform.parent.GetComponent<Monster>().RotateWeapon(dir);
        }

        bullet.transform.position = new Vector3(bulletPos.position.x, 0, bulletPos.position.z);
        bullet.bulletDamage = transform.parent.GetComponent<Monster>().stat.monsterDamage;

        if (bullet.GetComponent<DirectianalMovement>() != null)
        {
            bullet.GetComponent<DirectianalMovement>().SetDir(dir);
        }

        else if (bullet.gameObject.GetComponent<ThrowObjecMovement>() != null)
        {
            bullet.gameObject.GetComponent<ThrowObjecMovement>().SetStartEndPoint(bullet.transform.position, character.transform.position);
            bullet.destroyPos = character.transform.position;
        }
    }

    public void AttackEnd()
    {
        transform.parent.GetComponent<Monster>().attackCount = transform.parent.GetComponent<Monster>().InitAttackCount;
    }

    private MonsterBullet CreateBullet()
    {
        MonsterBullet bullet = Instantiate(bulletPrefab, bulletPos.position, transform.rotation, GameManager.Instance.monsterBulletStorage).GetComponent<MonsterBullet>();
        bullet.SetManagedPool(pool);
        bullet.parentObject = transform.parent;
        return bullet;
    }

    private void OnGetBullet(MonsterBullet bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    private void OnReleaseBullet(MonsterBullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void OnDestroyBullet(MonsterBullet bullet)
    {
        Destroy(bullet.gameObject);
    }
}
