using UnityEngine;

/// <summary>
/// 해골 농사 자동화의 전체 진행 흐름을 관리한다.
/// 행동 선택, 농작지 선택, 타일 선택, 예약, 이동, 작업 실행 및 반복을 제어한다.
/// </summary>
public class FarmingAutomationController
{
    private enum FarmingState
    {
        None,
        Searching,
        Moving,
        Working,
        Waiting
    }

    private SkullController _skull;

    private readonly FarmingActionSelector _actionSelector = new();
    private readonly FarmAreaSelector _areaSelector = new();
    private readonly FarmTileSelector _tileSelector = new();

    private IFarmingAction _currentAction;
    private FarmArea _currentArea;
    private FarmTile _currentTile;

    private FarmingState _state = FarmingState.None;

    private float _workTimer;
    private float _workDuration = 1f;

    private float _searchTimer;
    private float _searchInterval = 1f;

    public void StartAutomation(SkullController skull)
    {
        _skull = skull;
        _state = FarmingState.Searching;
    }


    public void Tick()
    {
        if (_skull == null)
            return;

        switch (_state)
        {
            case FarmingState.Searching:
                FindNextWork();
                break;

            case FarmingState.Moving:
                UpdateMoving();
                break;

            case FarmingState.Working:
                UpdateWorking();
                break;

            case FarmingState.Waiting:
                UpdateWaiting();
                break;
        }
    }


    public void StopAutomation()
    {
        if (_skull == null)
            return;

        _skull.StopMoving();

        ReleaseCurrentTarget();
        ClearCurrentTarget();

        _state = FarmingState.None;
        _skull = null;
    }


    // =========================================================
    // 작업 탐색
    // =========================================================

    private void FindNextWork()
    {
        bool isFarmingLevel2 = true;

        // 1. 무슨 행동을 할지 선택
        _currentAction = _actionSelector.SelectAction(isFarmingLevel2);

        if (_currentAction == null)
        {
            Debug.Log("현재 할 농사 작업이 없습니다. 대기합니다.");
            StartWaiting();
            return;
        }

        // 2. 어느 농작지에서 할지 선택
        _currentArea = _areaSelector.SelectArea(_skull.transform.position, _currentAction);

        if (_currentArea == null)
        {
            Debug.Log("현재 작업 가능한 농작지가 없습니다.");
            StartWaiting();
            return;
        }

        // 3. 어느 타일에서 할지 선택
        _currentTile = _tileSelector.SelectNearestTile(_skull.transform.position, _currentArea, _currentAction);

        if (_currentTile == null)
        {
            Debug.Log("현재 작업 가능한 농사 타일이 없습니다.");
            StartWaiting();
            return;
        }

        // 4. 타일 예약
        if (!_currentTile.TryReserve(_skull))
        {
            Debug.Log("농사 타일 예약 실패");
            RestartSearching();
            return;
        }

        // 5. 이동
        MoveToCurrentTile();
    }

    // =========================================================
    // 이동
    // =========================================================

    private void MoveToCurrentTile()
    {
        _state = FarmingState.Moving;

        Vector3 targetPosition = GetInteractionPosition(_currentTile);

        _skull.MoveTo(targetPosition, OnArrivedTile);
    }


    private void UpdateMoving()
    {
        if (_currentTile == null || _currentAction == null)
        {
            RestartSearching();
            return;
        }

        // 이동하는 사이 작업 조건이 깨졌는지 확인
        if (!_currentAction.CanExecute(_currentTile))
        {
            Debug.Log("이동 중 농사 조건이 변경되어 재탐색합니다.");

            _skull.StopMoving();
            RestartSearching();
        }
    }


    private void OnArrivedTile()
    {
        if (_currentTile == null || _currentAction == null)
        {
            RestartSearching();
            return;
        }

        // 도착 시 한 번 더 조건 검사
        if (!_currentAction.CanExecute(_currentTile))
        {
            Debug.Log("도착했지만 더 이상 작업할 수 없습니다.");
            RestartSearching();
            return;
        }

        Debug.Log("농사 타일 도착");

        _workTimer = 0f;
        _state = FarmingState.Working;
    }


    // =========================================================
    // 작업
    // =========================================================

    private void UpdateWorking()
    {
        if (_currentTile == null || _currentAction == null)
        {
            RestartSearching();
            return;
        }

        // 작업 중에도 조건이 깨질 수 있음
        if (!_currentAction.CanExecute(_currentTile))
        {
            Debug.Log("작업 조건이 변경되어 현재 행동을 취소합니다.");
            RestartSearching();
            return;
        }

        _workTimer += Time.deltaTime;

        if (_workTimer < _workDuration)
            return;

        // 실제 농사 행동 실행
        _currentAction.Execute(_currentTile, _skull);

        Debug.Log("농사 행동 완료");

        ReleaseCurrentTarget();
        ClearCurrentTarget();

        // 다음 작업 탐색
        _state = FarmingState.Searching;
    }


    // =========================================================
    // 타겟 처리
    // =========================================================

    private void RestartSearching()
    {
        ReleaseCurrentTarget();
        ClearCurrentTarget();

        _state = FarmingState.Searching;
    }


    private void ReleaseCurrentTarget()
    {
        if (_currentTile == null || _skull == null)
            return;

        _currentTile.Release(_skull);
    }


    private void ClearCurrentTarget()
    {
        _currentAction = null;
        _currentArea = null;
        _currentTile = null;

        _workTimer = 0f;
    }


    // =========================================================
    // 종료
    // =========================================================

    private void StopFarming()
    {
        ReleaseCurrentTarget();
        ClearCurrentTarget();

        _state = FarmingState.None;

        // SkullFarmingState에서 Idle로 돌아가게 처리할 예정
        Debug.Log("농사 자동화 종료");
    }

    // =========================================================
    // 대기
    // =========================================================
    private void StartWaiting()
    {
        ReleaseCurrentTarget();
        ClearCurrentTarget();

        _searchTimer = 0f;
        _state = FarmingState.Waiting;
    }

    private void UpdateWaiting()
    {
        _searchTimer += Time.deltaTime;

        if (_searchTimer < _searchInterval)
            return;

        _searchTimer = 0f;
        _state = FarmingState.Searching;
    }


    // =========================================================
    // 농사 상호작용 위치
    // =========================================================

    private Vector3 GetInteractionPosition(FarmTile tile)
    {
        Vector3Int tilePos = tile.CellPosition;

        Vector3Int[] directions =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        Vector3 nearestPosition = Vector3.zero;
        float nearestDistance = float.MaxValue;

        foreach (Vector3Int direction in directions)
        {
            Vector3Int interactionCell = tilePos + direction;
            Vector3 worldPosition = TilemapFarmSystem.Instance.GetWorldPosition(interactionCell);

            float distance = Vector3.SqrMagnitude(worldPosition - _skull.transform.position);

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestPosition = worldPosition;
        }

        return nearestPosition;
    }
}