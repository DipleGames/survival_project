using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public enum RangeMode
{
    Arc,
    Circle
}

public class WeaponManager : Singleton<WeaponManager>
{
    [Header("Time Control")]
    [SerializeField] public float changeDelay = 0.5f;
    [SerializeField] public float delayTime;
    [HideInInspector] public bool canAttack;
    [Range(0f, 2f)]
    [HideInInspector] public float chargeTime = 0f;

    private int beforeIndex = 0;
    private int currentIndex = 0;
    private int nextIndex = 0;

    public int BeforeIndex { get { return beforeIndex; } set { beforeIndex = value; } }
    public int CurrentIndex { get { return currentIndex; } set { currentIndex = value; } }
    public int NextIndex { get { return nextIndex; } set { nextIndex = value; } }

    [Header("Weapon Info")]
    public Transform weaponParent;
    [HideInInspector] public int weaponCount = 4;
    [HideInInspector] public bool canWeaponChange = true;
    [SerializeField] public SpriteRenderer[] weaponRenderer;
    public int rangeMode;


    private void Start()
    {
        currentIndex = 0;
        canAttack = false;
        delayTime = changeDelay;
        chargeTime = 0f;
    }

    protected void Update()
    {
        if (delayTime > 0) { delayTime -= Time.deltaTime; }
        else {
            if (!canAttack)
            {
                canAttack = true;
                canWeaponChange = true;
            }
        }
    }

    public Sprite GetWeaponImage(int targetIdx)
    {
        if (weaponRenderer[targetIdx] != null)
            return weaponRenderer[targetIdx].sprite;
        else
            return null;
    }

    public void ChangeCurrentIdxColor(Color targetColor)
    {
        if (weaponRenderer[currentIndex] != null)
            weaponRenderer[currentIndex].color = targetColor;
    }
    public void ChangeTargetIdxColor(int targetIdx, Color targetColor)
    {
        if (weaponRenderer[currentIndex] != null)
            weaponRenderer[currentIndex].color = targetColor;
    }

    public void SetChangeDelay()
    {
        delayTime = changeDelay;
    }

}

