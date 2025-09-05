using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseRecipe : MonoBehaviour
{
    [System.Serializable]
    public class MaterialCost
    {
        public MaterialType type;   // 필요한 재료
        public int amount;          // 개수
    }

    [System.Serializable]
    public class ActiveRange
    {
        public int left = 0, right = 0, up = 0, down = 0; // 포함 범위(각 방향 칸 수, 양끝 포함)
    }

    [System.Serializable]
    public class LevelRecipe
    {
        public List<MaterialCost> costs = new List<MaterialCost>(); // 하우스 레벨업 재료
        public GameObject house;    // 레벨에 따른 집 마다의 외형(일단 그림이 없어 제외)
        public ActiveRange activeRange = new ActiveRange(); // 레벨에 따른 활동 범위
        public int houseDurability = 0; // 레벨에 따른 집 내구도
    }
    
    [SerializeField] private List<LevelRecipe> levelRecipes = new List<LevelRecipe>();
    public int LevelCount => levelRecipes.Count;
    public LevelRecipe Get(int level)
    {
        if (levelRecipes == null || level < 0 || level >= levelRecipes.Count) return null;
        return levelRecipes[level];
    }
}
