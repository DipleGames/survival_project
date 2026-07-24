using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkullController : MonoBehaviour
{
    private SkullView _skullView;
    private SkullStateMachine _stateMachine;

    private SkullIdleState _idleState;
    private SkullMoveToWorkState _moveToWorkState;
    private SkullFarmingState _farmingState;
    private SkullMiningState _miningState;
    private SkullFishingState _fishingState;

    [Header("해골이 지닌 물건")]
    public CropSO equippedCrop;
    public GameObject equippedPick;
    public GameObject equippedFishingRod;



    private void Awake()
    {
        _skullView = GetComponent<SkullView>();

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
        WorkAreaData targetArea = WorkAreaManager.Instance.GetNearestAvailableArea(WorkType.Farming, transform.position);

        if (targetArea == null)
        {
            Debug.Log("사용 가능한 농사 영역이 없습니다.");
            return;
        }

        targetArea.Reserve(this);

        _farmingState.EquipCrop(equippedCrop);
        _moveToWorkState.SetTarget(targetArea, _farmingState);
        ChangeState(_moveToWorkState);

        _skullView.CloseSkullUI();
    }

    public void StartMining()
    {
        WorkAreaData targetArea = WorkAreaManager.Instance.GetNearestAvailableArea(WorkType.Mining, transform.position);

        if (targetArea == null)
        {
            Debug.Log("사용 가능한 채광 영역이 없습니다.");
            return;
        }

        targetArea.Reserve(this);

        _moveToWorkState.SetTarget(targetArea, _miningState);
        ChangeState(_moveToWorkState);

        _skullView.CloseSkullUI();
    }

    public void StartFishing()
    {
        WorkAreaData targetArea = WorkAreaManager.Instance.GetNearestAvailableArea(WorkType.Fishing, transform.position);

        if (targetArea == null)
        {
            Debug.Log("사용 가능한 낚시 영역이 없습니다.");
            return;
        }

        targetArea.Reserve(this);

        _moveToWorkState.SetTarget(targetArea, _fishingState);
        ChangeState(_moveToWorkState);

        _skullView.CloseSkullUI();
    }

    public void StopWork()
    {
        ChangeState(_idleState);
    }

    public void ChangeState(IState nextState)
    {
        _stateMachine.ChangeState(nextState);
    }

    private Coroutine _workMoveCoroutine;

    public void StartMoveToNextPoint(WorkType workType)
    {
        StopMoveToNextPoint();

        _workMoveCoroutine = StartCoroutine(MoveToNextPoint(workType));
    }

    public void StopMoveToNextPoint()
    {
        if (_workMoveCoroutine == null)
            return;

        StopCoroutine(_workMoveCoroutine);
        _workMoveCoroutine = null;
    }

    private IEnumerator MoveToNextPoint(WorkType workType)
    {
        const float moveSpeed = 3f;
        const float stopDistance = 0.1f;
        const float workDuration = 2f;

        while (true)
        {
            Vector3 targetPos;

            switch (workType)
            {
                case WorkType.Farming:
                {
                    List<Vector3Int> farmAreaList = WorkAreaManager.Instance.farmAreaList;

                    if (farmAreaList.Count == 0)
                    {
                        _workMoveCoroutine = null;
                        yield break;
                    }

                    int randomIndex =
                        UnityEngine.Random.Range(0, farmAreaList.Count);

                    Vector3Int cellPos = farmAreaList[randomIndex];

                    targetPos = TilemapFarmSystem.Instance.fieldTilemap
                        .GetCellCenterWorld(cellPos);

                    break;
                }

                case WorkType.Mining:
                case WorkType.Fishing:
                default:
                    _workMoveCoroutine = null;
                    yield break;
            }

            while (Vector3.Distance(transform.position, targetPos) > stopDistance)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.position = targetPos;

            // 여기서 농사 애니메이션 재생
            // _skullView.PlayFarmingAnimation();

            yield return new WaitForSeconds(workDuration);
        }
    }
}