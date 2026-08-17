using System.Collections.Generic;
using UnityEngine;

public class FarmArea
{
    private readonly List<FarmTile> _tiles = new();

    public IReadOnlyList<FarmTile> Tiles => _tiles;
    public Vector3Int CenterCellPosition { get; }

    public FarmArea(Vector3Int centerCellPosition)
    {
        CenterCellPosition = centerCellPosition;
    }

    public void AddTile(FarmTile tile)
    {
        if (tile == null || _tiles.Contains(tile))
            return;

        _tiles.Add(tile);
    }

    public void RemoveTile(FarmTile tile)
    {
        _tiles.Remove(tile);
    }
}