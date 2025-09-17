
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemInvenSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemCount;
    
    GameObject dragSlot;
    Item item;

    [HideInInspector] public ItemInventory inventory;

    public void SetSlot(Item item)
    {
        this.item = item;

        itemImage.gameObject.SetActive(item != null);
        itemCount.gameObject.SetActive(item != null);

        if (item != null)
        {
            itemImage.sprite = Resources.Load<Sprite>($"Item/{item.ItemId}");
            itemCount.text = $"x {GameManager.Instance.haveItems[item.ItemId]}";
        }
    }

    public void SetDescription()
    {
        if (item != null)
        {
            inventory.SetItemDescription(item);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null)
            return;

        if (dragSlot == null)
            dragSlot = inventory.DragSlot;

        dragSlot.GetComponent<ItemInventoryDrag>().item = item;
        dragSlot.GetComponentInChildren<Image>().sprite = itemImage.sprite;
        dragSlot.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item == null)
            return;

        dragSlot.GetComponent<ItemInventoryDrag>().RectTransform.anchoredPosition = eventData.position;
        dragSlot.SetActive(true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (item == null)
            return;

        dragSlot.gameObject.SetActive(false);
    }
}
