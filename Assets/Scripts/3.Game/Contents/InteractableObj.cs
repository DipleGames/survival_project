using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObj : MonoBehaviour, IInteractable, IHoverable
{
    protected bool canInteract;

    public virtual void InteractionLeftButtonFuc(GameObject hitObject) { }
    public virtual void InteractionRightButtonFuc(GameObject hitObject) { }

    public virtual void BeginInteraction() { }
    public virtual void EndInteraction() { }

    public virtual void OnHoverEnter() { }
    public virtual void OnHoverExit() { }
}
