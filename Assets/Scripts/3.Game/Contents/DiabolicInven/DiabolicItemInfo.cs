using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemShape
{
    [SerializeField] int _width;
    [SerializeField] int _height;
    [SerializeField] bool[] _shape;

    public int Width => _width;
    public int Height => _height;
    public bool[] Shape => _shape;
/*
    public ItemShape(int width, int height)
    {
        _width = width;
        _height = height;
        _shape = new bool[_width * _height];
    }

    public ItemShape(bool[,] shape)
    {
        _width = shape.GetLength(0);
        _height = shape.GetLength(1);
        _shape = new bool[_width * _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _shape[GetIndex(x, y)] = shape[x, y];
            }
        }
    }

    private int GetIndex(int x, int y)
    {
        y = (_height - 1) - y;
        return x + _width * y;
    }*/
}

[CreateAssetMenu(fileName = "new Item", menuName = "GameData/DiabolicItem")]
public class DiabolicItemInfo : ScriptableObject
{
    [SerializeField] int itemNum;
    [SerializeField] int weightValue;
    [SerializeField] Sprite itemSprite;
    [SerializeField] string itemName;
    [SerializeField] AudioClip equipSound;

    [Header("Stat")]
    [SerializeField] int maxHp;
    [SerializeField] int damage;
    [SerializeField] int closeDamage;
    [SerializeField] int longDamage;
    [SerializeField] int recoverHp;
    [SerializeField] int defence;
    [SerializeField] int attackSpeed;
    [SerializeField] int speed;
    [SerializeField] int critical;
    [SerializeField] int avoid;

    [Header("Special Stat")]
    [SerializeField] bool isDoubleAxe;
    [SerializeField] bool isRum;
    [SerializeField] bool isAmmoPouch;
    [SerializeField] bool isHandMirror;
    [SerializeField] bool isRustyHarpoon;
    [SerializeField] bool isBaitWarm;
    [SerializeField] bool isGrape;
    [SerializeField] bool isTabatiere;
    [SerializeField] bool isSoulMate;
    [SerializeField] bool isInvincible;
    [SerializeField] bool isSilverBullet;
    [SerializeField] bool isBloodMadness;
    [SerializeField] bool isRottenCheese;
    [SerializeField] bool isTurtle;
    [SerializeField] bool isMirror;

    [SerializeField] int maxCount;
    [SerializeField] ItemShape itemShape;  // 아이템 모양
    [TextArea]
    [SerializeField] string specialStatInfo;
    [TextArea]
    [SerializeField] string description;

    Dictionary<Status, int> itemStatus = new Dictionary<Status, int>();
    Dictionary<SpecialStatus, bool> itemSpecialStatus = new Dictionary<SpecialStatus, bool>();

    public int ItemNum => itemNum;
    public int WeightValue => weightValue;
    public Sprite ItemSprite => itemSprite;
    public string ItemName => itemName;
    public AudioClip EquipSound => equipSound;
    public int MaxCount => maxCount;
    public ItemShape ItemShape => itemShape;
    public string SpecialStatInfo => specialStatInfo;
    public string Description => description;

    public Dictionary<Status, int> Stat()
    {
        if (itemStatus == null)
        {
            itemStatus.Add(Status.Maxhp, maxHp);
            itemStatus.Add(Status.Damage, damage);
            itemStatus.Add(Status.CloseDamage, closeDamage);
            itemStatus.Add(Status.LongDamage, longDamage);
            itemStatus.Add(Status.Recover, recoverHp);
            itemStatus.Add(Status.Defence, defence);
            itemStatus.Add(Status.AttackSpeed, attackSpeed);
            itemStatus.Add(Status.MoveSpeed, speed);
            itemStatus.Add(Status.Critical, critical);
            itemStatus.Add(Status.Avoid, avoid);
        }

        else
        {
            itemStatus[Status.Maxhp] = maxHp;
            itemStatus[Status.Damage] = damage;
            itemStatus[Status.CloseDamage] = closeDamage;
            itemStatus[Status.LongDamage] = longDamage;
            itemStatus[Status.Recover] = recoverHp;
            itemStatus[Status.Defence] = defence;
            itemStatus[Status.AttackSpeed] = attackSpeed;
            itemStatus[Status.MoveSpeed] = speed;
            itemStatus[Status.Critical] = critical;
            itemStatus[Status.Avoid] = avoid;
        }

        return itemStatus;
    }

    public Dictionary<SpecialStatus, bool> SpecialStat()
    {
        if (itemSpecialStatus == null)
        {
            itemSpecialStatus.Add(SpecialStatus.DoubleAxe, isDoubleAxe);
            itemSpecialStatus.Add(SpecialStatus.Rum, isRum);
            itemSpecialStatus.Add(SpecialStatus.AmmoPouch, isAmmoPouch);
            itemSpecialStatus.Add(SpecialStatus.RustyHarpoon, isRustyHarpoon);
            itemSpecialStatus.Add(SpecialStatus.BaitWarm, isBaitWarm);
            itemSpecialStatus.Add(SpecialStatus.Grape, isGrape);
            itemSpecialStatus.Add(SpecialStatus.Tabatiere, isTabatiere);
            itemSpecialStatus.Add(SpecialStatus.Soulmate, isSoulMate);
            itemSpecialStatus.Add(SpecialStatus.Invincible, isInvincible);
            itemSpecialStatus.Add(SpecialStatus.SilverBullet, isSilverBullet);
            itemSpecialStatus.Add(SpecialStatus.BloodMadness, isBloodMadness);
            itemSpecialStatus.Add(SpecialStatus.RottenCheese, isRottenCheese);
            itemSpecialStatus.Add(SpecialStatus.TurTle, isTurtle);
            itemSpecialStatus.Add(SpecialStatus.Mirror, isMirror);
        }

        else
        {
            itemSpecialStatus[SpecialStatus.DoubleAxe] = isDoubleAxe;
            itemSpecialStatus[SpecialStatus.Rum] = isRum;
            itemSpecialStatus[SpecialStatus.AmmoPouch] = isAmmoPouch;
            itemSpecialStatus[SpecialStatus.RustyHarpoon] = isRustyHarpoon;
            itemSpecialStatus[SpecialStatus.BaitWarm] = isBaitWarm;
            itemSpecialStatus[SpecialStatus.Grape] = isGrape;
            itemSpecialStatus[SpecialStatus.Tabatiere] = isTabatiere;
            itemSpecialStatus[SpecialStatus.Soulmate] = isSoulMate;
            itemSpecialStatus[SpecialStatus.Invincible] = isInvincible;
            itemSpecialStatus[SpecialStatus.SilverBullet] = isSilverBullet;
            itemSpecialStatus[SpecialStatus.BloodMadness] = isBloodMadness;
            itemSpecialStatus[SpecialStatus.RottenCheese] = isRottenCheese;
            itemSpecialStatus[SpecialStatus.TurTle] = isTurtle;
            itemSpecialStatus[SpecialStatus.Mirror] = isMirror;
        }

        return itemSpecialStatus;
    }
}
