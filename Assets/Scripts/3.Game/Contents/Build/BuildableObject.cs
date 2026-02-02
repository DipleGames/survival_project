using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public enum BuildType
{
    Single,     // 낱개 (1x1 칸 하나만 설치)
    Line,       // 1자형 (직선으로 쭉 설치)
    Area,       // 채워진 사각형 (안쪽까지 꽉 채워서 설치)
}

public enum LineType
{
    NE_SW,     
    NW_SE,   
}


public abstract class BuildableObject : MonoBehaviour
{
    public int requiredCell;
    public BuildType buildType;
    public List<Vector3Int> buildArea = new List<Vector3Int>();
    public List<Vector3Int> fullBuildArea;
    public bool isBuilt = false;

    public abstract List<Vector3Int> GetBuildArea(Vector3Int cellIndex);
    public virtual Vector3 GetVisualCenter(Grid grid)
    {
        if (buildArea.Count == 0) return Vector3.zero;
        
        Vector3 sum = Vector3.zero;
        foreach (var cell in buildArea)
        {
            sum += grid.GetCellCenterWorld(cell);
        }
        return sum / buildArea.Count; // 모든 셀 위치의 평균값
    }
}

