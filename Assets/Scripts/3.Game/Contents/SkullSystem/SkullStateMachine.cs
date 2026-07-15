using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullStateMachine : MonoBehaviour
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