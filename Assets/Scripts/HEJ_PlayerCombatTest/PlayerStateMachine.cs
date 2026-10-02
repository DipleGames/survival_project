using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [SerializeField] private ComboAttackController _comboAttackController;
    [SerializeField] private PlayerMove _playerMove;
    // 현재 실행 중인 상태
    private IPlayerState _currentState;

    // 한 번 생성한 뒤 재사용할 상태 객체
    private PlayerIdleState _idleState;
    private PlayerAttackState _attackState;

    // 외부에서 상태 전환을 요청할 때 사용할 읽기 전용 프로퍼티
    public PlayerMove PlayerMove => _playerMove;
    public IPlayerState CurrentState => _currentState;
    public PlayerIdleState IdleState => _idleState;
    public PlayerAttackState AttackState => _attackState;

    private void Awake()
    {
        _idleState = new PlayerIdleState(this, _comboAttackController.Animator);
        _attackState = new PlayerAttackState(this, _comboAttackController);
        _playerMove = GetComponent<PlayerMove>();

        // 처음에는 대기 상태로 시작
        ChangeState(_idleState);
    }

    public void ChangeState(IPlayerState newState)
    {
        // 잘못된 상태나 현재와 같은 상태로의 전환은 무시
        if (newState == null || _currentState == newState) return;

        // 기존 상태가 있을 때만 종료
        if (_currentState != null)
        {
            _currentState.Exit();
        }

        // 현재 상태 교체 후 진입
        _currentState = newState;
        _currentState.Enter();
    }

    public void HandleAttackInput(WeaponData currentWeapon, Vector3 direction)
    {
        // 초기화 전에는 입력을 전달하지 않음
        if (_currentState == null) return;

        _currentState.HandleAttackInput(currentWeapon, direction);
    }
}