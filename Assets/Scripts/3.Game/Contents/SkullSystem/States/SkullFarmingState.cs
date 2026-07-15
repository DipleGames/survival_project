using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullFarmingState : IState
{
    private readonly SkullController _skull;

    public SkullFarmingState(SkullController skull)
    {
        _skull = skull;
    }

    public void Enter()
    {
        Debug.Log("농사 시작");
    }

    public void Update()
    {
        Debug.Log("농사 중");
    }

    public void Exit()
    {
        Debug.Log("농사 종료");
    }
}
