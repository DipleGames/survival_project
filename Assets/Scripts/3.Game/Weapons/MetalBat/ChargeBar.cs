using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ChargeBar : MonoBehaviour
{
    [SerializeField] private Slider chargeBar;
    [SerializeField] Image chargebarFill;

    WeaponManager weaponManager;

    private void Awake()
    {
        weaponManager = WeaponManager.Instance;
    }

    private void OnEnable()
    {
        chargeBar.value = 0;
        chargebarFill.color = Color.yellow;
    }

    private void Update()
    {
        if (weaponManager.chargeTime > 0f)
        {
            chargeBar.value = weaponManager.chargeTime / 2f;

            if (chargeBar.value == 1f)
                chargebarFill.color = Color.red;
        }

    }

}
