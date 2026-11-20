
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//UI 표시만 담당하도록 분리

[Serializable]
public class StatRow
{
    public StatType type;
    public TMP_Text valueText;
}

public class PlayerStatsView : MonoBehaviour
{
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject statPanel;
    [SerializeField] private StatRow[] statRows;

    public Button ToggleButton => toggleButton;

    // 패널 표시 및 숨김
    public void SetPanelVisible(bool visible)
    {
        statPanel.SetActive(visible);
    }

    // 전달받은 최종 스탯을 UI에 반영
    public void Render(IReadOnlyDictionary<StatType, float> stats)
    {
        foreach (StatRow row in statRows)
        {
            float value = stats.TryGetValue(row.type, out float stat)
                ? stat : 0f;

            row.valueText.text = value.ToString("0.##");
        }
    }
}
