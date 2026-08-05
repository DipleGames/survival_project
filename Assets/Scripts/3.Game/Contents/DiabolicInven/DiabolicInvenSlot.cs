using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiabolicInvenSlot : MonoBehaviour
{
    [SerializeField] Image canSetImage;

    DiabolicInven inven;

    bool isEmpty = true;

    public bool IsEmpty => isEmpty;

    DragUI dragUI;

    private void Start()
    {
        inven = DiabolicInven.Instance;
        dragUI = DragUI.Instance;
        canSetImage.gameObject.SetActive(false);
    }

    public void PointerEnter()
    {
        inven.currentIndex = transform.GetSiblingIndex();

        if (dragUI.DragItem == null)
            return;

        inven.ChangeSlotsColor();
    }

    public void PointerExit()
    {
        if (dragUI.DragItem == null)
            return;

        inven.currentIndex = -1;
        inven.ChagneCanSetImage(false);
        inven.OffSlotSetImage();
    }

    public void ChangeCanSetImageColor(bool canImageSet)
    {
        canSetImage.color = canImageSet ? Color.blue : Color.red;
        canSetImage.gameObject.SetActive(true);
    }

    public void OffCanSetImage()
    {
        canSetImage.gameObject.SetActive(false);
    }

    public void ChagneEmptyState(bool _isEmpty)
    {
        isEmpty = _isEmpty;
    }
}
