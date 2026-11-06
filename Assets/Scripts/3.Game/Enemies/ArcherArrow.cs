using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ArcherArrow : MonoBehaviour
{
    [SerializeField] private float lifetime = 8f;
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private string playerTag = "Character";

    private Rigidbody rb;
    private Vector3 direction;
    private float speed;
    private int damage;

    private GameObject owner;
    private LayerMask playerLayer;

    private bool initialized;
    private bool reflected;
    private Transform reflectedTarget;

    private bool consumed;
    private float fixedY;

    public SpriteRenderer spriteRenderers;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void Initialize(Vector3 initialDirection, float arrowSpeed, int arrowDamage,
        GameObject ownerObject, LayerMask targetPlayerLayer)
    {
        direction = initialDirection;
        direction.y = 0f;
        direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.right;

        speed = arrowSpeed;
        damage = arrowDamage;
        owner = ownerObject;
        playerLayer = targetPlayerLayer;

        AlignSpriteToDirection();

        fixedY = transform.position.y;
        initialized = true;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized || consumed) 
            return;

        transform.position += direction * speed * Time.deltaTime;

        Vector3 position = transform.position;
        position.y = fixedY;
        transform.position = position;
    }

    /// <summary>
    /// PlayerAttack가 가장 가까운 대상 판정에서 화살을 선택하면 호출합니다.
    /// </summary>
    public bool TryReflect(Transform playerTransform)
    {
        if (!initialized || consumed || reflected || playerTransform == null)
            return false;

        if (owner == null)
            return false;

        Vector3 toTarget = owner.transform.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f)
            return false;

        direction = toTarget.normalized;
        reflected = true;

        AlignSpriteToDirection();

        // owner는 유지하여 기존 궁수 명중 판정에 사용
        return true;
    }

    public void SetFacing(bool facingLeft)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.flipX = facingLeft;
    }

    private void AlignSpriteToDirection()
    {
        Vector3 targetDirection = direction;
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude < 0.0001f)
            return;

        targetDirection.Normalize();

        // 현재 스프라이트가 바라보는 오른쪽 방향을 기준으로 회전
        Vector3 currentRight = transform.right;
        currentRight.y = 0f;

        if (currentRight.sqrMagnitude < 0.0001f)
            return;

        currentRight.Normalize();

        float angle = Vector3.SignedAngle(
            currentRight,
            targetDirection,
            Vector3.up
        );

        transform.rotation =
            Quaternion.AngleAxis(angle, Vector3.up) *
            transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized || consumed) return;

        if (reflected)
        {
            ArcherEnemy archer = other.GetComponentInParent<ArcherEnemy>();
            if (archer != null)
            {
                consumed = true;
                archer.ApplyReflectedArrowStun();
                Destroy(gameObject);
                return;
            }

            // 반사 화살이 플레이어에게 되돌아오거나 다른 장애물에 닿으면 소멸합니다.
            if (IsInLayerMask(other.gameObject.layer, playerLayer) ||
                IsInLayerMask(other.gameObject.layer, terrainLayer))
            {
                consumed = true;
                Destroy(gameObject);
            }
            return;
        }

        if (IsInLayerMask(other.gameObject.layer, playerLayer) ||
            other.CompareTag(playerTag))
        {
            consumed = true;
            Debug.Log($"[옛 신앙 궁수] 화살 적중: {other.name}, 피해: {damage}");
            Destroy(gameObject);
            return;
        }

        if (IsInLayerMask(other.gameObject.layer, terrainLayer))
        {
            consumed = true;
            Destroy(gameObject);
        }
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
