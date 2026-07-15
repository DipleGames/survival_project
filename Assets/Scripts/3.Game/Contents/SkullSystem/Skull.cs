using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Skull : InteractableObj
{
    public GameObject ui;

    public override void InteractionLeftButtonFuc(GameObject hitObject)
    {
        ui.SetActive(true);
    }
    public override void InteractionRightButtonFuc(GameObject hitObject)
    {
        
    }

    public override void BeginInteraction()
    {
        
    }
    public override void EndInteraction()
    {
        
    }

    public override void OnHoverEnter()
    {
        
    }
    public override void OnHoverExit()
    {
        
    }
}
