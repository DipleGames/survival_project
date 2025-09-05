using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseInteraction : MonoBehaviour, IMouseInteraction
{
    [SerializeField] private HouseUpgrade upgrade;
    [SerializeField] private HouseRepair repair;

    public void InteractionLeftButtonFuc(GameObject hitObject)
    {
        if (upgrade)
            upgrade.TryUpgrade();
    }

    public void InteractionRightButtonFuc(GameObject hitObject)
    {
        if (repair)
            repair.TryRepair();
    }

    public void CanInteraction(bool _canInteraction)
    {
        throw new System.NotImplementedException();
    }

    public bool ReturnCanInteraction()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator EndInteraction(Animator anim, float waitTime)
    {
        throw new System.NotImplementedException();
    }
}
