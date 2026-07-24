using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class ObjectFarmSystem : Singleton<ObjectFarmSystem>, IFarmSystem
{
    public enum FarmTileState
    {
        Empty,
        Cultivated,
        Occupied
    }

    public enum FarmWorkResult 
    {
        Planted,
        Watered,
        Harvested,
        NoWork
    }

    [SerializeField] private Grid _grid;
    [SerializeField] private FarmPlot _farmPlot;
    [SerializeField] private RuntimeGridRenderer _runtimeGridRenderer;
    
    private Dictionary<Vector3Int, FarmTileState> _farmTileDict = new();
    private Dictionary<Vector3Int, CropData> _cropDict = new();

    public bool isFarmMode = false;

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPos = _grid.WorldToCell(mouseWorldPos);

        if(Input.GetKeyDown(KeyCode.F))
        {
            isFarmMode = isFarmMode == true ? false : true;
            _runtimeGridRenderer.gameObject.SetActive(isFarmMode);
        
        }

        if(!isFarmMode) return;

        if(Input.GetMouseButtonDown(0))
        {
            Cultivate(cellPos);
        }
    }

    public void Cultivate(Vector3Int pos)
    {
        if (_farmTileDict.TryGetValue(pos, out FarmTileState state))
        {
            if (state != FarmTileState.Empty)
                return;
        }

        Vector3 worldPos = _grid.GetCellCenterWorld(pos);
        Instantiate(_farmPlot, worldPos, Quaternion.identity);

        _farmTileDict[pos] = FarmTileState.Cultivated;
    }

    public void Plant(Vector3Int pos, CropSO cropSO)
    {
     
    }

    public void Water(Vector3Int pos)
    {
       
    }

    public void Harvest(Vector3Int pos)
    {
 
    }
}
