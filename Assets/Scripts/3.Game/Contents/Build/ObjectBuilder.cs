using System.Collections.Generic;
using System;
using UnityEngine;

public class ObjectBuilder : MonoBehaviour
{
    private BuildExceptionManager buildExceptionManager;

    [Header("선택된 셀 / 오브젝트")]
    public GameObject selectedCell; // 선택된 셀
    [SerializeField] private GameObject selectedObj; // 선택된 오브젝트
    [SerializeField] private BuildableObject objScript; // 선택된 오브젝트의 오브젝트 스크립트

    [Header("울타리 오브젝트")]
    public GameObject[] fense;
    [SerializeField] private int currentNum = 0;

    [Header("타워 오브젝트")]
    public GameObject tower;


    [Header("현재 오브젝트가 설치된 셀 리스트 / 부모")]
    public List<Vector3Int> builtCells = new List<Vector3Int>();
    public GameObject builtObjectsRoot;

    private Vector3 targetPos;
    private GameObject previewObject; // 마우스 따라다니는 미리보기용
    private List<Vector3Int> buildArea; // 설치 영역
    private List<GameObject> selecetedCellPool = new List<GameObject>(); // 설치영역을 표시할 오브젝트를 담는 리스트

    public bool isBuildMode = false;

    void Awake()
    {
        for(int i=0; i<9; i++)
        {
            var cell = Instantiate(selectedCell);
            cell.SetActive(false);
            selecetedCellPool.Add(cell);
        }

        buildExceptionManager = BuildExceptionManager.Instance;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {    
            isBuildMode = true;
            SelectObject(fense[currentNum]);
            for(int i=0; i<objScript.requiredCell; i++)
            {
                selecetedCellPool[i].SetActive(true);
            }
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {    
            isBuildMode = true;
            SelectObject(tower);
            for(int i=0; i<objScript.requiredCell; i++)
            {
                selecetedCellPool[i].SetActive(true);
            }
        }

        if(!isBuildMode) return; 
        OnBuildMode();

        // 4. 클릭 시 실제 설치
        if(Input.GetMouseButtonDown(0)) 
        {
            BuildObject(selectedObj, targetPos);
        }
    }

    void OnBuildMode()
    {
        if(objScript.buildType == BuildType.Line)
        {
            targetPos = GridMapper.Instance.cellCenterPos;
        }
        else
        {
            targetPos = objScript.GetVisualCenter(GridMapper.Instance.grid);
        }
        //previewObject.transform.GetChild(1).gameObject.SetActive(false); // navObstacleSet 끄기

        if(Input.GetKeyDown(KeyCode.R) && objScript.buildType == BuildType.Line) // 울타리 방향 체인지 메서드 (나중에 따로 분리)
        {
            switch(currentNum)
            {
                case 0:
                    currentNum = 1;
                    break;
                case 1:
                    currentNum = 0;
                    break;
            }
            Destroy(previewObject);
            SelectObject(fense[currentNum]);
        }

        previewObject.transform.position = targetPos;
        previewObject.transform.rotation = Quaternion.Euler(90,0,0); // xz평면에 설치

        buildArea = objScript.GetBuildArea(GridMapper.Instance.cellIndex);

        ShowBuildArea(buildArea);
    }

    void ShowBuildArea(List<Vector3Int> buildArea)
    {
        for(int i=0; i<buildArea.Count; i++)
        {
            selecetedCellPool[i].transform.position = GridMapper.Instance.grid.GetCellCenterWorld(buildArea[i]); // 선택된 셀 표시
        }
    }

    void BuildObject(GameObject selectedObject, Vector3 targetPos)
    {
        GameObject obj = Instantiate(selectedObject, targetPos, Quaternion.Euler(90,0,0));
        obj.transform.SetParent(builtObjectsRoot.transform);
        BuildableObject buildableObject = obj.GetComponent<BuildableObject>();
        buildableObject.isBuilt = true;

        Destroy(previewObject);

        for(int i=0; i<buildArea.Count; i++)
        {
            builtCells.Add(buildArea[i]);
            selecetedCellPool[i].SetActive(false);
        }

        isBuildMode = false;
    }

    void SelectObject(GameObject selectedObject)
    {
        if(previewObject != null)
        {
            Destroy(previewObject);
        } 

        if(selectedObj != null) // 선택한 오브젝트가 null이 아니라면 즉, 기존에 선택되어있던 오브젝트가 있다면
        {    
            for(int i=0; i<buildArea.Count; i++)
            {
                selecetedCellPool[i].SetActive(false); // 선택된셀 영역을 다 끈다.
            }
        }

        selectedObj = selectedObject;
        objScript = selectedObj.GetComponent<BuildableObject>();
        previewObject = Instantiate(selectedObj);
        for(int i=0; i<objScript.requiredCell; i++)
        {
            selecetedCellPool[i].SetActive(true);
        }
    }
}
