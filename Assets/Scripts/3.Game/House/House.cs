using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class House : MonoBehaviour, IMouseInteraction, IDamageable
{
    private bool isPlayerInHouse = false;
    void Start()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Character"))
        {
            isPlayerInHouse = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Character"))
        {
            isPlayerInHouse = false;
        }
    }

    public bool GetIsPlayerInHouse()
    {
        return isPlayerInHouse;
    }

    public void InteractionLeftButtonFuc(GameObject hitObject)
    {

    }

    public void InteractionRightButtonFuc(GameObject hitObject)
    {
        throw new System.NotImplementedException();
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

    public void Attacked(float damage, GameObject hitObject)
    {
        Debug.Log("House Attacked");
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        
    }
}
