using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class MonsterFinder : MonoBehaviour
{
    [SerializeField] float detactRadius;
    [Range(0f, 360f)]
    [SerializeField] float detactAngle;
    [SerializeField] LayerMask detactLayer;
    [HideInInspector]
    [SerializeField] public List<Collider> detactedMonsters;

    // Vector3 mousePos;
    Ray ray;
    Vector3 mouseWorldPos;

    WeaponManager weaponManager;

    private void Awake()
    {
        weaponManager = WeaponManager.Instance;
        detactedMonsters = new List<Collider>();
    }

    void Update()
    {
        // mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // mousePos.y = transform.position.y;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))  // 광선이 어떤 물체에 닿았는지 확인
            mouseWorldPos = hit.point;

        mouseWorldPos.y = transform.position.y;
    }

    public List<Collider> FindMonster()
    {
        Vector3 attackDir = (mouseWorldPos - transform.position).normalized;
        var monsters = Physics.OverlapSphere(transform.position, detactRadius, detactLayer);
        
        detactedMonsters.Clear();

        foreach (var monster in monsters) 
        {
            Vector3 monDir = (monster.transform.position - transform.position).normalized;

            if (Vector3.Angle(attackDir, monDir) < detactAngle / 2)
                detactedMonsters.Add(monster);
        }

        // Debug.Log(string.Join(',', detactedMonsters.Select(x => x.gameObject.name)));
        return detactedMonsters;
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.white;
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, detactRadius);

        Handles.color = Color.blue;
        Handles.DrawWireDisc(mouseWorldPos, Vector3.up, 0.1f);

        Vector3 viewAngleA = DirFromAngle(-detactAngle / 2);
        Vector3 viewAngleB = DirFromAngle(detactAngle / 2);

        Handles.color = Color.red;
        Handles.DrawLine(transform.position, transform.position + viewAngleA * detactRadius);
        Handles.DrawLine(transform.position, transform.position + viewAngleB * detactRadius);
    }

    public Vector3 DirFromAngle(float angleInDegrees)
    {
        Vector3 dirToMouse = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(dirToMouse.z, dirToMouse.x) * Mathf.Rad2Deg;

        float finalAngle = angle + angleInDegrees;

        float rad = finalAngle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
    }
}