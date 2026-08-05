using UnityEngine;

public class ParabolaLineRenderer : MonoBehaviour
{
    [SerializeField] protected int height = 3;

    protected Vector3 startPos, endPos;

    LineRenderer lineRenderer;
    protected Character character;

    Color activeColor;
    Color inactiveColor = new Color(0.15f, 0.4f, 0);

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        character = Character.Instance;
        activeColor = lineRenderer.material.color;
    }

    void Update()
    {
        GetClickPos(character.transform.position);

        for (int i = 0; i < lineRenderer.positionCount; i++)
            lineRenderer.SetPosition(i, ParabolicPos(startPos, endPos, i, lineRenderer.positionCount - 1));
    }

    protected void GetClickPos(Vector3 _startPos)
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.y = 0;

        startPos = _startPos;

        Vector3 distance = startPos - mouse;

        if(distance.magnitude > 5)
            distance = distance.normalized * 5;

        endPos = startPos - distance;
    }

    protected Vector3 ParabolicPos(Vector3 start, Vector3 end, float index, float total)
    {
        // lerp = a + (b - a) * t
        Vector3 lerpPos = Vector3.Lerp(start, end, index / total);
        float offset = Mathf.Sin(index / total * Mathf.PI) * height;
        
        Vector3 point = lerpPos + (Vector3.forward * offset);

        return point;
    }

    public void ChangeLineColor(bool canFire)
    {
        lineRenderer.material.color = canFire ? activeColor : inactiveColor;
    }
}
