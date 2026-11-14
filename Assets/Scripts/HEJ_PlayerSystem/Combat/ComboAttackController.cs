using System;
using UnityEngine;
using Cinemachine;

public class ComboAttackController : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    [SerializeField] private MeleeHitDetector _hitDetector;

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

    private void OnDisable()
    {
        CancelAttack();
    }

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

        _hitDetector.EndReflectionWindow();
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
        if (_hitDetector != null) _hitDetector.EndReflectionWindow();
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


    private void PlayCurrentStep(Vector3 attackDirection)
    {
        AttackStepData step = _weaponData.ComboData.GetStep(_currentStepIndex);
        _hitDetector.BeginStep();  // 타격 감지 시작
        TryReflectArrowDuringCurrentStep();
        transform.position += attackDirection * 0.3f;
        _playerMove.FaceDirection(attackDirection);
        _hitDetector.BeginReflectionWindow(step, TryReflectArrowDuringCurrentStep);
        TryReflectArrowDuringCurrentStep();
        _animator.Play(step.AnimationName, 0, 0f);
        Debug.Log($"{_currentStepIndex + 1}타 !");
    }

    public void HandleAttackHit()
    {
        if (!_isAttacking) return;

        AttackStepData step = _weaponData.ComboData.GetStep(_currentStepIndex);
        float damage = _player.Status.GetStat(StatType.AttackPower); // 최종 스탯 공격력
        bool hasHit = _hitDetector.DetectHit(step, _attackDirection, damage);
        if (!hasHit) return;

        PlayHitImpulse();
        if (step.AttackSFX != null)
        {
            AudioManager.Instance.PlaySFX(step.AttackSFX);
        }
    }

    // 타격시 흔들림 효과 (예시로 공격타이밍에 호출하다가 나중에 타격시로 바꿀예정)
    public void PlayHitImpulse()
    {
        _impulseSource.GenerateImpulse();
    }

    private void TryReflectArrowDuringCurrentStep()
    {
        if (!_isAttacking || _weaponData == null || _hitDetector == null) return;

        AttackStepData step = _weaponData.ComboData.GetStep(_currentStepIndex);
        if (_hitDetector.TryReflectNearestArrow(step, _attackDirection))
        {
            PlayHitImpulse();
        }
    }
}
