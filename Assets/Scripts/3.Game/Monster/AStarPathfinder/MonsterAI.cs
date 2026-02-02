using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MonsterAI : MonoBehaviour
{
    public Animator anim;
    public SpriteRenderer spriteRenderer;

    [Header("Refs")]
    [SerializeField] private Transform target;          // Player
    [SerializeField] private AStarPathfinder pathfinder;

    [Header("Move")]
    public float MoveSpeed = 3.5f;
    public float WaypointReachDist = 0.15f;

    [Header("Repath")]
    public float RepathInterval = 0.25f;               // 0.25s마다 갱신
    public float RepathMinTargetMove = 0.5f;           // 타겟이 이만큼 움직였을 때만 갱신

    [Header("target")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject fence;

    private Vector3[] _path;
    private int _pathIndex;
    private float _repathTimer;
    private Vector3 _lastTargetPos;

    private bool isTouch = false;

    private void Awake()
    {
        player = GameObject.FindWithTag("Character");
    }

    private void Start()
    {
        target = player.transform;

        _lastTargetPos = target.position;
        RequestPath();
    }

    private void Update()
    {
        _repathTimer += Time.deltaTime;

        float targetMoved = Vector3.Distance(_lastTargetPos, target.position);
        if (_repathTimer >= RepathInterval || targetMoved >= RepathMinTargetMove) // &&에서 || 으로 Test
        {
            _repathTimer = 0f;
            _lastTargetPos = target.position;
            if(pathfinder.pathResult == PathResult.Success)
            { 
                RequestPath();
            }
        }
        if(pathfinder.pathResult == PathResult.Success)
        {
            FollowPath();
        }
        else if(pathfinder.pathResult == PathResult.Blocked)
        {
            if(!isTouch)
            {
                transform.position += (pathfinder.pathControllTower.baseCamp.transform.position - transform.position).normalized * MoveSpeed * Time.deltaTime;
            }
        }
    }
    private void RequestPath()
    {
        if (pathfinder.TryFindPath(transform.position, target.position, out Vector3[] waypoints))
        {
            _path = waypoints;
            _pathIndex = 0;
        }
    }

    private void FollowPath()
    {
        if (_path == null || _path.Length == 0 || _pathIndex >= _path.Length)
            return;

        Vector3 waypoint = _path[_pathIndex];
        waypoint.y = transform.position.y; // XZ 이동 고정

        Vector3 dir = (waypoint - transform.position).normalized;
        transform.position += dir * MoveSpeed * Time.deltaTime;
        Filp(dir);
        anim.SetBool("isWalk", true);

        if (Vector3.Distance(transform.position, waypoint) <= WaypointReachDist)
            _pathIndex++;
    }

    private void Filp(Vector3 dir)
    {
        spriteRenderer.flipX = dir.x > 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Fence"))
        {
            isTouch = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Fence"))
        {
            isTouch = false;
        }
    }

    float _tick = 0f;
    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Fence") && pathfinder.pathResult == PathResult.Blocked)
        {
            _tick += Time.deltaTime;
            if(_tick > 1.5f)
            {
                other.GetComponent<FenceModel>().FenceHP -= 10;
                _tick = 0f;
            }
        }
    }


    [Header("Gizmos")]
    public bool DrawPathGizmos = true;
    public float GizmoPointRadius = 0.12f;
    private void OnDrawGizmos()
    {
        if (!DrawPathGizmos) return;
        if (_path == null || _path.Length == 0) return;

        // 현재 높이로 맞춰서 보기 좋게
        Vector3 prev = transform.position;

        for (int i = 0; i < _path.Length; i++)
        {
            Vector3 p = _path[i];
            p.y = transform.position.y;

            // 이미 지나간 포인트 vs 앞으로 갈 포인트 구분
            Gizmos.color = (i < _pathIndex) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.green;

            Gizmos.DrawLine(prev, p);
            Gizmos.DrawSphere(p, GizmoPointRadius);

#if UNITY_EDITOR
            // 인덱스 라벨(에디터에서만)
            Handles.Label(p + Vector3.up * 0.1f, i.ToString());
#endif

            prev = p;
        }

        // 현재 목표 표시
        if (target != null)
        {
            Gizmos.color = Color.red;
            Vector3 t = target.position;
            t.y = transform.position.y;
            Gizmos.DrawWireSphere(t, 0.25f);
        }
    }

}
