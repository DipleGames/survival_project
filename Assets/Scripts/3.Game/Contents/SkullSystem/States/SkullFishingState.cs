using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
