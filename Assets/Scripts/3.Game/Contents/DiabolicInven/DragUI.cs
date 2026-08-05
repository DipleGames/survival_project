using UnityEngine;
using UnityEngine.UI;

public class DragUI : Singleton<DragUI>
{
    [SerializeField] Canvas canvas;
    [SerializeField] Image dragItemImage;

    DiabolicItemInfo dragItem;

    public DiabolicItemInfo DragItem => dragItem;

    public bool isDragging = false;


    private void Start()
    {
        dragItemImage.gameObject.SetActive(false);
    }

    public void SettingDragUI(DiabolicItemInfo selectedItem)
    {
        dragItem = selectedItem;

        if (dragItem != null)
        {
            dragItemImage.sprite = selectedItem.ItemSprite;
            transform.localScale = new Vector3(selectedItem.ItemShape.Width, selectedItem.ItemShape.Height, 0);
           
            GetComponent<RectTransform>().pivot = new Vector2(1 / (selectedItem.ItemShape.Width * 2f), 1 - 1 / (selectedItem.ItemShape.Height * 2f));
           
            dragItemImage.gameObject.SetActive(true);

            Cursor.visible = false;
            GameManager.Instance.isCursorVisible = false;
        }
    }

    public void MoveDragUI()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, Camera.main, out Vector2 mousePos);

        GetComponent<RectTransform>().localPosition = mousePos;
    }

    public void OffDragUI()
    {
        dragItemImage.gameObject.SetActive(false);
        dragItem = null;

        Cursor.visible = true;
        GameManager.Instance.isCursorVisible = true;
    }
}
