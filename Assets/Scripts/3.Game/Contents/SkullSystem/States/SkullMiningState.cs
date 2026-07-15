using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
