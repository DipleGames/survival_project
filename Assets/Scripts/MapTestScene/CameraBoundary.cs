using UnityEngine;

public class CameraBoundary : MonoBehaviour
{
    [SerializeField] private Collider _boundaryCollider;

    public Collider BoundaryCollider => _boundaryCollider;

    private void Reset()
    {
        _boundaryCollider = GetComponent<Collider>();
    }
}