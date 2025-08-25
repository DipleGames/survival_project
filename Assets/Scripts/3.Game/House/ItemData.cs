using System;
using System.IO;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;

[System.Serializable]
public class ItemInfo
{
    public int itemId;
    public string itemName;
    public int itemType;
    public string acquisitions;
    public string needMaterialTypes;
    public string needMaterialCounts;
    public string takeTimeByAcquisition;
    public string takePercentByAcquisition;
    public int isConsumable;
    public int maxCount;
    public string effect;
    public string decription;

    public ItemInfo(int itemId, string itemName, int itemType, string acquisitions, string needMaterialTypes, string needMaterialCounts, string takeTime, string takePercent, int isConsumable, int maxCount, string effect, string decription)
    {
        this.itemId = itemId;
        this.itemName = itemName;
        this.itemType = itemType;
        this.acquisitions = acquisitions;
        this.needMaterialTypes = needMaterialTypes;
        this.needMaterialCounts = needMaterialCounts;
        this.takeTimeByAcquisition = takeTime;
        this.takePercentByAcquisition = takePercent;
        this.isConsumable = isConsumable;
        this.maxCount = maxCount;
        this.decription = decription;
        this.effect = effect;
    }
}

public class ItemData : Singleton<ItemData>
{
    [SerializeField] public ItemInfo[] itemInfos;

    int count;

    private class ItemJson
    {
        public ItemInfo[] items;    // Json rootKey

        public ItemJson(ItemInfo[] items)
        {
            this.items = items;
        }
    }

    [ContextMenu("MakeJsonFile")]
    public void MakeJsonFile()
    {
        count = 2;
        ItemJson itemJson = new ItemJson(GetItemArray());

        string json = JsonUtility.ToJson(itemJson, true); 
        SaveFile("ItemtData", json);
    }

    private ItemInfo[] GetItemArray()
    {
        ItemInfo[] items = new ItemInfo[count];

        for (int i = 0; i < items.Length; i++)
        {
            ItemInfo newItem = new ItemInfo(0, "", 0, "", "", "", "", "", 0, 0, "", "");
            items[i] = newItem;
        }

        return items;
    }

    private void SaveFile(string fileName, string text)
    {
        string path = string.Format("{0}/{1}.txt", Application.dataPath, fileName);
        StreamWriter sw = new StreamWriter(path);
        sw.Write(text);
        sw.Close();

        Debug.Log("파일 저장 완료");
    }

    [ContextMenu("RefreshJsonFile")]
    public void RefreshJsonFile()
    {
        ItemJson itemJson = new ItemJson(RefreshJson());     

        string json = JsonUtility.ToJson(itemJson, true);
        SaveFile("ItemtData", json);
    }

    private ItemInfo[] RefreshJson()
    {
        ItemInfo[] allItemArray = itemInfos;

        return allItemArray;
    }

    [ContextMenu("AddJsonFile")]
    public void AddJsonFile()
    {
        ItemJson itemJson = new ItemJson(AddItemArray(1));

        string json = JsonUtility.ToJson(itemJson, true);
        SaveFile("ItemtData", json);
    }

    private ItemInfo[] AddItemArray(int add)
    {
        count += add;

        ItemInfo[] items = new ItemInfo[count];

        string json = LoadFile("ItemtData");
        object convert = JsonUtility.FromJson(json, typeof(ItemJson));
        ItemJson itemJson = convert as ItemJson;
        ItemInfo[] beforeItems = itemJson.items;

        for (int i = 0; i < beforeItems.Length; i++)
        {
            items[i] = beforeItems[i];
        }

        for (int j = beforeItems.Length; j < items.Length; j++)
        {
            ItemInfo newItem = new ItemInfo(0, "", 0, "", "", "", "", "", 0, 0, "", "");
            items[j] = newItem;
        }

        return items;
    }

    [ContextMenu("LoadJsonFile")]
    public void LoadJsonFile()
    {
        string json = LoadFile("ItemtData");
        object convert = JsonUtility.FromJson(json, typeof(ItemJson));
        ItemJson itemJson = convert as ItemJson;
        ItemInfo[] items = itemJson.items;

        itemInfos = items;

        Debug.Log("아이템 데이터 로드 완료");
    }

    private string LoadFile(string fileName)
    {
        string path = string.Format("{0}/{1}.txt", Application.dataPath, fileName);
        StreamReader sr = new StreamReader(path);
        string readToEnd = sr.ReadToEnd();
        sr.Close();

        return readToEnd;
    }
}
