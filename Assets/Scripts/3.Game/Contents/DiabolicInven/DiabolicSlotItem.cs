using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiabolicSlotItem : MonoBehaviour
{
    [SerializeField] Image itemImage;
    [SerializeField] AudioClip unEquipSound;

    DragUI dragUI;
    GameManager gameManager;
    DiabolicInven inven;
    ItemManager itemManager;
    Character character;
    SoundManager soundManager;

    DiabolicItemInfo itemData;
    Dictionary<Status, int> itemStatus;
    Dictionary<SpecialStatus, bool> itemSpecialStatus;

    List<int> indexes;
    int firstIndex;

    int pieceSlotIndex;
    int itemQuantity;

    private void Awake()
    {
        dragUI = DragUI.Instance;
        gameManager = GameManager.Instance;
        inven = DiabolicInven.Instance;
        itemManager = ItemManager.Instance;
        character = Character.Instance;
        soundManager = SoundManager.Instance;

        //GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    private void OnEnable()
    {
        if (itemManager.currentEquipItems == null)
            return;

        foreach(var equipItemData in itemManager.currentEquipItems)
        {
            if(equipItemData.Key == itemData)
            {
                if(itemQuantity != itemManager.itemQuantity[pieceSlotIndex])
                {
                    Dictionary<Status, int> itemStatus = itemData.Stat();

                    for (int i = 0; i < gameManager.status.Count; i++)
                    {
                        if (itemStatus[(Status)i] == 0)
                            continue;

                        gameManager.status[(Status)i] += itemStatus[(Status)i] * (itemManager.itemQuantity[pieceSlotIndex] - itemQuantity);
                    }

                    character.UpdateStat();
                }

                break;
            }
        }
    }

    private void Update()
    {
        SubtractItem();
    }

    public void ItemSetOnInventory(Vector3 instantPos, DiabolicItemInfo item, List<int> _indexes, int _firstIndex, int _pieceSlotIndex, int _itemQuantity)
    {
        itemData = item;
        SetImage(item);
        GetComponent<RectTransform>().anchoredPosition = instantPos;
        indexes = _indexes;
        firstIndex = _firstIndex;
        pieceSlotIndex = _pieceSlotIndex;
        itemQuantity = _itemQuantity;
    }

    void SetImage(DiabolicItemInfo item)
    {
        if(dragUI == null)
            dragUI = DragUI.Instance;

        GetComponent<RectTransform>().localScale = dragUI.transform.localScale;
        GetComponent<RectTransform>().pivot = dragUI.GetComponent<RectTransform>().pivot;
        itemImage.sprite = item.ItemSprite;
    }

    void SubtractItem()
    {
        if (Input.GetMouseButtonUp(1))
        {
            bool canSub = false;

            foreach (var i in indexes)
            {
                if(i == inven.currentIndex)
                {
                    canSub = true; 
                    break;
                }
            }

            if (canSub)
            {
                itemStatus = itemData.Stat();
                itemSpecialStatus = itemData.SpecialStat();

                for (int i = 0; i < gameManager.status.Count; i++)
                {
                    if (itemStatus[(Status)i] == 0)
                        continue;

                    gameManager.status[(Status)i] -= itemStatus[(Status)i] * itemManager.itemQuantity[pieceSlotIndex];
                }

                for (int i = 0; i < gameManager.specialStatus.Count; ++i)
                {
                    if (itemSpecialStatus[(SpecialStatus)i])
                        gameManager.specialStatus[(SpecialStatus)i] = false;
                }

                int row = firstIndex / 4;
                int column = firstIndex % 4;

                inven.SetSlotIsEmpty(itemData.ItemShape.Height, itemData.ItemShape.Width, itemData.ItemShape.Shape, row, column, true);

                Destroy(gameObject);

                soundManager.PlaySFX(unEquipSound);

                Character.Instance.UpdateStat();
                PieceItemInven.Instance.SlotBlockImage(pieceSlotIndex);
                DiabolicInven.Instance.canSetImage = false;
            }
        }
    }
}
