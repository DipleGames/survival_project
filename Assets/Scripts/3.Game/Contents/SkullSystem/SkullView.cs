using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkullView : MonoBehaviour
{
    [Header("패널")]
    public GameObject mainSelectPanel;
    public GameObject lifePanel;
    public GameObject battlePanel;

    [Header("버튼")]
    public Button LifeSelectBtn;
    public Button BattleSelectBtn;

    void Awake()
    {
        LifeSelectBtn.onClick.AddListener(OnClickedLifeSelectBtn);
        BattleSelectBtn.onClick.AddListener(OnClickedBattleSelectBtn);
    }

    void OnClickedLifeSelectBtn()
    {
        mainSelectPanel.SetActive(false);
        battlePanel.SetActive(false);
        lifePanel.SetActive(true);
    }

    void OnClickedBattleSelectBtn()
    {
        mainSelectPanel.SetActive(false);
        lifePanel.SetActive(false);
        battlePanel.SetActive(true);
    }
}
