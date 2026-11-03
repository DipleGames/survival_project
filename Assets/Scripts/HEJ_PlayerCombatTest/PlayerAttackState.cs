using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : IPlayerState
{
    private PlayerStateMachine _playerStateMachine;
    private ComboAttackController _comboAttackController;

    private WeaponData _weaponData;
    private Vector3 _attackDirection;


    public PlayerAttackState(PlayerStateMachine playerStateMachine, ComboAttackController comboAttackController)
    {
        _playerStateMachine = playerStateMachine;
        _comboAttackController = comboAttackController;
    }

    public void Prepare(WeaponData weaponData, Vector3 attackDirection)
    {
        _weaponData = weaponData;
        _attackDirection = attackDirection;
    }
    public void Enter()
    {
        Debug.Log("공격상태 진입");
        _playerStateMachine.PlayerMove.MovementLocked = true;
        _comboAttackController.AttackFinished += HandleAttackFinished;
        _comboAttackController.StartAttack(_weaponData, _attackDirection);
    }

    public void HandleAttackInput(WeaponData weaponData, Vector3 attackDirection)
    {
        _comboAttackController.QueueNextAttack(attackDirection);
    }

    public void Exit()
    {
        Debug.Log("공격상태 나가기");
        _playerStateMachine.PlayerMove.MovementLocked = false;
        _comboAttackController.AttackFinished -= HandleAttackFinished;
        _comboAttackController.CancelAttack();
    }

    private void HandleAttackFinished()
    {
        _playerStateMachine.ChangeState(_playerStateMachine.IdleState);
    }
}
