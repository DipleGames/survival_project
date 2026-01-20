using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBuildInfo : BuildableObject
{
    public override List<Vector3Int> GetBuildArea(Vector3Int cellIndex)
    {
        buildArea.Clear();

        switch(requiredCell)
        {
            case 4:
                buildArea.Add(cellIndex);
                buildArea.Add(cellIndex + new Vector3Int(1,0,0));
                buildArea.Add(cellIndex + new Vector3Int(0,-1,0));
                buildArea.Add(cellIndex + new Vector3Int(1,-1,0));
                break;
            case 9:
                buildArea.Add(cellIndex);
                buildArea.Add(cellIndex + new Vector3Int(1,0,0));
                buildArea.Add(cellIndex + new Vector3Int(-1,0,0));
                buildArea.Add(cellIndex + new Vector3Int(-1,-1,0));
                buildArea.Add(cellIndex + new Vector3Int(0,-1,0));
                buildArea.Add(cellIndex + new Vector3Int(1,-1,0));
                buildArea.Add(cellIndex + new Vector3Int(-1,1,0));
                buildArea.Add(cellIndex + new Vector3Int(0,1,0));
                buildArea.Add(cellIndex + new Vector3Int(1,1,0));
                break;
        }
        return buildArea;
    }

    public override Vector3 GetVisualCenter(Grid grid)
    {
        return base.GetVisualCenter(grid);
    }
}
