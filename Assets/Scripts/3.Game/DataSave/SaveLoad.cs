using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveLoad : MonoBehaviour
{
    // 저장 슬롯 번호
    [SerializeField] int slot = 0;
    // 게임저장 슬롯
    [SerializeField] GameObject slotButtonPrefab;
    [SerializeField] Transform saveSlotList;
    [SerializeField] int maxSlots = 20;
    public GameObject SaveLoadPanel;
    private List<UnityEngine.UI.Button> slotButtons = new List<UnityEngine.UI.Button>();
    [SerializeField] EquipmentManager EquipmentManager;
    [SerializeField] ItemInventory ItemInventory;
    [SerializeField] QuickSlot QuickSlot;
    [SerializeField] GameManager GameManager;

    public enum SaveLoadMode
    {
        Save,
        Load
    }

    private SaveLoadMode currentMode;

    public void OpenSavePanel()
    {
        PefreshSlots();
        currentMode = SaveLoadMode.Save;
        SaveLoadPanel.SetActive(true);
    }

    public void OpenLoadPanel()
    {
        PefreshSlots();
        currentMode = SaveLoadMode.Load;
        SaveLoadPanel.SetActive(true);
    }

    private void Start()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            int slotIndex = i;

            GameObject slotObject =
                Instantiate(slotButtonPrefab, saveSlotList);

            var button = slotObject.GetComponent<Button>();

            slotButtons.Add(button);
            button.onClick.AddListener(() => OnSlotClick(slotIndex));
        }
    }
    private void OnSlotClick(int slotIndex)
    {
        if (currentMode == SaveLoadMode.Save)
        {
            SaveGame(slotIndex);
        }
        else
        {
            LoadGame(slotIndex);
        }
        SaveLoadPanel.SetActive(false);
    }

    public void CloseSaveLoadPanel()
    {
        SaveLoadPanel.SetActive(false);
    }

    public void SaveGame(int slot)
    {

        Player player = GameManager.Instance.Player;
        if (player == null) return;
        SaveData data = new SaveData();
        {
            data.saveVersion = 1;
            // 현재 활성화중인 씬 이름
            data.ScenceName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            //위치
            data.player.position = GameManager.Instance.Player.transform.position;
            //체력
            data.player.currentHp = GameManager.Instance.Player.Health.CurrentHp;
            //장착중인 장비
            if (EquipmentManager != null)
            {
                data.player.equippedItemId = EquipmentManager.EquippedItemId;
            }
            // 보유 아이템의 key와 value 저장
            data.player.inventory.items = new List<ItemStackSaveData>();
            foreach (var item in ItemManager.Instance.haveItems)
            {
                data.player.inventory.items.Add(new ItemStackSaveData
                {
                    itemId = item.Key,
                    count = item.Value
                });
            }
            // 인벤토리 슬롯상태 저장
            var inventoryData = ItemInventory.CallInventoryData();
            data.player.inventory.inventorySlots.Clear();
            foreach (var category in inventoryData)
            {
                int?[] slots = category.Value;

                for (int i = 0; i < slots.Length; i++)
                {
                    data.player.inventory.inventorySlots.Add(
                        new InventorySlotSaveData
                        {
                            category = category.Key,
                            slotIndex = i,
                            itemId = slots[i] ?? -1
                        });
                }
            }
            // 그외 재료 - 아직 Load 미지정
            data.player.inventory.materials = new List<MaterialSaveData>();
            foreach (var material in ItemManager.Instance.haveMaterials)
            {
                data.player.inventory.materials.Add(new MaterialSaveData
                {
                    materialId = (int)material.Key,
                    count = material.Value
                });
            }
            data.player.inventory.quickSlots = new List<QuickSlotSaveData>();

        }

        //폴더 생성
        //폴더가 있으면 Directory.CreateDirectory()는 아무런 동작을 하지 않음
        //GetPath()에서 반환된 경로에 파일을 생성하고 저장
        Directory.CreateDirectory(Path.GetDirectoryName(GetPath(slot)));
        File.WriteAllText(GetPath(slot), JsonUtility.ToJson(data, true));
        Debug.Log("저장 완료: " + GetPath(slot));
    }

    public void LoadGame(int slot)
    {
        if (!File.Exists(GetPath(slot)))
        {
            Debug.LogWarning("저장 파일이 없습니다.");
            return;
        }
        string json = File.ReadAllText(GetPath(slot));
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            Debug.LogError("불러오기 실패: 데이터가 없습니다.");
            return;
        }
        // GameManager가 플레이어 생성과 카메라 연결을 처리하고있어서 Load를 GameManager에 위임
        GameManager.Instance.LoadSavedGame(data);

    }

    public void PefreshSlots()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            TMP_Text text = slotButtons[i].GetComponentInChildren<TMP_Text>();

            if (File.Exists(GetPath(i)))
            {
                string json = File.ReadAllText(GetPath(i));
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                text.text = $"슬롯 {i + 1}\n" +
                            $"위치: ({data.player.position.x:F1}, " +
                            $"{data.player.position.y:F1}, {data.player.position.z:F1})";
            }
            else
            {
                text.text = $"슬롯 {i + 1}\n빈 슬롯";
            }
        }
    }

    //프로젝트 최상단에 Save 폴더를 만들고 그 안에 저장하고 싶을 때 사용
    private string GetPath(int slot)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        return Path.Combine(projectRoot, "Save", $"save{slot}.json");
    }

    public void RestoreGame(SaveData data)
    {


        Player player = GameManager.Instance.Player;
        if (player == null)
        {
            Debug.LogError("복원할 플레이어가 없습니다.");
            return;
        }
        // 체력 복원
        player.Health.LoadCurrentHp(data.player.currentHp);

        // 위치 복원
        Vector3 pos = data.player.position;
        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled)
        {
            if (!agent.Warp(pos))
            {
                Debug.LogWarning("위치 복원 실패: NavMesh를 확인하세요.");
                return;
            }
        }
        else
        {
            player.transform.position = pos;
        }

        InventorySaveData inventory = data.player.inventory;

        if (inventory == null ||
            ItemManager.Instance == null ||
            ItemInventory == null)
        {
            Debug.LogError("인벤토리 복원 데이터 또는 참조가 없습니다.");
            return;
        }
        // 보유 아이템 수량 복원
        if (!ItemManager.Instance.RestoreItems(inventory.items))
            return;
        // 슬롯 배치 및 UI 복원
        if (!ItemInventory.RestoreSlots(inventory.inventorySlots))
            return;
        // 장비 복원 
        EquipmentManager.LoadEquipment(data.player.equippedItemId);

        Debug.Log("플레이어 복원 완료");
    }
}