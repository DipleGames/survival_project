using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboAttackController : MonoBehaviour
{

    [SerializeField] private Animator _animator;

    private WeaponData _weaponData;
    private Vector3 _attackDirection;
    private bool _isAttacking = false;

    private int _currentStepIndex = 0; // 현재 타수
    private bool _hasQueuedAttack = false; // 다음 공격 예약 여부
    private Vector3 _nextAttackDirection = Vector3.zero; // 예약한 공격의 방향

    // 읽기 전용
    public bool IsAttacking => _isAttacking;
    public Animator Animator => _animator;

    public event Action AttackFinished;

    public void StartAttack(WeaponData weaponData, Vector3 attackDirection)
    {
        if(_isAttacking) return;

        if(weaponData == null || !weaponData.HasValidCombo) return;

        _weaponData = weaponData;
        _attackDirection = attackDirection;

        _currentStepIndex = 0;
        _hasQueuedAttack = false;
        _nextAttackDirection = Vector3.zero;

        _isAttacking = true;
        PlayCurrentStep();
    }

    public void FinishAttack()
    {
        if(!_isAttacking) return;

        _isAttacking = false;
        _weaponData = null;
        _attackDirection = Vector3.zero;

        _currentStepIndex = 0;
        _hasQueuedAttack = false;
        _nextAttackDirection = Vector3.zero;

        AttackFinished?.Invoke();
    }

    public void CancelAttack()
    {
        _isAttacking = false;
        _weaponData = null;
        _attackDirection = Vector3.zero;

        _currentStepIndex = 0;
        _hasQueuedAttack = false;
        _nextAttackDirection = Vector3.zero;
    }

    public void QueueNextAttack(Vector3 nextAttackDirection)
    {
        if(!_isAttacking) return;

        if(_weaponData.MaxComboCount == _currentStepIndex + 1) return;

        if(_hasQueuedAttack) return;

        _hasQueuedAttack = true;
        _nextAttackDirection = nextAttackDirection;
    }

    public void HandleStepFinished()
    {
        if(!_isAttacking) return;

        if(_hasQueuedAttack && _currentStepIndex + 1 < _weaponData.MaxComboCount)
        {
            _hasQueuedAttack = false;
            _currentStepIndex++;
            _attackDirection = _nextAttackDirection;
            _nextAttackDirection = Vector3.zero;

            PlayCurrentStep();
        }
        else
        {
            FinishAttack();
        }
    }


    private void PlayCurrentStep()
    {
        AttackStepData attackStepData = _weaponData.ComboData.GetStep(_currentStepIndex);
        Debug.Log($"{_currentStepIndex + 1}타 !");
        _animator.Play(attackStepData.AnimationName, 0, 0f);

    }
}
