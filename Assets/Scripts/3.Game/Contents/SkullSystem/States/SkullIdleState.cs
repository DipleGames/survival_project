using UnityEngine;

public class SkullIdleState : IState
{
    private readonly SkullController _skull;

    public SkullIdleState(SkullController skull)
    {
        _skull = skull;
    }

    public void Enter()
    {
        Debug.Log("해골 대기 시작");
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        Debug.Log("해골 대기 종료");
    }
}