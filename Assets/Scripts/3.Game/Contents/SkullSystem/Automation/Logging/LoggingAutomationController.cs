using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 해골의 나무 탐색, 예약, 이동, 벌목 반복을 관리한다.
/// 제공된 MiningAutomationController의 실행 구조와 SkullController API를 사용한다.
/// </summary>
public class LoggingAutomationController
{
    private enum LoggingState
    {
        None,
        Searching,
        Moving,
        Working
    }

    private readonly LoggingAction _loggingAction;
    private readonly float _workDuration;
    private readonly float _approachDistance;

    private SkullController _skull;
    private LoggingManager _loggingManager;
    private GatherableObj _currentTarget;
    private Animator _animator;
    private string _activeAnimation;

    private LoggingState _state = LoggingState.None;
    private Vector3 _searchCenter;
    private float _searchRadius;
    private float _workTimer;


    public LoggingAutomationController(ToolType toolType, float workDuration = 1f, float approachDistance = 0.8f)
    {
        _workDuration = Mathf.Max(0.1f, workDuration);
        _approachDistance = approachDistance;
        _loggingAction = new LoggingAction(toolType, _workDuration);
    }

    public void StartAutomation(SkullController skull, LoggingManager loggingManager, Vector3 searchCenter, float searchRadius, Animator animator = null)
    {
        StopAutomation();

        _skull = skull;
        _loggingManager = loggingManager;
        _searchCenter = searchCenter;
        _searchRadius = Mathf.Max(0.1f, searchRadius);
        _animator = animator;

        if (_animator == null && _skull != null)
            _animator = _skull.GetComponentInChildren<Animator>();

        _workTimer = 0f;
        _state = LoggingState.Searching;
    }

    public void Tick()
    {
        if (_skull == null)
            return;

        switch (_state)
        {
            case LoggingState.Searching:
                FindNextWork();
                break;
            case LoggingState.Moving:
                UpdateMoving();
                break;
            case LoggingState.Working:
                UpdateWorking();
                break;
        }
    }

    public void StopAutomation()
    {
        if (_skull != null)
            _skull.StopMoving();

        ReleaseCurrentTarget();

        _state = LoggingState.None;
        _workTimer = 0f;

        _loggingManager = null;
        _skull = null;
    }

    // =========================================================
    // 작업 탐색
    // =========================================================

    private void FindNextWork()
    {
        if (_loggingManager == null)
        {
            FinishAutomation();
            return;
        }

        GatherableObj target = LoggingTargetSelector.FindTarget(_loggingManager.Gathers,  _skull, _searchCenter, _searchRadius);

        if (target == null)
        {
            Debug.Log("벌목 가능한 오브젝트가 없어 자동화를 종료합니다.");
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

        _state = LoggingState.Moving;

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
        if (_state != LoggingState.Moving)
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
        _state = LoggingState.Working;

        Debug.Log($"해골이 {_currentTarget.name} 벌목을 시작합니다.");
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

        if (!_loggingAction.Execute(_skull, _currentTarget))
        {
            ReturnToSearching();
            return;
        }

        if (_currentTarget == null)
        {
            _currentTarget = null;
            _state = LoggingState.Searching;
        }
    }

    // =========================================================
    // 타겟 처리
    // =========================================================

    private bool IsCurrentTargetValid()
    {
        if (_skull == null || _currentTarget == null)
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
        _state = LoggingState.Searching;
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
