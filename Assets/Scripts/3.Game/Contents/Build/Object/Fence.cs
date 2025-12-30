using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fence : BuildableObject
{
    [Header("울타리 방향")]
    public LineType lineType;

    public override List<Vector3Int> GetBuildArea(Vector3Int cellIndex)
    {
        buildArea.Clear();

        switch(requiredCell)
        {
            case 1:
                buildArea.Add(cellIndex);
                break;
            case 2:
                if(lineType == LineType.NE_SW)
                {     
                    buildArea.Add(cellIndex);
                    buildArea.Add(cellIndex + new Vector3Int(1,0,0));
                }
                else if(lineType == LineType.NW_SE)
                {
                    buildArea.Add(cellIndex);
                    buildArea.Add(cellIndex + new Vector3Int(0,-1,0));
                }
                break;
            case 3:
                if(lineType == LineType.NE_SW)
                {     
                    buildArea.Add(cellIndex);
                    buildArea.Add(cellIndex + new Vector3Int(1,0,0));
                    buildArea.Add(cellIndex + new Vector3Int(2,0,0));
                }
                else if(lineType == LineType.NW_SE)
                {
                    buildArea.Add(cellIndex);
                    buildArea.Add(cellIndex + new Vector3Int(0,-1,0));
                    buildArea.Add(cellIndex + new Vector3Int(0,-2,0));
                }
                break;
        }
        return buildArea;
    }

    public override Vector3 GetVisualCenter(Grid grid)
    {
        return base.GetVisualCenter(grid);
    }

    public void SetUniqueArea(Vector3Int cellIndex)
    {
        if(lineType == LineType.NE_SW)
        {
            BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.Add(cellIndex);
            BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.Add(cellIndex + new Vector3Int(1,0,0));
            BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.Add(cellIndex + new Vector3Int(2,0,0));
            BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.Add(cellIndex + new Vector3Int(0,1,0));
            BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.Add(cellIndex + new Vector3Int(1,1,0));
            BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.Add(cellIndex + new Vector3Int(2,1,0));
            BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.Add(cellIndex + new Vector3Int(0,-1,0));
            BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.Add(cellIndex + new Vector3Int(1,-1,0));
            BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.Add(cellIndex + new Vector3Int(2,-1,0));
        }
        else if(lineType == LineType.NW_SE)
        {
            BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.Add(cellIndex);
            BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.Add(cellIndex + new Vector3Int(0,-1,0));
            BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.Add(cellIndex + new Vector3Int(0,-2,0));
            BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.Add(cellIndex + new Vector3Int(1,0,0));
            BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.Add(cellIndex + new Vector3Int(1,-1,0));
            BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.Add(cellIndex + new Vector3Int(1,-2,0));
            BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.Add(cellIndex + new Vector3Int(-1,0,0));
            BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.Add(cellIndex + new Vector3Int(-1,-1,0));
            BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.Add(cellIndex + new Vector3Int(-1,-2,0));
        }

        BuildExceptionManager.Instance.NE_SW_UniqueAreaList = BuildExceptionManager.Instance.NE_SW_UniqueAreaHash.ToList();
        BuildExceptionManager.Instance.NW_SE_UniqueAreaList = BuildExceptionManager.Instance.NW_SE_UniqueAreaHash.ToList();

        BuildExceptionManager.Instance.uniqueAreaDic[LineType.NE_SW] = BuildExceptionManager.Instance.NE_SW_UniqueAreaHash;
        BuildExceptionManager.Instance.uniqueAreaDic[LineType.NW_SE] = BuildExceptionManager.Instance.NE_SW_UniqueAreaHash;
    }

}
