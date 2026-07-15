using UnityEngine;

public class SkullController : MonoBehaviour
{
    private SkullStateMachine _stateMachine;

    private SkullIdleState _idleState;
    private SkullMoveToWorkState _moveToWorkState;
    private SkullFarmingState _farmingState;
    private SkullMiningState _miningState;
    private SkullFishingState _fishingState;

    private void Awake()
    {
        _stateMachine = new SkullStateMachine();

        _idleState = new SkullIdleState(this);
        _moveToWorkState = new SkullMoveToWorkState(this);
        _farmingState = new SkullFarmingState(this);
        _miningState = new SkullMiningState(this);
        _fishingState = new SkullFishingState(this);
    }

    private void Start()
    {
        ChangeState(_idleState);
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    public void StartFarming()
    {
        WorkAreaData targetArea =
            WorkAreaManager.Instance.GetNearestAvailableArea(
                WorkType.Farming,
                transform.position
            );

        if (targetArea == null)
        {
            Debug.Log("사용 가능한 농사 영역이 없습니다.");
            return;
        }

        targetArea.Reserve(this);

        _moveToWorkState.SetTarget(targetArea, _farmingState);
        ChangeState(_moveToWorkState);
    }

    public void StartMining()
    {
        WorkAreaData targetArea =
            WorkAreaManager.Instance.GetNearestAvailableArea(
                WorkType.Mining,
                transform.position
            );

        if (targetArea == null)
        {
            Debug.Log("사용 가능한 채광 영역이 없습니다.");
            return;
        }

        targetArea.Reserve(this);

        _moveToWorkState.SetTarget(targetArea, _miningState);
        ChangeState(_moveToWorkState);
    }

    public void StartFishing()
    {
        WorkAreaData targetArea =
            WorkAreaManager.Instance.GetNearestAvailableArea(
                WorkType.Fishing,
                transform.position
            );

        if (targetArea == null)
        {
            Debug.Log("사용 가능한 낚시 영역이 없습니다.");
            return;
        }

        targetArea.Reserve(this);

        _moveToWorkState.SetTarget(targetArea, _fishingState);
        ChangeState(_moveToWorkState);
    }

    public void StopWork()
    {
        ChangeState(_idleState);
    }

    public void ChangeState(IState nextState)
    {
        _stateMachine.ChangeState(nextState);
    }
}