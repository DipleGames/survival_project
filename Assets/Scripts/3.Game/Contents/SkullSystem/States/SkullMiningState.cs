using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 해골이 채광 작업을 수행 중인 상태를 담당한다.
/// 채광 자동화의 시작, 진행 및 종료를 관리한다.
/// </summary>
public class SkullMiningState : IState
{
    private readonly SkullController _skull;

    public SkullMiningState(SkullController skull)
    {
        _skull = skull;
    }

    public void Enter()
    {
        Debug.Log("광질 시작");
    }

    public void Update()
    {
        Debug.Log("광질 중");
    }

    public void Exit()
    {
        Debug.Log("광질 종료");
    }
}
