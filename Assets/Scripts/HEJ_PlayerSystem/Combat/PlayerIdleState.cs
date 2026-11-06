using UnityEngine;

public class PlayerIdleState : IPlayerState
{
    private PlayerStateMachine _playerStateMachine;
    private Animator _animator;
    public PlayerIdleState(PlayerStateMachine playerStateMachine, Animator animator)
    {
        _playerStateMachine = playerStateMachine;
        _animator = animator;
    }

    public void Enter()
    {
        Debug.Log("기본상태 진입");
        _animator.Play("PlayerIdle",0, 0f);
    }

    public void HandleAttackInput(WeaponData weaponData, Vector3 attackDirection)
    {
        _playerStateMachine.AttackState.Prepare(weaponData, attackDirection);
        _playerStateMachine.ChangeState(_playerStateMachine.AttackState);
    }

    public void Exit()
    {
        Debug.Log("기본상태 나가기");
    }
    
}
