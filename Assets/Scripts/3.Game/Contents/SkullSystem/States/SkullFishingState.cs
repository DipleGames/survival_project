using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 해골이 낚시 작업을 수행 중인 상태를 담당한다.
/// 낚시 자동화의 시작, 진행 및 종료를 관리한다.
/// </summary>
public class SkullFishingState : IState
{
    private readonly SkullController _skull;

    public SkullFishingState(SkullController skull)
    {
        _skull = skull;
    }
    
    public void Enter()
    {
        Debug.Log("낚시 시작");
    }

    public void Update()
    {
        Debug.Log("낚시 중");
    }

    public void Exit()
    {
        Debug.Log("낚시 종료");
    }
}
