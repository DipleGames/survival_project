
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

    public void OnDrag(PointerEventData eventData)
    {
        if (itemInfo == null)
            return;

        dragSlot.GetComponent<ItemInventoryDrag>().RectTransform.anchoredPosition = eventData.position;
        dragSlot.SetActive(true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemInfo == null)
            return;

        dragSlot.gameObject.SetActive(false);
    }
}
