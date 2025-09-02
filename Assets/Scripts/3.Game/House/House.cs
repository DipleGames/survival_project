using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class House : MonoBehaviour, IMouseInteraction, IDamageable
{
    [System.Serializable]
    public class MaterialCost
    {
        public MaterialType type;   // 필요한 재료
        public int amount;          // 개수
    }
    [System.Serializable]
    public class LevelRecipe
    {
        public List<MaterialCost> costs = new List<MaterialCost>();
        public GameObject house;    // 레벨에 따른 집 마다의 외형(일단 그림이 없어 제외)
        public int range = 3;       // 레벨에 따른 활동 범위
        public int houseDurability = 0; // 레벨에 따른 집 내구도
    }
    //[SerializeField] GameObject createDesk;
    [SerializeField] private int houseLevel = 0;
    [SerializeField] private List<LevelRecipe> levelRecipes = new List<LevelRecipe>();
    private Dictionary<MaterialType, int> requireMaterials = new();
    private GameObject currentHouse;
    [SerializeField] private Transform houseTransform;
    [SerializeField] private Tilemap groundTilemap;
    private Vector3Int currentRange;
    [SerializeField] int currentDurability = 0;
    public int maxDurability =>
    (houseLevel >= 0 && houseLevel < levelRecipes.Count && levelRecipes[houseLevel].houseDurability > 0) ?
    levelRecipes[houseLevel].houseDurability : 30;

    void Start()
    {
        if (!houseTransform) houseTransform = this.transform;
        ApplyRecipe();
        SpawnHouse();
        currentDurability = maxDurability;
        if (groundTilemap != null)
            currentRange = groundTilemap.WorldToCell(transform.position);
        else
            Debug.LogWarning("groundTilemap이 비어있습니다. 범위 체크가 비활성화됩니다.");
    }

    void ApplyRecipe()
    {
        requireMaterials.Clear();

        if(houseLevel < 0 || houseLevel >= levelRecipes.Count) return;

        foreach (var cost in levelRecipes[houseLevel].costs)
        {
            if (requireMaterials.TryGetValue(cost.type, out int prev))
                requireMaterials[cost.type] = prev + cost.amount;
            else
                requireMaterials.Add(cost.type, cost.amount);
        }
    }

    void CreateHouse()
    {
        if (!IsSatisfiedRequirement())
        {
            return;
        }
        UseMaterial();
        houseLevel++;
        Debug.Log($"현재 하우스 레벨: {houseLevel}");
        if (houseLevel < levelRecipes.Count)
        {
            ApplyRecipe();
            SpawnHouse();
            currentDurability = maxDurability;
            if (groundTilemap != null)
                currentRange = groundTilemap.WorldToCell(transform.position);
        }
    }

    bool IsSatisfiedRequirement()
    {
        if (requireMaterials.Count == 0)
        {
            Debug.Log("업그레이드 실패: 레시피가 존재하지 않음");
            return false;
        }
        foreach (var material in requireMaterials)
        {
            if (!GameManager.Instance.haveItems.ContainsKey(GameManager.Instance.idByMaterialType[material.Key]))
            {
                // 키가 없으면 0으로 간주(안전 가드) → 필요시 바로 실패 처리
                Debug.LogWarning($"업그레이드 실패: 보유 재료 딕셔너리에 키가 없음-{material.Key}");
                return false;
            }
            if (GameManager.Instance.haveItems[GameManager.Instance.idByMaterialType[material.Key]] < material.Value)
            {
                Debug.Log("업그레이드 실패: 재료 부족");
                return false;
            }
        }
        return true;
    }

    void UseMaterial()
    {
        foreach (var material in requireMaterials)
        {
            GameManager.Instance.haveItems[GameManager.Instance.idByMaterialType[material.Key]] -= material.Value;
        }
    }

    void SpawnHouse()
    {
        if (currentHouse)
        {
            Destroy(currentHouse);
            currentHouse = null;
        }
        if (houseLevel < 0 || houseLevel >= levelRecipes.Count) return;
        var prefab = levelRecipes[houseLevel].house;
        if (!prefab)
        {
            Debug.LogWarning($"{houseLevel} 하우스 외형 프리팹이 비어있습니다.");
            return;
        }
        currentHouse = Instantiate(
            prefab,
            houseTransform.position,
            houseTransform.rotation,
            houseTransform
        );
    }

    public bool CanInteractAtWorld(Vector3 worldPos)
    {
        if (!groundTilemap) return true;
        var cell = groundTilemap.WorldToCell(worldPos);
        return CanInteractAtCell(cell);
    }

    public bool CanInteractAtCell(Vector3Int cell)
    {
        if (levelRecipes == null || levelRecipes.Count == 0) return true;

        int range = Mathf.Max(0, levelRecipes[houseLevel].range); // 음수 방지
        int dx = cell.x - currentRange.x;
        int dy = cell.y - currentRange.y;
        return (Mathf.Abs(dx) <= range) && (Mathf.Abs(dy) <= range);
    }

    private float GetLostRatio()
    {
        int max = Mathf.Max(1, maxDurability);
        int lost = Mathf.Clamp(max - currentDurability, 0, max);
        return (float)lost / max;
    }
    public Dictionary<MaterialType, int> CalcRepairCost()
    {
        var cost = new Dictionary<MaterialType, int>();
        if (houseLevel < 0 || houseLevel >= levelRecipes.Count) return cost;

        float ratio = GetLostRatio();
        if (ratio <= 0f) return cost;
        foreach (var material in levelRecipes[houseLevel].costs)
        {
            if (material.amount <= 0) continue;
            int needed = Mathf.CeilToInt(material.amount * ratio);
            if (needed == 0) needed = 1;
            if (needed > 0)
            {
                if (cost.ContainsKey(material.type)) cost[material.type] += needed;
                else cost.Add(material.type, needed);
            }
        }
        return cost;
    }

    public bool CanRepair()
    {
        var cost = CalcRepairCost();
        if (cost.Count == 0) return false;

        foreach (var kv in cost)
        {
            int matId = GameManager.Instance.idByMaterialType[kv.Key];
            if (!GameManager.Instance.haveItems.TryGetValue(matId, out int have) || have < kv.Value)
                return false;
        }
        return true;
    }

    public bool Repair()
    {
        var cost = CalcRepairCost();
        if (cost.Count == 0) return false;
        foreach (var kv in cost)
        {
            int matId = GameManager.Instance.idByMaterialType[kv.Key];
            if (!GameManager.Instance.haveItems.TryGetValue(matId, out int have) || have < kv.Value)
            {
                Debug.Log("수리 실패: 재료 부족");
                return false;
            }
        }
        foreach (var kv in cost)
        {
            int matId = GameManager.Instance.idByMaterialType[kv.Key];
            GameManager.Instance.haveItems[matId] -= kv.Value;
        }
        currentDurability = maxDurability;
        Debug.Log("수리 완료: 내구도 최대치로 회복");
        return true;
    }

    public void InteractionLeftButtonFuc(GameObject hitObject)
    {
        CreateHouse();
    }

    public void InteractionRightButtonFuc(GameObject hitObject)
    {
        throw new System.NotImplementedException();
    }

    public void CanInteraction(bool _canInteraction)
    {
        throw new System.NotImplementedException();
    }

    public bool ReturnCanInteraction()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator EndInteraction(Animator anim, float waitTime)
    {
        throw new System.NotImplementedException();
    }

    public void Attacked(float damage, GameObject hitObject)
    {
        currentDurability -= (Mathf.CeilToInt(damage));
        if (currentDurability <= 0)
        {
            currentDurability = 0;
            //Destroy(gameObject);
            gameObject.SetActive(false);
        }
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        
    }
}
