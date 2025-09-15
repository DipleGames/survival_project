using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

enum RangeType
{
    Arc,
    Circle,
    Rectangle
};

public class EffectRange : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshFilter meshFilter;

    [SerializeField] int angle;
    [SerializeField] float radius;
    [SerializeField] int segments;
    int rangeMode;
    Mesh mesh;

    // rangeMode : {1:Arc}, {2:Circle}

    WeaponManager weaponManager;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();

        weaponManager = WeaponManager.Instance;

        segments = angle / 5;
    }

    private void Update()
    {
        if(weaponManager.chargeTime > 0f)
        {
            ShowEffectRange(weaponManager.rangeMode);
        }
    }

    void ShowEffectRange(int rangeMode)
    {
        switch(rangeMode)
        {
            case 0:
                DrawRange_Arc();
                break;
            case 1:
                DrawRange_Circle();
                break;
            default:
                Debug.Log("RangeMode Index Error!");
                return;
        }
    }

    void DrawRange_Arc()
    {
        if(mesh != null) mesh.Clear();

        float halfAngleRad = Mathf.Deg2Rad * angle / 2f;

        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = transform.position;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -halfAngleRad + (halfAngleRad * 2f) * i / segments;
            float x = Mathf.Sin(currentAngle) * radius;
            float z = Mathf.Cos(currentAngle) * radius;
            vertices[i + 1] = new Vector3(x, 0, z);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;        
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }
    void DrawRange_Circle()
    {

    }

}