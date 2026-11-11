using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SaveData
{
    // SaveData 버전 관리용 -> 이후 추가되는 데이터가 있을경우 그에 대응하기위해
    public int saveVersion = 1;
    // 현재 씬 이름
    public string ScenceName;
    // 저장한 시간을 기록할거면 사용
    // data.savedAtUtc = System.DateTime.UtcNow.ToString("O"); 저장할거면 이 코드를 사용
    // public string savedAtUtc;
    // 플레이어의 정보값은 PlayerSaveData에 저장
    public PlayerSaveData player = new();

/*    // 농지 한 칸마다의 상태와 작물 정보
    public List<FarmTileSaveData> farmTiles = new();
    // 여러 농지를 묶는 농장 구역 정보
    public List<FarmAreaSaveData> farmAreas = new();
    // 일꾼의 정보
    public List<SkullSaveData> skulls = new();
    // 등록된 작업 위치와 작업 종류
    public List<WorkAreaSaveData> workAreas = new();
    // 집별상태
    public List<HouseSaveData> houses = new();
    // 채집대상의 상태
    public List<ResourceNodeSaveData> resourceNodes = new();*/
    // 현재 던전의 구성과 진행상황
    // public DungeonSaveData dungeon;

}

