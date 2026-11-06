using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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

        Vector3 pos = player.transform.position;

        SaveData data = new SaveData
        {
            playerX = pos.x,
            playerY = pos.y,
            playerZ = pos.z
        };

        //폴더 생성
        //폴더가 있으면 Directory.CreateDirectory()는 아무런 동작을 하지 않음
        //GetPath()에서 반환된 경로에 파일을 생성하고 저장
        Directory.CreateDirectory(Path.GetDirectoryName(GetPath(slot)));
        File.WriteAllText(GetPath(slot), JsonUtility.ToJson(data, true));
        Debug.Log("저장 완료: " + GetPath(slot));
    }

    public void LoadGame(int slot)
    {
        Player player = GameManager.Instance.Player;
        if (player == null) return;

        if (!File.Exists(GetPath(slot)))
        {
            Debug.LogWarning("저장 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(GetPath(slot));
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null) return;

        Vector3 pos = new Vector3(
            data.playerX, data.playerY, data.playerZ
        );

        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled)
        {
            if (!agent.Warp(pos))
            {
                Debug.LogWarning("불러오기 실패: NavMesh를 확인하세요.");
                return;
            }
        }
        else
        {
            player.transform.position = pos;
        }

        Debug.Log("불러오기 완료");
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
                            $"위치: ({data.playerX:F1}, " +
                            $"{data.playerY:F1}, {data.playerZ:F1})";
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

}