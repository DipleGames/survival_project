using System.Collections.Generic;
using System;
using UnityEngine;

public class ObjectBuilder : Singleton<ObjectBuilder>
{
    [Header("관련 매니저")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private AStarPathfinder aStarPathfinder;

    [Header("선택된 셀 / 오브젝트")]
    public GameObject selectedCell;
    [SerializeField] private GameObject selectedObj;
    [SerializeField] private BuildableObject objScript;

    [Header("울타리 오브젝트")]
    public GameObject[] fense;
    [SerializeField] private int currentNum = 0;

    [Header("타워 오브젝트")]
    public GameObject tower;

    [Header("설치된 오브젝트 리스트")]
    public List<FenceModel> fences = new List<FenceModel>();
    public List<TowerModel> towers = new List<TowerModel>();

    [Header("오브젝트 부모")]
    public GameObject builtObjectsRoot;


    // ====== 핵심: 점유 관리 구조 ======
    // 울타리 셀 점유 카운트(겹침 허용이라 refCount 필요)
    private readonly Dictionary<Vector2Int, int> _fenceCellRefCount = new Dictionary<Vector2Int, int>();

    // 셀 -> 울타리들 (겹침 허용)
    private readonly Dictionary<Vector2Int, HashSet<FenceModel>> _fencesByCell = new Dictionary<Vector2Int, HashSet<FenceModel>>();

    // 타워 점유 셀(겹침 불가)
    private readonly HashSet<Vector2Int> _towerCells = new HashSet<Vector2Int>();

    // ====== 빌드 모드 ======
    private Vector3 targetPos;
    private GameObject previewObject;
    private List<Vector3Int> buildArea;
    private readonly List<GameObject> selecetedCellPool = new List<GameObject>(16);

    public event Action OnChangedObjectList;
    public bool isBuildMode = false;

    protected override void Awake()
    {
        base.Awake();

        if (selectedCell == null)
        {
            Debug.LogError("[ObjectBuilder] selectedCell 프리팹이 할당되지 않았습니다.");
            enabled = false;
            return;
        }

        // 설치영역 표시용 풀
        for (int i = 0; i < 9; i++)
        {
            var cell = Instantiate(selectedCell);
            cell.SetActive(false);
            selecetedCellPool.Add(cell);
        }
    }

    private void OnEnable()
    {
        // 이벤트 구독은 OnEnable/OnDisable 권장 (씬 로드 순서/Null 방지)
        if (aStarPathfinder != null && aStarPathfinder.pathControllTower != null)
            OnChangedObjectList += aStarPathfinder.pathControllTower.HasToPlayerPath;
    }

    private void OnDisable()
    {
        if (aStarPathfinder != null && aStarPathfinder.pathControllTower != null)
            OnChangedObjectList -= aStarPathfinder.pathControllTower.HasToPlayerPath;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isBuildMode = true;
            SelectObject(fense[currentNum]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            isBuildMode = true;
            SelectObject(tower);
        }

        if (!isBuildMode) return;
        OnBuildMode();

        if (Input.GetKeyDown(KeyCode.Q))
            QuitBuildMode();

        if (Input.GetMouseButtonDown(0))
            BuildObject(selectedObj, targetPos);
    }

    void OnBuildMode()
    {
        if (objScript == null) return;

        if (objScript.buildType == BuildType.Line)
            targetPos = GridMapper.Instance.cellCenterPos;
        else
            targetPos = objScript.GetVisualCenter(GridMapper.Instance.grid);

        if (Input.GetKeyDown(KeyCode.R) && objScript.buildType == BuildType.Line)
        {
            currentNum = (currentNum == 0) ? 1 : 0;
            if (previewObject != null) Destroy(previewObject);
            SelectObject(fense[currentNum]);
        }

        previewObject.transform.position = targetPos;
        previewObject.transform.rotation = Quaternion.Euler(90, 0, 0);

        buildArea = objScript.GetBuildArea(GridMapper.Instance.cellIndex);
        ShowBuildArea(buildArea);
    }

    void ShowBuildArea(List<Vector3Int> area)
    {
        if (area == null || area.Count == 0) return;

        // 풀 크기 방어
        int count = Mathf.Min(area.Count, selecetedCellPool.Count);

        BuildType newType = objScript.buildType;

        Vector3Int start = area[0];
        Vector3Int end = area[area.Count - 1];

        for (int i = 0; i < count; i++)
        {
            Vector3Int c3 = area[i];
            Vector2Int c = new Vector2Int(c3.x, c3.y);

            selecetedCellPool[i].SetActive(true);
            selecetedCellPool[i].transform.position = GridMapper.Instance.grid.GetCellCenterWorld(c3);

            var sr = selecetedCellPool[i].GetComponent<SpriteRenderer>();

            bool blocked = IsBlockedForPlacement(newType, c, start, end);

            Color color = sr.color;
            color = blocked ? Color.red : Color.black;
            color.a = blocked ? 0.5f : 0.3f;
            sr.color = color;
        }

        // area가 더 짧아졌을 때 남는 풀 끄기
        for (int i = count; i < selecetedCellPool.Count; i++)
            selecetedCellPool[i].SetActive(false);
    }

    private bool IsBlockedForPlacement(BuildType newType, Vector2Int cell, Vector3Int start3, Vector3Int end3)
    {
        bool hasFence = _fenceCellRefCount.ContainsKey(cell);
        bool hasTower = _towerCells.Contains(cell);

        if (newType == BuildType.Line)
        {
            // 울타리 설치: 타워와는 겹치면 안 됨
            if (hasTower) return true;

            // 울타리끼리 겹침은 "끝점만" 허용
            if (hasFence)
            {
                Vector2Int start = new Vector2Int(start3.x, start3.y);
                Vector2Int end = new Vector2Int(end3.x, end3.y);
                bool isEndpoint = (cell == start) || (cell == end);
                return !isEndpoint;
            }

            return false;
        }
        else
        {
            // 타워/기타: 울타리든 타워든 겹치면 불가
            if (hasFence || hasTower) return true;
            return false;
        }
    }

    void BuildObject(GameObject selectedObject, Vector3 targetPos)
    {
        if (objScript == null || buildArea == null || buildArea.Count == 0) return;

        BuildType newType = objScript.buildType;

        Vector3Int start = buildArea[0];
        Vector3Int end = buildArea[buildArea.Count - 1];

        // 1) 설치 가능 체크
        for (int i = 0; i < buildArea.Count; i++)
        {
            Vector3Int c3 = buildArea[i];
            Vector2Int c = new Vector2Int(c3.x, c3.y);

            if (IsBlockedForPlacement(newType, c, start, end))
            {
                Debug.Log("이 영역엔 설치할 수 없습니다.");
                return;
            }
        }

        // 2) 실제 설치
        GameObject obj = Instantiate(selectedObject, targetPos, Quaternion.Euler(90, 0, 0));
        obj.transform.SetParent(builtObjectsRoot.transform);

        BuildableObject buildable = obj.GetComponent<BuildableObject>();
        if (buildable == null)
        {
            Debug.LogError("설치한 오브젝트에 BuildableObject가 없습니다.");
            Destroy(obj);
            return;
        }

        for(int i=0; i<buildArea.Count; i++) 
        { 
            selecetedCellPool[i].SetActive(false);
        }

        // 울타리는 겹친 끝점 포함한 원본 영역을 반드시 저장
        buildable.fullBuildArea = new List<Vector3Int>(buildArea);
        buildable.buildArea = new List<Vector3Int>(buildArea); // 필요하면 finalArea로 따로 운용 가능(현재는 full==build로 통일)

        RegisterBuiltObject(obj, buildable);

        // 빌드 모드 종료 처리
        if (previewObject != null) Destroy(previewObject);
        isBuildMode = false;

        // blocked 갱신 + 컨트롤타워 알림
        SyncBlockedToAStar();
        OnChangedObjectList?.Invoke();
    }

    public void DismantleObject(GameObject obj)
    {
        if (obj == null) return;

        UnregisterBuiltObject(obj);

        Destroy(obj);

        SyncBlockedToAStar();
        OnChangedObjectList?.Invoke();
    }

    void SelectObject(GameObject selectedObject)
    {
        if (previewObject != null) Destroy(previewObject);

        selectedObj = selectedObject;
        objScript = selectedObj.GetComponent<BuildableObject>();

        previewObject = Instantiate(selectedObj);

        // 필요 셀 수만큼 풀 켜기
        int need = (objScript != null) ? objScript.requiredCell : 0;
        for (int i = 0; i < selecetedCellPool.Count; i++)
            selecetedCellPool[i].SetActive(i < need);
    }

    void QuitBuildMode()
    {
        for (int i = 0; i < selecetedCellPool.Count; i++)
            selecetedCellPool[i].SetActive(false);

        if (previewObject != null) Destroy(previewObject);
        isBuildMode = false;
    }

    private void RegisterBuiltObject(GameObject obj, BuildableObject buildable)
    {
        if (buildable.buildType == BuildType.Line)
        {
            FenceModel fenceComp = obj.GetComponent<FenceModel>();
            if (fenceComp == null)
            {
                Debug.LogError("울타리 프리팹에 FenceModel이 없습니다.");
                return;
            }

            if (!fences.Contains(fenceComp))
                fences.Add(fenceComp);

            // refCount 증가 (겹친 셀 유지)
            var area = buildable.fullBuildArea ?? buildable.buildArea;
            for (int i = 0; i < area.Count; i++)
            {
                Vector3Int c3 = area[i];
                Vector2Int c = new Vector2Int(c3.x, c3.y);

                // ref++
                _fenceCellRefCount.TryGetValue(c, out int cnt);
                _fenceCellRefCount[c] = cnt + 1;

                // cell -> fences
                if (!_fencesByCell.TryGetValue(c, out var set))
                {
                    set = new HashSet<FenceModel>();
                    _fencesByCell[c] = set;
                }
                set.Add(fenceComp);
            }
        }
        else
        {
            TowerModel towerComp = obj.GetComponent<TowerModel>();
            if (towerComp == null)
            {
                Debug.LogError("타워 프리팹에 TowerModel이 없습니다.");
                return;
            }

            if (!towers.Contains(towerComp))
                towers.Add(towerComp);

            // 타워는 겹침 불가이므로 단순 점유
            var area = buildable.fullBuildArea ?? buildable.buildArea;
            for (int i = 0; i < area.Count; i++)
            {
                Vector3Int c3 = area[i];
                _towerCells.Add(new Vector2Int(c3.x, c3.y));
            }
        }

        buildable.isBuilt = true;
    }

    private void UnregisterBuiltObject(GameObject obj)
    {
        BuildableObject buildable = obj.GetComponent<BuildableObject>();
        if (buildable == null) return;

        if (buildable.buildType == BuildType.Line)
        {
            FenceModel fenceComp = obj.GetComponent<FenceModel>();
            if (fenceComp != null)
                fences.Remove(fenceComp);

            var area = buildable.fullBuildArea ?? buildable.buildArea;
            for (int i = 0; i < area.Count; i++)
            {
                Vector3Int c3 = area[i];
                Vector2Int c = new Vector2Int(c3.x, c3.y);

                // cell -> fences 제거
                if (_fencesByCell.TryGetValue(c, out var set))
                {
                    set.Remove(fenceComp);
                    if (set.Count == 0) _fencesByCell.Remove(c);
                }

                // ref--
                if (_fenceCellRefCount.TryGetValue(c, out int cnt))
                {
                    cnt--;
                    if (cnt <= 0) _fenceCellRefCount.Remove(c);
                    else _fenceCellRefCount[c] = cnt;
                }
            }
        }
        else
        {
            TowerModel towerComp = obj.GetComponent<TowerModel>();
            if (towerComp != null)
                towers.Remove(towerComp);

            var area = buildable.fullBuildArea ?? buildable.buildArea;
            for (int i = 0; i < area.Count; i++)
            {
                Vector3Int c3 = area[i];
                _towerCells.Remove(new Vector2Int(c3.x, c3.y));
            }
        }
    }

    // “그 셀에 걸린 울타리 아무거나 하나” 반환 (여러개면 그 중 하나)
    public bool TryGetAnyFenceByCell(Vector2Int cell, out FenceModel fence)
    {
        fence = null;
        if (_fencesByCell.TryGetValue(cell, out var set) && set.Count > 0)
        {
            foreach (var f in set) { fence = f; return true; }
        }
        return false;
    }

    private void SyncBlockedToAStar()
    {
        // blocked는 "울타리 refCount 남아있는 셀" + "타워 점유 셀"
        List<Vector2Int> blocked = new List<Vector2Int>(_fenceCellRefCount.Count + _towerCells.Count);

        foreach (var kv in _fenceCellRefCount)
            blocked.Add(kv.Key);

        foreach (var c in _towerCells)
            blocked.Add(c);

        gridManager.SetBlockedByNodeCoords(blocked);
    }
}
