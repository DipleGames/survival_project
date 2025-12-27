using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceBuilder : MonoBehaviour
{
    public GameObject fenseSet;
    private GameObject previewFence; // 마우스 따라다니는 미리보기용
    private Vector3 targetPos;
    public bool isBuildMode = false;


    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            isBuildMode = true;
            previewFence = Instantiate(fenseSet);
        }

        if(!isBuildMode) return; 
        OnBuildMode();

        // 4. 클릭 시 실제 설치
        if(Input.GetMouseButtonDown(0)) 
        {
            BuildFence(targetPos);
        }
    }

    void OnBuildMode()
    {
        // 미리보기 객체는 충돌체나 스크립트가 작동하지 않게 끄는 것이 좋음
        targetPos = GridMapper.Instance.cellCenterPos + new Vector3(0, 0, 0.22f);
        //Quaternion targetRot = Quaternion.identity;

        previewFence.transform.GetChild(4).gameObject.SetActive(false); // navObstacleSet 끄기

        switch(GridMapper.Instance.region) 
        {
            case 1: // 좌상단 변
                for(int i=0; i<4; i++)
                {
                    previewFence.transform.GetChild(i).gameObject.SetActive(false);
                }
                previewFence.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case 2: // 우상단 변
                for(int i=0; i<4; i++)
                {
                    previewFence.transform.GetChild(i).gameObject.SetActive(false);
                }
                previewFence.transform.GetChild(1).gameObject.SetActive(true);
                break;
            case 3: // 좌하단 변
                for(int i=0; i<4; i++)
                {
                    previewFence.transform.GetChild(i).gameObject.SetActive(false);
                }
                previewFence.transform.GetChild(2).gameObject.SetActive(true);
                break;
            case 4: // 우하단 변
                for(int i=0; i<4; i++)
                {
                    previewFence.transform.GetChild(i).gameObject.SetActive(false);
                }
                previewFence.transform.GetChild(3).gameObject.SetActive(true);
                break;
        }

        // 3. 미리보기 프리팹 이동
        previewFence.transform.position = targetPos;
        previewFence.transform.rotation = Quaternion.Euler(90,0,0); // xz평면에 설치
    }

    void BuildFence(Vector3 targetPos)
    {
        GameObject fenceObj = Instantiate(fenseSet, targetPos, Quaternion.Euler(90,0,0));
        Destroy(previewFence);
        switch(GridMapper.Instance.region) 
        {
            case 1: // 좌상단 변
                for(int i=0; i<4; i++)
                {
                    if(i == 0)
                    {
                        fenceObj.transform.GetChild(i).gameObject.SetActive(true);
                        fenceObj.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject.SetActive(true);
                    }
                    else
                    {
                        fenceObj.transform.GetChild(i).gameObject.SetActive(false);
                        fenceObj.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
            case 2: // 우상단 변
                for(int i=0; i<4; i++)
                {
                    if(i == 1)
                    {
                        fenceObj.transform.GetChild(i).gameObject.SetActive(true);
                        fenceObj.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject.SetActive(true);
                    }
                    else
                    {
                        fenceObj.transform.GetChild(i).gameObject.SetActive(false);
                        fenceObj.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
            case 3: // 좌하단 변
                for(int i=0; i<4; i++)
                {
                    if(i == 2)
                    {
                        fenceObj.transform.GetChild(i).gameObject.SetActive(true);
                        fenceObj.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject.SetActive(true);
                    }
                    else
                    {
                        fenceObj.transform.GetChild(i).gameObject.SetActive(false);
                        fenceObj.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
            case 4: // 우하단 변
                for(int i=0; i<4; i++)
                {
                    if(i == 3)
                    {
                        fenceObj.transform.GetChild(i).gameObject.SetActive(true);
                        fenceObj.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject.SetActive(true);
                    }
                    else
                    {
                        fenceObj.transform.GetChild(i).gameObject.SetActive(false);
                        fenceObj.transform.GetChild(4).gameObject.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                break;
        }
        isBuildMode = false;
    }
}
