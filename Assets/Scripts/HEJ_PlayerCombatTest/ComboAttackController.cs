using System;
using UnityEngine;
using Cinemachine;

public class ComboAttackController : MonoBehaviour
{

    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private CinemachineImpulseSource _impulseSource;

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
        PlayCurrentStep(_attackDirection);
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

            PlayCurrentStep(_attackDirection);
        }
        else
        {
            FinishAttack();
        }
    }


    private void PlayCurrentStep(Vector3 _attackDirection)
    {
        AttackStepData attackStepData = _weaponData.ComboData.GetStep(_currentStepIndex);
        transform.position += _attackDirection * 0.3f; 
        _playerMove.FaceDirection(_attackDirection);
        PlayHitImpulse();
        AudioTest.Instance.PlaySFX(attackStepData.AttackSFX);
        _animator.Play(attackStepData.AnimationName, 0, 0f);
        Debug.Log($"{_currentStepIndex + 1}타 !");
    }

    // 타격시 흔들림 효과 (예시로 공격타이밍에 호출하다가 나중에 타격시로 바꿀예정)
    public void PlayHitImpulse()
    {
        _impulseSource.GenerateImpulse();
    }
}
