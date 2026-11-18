using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExecutionWarningVFX : MonoBehaviour
{
    [Header("경고 영역 설정")]
    [SerializeField, Range(3, 100)]
    private int segments = 40;

    [Header("경고 색상")]
    [SerializeField]
    private Color startColor = new Color(1f, 0f, 0f, 0.15f);

    [SerializeField]
    private Color endColor = new Color(1f, 0f, 0f, 0.65f);

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh warningMesh;
    private Material warningMaterial;
    private Tween warningTween;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        warningMesh = new Mesh();
        warningMesh.name = "Execution Warning Mesh";
        meshFilter.mesh = warningMesh;

        // 원본 Material을 변경하지 않도록 인스턴스 사용
        warningMaterial = meshRenderer.material;
        meshRenderer.enabled = false;
    }

    public void Show(Vector3 direction, float radius, float angle, float duration)
    {
        warningTween?.Kill();
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.left;

        direction.Normalize();

        // 보스의 월드 방향을 로컬 방향으로 변환
        Vector3 localDirection = transform.InverseTransformDirection(direction);
        float centerAngle = Mathf.Atan2(localDirection.z, localDirection.x) * Mathf.Rad2Deg;

        GenerateMesh(radius, angle, centerAngle);

        warningMaterial.color = startColor;
        meshRenderer.enabled = true;

        warningTween = warningMaterial.DOColor(endColor, duration).SetEase(Ease.InQuad);
    }

    private void GenerateMesh(float radius, float angle, float centerAngle)
    {
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];
        vertices[0] = Vector3.zero;

        float startAngle = centerAngle - angle * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = (startAngle + angle * i / segments) * Mathf.Deg2Rad;

            vertices[i + 1] = new Vector3(Mathf.Cos(currentAngle) * radius, 0f, Mathf.Sin(currentAngle) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            int index = i * 3;

            // XZ 평면에서 위쪽을 향하는 삼각형
            triangles[index] = 0;
            triangles[index + 1] = i + 2;
            triangles[index + 2] = i + 1;
        }

        warningMesh.Clear();
        warningMesh.vertices = vertices;
        warningMesh.triangles = triangles;
        warningMesh.RecalculateNormals();
        warningMesh.RecalculateBounds();
    }

    public void Hide()
    {
        warningTween?.Kill();
        warningTween = null;

        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Hide();
    }

    private void OnDestroy()
    {
        warningTween?.Kill();

        if (warningMesh != null)
            Destroy(warningMesh);
            
        if (warningMaterial != null)
            Destroy(warningMaterial);
    }
}
