using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TowerView : MonoBehaviour
{
    public TowerModel towerModel;

    [Header("타워 UI")]
    [SerializeField] private TextMeshProUGUI attackPower;
    [SerializeField] private TextMeshProUGUI attackRange;


    public void InitTowerModel(TowerModel tm)
    {
        towerModel = tm;
    }

    public void SetTowerView()
    {
        attackPower.text = $"공격력 : {towerModel.attackPower}";
        attackRange.text = $"사거리 : {towerModel.attackRange}";
    }
}
