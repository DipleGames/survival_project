using UnityEngine;


/// <summary>
/// 해골이 농사 자동화를 수행 중인 상태를 담당한다.
/// 실제 농사 진행은 FarmingAutomationController에 위임한다.
/// </summary>
public class SkullFarmingState : IState
{
    private readonly SkullController _skull;
    private readonly FarmingAutomationController _automation;

    public SkullFarmingState(SkullController skull, FarmingAutomationController automation)
    {
        _skull = skull;
        _automation = automation;
    }

    public void Enter()
    {
        Debug.Log("농사 자동화 시작");

        _automation.StartAutomation(_skull);
    }

    public void Update()
    {
        _automation.Tick();
    }

    public void Exit()
    {
        Debug.Log("농사 자동화 종료");

        _automation.StopAutomation();
    }
}