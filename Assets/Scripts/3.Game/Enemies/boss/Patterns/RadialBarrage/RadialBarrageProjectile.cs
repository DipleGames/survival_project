using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class RadialBarrageProjectile : MonoBehaviour
{
    private Rigidbody body;
    private Transform owner;
    private Vector3 direction;
    private float speed;
    private float remainingDistance;
    private float radius;
    private int damage;
    private LayerMask playerLayers;
    private LayerMask collisionLayers;
    private Material instanceMaterial;
    private bool initialized;
    private bool consumed;

    public void Initialize(Transform boss, Vector3 moveDirection, float moveSpeed, float distance,
        float hitRadius, int hitDamage, LayerMask targetLayers, LayerMask obstacleLayers,
        Material material, Color color)
    {
        owner = boss;
        direction = moveDirection.normalized;
        speed = moveSpeed;
        remainingDistance = distance;
        radius = hitRadius;
        damage = hitDamage;
        playerLayers = targetLayers;
        collisionLayers = targetLayers.value | obstacleLayers.value;
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        GetComponent<SphereCollider>().isTrigger = true;
        instanceMaterial = new Material(material);
        instanceMaterial.color = color;
        GetComponent<Renderer>().sharedMaterial = instanceMaterial;
        initialized = true;
        Destroy(gameObject, distance / moveSpeed);

        // 생성 지점이 플레이어나 벽 내부인 경우
        foreach (Collider hit in Physics.OverlapSphere(body.position, radius, collisionLayers,
            QueryTriggerInteraction.Collide))
        {
            if (TryConsume(hit)) break;
        }
    }

    private void FixedUpdate()
    {
        if (!initialized || consumed) return;

        if (owner == null || !owner.gameObject.activeInHierarchy)
        {
            Consume();
            return;
        }

        float step = Mathf.Min(remainingDistance, speed * Time.fixedDeltaTime);

        // 빠른 투사체가 플레이어나 얇은 벽을 통과하지 않게 함
        RaycastHit[] hits = Physics.SphereCastAll(body.position, radius, direction, step,
            collisionLayers, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (TryConsume(hit.collider)) return;
        }

        body.MovePosition(body.position + direction * step);
        remainingDistance -= step;

        if (remainingDistance <= 0f) Consume();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (initialized && !consumed) TryConsume(other);
    }

    private bool TryConsume(Collider other)
    {
        if (consumed || other.transform.IsChildOf(transform)) return false;
        if (owner != null && other.transform.IsChildOf(owner)) return false;
        if (other.GetComponent<ArrowReflectionTrigger>() != null) return false;
        int layerBit = 1 << other.gameObject.layer;
        if ((collisionLayers.value & layerBit) == 0) return false;

        // 중복 콜라이더와 콜백에도 투사체 하나당 한 번만 피해
        consumed = true;

        if ((playerLayers.value & layerBit) != 0)
        {
            Player player = other.GetComponentInParent<Player>();

            if (player != null && player.Health != null)
                player.Health.TakeDamage(damage);
            else
                Debug.Log($"[보스 탄막] 플레이어 판정 적중: {other.name}, 피해량: {damage}");
        }
        Consume();
        return true;
    }

    private void Consume()
    {
        consumed = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (instanceMaterial != null) 
        {
            Destroy(instanceMaterial);
        }
    }
}
