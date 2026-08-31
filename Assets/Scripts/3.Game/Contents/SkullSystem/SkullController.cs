using System;
using System.Collections;
using MineTest;
using UnityEngine;


/// <summary>
/// 해골의 상태 전환과 공통 동작을 관리하는 중심 컨트롤러.
/// 이동, 작업 시작 및 중지, 상태 변경 등의 기능을 담당한다.
/// </summary>
public class SkullController : MonoBehaviour
{
    private SkullView _skullView;
    private SkullStateMachine _stateMachine;

    private SkullIdleState _idleState;
    private SkullFarmingState _farmingState;
    private SkullMiningState _miningState;
    private SkullLoggingState _loggingState;

    private FarmingAutomationController _farmingAutomation;
    private MiningAutomationController _miningAutomation;

    private Coroutine _moveCoroutine;

    // 채광 / 낚시처럼 단순 작업에서 현재 예약 중인 작업장
    private WorkAreaData _currentWorkArea;


    [Header("해골이 지닌 물건")]
    public CropSO equippedCropSO;
    public GameObject equippedPick;
    public GameObject equippedFishingRod;

    [Header("채광 자동화")]
    [SerializeField] private MiningManager _miningManager;
    [SerializeField] private Transform _miningSearchCenter;
    [SerializeField, Min(0.1f)] private float _miningSearchRadius = 20f;
    [SerializeField, Min(1)] private int _miningDamagePerHit = 10;
    [SerializeField, Min(0.1f)] private float _miningWorkDuration = 1f;
    [SerializeField, Min(0.1f)] private float _miningApproachDistance = 0.8f;


    private void Awake()
    {
        _skullView = GetComponent<SkullView>();
        _stateMachine = new SkullStateMachine();
        _farmingAutomation = new FarmingAutomationController();
        if (_miningManager == null)
        {
            _miningManager = FindObjectOfType<MiningManager>();
        }
        _miningAutomation = new MiningAutomationController(_miningDamagePerHit, _miningWorkDuration, _miningApproachDistance);

        _idleState = new SkullIdleState(this);
        _farmingState = new SkullFarmingState(this, _farmingAutomation);
        _miningState = new SkullMiningState(this, _miningAutomation, _miningManager, _miningSearchCenter, _miningSearchRadius);
        _loggingState = new SkullLoggingState(this);
    }


    private void Start()
    {
        ChangeState(_idleState);
    }


    private void Update()
    {
        _stateMachine.Update();
    }


    // =========================
    // State
    // =========================

    public void ChangeState(IState nextState)
    {
        _stateMachine.ChangeState(nextState);
    }


    // =========================
    // Farming
    // =========================

    public void StartFarming()
    {
        ChangeState(_farmingState);

        _skullView.CloseSkullUI();
    }


    // =========================
    // Mining
    // =========================

    public void StartMining()
    {
        if (_miningManager == null)
        {
            Debug.LogWarning("MiningManager를 찾을 수 없습니다.");
            return;
        }

        ChangeState(_miningState);

        _skullView.CloseSkullUI();
    }


    // =========================
    // Fishing
    // =========================

    public void StartLogging()
    {
        StartWork(WorkType.Logging, _loggingState);
    }


    // =========================
    // Simple Work
    // =========================

    private void StartWork(WorkType workType, IState workState)
    {
        WorkAreaData targetArea = WorkAreaSelector.FindNearestAvailableArea(workType, transform.position);

        if (targetArea == null)
        {
            Debug.Log($"사용 가능한 {workType} 작업 영역이 없습니다.");

            return;
        }


        if (!targetArea.TryReserve(this))
        {
            Debug.Log("작업 영역 예약 실패");
            return;
        }


        _currentWorkArea = targetArea;

        Vector3 targetPosition = WorkAreaManager.Instance.GetWorldPosition(targetArea.CellPosition);

        MoveTo(targetPosition,
        () =>
            {
                Debug.Log($"{workType} 작업장 도착");

                ChangeState(workState);
            }
        );


        _skullView.CloseSkullUI();
    }


    // =========================
    // Stop Work
    // =========================

    public void StopWork()
    {
        StopMoving();

        ReleaseCurrentWorkArea();

        ChangeState(_idleState);
    }


    public void ReleaseCurrentWorkArea()
    {
        if (_currentWorkArea == null)
            return;

        _currentWorkArea.Release(this);

        _currentWorkArea = null;
    }


    // =========================
    // Movement
    // =========================

    public void MoveTo(Vector3 targetPosition, Action onArrived = null)
    {
        StopMoving();

        _moveCoroutine = StartCoroutine(MoveRoutine(targetPosition, onArrived));
    }


    public void StopMoving()
    {
        if (_moveCoroutine == null)
            return;

        StopCoroutine(_moveCoroutine);

        _moveCoroutine = null;
    }


    private IEnumerator MoveRoutine(Vector3 targetPosition, Action onArrived)
    {
        const float moveSpeed = 3f;
        const float stopDistance = 0.1f;

        float stopDistanceSqr = stopDistance * stopDistance;


        while (Vector3.SqrMagnitude(transform.position - targetPosition) > stopDistanceSqr)
        {
            transform.position =Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = targetPosition;

        _moveCoroutine = null;

        onArrived?.Invoke();
    }
}