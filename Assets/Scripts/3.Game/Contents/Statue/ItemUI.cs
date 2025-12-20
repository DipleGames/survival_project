// ItemUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image icon;
    [SerializeField] TMP_Text countText;

    [HideInInspector] public InventorySlot currentSlot;

    public Item Item { get; private set; }
    public int Count { get; private set; }

    private void Awake()
    {
        if (icon == null) icon = GetComponent<Image>();
        if (countText == null) countText = GetComponentInChildren<TMP_Text>();
    }

    public void Init(InventorySlot slot, Item _item, int count)
    {
        currentSlot = slot;

        Item = _item;
        Count = count;

        icon.sprite = Resources.Load<Sprite>($"Item/{_item.ImageId}");
        countText.text = count.ToString();

        gameObject.SetActive(true);
    }

    public void SetCount(int count)
    {
        Count = count;
        if (countText) countText.text = count.ToString();
    }
}
