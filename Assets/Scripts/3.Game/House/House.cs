using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class House : MonoBehaviour, IMouseInteraction, IDamageable
{
    [System.Serializable]
    public class MaterialCost
    {
        public MaterialType type;   // „ìš”¬ë£Œ
        public int amount;          // ê°œìˆ˜
    }
    [System.Serializable]
    public class ActiveRange {
        public int left = 0, right = 0, up = 0, down = 0; // ¬í•¨ ë²”ìœ„(ê°ë°©í–¥ ì¹ ‘ë ¬í•¨)
    }
    [System.Serializable]
    public class LevelRecipe
    {
        public List<MaterialCost> costs = new List<MaterialCost>(); // ˜ìš°ˆë²¨¬ë£Œ
        public GameObject house;    // ˆë²¨°ë¥¸ ì§ë§ˆë‹¤¸í˜•(¼ë‹¨ ê·¸ë¦¼†ì–´ œì™¸)
        public ActiveRange activeRange = new ActiveRange(); // ˆë²¨°ë¥¸ œë™ ë²”ìœ„
        public int houseDurability = 0; // ˆë²¨°ë¥¸ ì§´êµ¬
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
            Debug.LogWarning("groundTilemapë¹„ì–´ˆìŠµˆë‹¤. ë²”ìœ„ ì²´í¬ê°€ ë¹„í™œ±í™”©ë‹ˆ");
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
        if (houseLevel >= levelRecipes.Count - 1)
        {
            Debug.Log("´ë ìµœë ˆë²¨…ë‹ˆ");
            return;
        }
        if (!IsSatisfiedRequirement())
        {
            return;
        }
        UseMaterial();
        houseLevel++;
        Debug.Log($"„ì¬ ˜ìš°ˆë²¨: {houseLevel}");
        ApplyRecipe();
        SpawnHouse();
        currentDurability = maxDurability;
        if (groundTilemap != null)
            currentRange = groundTilemap.WorldToCell(transform.position);
    }

    bool IsSatisfiedRequirement()
    {
        if (requireMaterials.Count == 0)
        {
            Debug.Log("…ê·¸ˆì´¤íŒ¨: ˆì‹œ¼ê ì¡´ì¬˜ì ŠìŒ");
            return false;
        }
        foreach (var material in requireMaterials)
        {
            if (!GameManager.Instance.haveItems.ContainsKey(GameManager.Instance.idByMaterialType[material.Key]))
            {
                Debug.LogWarning($"…ê·¸ˆì´¤íŒ¨: ë³´ìœ  ¬ë£Œ •ì…”ˆë¦¬¤ê †ìŒ-{material.Key}");
                return false;
            }
            if (GameManager.Instance.haveItems[GameManager.Instance.idByMaterialType[material.Key]] < material.Value)
            {
                Debug.Log("…ê·¸ˆì´¤íŒ¨: ¬ë£Œ ë¶€ì¡);
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
            Debug.LogWarning($"{houseLevel} ˜ìš°¸í˜• „ë¦¬¹ì´ ë¹„ì–´ˆìŠµˆë‹¤.");
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
        var range = levelRecipes[houseLevel].activeRange;
        int dx = cell.x - currentRange.x;
        int dy = cell.y - currentRange.y;
        bool insideX = (-range.left  <= dx) && (dx <= range.right);
        bool insideY = (-range.down  <= dy) && (dy <= range.up);
        return insideX && insideY;
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
        if (houseLevel <= 0 || levelRecipes.Count == 0) return cost;
        int idx = Mathf.Clamp(houseLevel - 1, 0, Mathf.Max(0, levelRecipes.Count - 1));
        float ratio = GetLostRatio();
        if (ratio <= 0f) return cost;
        foreach (var material in levelRecipes[idx].costs)
        {
            if (material.amount <= 0) continue;
            int needed = Mathf.CeilToInt(material.amount * ratio);
            if (needed == 0) needed = 1;
            if (cost.ContainsKey(material.type)) cost[material.type] += needed;
            else cost.Add(material.type, needed);
        }
        return cost;
    }

    public bool CanRepair()
    {
        if (houseLevel <= 0) return false;
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
                Debug.Log("˜ë¦¬ ¤íŒ¨: ¬ë£Œ ë¶€ì¡);
                return false;
            }
        }
        foreach (var kv in cost)
        {
            int matId = GameManager.Instance.idByMaterialType[kv.Key];
            GameManager.Instance.haveItems[matId] -= kv.Value;
        }
        currentDurability = maxDurability;
        Debug.Log("˜ë¦¬ „ë£Œ: ´êµ¬ìµœëì¹˜ë¡œ Œë³µ");
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
        if (currentDurability <= 0) return;
        currentDurability -= (Mathf.CeilToInt(damage));
        Debug.Log($"„ì¬ ì§´êµ¬ {currentDurability}");
        if (currentDurability <= 0)
        {
            currentDurability = 0;
            Debug.Log("ì§‘ì´ Œê³¼˜ì—ˆµë‹ˆ");
            //gameObject.SetActive(false);
        }
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        
    }
}
