using System;
using System.Collections;
using UnityEngine;

public interface IInteractable
{
    void InteractionLeftButtonFuc(GameObject hitObject);
    void InteractionRightButtonFuc(GameObject hitObject);
    void BeginInteraction();
    void EndInteraction();
}
public interface IHoverable
{
    void OnHoverEnter();
    void OnHoverExit();
}