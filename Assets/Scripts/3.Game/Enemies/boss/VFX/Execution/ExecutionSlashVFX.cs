using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExecutionSlashVFX : MonoBehaviour
{
    [SerializeField, Range(3, 100)]
    private int segments = 40;

    [SerializeField, Range(0.01f, 1f)]
    private float innerRadiusRatio = 0.65f;

    [SerializeField, Min(0.01f)]
    private float effectDuration = 0.25f;

    [SerializeField]
    private Color slashColor =
        new Color(1f, 0.25f, 0.15f, 0.9f);

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh slashMesh;
    private Material slashMaterial;
    private Tween effectTween;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        slashMesh = new Mesh();
        slashMesh.name = "Execution Slash Mesh";

        meshFilter.mesh = slashMesh;
        slashMaterial = meshRenderer.material;

        meshRenderer.enabled = false;
    }

    public void Play(Vector3 direction, float radius, float angle)
    {
        if (radius <= 0f)
            return;

        effectTween?.Kill();

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.left;

        direction.Normalize();

        Vector3 localDirection = transform.InverseTransformDirection(direction);

        float centerAngle = Mathf.Atan2(localDirection.z, localDirection.x) * Mathf.Rad2Deg;

        GenerateMesh(radius, angle, centerAngle);

        slashMaterial.color = slashColor;
        meshRenderer.enabled = true;

        effectTween = slashMaterial
            .DOFade(0f, effectDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                meshRenderer.enabled = false;
            });
    }

    private void GenerateMesh(float radius, float angle, float centerAngle)
    {
        Vector3[] vertices = new Vector3[(segments + 1) * 2];

        int[] triangles = new int[segments * 6];

        float innerRadius = radius * innerRadiusRatio;
        float startAngle = centerAngle - angle * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = (startAngle + angle * i / segments) * Mathf.Deg2Rad;

            float cos = Mathf.Cos(currentAngle);
            float sin = Mathf.Sin(currentAngle);

            int vertexIndex = i * 2;

            vertices[vertexIndex] = new Vector3(cos * innerRadius, 0f, sin * innerRadius);

            vertices[vertexIndex + 1] = new Vector3( cos * radius, 0f, sin * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            int index = i * 6;
            int vertexIndex = i * 2;

            triangles[index] = vertexIndex;
            triangles[index + 1] = vertexIndex + 3;
            triangles[index + 2] = vertexIndex + 1;

            triangles[index + 3] = vertexIndex;
            triangles[index + 4] = vertexIndex + 2;
            triangles[index + 5] = vertexIndex + 3;
        }

        slashMesh.Clear();
        slashMesh.vertices = vertices;
        slashMesh.triangles = triangles;

        slashMesh.RecalculateNormals();
        slashMesh.RecalculateBounds();
    }

    public void Stop()
    {
        effectTween?.Kill();
        effectTween = null;

        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Stop();
    }

    private void OnDestroy()
    {
        effectTween?.Kill();

        if (slashMesh != null)
            Destroy(slashMesh);

        if (slashMaterial != null)
            Destroy(slashMaterial);
    }
}
