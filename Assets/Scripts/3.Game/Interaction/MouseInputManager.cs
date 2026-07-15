using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MouseInputManager : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;

    private IHoverable currentHover;

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            MouseInteractionFuc((gameObject) => { gameObject.GetComponent<IInteractable>().InteractionLeftButtonFuc(gameObject); });
        }

        else if (Input.GetMouseButtonUp(1))
        {
            MouseInteractionFuc((gameObject) => { gameObject.GetComponent<IInteractable>().InteractionRightButtonFuc(gameObject); });
        }

        HoverInteract();
    }

    private void HoverInteract()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        IHoverable newHover = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerMask))
        {
            newHover = hit.collider.GetComponent<IHoverable>();
        }

        if (currentHover == newHover)
            return;

        currentHover?.OnHoverExit();
        currentHover = newHover;
        currentHover?.OnHoverEnter();
    }

    void MouseInteractionFuc(System.Action<GameObject> interactionFuc)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hit = Physics.RaycastAll(ray, 100, layerMask);

        if (hit.Length <= 0)
            return;

        for (int i = 0; i < hit.Length; i++)
        {
            var interactable = hit[i].transform.gameObject.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactionFuc(hit[i].transform.gameObject);

                return;
            }
        }
    }
}