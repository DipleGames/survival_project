using System.IO;
using UnityEngine;

[System.Serializable]
public class MonsterStat
{
    public int monsterNum;
    public string monsterName;
    public float monsterMaxHp;
    public float monsterDamage;
    public float monsterSpeed;
    public float attackDelay;
    public int attackCount;
    public float moveDelay;
    public int itemDropPercent;

    public MonsterStat(int monsterNum, string monsterName, float monsterMaxHp, float monsterDamage, float monsterSpeed, float attackDelay, int attackCount, float moveDelay, int itemDropPercent)
    {
        this.monsterNum = monsterNum;
        this.monsterName = monsterName;
        this.monsterMaxHp = monsterMaxHp;
        this.monsterDamage = monsterDamage;
        this.monsterSpeed = monsterSpeed;
        this.attackDelay = attackDelay;
        this.moveDelay = moveDelay;
        this.attackCount = attackCount;
        this.itemDropPercent = itemDropPercent;
    }
}

public class MonsterInfo : Singleton<MonsterInfo>
{
    [SerializeField] public MonsterStat[] monsterInfos;

    int count;

    private class MonsterStatJson
    {
        public MonsterStat[] stats;

        public MonsterStatJson(MonsterStat[] stats)
        {
            this.stats = stats;
        }
    }

    [ContextMenu("MakeJsonFile")]
    public void MakeJsonFile()
    {
        count = 6;
        MonsterStatJson statJson = new MonsterStatJson(GetMonsterArray());      // 그래서 monster 배열을 가지는 클래스를 하나 만들어서 여러 객체를 저장할 수 있게 만듦

        // ToJson(obj, bool): bool값이 true인 경우 가독성이 좋게, false인 경우 최소값으로 obj값을 출력
        string json = JsonUtility.ToJson(statJson, true);       // Json은 객체 하나만 저장가능
        SaveFile("monsterStatData", json);
    }

    private MonsterStat[] GetMonsterArray()
    {
        MonsterStat[] stats = new MonsterStat[count];

        for (int i = 0; i < stats.Length; i++)
        {
            MonsterStat newStat = new MonsterStat(0, "몬스터", 0, 0, 0, 0, 0, 0, 0);
            stats[i] = newStat;
        }

        return stats;
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
        MonsterStatJson statJson = new MonsterStatJson(RefreshJson());     

        string json = JsonUtility.ToJson(statJson, true);
        SaveFile("monsterStatData", json);
    }

    private MonsterStat[] RefreshJson()
    {
        MonsterStat[] allMonsterArray = monsterInfos;

        return allMonsterArray;
    }

    [ContextMenu("AddJsonFile")]
    public void AddJsonFile()
    {
        MonsterStatJson statJson = new MonsterStatJson(AddMonsterArray(1));      

        string json = JsonUtility.ToJson(statJson, true);
        SaveFile("monsterStatData", json);
    }

    private MonsterStat[] AddMonsterArray(int add)
    {
        count += add;

        MonsterStat[] stats = new MonsterStat[count];

        string json = LoadFile("monsterStatData");
        object convert = JsonUtility.FromJson(json, typeof(MonsterStatJson));
        MonsterStatJson statJson = convert as MonsterStatJson;
        MonsterStat[] beforeStats = statJson.stats;

        for (int i = 0; i < beforeStats.Length; i++)
        {
            stats[i] = beforeStats[i];
        }

        for (int j = beforeStats.Length; j < stats.Length; j++)
        {
            MonsterStat newStat = new MonsterStat(0, "몬스터", 0, 0, 0, 0, 0, 0, 0);
            stats[j] = newStat;
        }

        return stats;
    }

    [ContextMenu("LoadJsonFile")]
    public void LoadJsonFile()
    {
        string json = LoadFile("monsterStatData");
        object convert = JsonUtility.FromJson(json, typeof(MonsterStatJson));
        MonsterStatJson statJson = convert as MonsterStatJson;
        MonsterStat[] stats = statJson.stats;

        monsterInfos = stats;

        Debug.Log("몬스터 데이터 로드 완료");
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
