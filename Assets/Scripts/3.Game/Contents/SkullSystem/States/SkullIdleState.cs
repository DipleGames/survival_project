using UnityEngine;


/// <summary>
/// 해골이 아무 작업도 수행하지 않고 대기 중인 상태를 담당한다.
/// </summary>
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