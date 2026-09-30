using UnityEngine;
using System.Collections;

public enum ToolType
{
    None,
    Axe,
    Lighter,
}

public interface IGatherable : IInteractable, IHoverable
{
    void CancelGathering();
}

public class GatherableObj : InteractableObj, IGatherable
{
    [Header("채집 기본 설정")]
    [Tooltip("item id to gather")]
    [SerializeField] string itemId;
    [SerializeField] float interactableDistance;
    [SerializeField] Transform[] gatherPoint;
    [SerializeField] protected float interactionTime = 3f;
    [SerializeField] bool isSingleUse = true;
    [SerializeField] bool isGatherable = true;
    [SerializeField] string animationName = "Logging";

    [Header("벌목 관련 공통 필드")]
    public int requiredHitCount;
    public int currentHit;
    public string[] dropItems;
    public float respawnTime;

    [Header("도구 관련")]
    protected ToolType currentToolType;

    [Header("참조")]
    [SerializeField] GameObject character;

    // 런타임 상태
    protected bool isInteracting = false;
    bool isClose = false;

    SpriteRenderer spriteRenderer;
    Color outlineColor;

    private readonly WorkReservation _reservation = new WorkReservation();
    public UnityEngine.Object ReservedBy => _reservation.ReservedBy;
    public bool IsReserved => _reservation.IsReserved;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) { spriteRenderer = GetComponentInChildren<SpriteRenderer>(); }
    }

    protected virtual void Start()
    {
        outlineColor = spriteRenderer.material.GetColor("_SolidOutline");
    }

    protected virtual void Update()
    {
        if (Character.Instance == null)
            isClose = Vector3.Distance(character.transform.position, transform.position) < interactableDistance;
        else
            isClose = Vector3.Distance(Character.Instance.transform.position, transform.position) < interactableDistance;
    }

    public override void InteractionLeftButtonFuc(GameObject hitObject)
    {
        BeginInteraction();
    }

    public override void BeginInteraction()
    {
        if (!isGatherable)
            return;

        if (isInteracting)
            return;

        if (IsReserved)
            return;

        // 플레이어가 사용 중인 도구를 여기서 currentToolType에 반영
        currentToolType = GetPlayerToolType();

        if (!CanStartInteraction())
            return;

        isInteracting = true;

        InteractionData data = new()
        {
            MovePosition = gatherPoint[0].position,
            InteractionTime = interactionTime,
            AnimationName = animationName
        };

        character
            .GetComponent<TempPlayer>()
            .StartInteraction(data, this);
    }

    protected virtual ToolType GetPlayerToolType()
    {
        // TODO: 실제 플레이어 장비 시스템에서 가져오도록 연결
        return ToolType.None;
    }

    public override void EndInteraction()
    {
        OnInteractionFinished();
        isInteracting = false;
    }

    protected virtual void OnInteractionFinished()
    {
        GetItem(itemId);

        if (isSingleUse)
        {
            isGatherable = false;
            gameObject.SetActive(false);
        }
        else
        {
            ChangeGatherState();
        }
    }

    public override void OnHoverEnter()
    {
        if (!isClose)
            return;

        if (!isGatherable)
            return;

        if (IsReserved)
            return;

        if (isInteracting)
            return;

        outlineColor.a = 1;
        spriteRenderer.material.SetColor("_SolidOutline", outlineColor);

        FloatingText.Instance.Show(name);
    }

    public override void OnHoverExit()
    {
        FloatingText.Instance.Hide();

        outlineColor.a = 0;
        spriteRenderer.material.SetColor("_SolidOutline", outlineColor);
    }

    protected virtual void ChangeGatherState()
    {
        isGatherable = false;
    }

    protected virtual void ResetGatherState()
    {
        isGatherable = true;
        LoggingManager.Instance.RegisterGather(this);
    }

    protected virtual bool CanStartInteraction()
    {
        return true;
    }

    public void CancelGathering()
    {
        isInteracting = false;
    }

    protected void GetItem(string itemId)
    {
        Debug.Log($"Get Item : {itemId}");
    }

    /// <summary>
    /// 해골 상호작용 관련 메서드
    /// </summary>
    public void ExecuteGatherBySkull(SkullController skull, ToolType toolType)
    {
        currentToolType = toolType;   // 해골이 든 도구도 동일한 필드에 반영

        currentHit++;
        Debug.Log($"현재 도구 : {toolType} / 도끼질 횟수 : {currentHit}");
        if (currentHit == requiredHitCount)
        {
            LoggingManager.Instance.UnregisterGather(this);
            currentHit = 0;
            if(isSingleUse) // 한번만 사용가능한 채집자원이면
            {
                gameObject.SetActive(false);
            }
            else // 재사용 가능한 채집자원이면
            {
                ChangeGatherState();
                StartCoroutine(RespawnAfterDelay(respawnTime));
            }
        }
        // 아이템 관련 코드 추가
    }

    IEnumerator RespawnAfterDelay(float delay)
    {
        yield return CoroutineCaching.WaitForSeconds(delay);
        ResetGatherState();
    }

    public bool CanBeGatheredBy(UnityEngine.Object worker)
    {
        return isActiveAndEnabled && isGatherable && !isInteracting && _reservation.CanReserve(worker);
    }

    public bool TryReserve(UnityEngine.Object worker)
    {
        return CanBeGatheredBy(worker) && _reservation.TryReserve(worker);
    }

    public bool IsReservedBy(UnityEngine.Object worker)
    {
        return _reservation.IsReservedBy(worker);
    }

    public void Release(UnityEngine.Object worker)
    {
        _reservation.Release(worker);
    }
}