using UnityEngine;

public class RadialBarrageWarningVFX : MonoBehaviour
{
    private readonly Mesh[] meshes = new Mesh[2];
    private readonly Material[] materials = new Material[2];

    public void Show(float radius, float gapDirection, float gapAngle, Material material,
        Color dangerColor, Color safeColor)
    {
        CreateSector(0, "위험 구역", radius, gapDirection + gapAngle * 0.5f,
            360f - gapAngle, material, dangerColor);
        CreateSector(1, "다음 탄막 안전지대", radius, gapDirection - gapAngle * 0.5f,
            gapAngle, material, safeColor);
    }

    private void CreateSector(int index, string label, float radius, float startAngle,
        float angle, Material material, Color color)
    {
        const int segments = 80;
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        for (int i = 0; i <= segments; i++)
        {
            float radians = (startAngle + angle * i / segments) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(radians) * radius, 0f,
                Mathf.Sin(radians) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 2;
            triangles[i * 3 + 2] = i + 1;
        }

        meshes[index] = new Mesh { name = label };
        meshes[index].vertices = vertices;
        meshes[index].triangles = triangles;
        meshes[index].RecalculateNormals();
        meshes[index].RecalculateBounds();
        materials[index] = new Material(material);
        materials[index].color = color;

        GameObject sector = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));

        sector.transform.SetParent(transform, false);
        sector.GetComponent<MeshFilter>().sharedMesh = meshes[index];
        sector.GetComponent<MeshRenderer>().sharedMaterial = materials[index];
    }

    private void OnDestroy()
    {
        for (int i = 0; i < meshes.Length; i++)
        {
            if (meshes[i] != null) 
            {
                Destroy(meshes[i]);
            }
            
            if (materials[i] != null) 
            {
                Destroy(materials[i]);
            }
        }
    }
}
