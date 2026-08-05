using UnityEngine;

public class Obstacle : MonoBehaviour, IDamageable
{
    [SerializeField] LayerMask collLayer;
    [SerializeField] Sprite[] obstacleSprites;
    [SerializeField] GameObject[] shadows;
    [SerializeField] SpriteRenderer spriteRenderer;

    private void Awake()
    {
        for(int i=0;i<shadows.Length;++i)
        {
            shadows[i].gameObject.SetActive(false);
        }
    }

    public void Attacked(float damage, GameObject hitObject)
    {
        if(hitObject.GetComponent<ProjectileObjectPool>() != null)
        {
            hitObject.GetComponent<ProjectileObjectPool>().DestroyProjectile();
        }

        else if(hitObject.GetComponent<MonsterBullet>() != null)
        {
            hitObject.GetComponent<MonsterBullet>().DestroyBullet();
        }
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    { 
        //if ((collLayer | (1 << other.gameObject.layer )) != collLayer)
        if(other.gameObject.layer == LayerMask.NameToLayer("MouseInteraction"))
        {
            Destroy(other.gameObject);
        }
    }

    public void SetObstacleImage(int num)
    {
        spriteRenderer.sprite = obstacleSprites[num];

        if(num == 0)
        {
            spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }

        else if(num == 1)
        {
            spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0, 0, -90);
        }

        else if(num == 2)
        {
            spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            spriteRenderer.flipX = true;
        }

        else if (num == 3)
        {
            spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0, 0, -180);
            spriteRenderer.flipX = false;
        }

        shadows[num].gameObject.SetActive(true);
    }
}
