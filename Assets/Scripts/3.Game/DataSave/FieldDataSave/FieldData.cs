using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class FarmTileSaveData
{
    public int tileId;
    public int cropId = -1; // -1이면 작물이 없는 상태
    public float growthProgress = 0f; // 작물 성장 진행도
    public bool isWatered = false; // 물을 준 상태인지 여부
}
[Serializable]
public class FarmAreaSaveData
{
    public int areaId;
    public string areaName;
    public List<int> tileIds = new(); // 이 농장 구역에 속한 농지 타일 ID 목록
}
[Serializable]
public class SkullSaveData
{
    public int skullId;
    public string skullName;
    public Vector3 position; // 일꾼의 위치
    public float health; // 일꾼의 체력
    public int assignedTaskId = -1; // -1이면 작업이 없는 상태
}
[Serializable]
public class WorkAreaSaveData
{
    public int workAreaId;
    public string workAreaName;
    public Vector3 position; // 작업 위치
    public int taskType; // 작업 종류 (예: 0 = 농사, 1 = 채집 등)
}

[Serializable]
public class HouseSaveData
{
    public int houseId;
    public string houseName;
    public Vector3 position; // 집 위치
    public bool isOccupied; // 집이 점유되어 있는지 여부
}

[Serializable]
public class ResourceNodeSaveData
{
    public int resourceNodeId;
    public string resourceNodeName;
    public Vector3 position; // 채집 대상 위치
    public int resourceType; // 자원 종류 (예: 0 = 나무, 1 = 돌 등)
    public int remainingResources; // 남은 자원 수량
}