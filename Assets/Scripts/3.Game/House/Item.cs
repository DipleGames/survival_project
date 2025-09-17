using System;
using System.Collections.Generic;
using UnityEngine;

public enum MaterialType
{
    Wood,
    Bamboo,
    BlackWood,
    Branch,
    Fruit,
    BetterFruit,
    BestFruit,
    Fish,
    HighFish,
    SilkFish,
    Patch,
    Vodka,
    Rope,
    Bottle,
    Flour,
    Salt,
    SeeSalt,
    Soil,
    Mud,
    Stick,
    WaterBottle,

    Rock,
    Count,

    Shell,
    BlueCoral,
    Sand,
    RedDye,
    YellowDye,
    GreenDye,
    BlueDye,
    PurpleDye,
}

public enum ItemType { Facility, Consumable, Tool, Material, Piece, Weapon, BeachPuzzlePiece };

public enum Acquisition
{
    CraftTable, CampFire, Logging, Bush, Fishing, FishPot, Item
}

public class Item
{

    /// <summary>
    /// XXXXXX
    /// ^^    : 01 = 아이템
    ///   ^^  : 아이템 종류 구분
    /// </summary>
    public int ItemId { get; }
    public string ItemName { get; }
    public ItemType Type { get; }
    public Dictionary<MaterialType, int> NeedMaterials { get; }
    public int ImageId { get; }
    public List<Acquisition> AcquisitionList { get; }
    public Dictionary<Acquisition, int> takeTimeByAcquisition { get; }
    public float CreateTime { get; }
    public bool IsConsumable { get; }
    public string ItemEffect {  get; }
    public string Description { get; }

    /*public Item(ulong itemId, ItemType type, Dictionary<MaterialType, int> needMaterials, DiabolicItemInfo info)
    {
        ItemId = itemId;
        NeedMaterials = needMaterials;
        Type = type;
        PieceId = info.ItemNum;
    }*/

    public Item(int itemId, string itemName, int type, string needMaterialTypes, string needMaterialCounts, string takeTimes, string acquisitions, int isConsumable, string effect, string decription)
    {
        ItemId = itemId;
        ItemName = itemName;
        ImageId = itemId;

        string[] tempTypeString = needMaterialTypes.Split(',');
        string[] tempCountString = needMaterialCounts.Split(",");

        Dictionary<MaterialType, int> tempDic = new Dictionary<MaterialType, int>();

        for (int i = 0; i < tempTypeString.Length; i++)
        {
            if(string.IsNullOrEmpty(tempTypeString[i]))
                continue;

            MaterialType mType = (MaterialType)int.Parse(tempTypeString[i]);
            int mCount = int.Parse(tempCountString[i]);

            tempDic.Add(mType, mCount);
        }

        NeedMaterials = tempDic;
        Type = (ItemType)type;

        string[] tempTakeTimes = takeTimes.Split(",");
        string[] tempAquisitions = acquisitions.Split(",");

        Dictionary<Acquisition, int> tempTakeTimesByAcquisition = new Dictionary<Acquisition, int>();
        AcquisitionList = new List<Acquisition>();

        for (int i = 0; i < tempAquisitions.Length; i++)
        {
            Acquisition acquisition = (Acquisition)int.Parse(tempAquisitions[i]);
            int takeTime = int.Parse(tempTakeTimes[i]);

            AcquisitionList.Add(acquisition);
            tempTakeTimesByAcquisition.Add(acquisition, takeTime);
        }

        takeTimeByAcquisition = tempTakeTimesByAcquisition;

        IsConsumable = Convert.ToBoolean(isConsumable);
        ItemEffect = effect.Replace("\\n", "\n");
        Description = decription;
    }

    public void AddItem()
    {
        /*switch (Type)
        {
            case ItemType.PieceItem:
                ItemManager.Instance.AddItem(PieceId);
                break;

        }*/
    }
}
