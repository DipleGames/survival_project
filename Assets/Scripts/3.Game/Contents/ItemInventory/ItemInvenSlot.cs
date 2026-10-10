
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemInvenSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemCount;
    
    GameObject dragSlot;
    ItemInfo itemInfo;

    [HideInInspector] public ItemInventory inventory;

    public void SetSlot(ItemInfo itemInfo)
    {
        this.itemInfo = itemInfo;

        itemImage.gameObject.SetActive(itemInfo != null);
        itemCount.gameObject.SetActive(itemInfo != null);

        if (itemInfo != null)
        {
            itemImage.sprite = Resources.Load<Sprite>($"Item/{itemInfo.itemId}");
            itemCount.text = $"x {GameManager.Instance.haveItems[itemInfo.itemId]}";
        }
    }

    public void SetDescription()
    {
        if (itemInfo != null)
        {
            inventory.SetItemDescription(itemInfo);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemInfo == null)
            return;

        if (dragSlot == null)
            dragSlot = inventory.DragSlot;

        dragSlot.GetComponent<ItemInventoryDrag>().itemInfo = itemInfo;
        dragSlot.GetComponentInChildren<Image>().sprite = itemImage.sprite;
        dragSlot.gameObject.SetActive(true);
    }

    // 인벤토리 슬롯에서 아이템을 드래그할 때 ItemInventory의 MoveDragSlot()을 호출
    public void OnDrag(PointerEventData eventData)
    {
        if (itemInfo == null) return;

        inventory.MoveDragSlot(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemInfo == null)
            return;

        dragSlot.gameObject.SetActive(false);
    }
}
