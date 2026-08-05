using UnityEngine;
using UnityEngine.Pool;

public class ProjectileObjectPool : MonoBehaviour
{
    [SerializeField] float projectileDamage;
    [SerializeField] int penetrateCount;
    public bool canCri;
    bool isCri = false;

    IObjectPool<ProjectileObjectPool> pool;

    int currentPenetrateCount;

    GameManager gameManager;
    Character character;

    public bool isPenetrate = false;
    private void Start()
    {
        gameManager = GameManager.Instance;
        character = Character.Instance;

        currentPenetrateCount = penetrateCount != -1 ? penetrateCount : 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("MonsterAttacked") && other.transform.parent.GetComponent<Monster>() != null)
        if(other.GetComponent<IDamageable>() != null)
        {
            //Monster monster = other.transform.parent.GetComponent<Monster>();

            float damage = (projectileDamage + gameManager.status[Status.Damage] + gameManager.status[Status.LongDamage] + gameManager.bloodDamage) * (100 + character.percentDamage) * 0.01f;

            if (canCri)
            {
                isCri = gameManager.status[Status.Critical] >= Random.Range(0f, 100f);
                damage *= isCri ? 2 : 1;
            }

            other.GetComponent<IDamageable>().Attacked(damage, gameObject);
            other.GetComponent<IDamageable>().RendDamageUI(damage, other.transform.position, canCri, isCri);

            if (!isPenetrate)
            {
                currentPenetrateCount = 1;
            }

            if (penetrateCount > 0)
                currentPenetrateCount--;

            if (currentPenetrateCount <= 0)
                DestroyProjectile();
        }
    }

    public void SetManagedPool(IObjectPool<ProjectileObjectPool> pool)
    {
        this.pool = pool;
    }

    public void DestroyProjectile()
    {
        if (gameObject.activeSelf)
        {
            currentPenetrateCount = penetrateCount != -1 ? penetrateCount : 1;
            pool.Release(this);
        }
    }

    public void SetDamage(float damage)
    {
        projectileDamage = damage;
    }
}
