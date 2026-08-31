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
    // private readonly FarmingAutomationController _automation;

    public SkullLoggingState(SkullController skull)
    {
        _skull = skull;
    }
    
    public void Enter()
    {
        Debug.Log("벌목 자동화 시작");

        //_automation.StartAutomation(_skull);
    }

    public void Update()
    {
        //_automation.Tick();
    }

    public void Exit()
    {
        Debug.Log("농사 자동화 종료");

        //_automation.StopAutomation();
    }
}
