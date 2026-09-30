using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MineTest;


/// <summary>
/// 해골이 채광 작업을 수행 중인 상태를 담당한다.
/// 채광 자동화의 시작, 진행 및 종료를 관리한다.
/// </summary>
public class SkullMiningState : IState
{
    private readonly SkullController _skull;
    private readonly MiningAutomationController _automation;
    private readonly MiningManager _miningManager;
    private readonly Transform _searchCenter;
    private readonly float _searchRadius;

     public SkullMiningState(SkullController skull, MiningAutomationController automation, MiningManager miningManager, Transform searchCenter, float searchRadius)
    {
        _skull = skull;
        _automation = automation;
        _miningManager = miningManager;
        _searchCenter = searchCenter;
        _searchRadius = searchRadius;
    }

    public void Enter()
    {
        Debug.Log("광질 자동화 시작");

        Vector3 searchCenter = _searchCenter != null
            ? _searchCenter.position
            : _skull.transform.position;

        _automation.StartAutomation(_skull, _miningManager, searchCenter, _searchRadius);
    }

    public void Update()
    {
        _automation.Tick();
    }

    public void Exit()
    {
        Debug.Log("광질 자동화 종료");

        _automation.StopAutomation();
    }
}
