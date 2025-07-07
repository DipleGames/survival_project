using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(RepairRequirement))]
public class House : MonoBehaviour, IMouseInteraction, IDamageable, IRepairable
{
    [SerializeField] float hp = 90;
    [SerializeField] GameObject createDesk;
    int houseLevel = 1;
    RepairRequirement repairRequirement;

    Dictionary<MaterialType, int> requireMaterials = new Dictionary<MaterialType, int>();

    private void Awake()
    {
        repairRequirement = GetComponent<RepairRequirement>();
    }
    private void Start()
    {
        ChangeLevel();
    }

    void ChangeLevel()
    {
        switch (houseLevel)
        {
            case 0:
                requireMaterials.Add(MaterialType.Wood, 10);
                //GameObject desk = Instantiate(createDesk, transform);
                //desk.transform.position = transform.position;
                break;

            case 1:
                requireMaterials.Add(MaterialType.Wood, 20);
                break;

            case 2:
                requireMaterials.Add(MaterialType.Wood, 30);
                break;

            case 3:
                requireMaterials.Add(MaterialType.Wood, 30);
                break;
        }
    }
    void CreateHouse()
    {
        if (!isSatisFyRequirement())
            return;

        Debug.Log("upgrade");
        houseLevel++;
        requireMaterials.Clear();
        ChangeLevel();
    }

    bool isSatisFyRequirement()
    {
        if (requireMaterials.Count == 0)
            return false;

        foreach (var material in requireMaterials)
        {
            if (!GameManager.Instance.haveMaterials.ContainsKey(material.Key))
                return false;

            if (GameManager.Instance.haveMaterials[material.Key] < material.Value)
                return false;
        }

        foreach (var material in requireMaterials)
        {
            GameManager.Instance.haveMaterials[material.Key] -= material.Value;
            TempMaterialCount(material.Key, material.Value);
        }

        return true;
    }

    void TempMaterialCount(MaterialType type, int count)
    {
        switch (type) 
        {
            case MaterialType.Wood:
                GameManager.Instance.woodCount -= count; 
                break;
            case MaterialType.Branch:
                //GameManager.Instance.
                break;

            default:
                break;
        }
    }

    public void InteractionLeftButtonFuc(GameObject hitObject)
    {
        CreateHouse();
    }

    public void InteractionRightButtonFuc(GameObject hitObject)
    {
        Repair();
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
        hp -= damage;

        if (hp <= 0)
        {
            hp = 0;
            //Destroy(gameObject);
            gameObject.SetActive(false);
        }
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        throw new System.NotImplementedException();
    }

    public void Repair()
    {
        if (hp >= 100) return;

        RepairMaterial repair_material = repairRequirement.GetMaterialList(houseLevel, hp);

        if (repair_material == null)
        {
            Debug.Log("RepairMaterial returned NULL");
            return;
        }

        if (GameManager.Instance.haveItems[GameManager.Instance.idByMaterialType[repair_material.type]] < repair_material.quantity)
        {
            Debug.Log($"Need more {repair_material.type}");
            return;
        }

        GameManager.Instance.haveItems[GameManager.Instance.idByMaterialType[repair_material.type]] -= repair_material.quantity;
        Debug.Log($"{repair_material.type} {GameManager.Instance.haveItems[GameManager.Instance.idByMaterialType[repair_material.type]]} remaining");
        hp = 100;
    }
}