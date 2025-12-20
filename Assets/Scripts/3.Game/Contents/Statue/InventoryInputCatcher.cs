using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class InventoryInputCatcher : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private ItemDragManager dragManager;
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private ScrollRect scrollRect;

    private readonly List<RaycastResult> _hits = new();
    private bool _forwardingToScroll; // 이번 제스처가 ScrollRect로 포워딩 중인지
    private bool _draggingItem;       // 이번 제스처가 아이템 드래그인지

    private void Awake()
    {
        dragManager = ItemDragManager.Instance;
        if (raycaster == null)
            raycaster = GetComponentInParent<GraphicRaycaster>();
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();
        if (scrollRect == null)
            scrollRect = GetComponentInParent<ScrollRect>();
    }

    // 드래그 후보 단계에서 어떤 드래그를 할지 결정
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        _draggingItem = false;
        _forwardingToScroll = false;

        var item = RaycastFor<ItemUI>(eventData);
        if (item != null)
        {
            // 아이템 드래그 예정
            _draggingItem = true;
        }
        else
        {
            // 빈 공간 → ScrollRect로 드래그 포워딩
            _forwardingToScroll = (scrollRect != null);
            if (_forwardingToScroll)
                scrollRect.OnInitializePotentialDrag(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_draggingItem)
        {
            var item = RaycastFor<ItemUI>(eventData);
            if (item != null)
                dragManager.StartDrag(item, eventData);
            return;
        }

        if (_forwardingToScroll && scrollRect != null)
        {
            scrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggingItem)
        {
            dragManager.Drag(eventData);
            return;
        }

        if (_forwardingToScroll && scrollRect != null)
        {
            scrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_draggingItem)
        {
            dragManager?.EndDrag(eventData);
        }
        else if (_forwardingToScroll && scrollRect != null)
        {
            scrollRect.OnEndDrag(eventData);
        }

        _draggingItem = false;
        _forwardingToScroll = false;
    }

    void OnDisable()
    {
        _draggingItem = false;
        _forwardingToScroll = false;
    }

    // 커서 아래에서 T 컴포넌트(혹은 부모 중 T)를 찾는다.
    private T RaycastFor<T>(PointerEventData eventData) where T : Component
    {
        _hits.Clear();
        raycaster.Raycast(eventData, _hits);
        foreach (var h in _hits)
        {
            if (h.gameObject.TryGetComponent<T>(out var t)) return t;
            var p = h.gameObject.GetComponentInParent<T>();
            if (p) return p;
        }
        return null;
    }

}
