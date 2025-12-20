using UnityEngine.EventSystems;

public interface IDropTarget
{
    bool CanDrop(ItemUI item, PointerEventData eventData);
    void Drop(ItemUI item, PointerEventData eventData);
}