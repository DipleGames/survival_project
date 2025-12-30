using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 나중에 빌드 예외구역이 생길 시 사용할 스크립트
/// </summary>
public class BuildExceptionManager : Singleton<BuildExceptionManager>
{
    public List<Vector3Int> NE_SW_UniqueAreaList = new List<Vector3Int>();
    public List<Vector3Int> NW_SE_UniqueAreaList = new List<Vector3Int>();
    public HashSet<Vector3Int> NE_SW_UniqueAreaHash = new HashSet<Vector3Int>();
    public HashSet<Vector3Int> NW_SE_UniqueAreaHash = new HashSet<Vector3Int>();
    public Dictionary<LineType, HashSet<Vector3Int>> uniqueAreaDic = new Dictionary<LineType, HashSet<Vector3Int>>();
}
