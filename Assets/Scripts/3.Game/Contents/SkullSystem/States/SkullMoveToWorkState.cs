using UnityEngine;

public class SkullMoveToWorkState : IState
{
    private readonly SkullController _skull;

    private WorkAreaData _targetArea;
    private IState _nextState;

    public SkullMoveToWorkState(SkullController skull)
    {
        _skull = skull;
    }

    public void SetTarget(WorkAreaData targetArea, IState nextState)
    {
        _targetArea = targetArea;
        _nextState = nextState;
    }

    public void Enter()
    {
        Debug.Log("작업 위치로 이동 시작");
    }

    public void Update()
    {
        if (_targetArea == null)
        {
            _skull.StopWork();
            return;
        }

        Vector3 targetPosition = WorkAreaManager.Instance.GetWorldPosition(_targetArea.CellPosition);

        float distance = Vector3.Distance(_skull.transform.position, targetPosition);

        if (distance <= 0.2f)
        {
            _skull.ChangeState(_nextState);
            return;
        }

        Vector3 direction = (targetPosition - _skull.transform.position).normalized;

        _skull.transform.position += direction * 3f * Time.deltaTime;
    }

    public void Exit()
    {
    }
}