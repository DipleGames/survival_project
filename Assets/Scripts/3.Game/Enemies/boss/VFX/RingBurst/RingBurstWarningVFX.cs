using UnityEngine;

public class RingBurstWarningVFX : MonoBehaviour
{
    private Mesh mesh;
    private Material instanceMaterial;

    public void Show(float inner, float outer, Material material, Color color)
    {
        const int segments = 128;
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 6];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            vertices[i * 2] = direction * inner;
            vertices[i * 2 + 1] = direction * outer;
            if (i == segments) continue;
            int v = i * 2;
            int t = i * 6;
            triangles[t] = v;
            triangles[t + 1] = v + 2;
            triangles[t + 2] = v + 1;
            triangles[t + 3] = v + 1;
            triangles[t + 4] = v + 2;
            triangles[t + 5] = v + 3;
        }
        mesh = new Mesh { name = "Ring Burst Area" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        instanceMaterial = new Material(material);
        instanceMaterial.color = color;
        gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = instanceMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private void OnDestroy()
    {
        if (mesh != null) Destroy(mesh);
        if (instanceMaterial != null) Destroy(instanceMaterial);
    }
}
