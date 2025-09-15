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
    Collider detactedOne;

    Ray ray;
    Vector3 mouseWorldPos;

    // WeaponManager weaponManager;

    private void Awake()
    {
        // weaponManager = WeaponManager.Instance;
        detactedMonsters = new List<Collider>();
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))      // 광선이 어떤 물체에 닿았는지 확인
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

    public Collider FindNearest()
    {
        Vector3 attackDir = (mouseWorldPos - transform.position).normalized;
        var monsters = Physics.OverlapSphere(transform.position, detactRadius, detactLayer);

        detactedOne = null;
        float targetDist = 100f;
        float tempDist;

        if (monsters.Length == 0)
            return null;

        foreach (var monster in monsters)
        {
            Vector3 monDir = (monster.transform.position - transform.position).normalized;

            if (Vector3.Angle(attackDir, monDir) < detactAngle / 2)
            {
                tempDist = GetDistance(transform.position, monster.transform.position);

                if (detactedOne == null)
                {
                    detactedOne = monster;
                    targetDist = tempDist;
                }
                else if (targetDist > tempDist)
                {
                    detactedOne = monster;
                }
            }
        }
        
        return detactedOne;
    }

    private float GetDistance(Vector3 target1, Vector3 target2)
    {
        Vector3 temp1 = target1;
        Vector3 temp2 = target2;
        temp1.y = 0f;
        temp2.y = 0f;

        return Vector3.Distance(temp1, temp2);
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