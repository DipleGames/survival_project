using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 해골의 현재 상태를 관리하는 상태 머신.
/// 상태 변경 시 기존 상태를 종료하고 새로운 상태를 시작하며 현재 상태를 갱신한다.
/// </summary>
public class SkullStateMachine
{
    private IState _currentState;

    public void ChangeState(IState newState)
    {
        if (_currentState == newState)
            return;

        _currentState?.Exit();

        _currentState = newState;

        _currentState?.Enter();
    }

    public void Update()
    {
        _currentState?.Update();
    }
}