using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairRequirement : MonoBehaviour
{
    [SerializeField] List<RepairMaterial> repair_materials;

    private Dictionary<int, SortedList<float, RepairMaterial>> materialMap;

    void Awake()
    {
        BuildMaterialMap();
    }

    // Dictionary 초기화
    void BuildMaterialMap()
    {
        materialMap = new Dictionary<int, SortedList<float, RepairMaterial>>();

        foreach (var material in repair_materials)
        {
            if (!materialMap.ContainsKey(material.level))
            {
                materialMap[material.level] = new SortedList<float, RepairMaterial>();
            }

            var hpMap = materialMap[material.level];
            if (!hpMap.ContainsKey(material.hp))
            {
                hpMap.Add(material.hp, material);
            }
        }
    }

    public RepairMaterial GetMaterialList(int level, float hp)
    {
        if (!materialMap.ContainsKey(level))
            return null;

        var hpMap = materialMap[level];
        RepairMaterial result = null;

        for (int i = hpMap.Count - 1; i >= 0; i--)
        {
            if (hp >= hpMap.Keys[i])
            {
                result = hpMap.Values[i];
                break;
            }
        }

        return result;
    }

    //public RepairMaterial GetMaterialList(int level, float hp)
    //{
    //    RepairMaterial result = new RepairMaterial();
    //    float maxHpinList = float.MinValue;
    //    foreach (var material in repair_materials)
    //    {
    //        if (material.level == level && hp >= material.hp && material.hp >= maxHpinList)
    //        {
    //            maxHpinList = material.hp;
    //            result = material;
    //        }
    //    }
    //    return result;
    //}
}

[System.Serializable]
public class RepairMaterial
{ 
    public int level;
    public float hp;
    public MaterialType type;
    public int quantity;
}