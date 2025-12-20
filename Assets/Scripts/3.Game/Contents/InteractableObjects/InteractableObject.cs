using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour, IMouseInteraction
{
    [SerializeField]protected bool isApproach = false;

    public virtual void InteractionLeftButtonFuc(GameObject hitObject)
    {
#if UNITY_EDITOR
        if(isApproach) Debug.Log($"{gameObject.name}");
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Character"))
        {
            UIFloater.Instance.RequestShowClickUI(transform, this);
            isApproach = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Character"))
        {
            UIFloater.Instance.RequestHideClickUI(this);
            isApproach = false;
        }
    }

    public virtual void InteractionRightButtonFuc(GameObject hitObject)
    {
        throw new System.NotImplementedException();
    }
    public virtual void CanInteraction(bool _canInteraction)
    {
        throw new System.NotImplementedException();
    }
    public virtual IEnumerator EndInteraction(Animator anim, float waitTime)
    {
        throw new System.NotImplementedException();
    }
    public virtual bool ReturnCanInteraction()
    {
        throw new System.NotImplementedException();
    }
}
