using UnityEngine;

public class CameraView : MonoBehaviour
{
    [Header("현재 장소")]
    [SerializeField] private GameObject _currentPlace;

    [Header("추적 대상")]
    [SerializeField] private Transform _target;

    [Header("카메라 설정")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _followSpeed = 5f;

    private Collider _currentBoundary;
    private Vector3 _offset;

    private void Awake()
    {
        if (_mainCamera == null)
            _mainCamera = GetComponent<Camera>();

        if (_target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
                _target = player.transform;
        }
    }

    private void Start()
    {
        if (_target == null)
        {
            Debug.LogError("카메라가 따라갈 Player를 찾지 못했습니다.");
            enabled = false;
            return;
        }

        _offset = transform.position - _target.position;

        if (_currentPlace != null)
            SetCurrentPlace(_currentPlace);
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = _target.position + _offset;

        if (_currentBoundary != null)
            targetPosition = ClampPosition(targetPosition);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            _followSpeed * Time.deltaTime
        );
    }

    public void SetCurrentPlace(GameObject newPlace)
    {
        if (newPlace == null)
        {
            Debug.LogWarning("새로운 장소가 null입니다.");
            return;
        }

        _currentPlace = newPlace;

        CameraBoundary boundary = _currentPlace.GetComponentInChildren<CameraBoundary>();

        if (boundary == null)
        {
            Debug.LogWarning(
                $"{_currentPlace.name}에서 CameraBoundary를 찾지 못했습니다."
            );

            _currentBoundary = null;
            return;
        }

        _currentBoundary = boundary.BoundaryCollider;
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        Bounds bounds = _currentBoundary.bounds;

        float cameraHalfHeight = _mainCamera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * _mainCamera.aspect;

        float minX = bounds.min.x + cameraHalfWidth;
        float maxX = bounds.max.x - cameraHalfWidth;

        float minY = bounds.min.y + cameraHalfHeight;
        float maxY = bounds.max.y - cameraHalfHeight;

        if (minX > maxX)
            position.x = bounds.center.x;
        else
            position.x = Mathf.Clamp(position.x, minX, maxX);

        if (minY > maxY)
            position.y = bounds.center.y;
        else
            position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }
}