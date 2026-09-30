using MineTest;
using UnityEngine;

/// <summary>
/// 해골의 광물 탐색, 예약, 이동, 채광 반복을 관리한다.
/// </summary>
public class MiningAutomationController
{
    private enum MiningState
    {
        None,
        Searching,
        Moving,
        Working
    }

    private readonly MiningAction _miningAction;
    private readonly float _workDuration;
    private readonly float _approachDistance;

    private SkullController _skull;
    private MiningManager _miningManager;
    private MiningNode _currentTarget;

    private MiningState _state = MiningState.None;

    private Vector3 _searchCenter;
    private float _searchRadius;
    private float _workTimer;

    public MiningAutomationController(int damagePerHit = 10, float workDuration = 1f, float approachDistance = 0.8f)
    {
        _miningAction = new MiningAction(damagePerHit);
        _workDuration = Mathf.Max(0.1f, workDuration);
        _approachDistance = Mathf.Max(0.1f, approachDistance);
    }

    public void StartAutomation(SkullController skull, MiningManager miningManager, Vector3 searchCenter, float searchRadius)
    {
        StopAutomation();

        _skull = skull;
        _miningManager = miningManager;
        _searchCenter = searchCenter;
        _searchRadius = Mathf.Max(0.1f, searchRadius);

        _workTimer = 0f;
        _state = MiningState.Searching;
    }

    public void Tick()
    {
        if (_skull == null)
            return;

        switch (_state)
        {
            case MiningState.Searching:
                FindNextWork();
                break;

            case MiningState.Moving:
                UpdateMoving();
                break;

            case MiningState.Working:
                UpdateWorking();
                break;
        }
    }

    public void StopAutomation()
    {
        if (_skull != null)
            _skull.StopMoving();

        ReleaseCurrentTarget();

        _state = MiningState.None;
        _workTimer = 0f;

        _miningManager = null;
        _skull = null;
    }

    // =========================================================
    // 작업 탐색
    // =========================================================

    private void FindNextWork()
    {
        if (_miningManager == null)
        {
            FinishAutomation();
            return;
        }

        MiningNode target = MiningTargetSelector.FindTarget(_miningManager.Nodes,  _skull, _searchCenter, _searchRadius);

        if (target == null)
        {
            Debug.Log("채광 가능한 광물이 없어 자동화를 종료합니다.");
            FinishAutomation();
            return;
        }

        if (!target.TryReserve(_skull))
            return;

        _currentTarget = target;

        MoveToCurrentTarget();
    }

    // =========================================================
    // 이동
    // =========================================================

    private void MoveToCurrentTarget()
    {
        if (!IsCurrentTargetValid())
        {
            ReturnToSearching();
            return;
        }

        Vector3 targetPosition = _currentTarget.transform.position;
        targetPosition += Vector3.forward * _approachDistance;
        targetPosition.y = _skull.transform.position.y;

        _state = MiningState.Moving;

        _skull.MoveTo(targetPosition, OnArrivedAtTarget);
    }

    private void UpdateMoving()
    {
        if (IsCurrentTargetValid())
            return;

        _skull.StopMoving();

        ReturnToSearching();
    }

    private void OnArrivedAtTarget()
    {
        if (_state != MiningState.Moving)
            return;

        if (!IsCurrentTargetValid())
        {
            ReturnToSearching();
            return;
        }

        Vector3 direction = _currentTarget.transform.position - _skull.transform.position;

        // 방향 전환 기능이 준비되면 여기서 해골 방향을 변경한다.
        // _skull.SetFacingFromWorldX(direction.x);

        _workTimer = 0f;
        _state = MiningState.Working;

        Debug.Log($"해골이 {_currentTarget.name} 채광을 시작합니다.");
    }

    // =========================================================
    // 작업
    // =========================================================

    private void UpdateWorking()
    {
        if (!IsCurrentTargetValid())
        {
            ReturnToSearching();
            return;
        }

        _workTimer += Time.deltaTime;

        if (_workTimer < _workDuration)
            return;

        _workTimer = 0f;

        if (!_miningAction.Execute(_skull, _currentTarget))
        {
            ReturnToSearching();
            return;
        }

        if (_currentTarget == null || !_currentTarget.CanBeMined)
        {
            _currentTarget = null;
            _state = MiningState.Searching;
        }
    }

    // =========================================================
    // 타겟 처리
    // =========================================================

    private bool IsCurrentTargetValid()
    {
        if (_skull == null || _currentTarget == null)
            return false;

        if (!_currentTarget.CanBeMined)
            return false;

        return _currentTarget.IsReservedBy(_skull);
    }

    private void ReleaseCurrentTarget()
    {
        if (_currentTarget == null)
            return;

        if (_skull != null)
            _currentTarget.Release(_skull);

        _currentTarget = null;
    }

    private void ReturnToSearching()
    {
        ReleaseCurrentTarget();

        _workTimer = 0f;
        _state = MiningState.Searching;
    }

    // =========================================================
    // 종료
    // =========================================================

    private void FinishAutomation()
    {
        SkullController skull = _skull;

        StopAutomation();

        if (skull != null)
            skull.StopWork();
    }
}