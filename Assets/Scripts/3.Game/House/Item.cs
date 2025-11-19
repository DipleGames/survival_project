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
[System.Flags]
public enum ItemType
{
    None = 0,
    Facility = 1 << 0,
    Consumable = 1 << 1,
    Tool = 1 << 2,
    Material = 1 << 3,
    Piece = 1 << 4,
    Weapon = 1 << 5,
    BeachPuzzlePiece = 1 << 6,
    Preyable = 1 << 7,
};

// ItemType tempType = (ItemType)(1 << { 기존 ItemType변�명 }); ;

//public enum ItemType
//{
//    Facility,
//    Consumable,
//    Tool,
//    Material,
//    Piece,
//    Weapon,
//    BeachPuzzlePiece,
//};

public enum Acquisition
{
    CraftTable, CampFire, Logging, Bush, Fishing, FishPot, Item
}

public enum PreyType
{
    NONE = 0, FISH = 1, FOOD, FRUIT, ORE,
}

public class Item
{

    /// <summary>
    /// XXXXXX
    /// ^^    : 01 = �이
    ///   ^^  : �이종류 구분
    /// </summary>
    public int ItemId { get; }
    public string ItemName { get; }
    public ItemType Type { get; }
    public Dictionary<MaterialType, int> NeedMaterials { get; }
    public int ImageId { get; }
    public List<Acquisition> AcquisitionList { get; }
    public Dictionary<Acquisition, int> takeTimeByAcquisition { get; }
    public int MaxCount { get; }
    public float CreateTime { get; }
    public bool IsConsumable { get; }
    public PreyType Preytype { get; }
    public string ItemEffect {  get; }
    public string Description { get; }

    /*public Item(ulong itemId, ItemType type, Dictionary<MaterialType, int> needMaterials, DiabolicItemInfo info)
    {
        ItemId = itemId;
        NeedMaterials = needMaterials;
        Type = targetType;
        PieceId = info.ItemNum;
    }*/

    //public Item(int itemId, string itemName, int targetType, string acquisitions, string needMaterialTypes, string needMaterialCounts, string takeTimes, string takePercent, int isConsumable, int maxCount, float createTime, string effect, string decription)
    //{
    //    ItemId = itemId;
    //    ItemName = itemName;
    //    ImageId = itemId;

    //    string[] tempTypeString = needMaterialTypes.Split(',');
    //    string[] tempCountString = needMaterialCounts.Split(",");

    //    Dictionary<MaterialType, int> tempDic = new Dictionary<MaterialType, int>();

    //    for (int i = 0; i < tempTypeString.Length; i++)
    //    {
    //        if(string.IsNullOrEmpty(tempTypeString[i]))
    //            continue;

    //        MaterialType mType = (MaterialType)int.Parse(tempTypeString[i]);
    //        int mCount = int.Parse(tempCountString[i]);

    //        tempDic.Add(mType, mCount);
    //    }

    //    NeedMaterials = tempDic;
    //    Type = (ItemType)targetType;

    //    string[] tempTakeTimes = takeTimes.Split(",");
    //    string[] tempAquisitions = acquisitions.Split(",");

    //    Dictionary<Acquisition, int> tempTakeTimesByAcquisition = new Dictionary<Acquisition, int>();
    //    AcquisitionList = new List<Acquisition>();

    //    for (int i = 0; i < tempAquisitions.Length; i++)
    //    {
    //        Acquisition acquisition = (Acquisition)int.Parse(tempAquisitions[i]);
    //        int takeTime = int.Parse(tempTakeTimes[i]);

    //        AcquisitionList.Add(acquisition);
    //        tempTakeTimesByAcquisition.Add(acquisition, takeTime);
    //    }

    //    takeTimeByAcquisition = tempTakeTimesByAcquisition;

    //    IsConsumable = Convert.ToBoolean(isConsumable);
    //    MaxCount = maxCount;
    //    CreateTime = createTime;
    //    ItemEffect = effect.Replace("\\n", "\n");
    //    Description = decription;
    //}
    public Item(int itemId, string itemName, string type, string acquisitions, string needMaterialTypes, string needMaterialCounts, string takeTimes, string takePercent, int isConsumable, int maxCount, float createTime, int preytype, string effect, string decription)
    {
        ItemId = itemId;
        ItemName = itemName;
        ImageId = itemId;

        string[] tempTypeString = needMaterialTypes.Split(',');
        string[] tempCountString = needMaterialCounts.Split(",");

        Dictionary<MaterialType, int> tempDic = new Dictionary<MaterialType, int>();

        for (int i = 0; i < tempTypeString.Length; i++)
        {
            if (string.IsNullOrEmpty(tempTypeString[i]))
                continue;

            MaterialType mType = (MaterialType)int.Parse(tempTypeString[i]);
            int mCount = int.Parse(tempCountString[i]);

            tempDic.Add(mType, mCount);
        }

        NeedMaterials = tempDic;

        int _type = 0;
        for (int i = 0; i < type.Length; i++)
        {
            if (type[i] == '1')
                _type |= (1 << i);
        }
        Type = (ItemType)_type;

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
        MaxCount = maxCount;
        CreateTime = createTime;
        Preytype = (PreyType)preytype;
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

    /// <summary>
    /// �비교 �산 
    /// </summary>
    /// <param name="targetType">비교비트 마스�/param>
    /// <param name="andLogic">targetType모두 만족�야�는지, �나�도 만족�면 �는지</param>
    /// <returns></returns>
    public bool CompareType(ItemType targetType, bool andLogic)
    {
        if (targetType == ItemType.None)
        {
            return andLogic;
        }

        if (andLogic)
        {
            return (Type & targetType) == targetType;
        }
        else
        {
            return (Type & targetType) != 0;
        }
    }
    public bool CompareTypeExact(ItemType mask)
    => Type == mask;
}
