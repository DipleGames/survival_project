using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 해골이 낚시 작업을 수행 중인 상태를 담당한다.
/// 낚시 자동화의 시작, 진행 및 종료를 관리한다.
/// </summary>
public class SkullLoggingState : IState
{
    private readonly SkullController _skull;
    private readonly LoggingAutomationController _automation;
    private readonly LoggingManager _loggingManager;
    private readonly Transform _searchCenter;
    private readonly float _searchRadius;

     public SkullLoggingState(SkullController skull, LoggingAutomationController automation, LoggingManager loggingManager, Transform searchCenter, float searchRadius)
    {
        _skull = skull;
        _automation = automation;
        _loggingManager = loggingManager;
        _searchCenter = searchCenter;
        _searchRadius = searchRadius;
    }

    public void Enter()
    {
        Debug.Log("벌목 자동화 시작");

        Vector3 searchCenter = _searchCenter != null
            ? _searchCenter.position
            : _skull.transform.position;

        _automation.StartAutomation(_skull, _loggingManager, searchCenter, _searchRadius);
    }

    public void Update()
    {
        _automation.Tick();
    }

    public void Exit()
    {
        Debug.Log("벌목 자동화 종료");

        _automation.StopAutomation();
    }
}
