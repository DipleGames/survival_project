using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 인벤토리 컨트롤 스크립트 - 통합된 패널들의 최상위 패널에서 컨트롤
// 인벤토리에서 A,D 키를 눌러 다른탭으로 넘어가는 형태
// 시험용 TAP키로 넘어가는 것 추가
public class InvenControl : MonoBehaviour
{

    // 컨트롤 할 패널들을 집어넣어서 index로 조절
    [SerializeField] GameObject[] panels;
    int currentIndex;
    [SerializeField] bool UIisON = false;
    [SerializeField] ItemInventory itemInventory;
    // Start is called before the first frame update

    [SerializeField] GameObject[] UIPanels; // UI 패널들을 배열로 선언
    [SerializeField] public bool isUIActive = false; // 켜진 UI가 있는지
    
    void Start()
    {
        currentIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(isUIActive)
        {
            return; // 다른 UI가 켜져있으면 상호작용 X
        }

        // 기존 ItemDictory의 U키 입력, ItemInventory의 I키 입력을 통합하여 컨트롤
        if (Input.GetKeyDown(KeyCode.U))
        {
            // 중복되는 코드들 PanelControl로 처리
            PanelControl(0);
        }
        
        /*if (Input.GetKeyDown(KeyCode.I))
        {            
            PanelControl(1);   
        }*/
        if (Input.GetKeyDown(KeySetting.keys[KeyAction.INVENTORY]))
        {
            PanelControl(1);
        }

        // UI가 켜져있을경우 A,D 키를 눌러 다른 탭으로 이동
        if (UIisON)
        {
            // A 입력시 index가 0인경우 마지막 인덱스로 이동
            // A 입력시 index가 0이 아닌경우 index-1로 이동
            if (Input.GetKeyDown(KeyCode.A) && panels.Length > 1)
            {
                panels[currentIndex].SetActive(false);
                currentIndex = currentIndex > 0 ? currentIndex - 1 : panels.Length - 1;
                panels[currentIndex].SetActive(true);
                if (currentIndex == 0)
                {
                    itemInvenCon();
                }
            }
            // D 입력시 index가 panels.Lengtn-1 인경우 0번 인덱스로 이동
            // D 입력시 index가 0이 아닌경우 index-1로 이동
            if (Input.GetKeyDown(KeyCode.D) && panels.Length > 1)
            {
                panels[currentIndex].SetActive(false);
                currentIndex = currentIndex < panels.Length - 1 ? currentIndex + 1 : 0;
                panels[currentIndex].SetActive(true);
                if (currentIndex == 0)
                {
                    itemInvenCon();
                }
            }


            //TAP은 어차피 D키 입력과 동일한 처리
            /*if(Input.GetKeyDown(KeyCode.Tab) && panels.Length > 1)
            {
                panels[currentIndex].SetActive(false);
                currentIndex = currentIndex < panels.Length - 1 ? currentIndex + 1 : 0;
                panels[currentIndex].SetActive(true);
            }*/
        }
    }

    public void PanelControl(int index)
    {
        // 열려 있는 페이지의 키를 다시 누르면 닫기
        if (UIisON && currentIndex == index)
        {
            panels[currentIndex].SetActive(false);
            UIisON = false;
            return;
        }

        // 기존 패널들을 먼저 끄기
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }

        // 선택한 패널 열기
        currentIndex = index;
        panels[currentIndex].SetActive(true);
        UIisON = true;

        if (currentIndex == 0)
        {
            itemInvenCon();
        }
    }

    public void itemInvenCon()
    {
        itemInventory.ChangeCategory(0);// 기존 코드에서 0을 넣는것같아서 일단 0 입력 => 작동중에 문제없는상태
    }

    public void CloseUI(int Index)
    {
        for(int i = 0; i<UIPanels.Length; i++)
        {
            if(Index != i)
            UIPanels[i].SetActive(false);
        }
        UIisON = false;
    }
}
