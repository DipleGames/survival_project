using UnityEngine;

public interface IPlayerState
{
    void Enter();

    void HandleAttackInput(WeaponData weaponData, Vector3 attackDirection);

    void Exit();
}
