using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 해골과 관련된 시각적 요소와 UI 표현을 담당한다.
/// 상호작용 UI, 애니메이션, 장비 표시 등의 화면 출력을 관리한다.
/// </summary>
public class SkullView : MonoBehaviour
{
    [Header("UICanvas")]
    public GameObject ui;

    [Header("패널")]
    public GameObject mainSelectPanel;
    public GameObject lifePanel;
    public GameObject battlePanel;
    public GameObject equippedObjPanel;

    [Header("버튼")]
    public Button LifeSelectBtn;
    public Button BattleSelectBtn;
    public Button equippedObjBtn;

    void Awake()
    {
        LifeSelectBtn.onClick.AddListener(OnClickedLifeSelectBtn);
        BattleSelectBtn.onClick.AddListener(OnClickedBattleSelectBtn);
        equippedObjBtn.onClick.AddListener(OnClickedEquippedObjBtn);
    }

    public void SwitchSkullUI()
    {
        ui.SetActive(!ui.activeSelf);

        mainSelectPanel.SetActive(true);
        battlePanel.SetActive(false);
        equippedObjPanel.SetActive(false);
        lifePanel.SetActive(false);
    }

    void OnClickedLifeSelectBtn()
    {
        mainSelectPanel.SetActive(false);
        battlePanel.SetActive(false);
        equippedObjPanel.SetActive(false);
        lifePanel.SetActive(true);

    }

    void OnClickedBattleSelectBtn()
    {
        mainSelectPanel.SetActive(false);
        lifePanel.SetActive(false);
        equippedObjPanel.SetActive(false);
        battlePanel.SetActive(true);
    }

    void OnClickedEquippedObjBtn()
    {
        mainSelectPanel.SetActive(false);
        lifePanel.SetActive(false);
        battlePanel.SetActive(false);
        equippedObjPanel.SetActive(true);
    }

    public void CloseSkullUI()
    {
        mainSelectPanel.SetActive(false);
        lifePanel.SetActive(false);
        battlePanel.SetActive(false);
        equippedObjPanel.SetActive(false);
    }
}
