using UnityEngine;
using UnityEngine.Pool;

public class MonsterBullet : MonoBehaviour
{
    public float bulletDamage;
    [SerializeField] GameObject arrivePointMarkPrefab;

    IObjectPool<MonsterBullet> managedPool;

    GameManager gameManager;
    GamesceneManager gameSceneManager;

    float realDamage;

    [HideInInspector] public Vector3 destroyPos;

    [HideInInspector] public Transform parentObject;

    [HideInInspector] public GameObject arrivePointMark = null;

    private void Start()
    {
        gameManager = GameManager.Instance;
        gameSceneManager = GamesceneManager.Instance;

        if(arrivePointMarkPrefab != null)
        {
            arrivePointMark = Instantiate(arrivePointMarkPrefab, GameManager.Instance.monsterBulletStorage);
        }
    }

    void Update()
    {
        if (!gameSceneManager.isNight || transform.position == destroyPos)
        {
            DestroyBullet();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            //realDamage = bulletDamage * (1 + Mathf.Floor(gameManager.round / 30)) + Mathf.Floor(gameManager.round / 5) * 2f;
            realDamage = bulletDamage;
            other.transform.parent.GetComponent<Character>().OnDamaged(realDamage, parentObject.GetComponentInChildren<MonsterHit>().gameObject);
            DestroyBullet();
        }

        else if (other.tag == "Obstacle")
        {
            DestroyBullet();
        }

        else
        {
            IDamageable damageable = other.GetComponentInChildren<IDamageable>();
            if (damageable != null)
            {
                damageable.Attacked(realDamage, null);
                DestroyBullet();
            }
        }
    }

    public void SetManagedPool(IObjectPool<MonsterBullet> pool)
    {
        managedPool = pool;
    }

    public virtual void DestroyBullet()
    {
        if (gameObject.activeSelf)
        {
            managedPool.Release(this);

            if (arrivePointMark != null)
                arrivePointMark.SetActive(false);
        }
    }
}
