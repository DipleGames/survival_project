using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAnimationEventRelay : MonoBehaviour
{
    [SerializeField] private ComboAttackController _comboAttackController;

    public void OnAttackStepFinished()
    {
        _comboAttackController.HandleStepFinished();
    }
}
