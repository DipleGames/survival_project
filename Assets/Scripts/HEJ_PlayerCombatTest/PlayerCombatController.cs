using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    // 공격 입력을 전달받을 컴포넌트
    [SerializeField] private PlayerInputReader _inputReader;

    // 현재 플레이어 상태를 전달받을 컴포넌트
    [SerializeField] private PlayerStateMachine _stateMachine;

    // 현재 사용할 무기. 지금은 테스트용 에셋을 직접 연결
    [SerializeField] private WeaponData _currentWeapon;

    // 이후 공격 상태에서 무기 데이터를 확인할 때 사용
    public WeaponData CurrentWeapon => _currentWeapon;

    private void OnEnable()
    {
        if (_inputReader == null)
        {
            Debug.LogError("PlayerInputReader를 연결해주세요.", this);
            return;
        }

        if (_stateMachine == null)
        {
            Debug.LogError("PlayerStateMachine를 연결해주세요.", this);
            return;
        }

        // 입력 이벤트 구독
        _inputReader.AttackPressed += HandleAttackInput;
    }

    private void OnDisable()
    {
        // 비활성화되면 입력 이벤트 구독 해제
        if (_inputReader != null)
        {
            _inputReader.AttackPressed -= HandleAttackInput;
        }
    }

    private void HandleAttackInput(Vector2 direction)
    {
        // 무기가 없으면 공격 요청 중단
        if (_currentWeapon == null) return;

        // 콤보 에셋과 사용할 타수 개수 검사
        if (!_currentWeapon.HasValidCombo) return;

        // 일단 근접 무기만 테스트
        if (_currentWeapon.WeaponType != WeaponType.Melee) return;

        // 전달받은 Vector2의 두 성분을 월드 XZ 방향으로 복원
        Vector3 worldDirection = default;
        worldDirection.x = direction.x;
        worldDirection.z = direction.y;

        // 입력 전달과 무기 조건 통과 여부를 확인하는 임시 표시
        Debug.DrawRay(transform.position, worldDirection * 2f, Color.red, 1f, false);

        // 다음 단계: 현재 플레이어 상태에 공격 요청 전달
        _stateMachine.HandleAttackInput(_currentWeapon, worldDirection);
    }
}