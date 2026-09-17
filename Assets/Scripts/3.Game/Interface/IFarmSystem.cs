using UnityEngine;

public interface IFarmSystem
{
    void Cultivate(Vector3Int cellPos);
    void Plant(Vector3Int cellPos, CropSO cropSO);
    void Water(Vector3Int cellPos);
    CropData Harvest(Vector3Int cellPos);
}