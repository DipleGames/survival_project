using System.Collections;
using UnityEngine;

public interface IMouseInteraction
{
    void InteractionLeftButtonFuc(GameObject hitObject);
    void InteractionRightButtonFuc(GameObject hitObject);

    void CanInteraction(bool _canInteraction);
    bool ReturnCanInteraction();
    IEnumerator EndInteraction(Animator anim, float waitTime);
}

public class MouseInteraction : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;

    Player player;

    private void Start()
    {
        player = GameManager.Instance.Player;
    }

    private void Update()
    {

        if (Input.GetMouseButtonUp(0))
        {
            MouseInteractionFuc((gameObject) => { gameObject.GetComponent<IMouseInteraction>().InteractionLeftButtonFuc(gameObject); });
        }

        else if (Input.GetMouseButtonUp(1))
        {
            MouseInteractionFuc((gameObject) => { gameObject.GetComponent<IMouseInteraction>().InteractionRightButtonFuc(gameObject); });
        }
    }

    void MouseInteractionFuc(System.Action<GameObject> interactionFuc)
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hit = Physics.RaycastAll(ray, 100, layerMask);

        if (hit.Length <= 0)
            return;

        for (int i = 0; i < hit.Length; i++)
        {
            var interactable = hit[i].transform.gameObject.GetComponent<IMouseInteraction>();

            if (interactable != null)
            {
                interactionFuc(hit[i].transform.gameObject);

                return;
            }
        }
    }
}
