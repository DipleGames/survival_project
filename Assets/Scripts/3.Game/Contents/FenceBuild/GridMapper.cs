using UnityEngine;

public class GridMapper : Singleton<GridMapper>
{
    [SerializeField] private Grid grid;
    
    public Vector3Int cellIndex;
    public Vector3 cellCenterPos;
    public int region = 0;
    public Vector3 mouseXZPos; // 마우스의 실제 XZ 좌표

    void Update()
    {
        // 1. 레이캐스트를 이용해 마우스의 XZ 평면 좌표 구하기
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane xzPlane = new Plane(Vector3.up, Vector3.zero); // Y=0인 평면

        if (xzPlane.Raycast(ray, out float distance))
        {
            mouseXZPos = ray.GetPoint(distance);
            
            // 2. 그리드 셀 인덱스 구하기 (Unity Grid가 XZ를 지원하면 바로 변환)
            cellIndex = grid.WorldToCell(mouseXZPos);
            
            // 3. 셀 중심 좌표 구하기
            cellCenterPos = grid.GetCellCenterWorld(cellIndex);
            
            // 4. 타일 중심 기준 마우스의 상대 좌표 (Y는 무시)
            Vector3 localPos = mouseXZPos - cellCenterPos;

            // 5. 4등분 영역 판정 (사용자 요청: 십자(+) 형태 분할)
            if (localPos.x <= 0 && localPos.z >= 0) region = 1;      // 좌측 상단
            else if (localPos.x > 0 && localPos.z > 0) region = 2;  // 우측 상단
            else if (localPos.x < 0 && localPos.z < 0) region = 3;  // 좌측 하단
            else if (localPos.x >= 0 && localPos.z <= 0) region = 4; // 우측 하단
        }
    }
}