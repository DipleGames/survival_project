using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class QuickSlot : MonoBehaviour,
    IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler // 퀵슬롯 클릭 이벤트 처리를 위해 추가함
{
    [SerializeField] Image itemImage;
    [SerializeField] ItemInventoryDrag dragSlot;
    [SerializeField] QuickSlotManager quickSlotManager;
    [SerializeField] Image slotBackground;
    [SerializeField] private QuickSlotController quickSlotController; // 퀵슬롯 클릭 이벤트 처리를 위해 추가함
    [SerializeField] private TMP_Text quantityText; // 아이템 수량 표시를 위한 TMP_Text 필드 추가
    private static readonly HashSet<QuickSlot> allSlots = new();
    ItemInfo itemInfo;

    // 다른 슬롯에서 현재 아이템을 조회하기 위한 프로퍼티
    public ItemInfo ItemInfo => itemInfo;

    private void Awake()
    {
        allSlots.Add(this);
        // 게임 시작 시 빈 슬롯 상태로 초기화
        SetItemDirect(null);
    }

    private void OnDestroy()
    {
        allSlots.Remove(this);
    }

    // 퀵슬롯에 아이템 설정
    public void SetItem(ItemInfo item)
    {
        // 이미 다른 퀵슬롯에 등록된 동일 아이템 제거
        if (item != null)
        {
            foreach (QuickSlot slot in allSlots)
            {
                if (slot != this && slot.itemInfo?.itemId == item.itemId)
                {
                    slot.SetItemDirect(null);
                }
            }
        }

        SetItemDirect(item);
    }
    // 아이템 정보와 이미지만 직접 변경
    private void SetItemDirect(ItemInfo item)
    {
        itemInfo = item;

        // 아이템 아이콘 변경
        itemImage.sprite = item == null ? null : Resources.Load<Sprite>($"Item/{item.itemId}");

        itemImage.enabled = item != null;

        // 기존 슬롯 배경 유지, 색상만 변경
        slotBackground.color = item == null ? Color.white : new Color(0.85f, 0.7f, 0.5f, 1f);

        // 아이템 등록 및 제거 시 수량도 갱신
        RefreshQuantity();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 코드 QuickSlotManager에서 처리
        quickSlotManager.HandleDrop(this, eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemInfo == null) return;

        dragSlot.itemInfo = itemInfo;
        dragSlot.GetComponentInChildren<Image>().sprite = itemImage.sprite;
        dragSlot.gameObject.SetActive(true);

        // 원본 아이템은 드롭 성공 전까지 유지
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (itemInfo == null) return;

        RectTransform dragRect = dragSlot.GetComponent<RectTransform>();

        RectTransform parentRect = dragRect.parent as RectTransform;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector3 worldPos))
        {
            dragRect.position = worldPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragSlot.gameObject.SetActive(false);
    }
    public static void ClearItemFromAll(int itemId)
    {
        foreach (QuickSlot slot in allSlots)
        {
            if (slot.itemInfo?.itemId == itemId)
            {
                slot.SetItemDirect(null);
            }
        }
    }
    //클릭으로 아이템을 사용할 수 있도록 처리하는 메서드
    public void OnPointerClick(PointerEventData eventData) 
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            quickSlotController.TryUseSlot(this);
        }
    }
    public void RefreshQuantity()
    {
        // 빈 슬롯이라면 수량 숨김
        if (itemInfo == null || ItemManager.Instance == null)
        {
            quantityText.gameObject.SetActive(false);
            return;
        }

        // 실제 인벤토리 보유 수량 조회
        int count = ItemManager.Instance.GetItemCount(itemInfo.itemId);

        // 수량이 2개 이상일 때만 표시
        bool showQuantity = count >= 2;

        quantityText.gameObject.SetActive(showQuantity);

        if (showQuantity)
            quantityText.text = count.ToString();
    }

}
