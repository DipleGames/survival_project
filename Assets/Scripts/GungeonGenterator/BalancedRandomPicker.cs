using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BalancedRandomPicker
{
    private int[] counts;

    public BalancedRandomPicker(int size){ counts = new int[size]; }

    public int GetNextIndex()
    {
        int maxCount = counts.Max();
        int[] weights = new int[counts.Length];
        int totalWeight = 0;

        //많이 뽑혔을수록 가중치 낮아짐
        for (int i = 0; i < counts.Length; i++)
        {
            // +1을 해야 확률이 제일 적은 후보 또한 최소한의 확률을 가짐
            weights[i] = maxCount - counts[i] + 1;
            totalWeight += weights[i];
        }

        //가중치 기반 랜덤 돌리기
        int randomValue = Random.Range(0, totalWeight);
        int cursor = 0;
        int selectedIndex = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            cursor += weights[i];
            if (randomValue < cursor){selectedIndex = i; break; }
        }

        //선택된 인덱스 카운트 증가
        counts[selectedIndex]++;

        //오버플로우 방지 (모든 값이 1 이상이면 전부 1씩 깎음)
        int minCount = counts.Min();
        if (minCount > 0)
        {
            for (int i = 0; i < counts.Length; i++) counts[i] -= minCount;
        }

        return selectedIndex;
    }
}
