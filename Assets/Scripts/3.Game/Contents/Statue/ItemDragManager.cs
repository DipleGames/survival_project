using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragManager : MonoBehaviour
{
    public static ItemDragManager Instance { get; private set; }

    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private GameObject dragGhostPrefab;

    private RectTransform ghostRT;
    private Image ghostImage;

    private ItemUI draggingItem;
    private InventorySlot originSlot;
    private readonly List<RaycastResult> _hits = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void EnsureGhost()
    {
        if (ghostRT) return;
        var go = Instantiate(dragGhostPrefab, rootCanvas.transform);
        ghostRT = go.transform as RectTransform;
        ghostImage = go.GetComponent<Image>();
        if (ghostImage) ghostImage.raycastTarget = false;
        go.SetActive(false);
    }

    public void StartDrag(ItemUI item, PointerEventData e)
    {
        if (draggingItem != null || item == null) return;

        draggingItem = item;
        originSlot = item.currentSlot;

        // 원본은 반투명+레이캐스트 차단
        var cg = item.GetComponent<CanvasGroup>();
        if (!cg) cg = item.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;

        // 고스트
        EnsureGhost();
        if (ghostImage && item.icon) ghostImage.sprite = item.icon.sprite;
        ghostRT.gameObject.SetActive(true);
        ghostRT.SetAsLastSibling();

        var parentRect = ghostRT.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, e.position, Camera.main, out var local))
        {
            ghostRT.anchoredPosition = local;
        }
    }

    public void Drag(PointerEventData e)
    {
        if (!draggingItem) return;
        
        var parentRect = ghostRT.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, e.position, Camera.main, out var local))
        {
            ghostRT.anchoredPosition = local;
        }
    }

    public void EndDrag(PointerEventData e)
    {
        if (!draggingItem) return;
        var target = RaycastForDropTarget(e);

        if (target != null && target.CanDrop(draggingItem, e))
            target.Drop(draggingItem, e);
        else
            if(originSlot != null) originSlot.SetItem(draggingItem);

        // 복구
        var cg = draggingItem.GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
        }
        ghostRT.gameObject.SetActive(false);

        draggingItem = null;
        originSlot = null;
    }

    private IDropTarget RaycastForDropTarget(PointerEventData e)
    {
        _hits.Clear();
        raycaster.Raycast(e, _hits);
        foreach (var h in _hits)
        {
            if (h.gameObject.TryGetComponent<IDropTarget>(out var t)) return t;
            var p = h.gameObject.GetComponentInParent<IDropTarget>();
            if (p != null) return p;
        }
        return null;
    }
}
