using UnityEngine;

public class CheckCantObstacleSet : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Obstacle"))
        {
            transform.parent.parent.GetComponent<IMouseInteraction>().CanInteraction(false);
            transform.parent.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            transform.parent.parent.GetComponent<IMouseInteraction>().CanInteraction(true);
            transform.parent.GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }
}
