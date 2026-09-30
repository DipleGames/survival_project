using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

// CSV 데이터를 읽어 JSON 형태로 변환 및 병합 저장
public class ConvertCsvToJson : MonoBehaviour
{
    [Header("저장 설정 (폴더가 없으면 자동 생성됩니다. 예: SetData 또는 Resources/Data)")]
    [Tooltip("Assets 폴더 하위의 저장 경로를 입력하세요.")]
    [SerializeField] private string outputFolder = "SetData";

    [Header("File Settings")]
    [Tooltip("웹 구글 시트 URL, 프로젝트 상대 경로(Assets/...), 절대 경로 모두 입력 가능합니다.")]
    public string csvFilePath;
    public string jsonOutputFileName = "ItemDataJson_Converted";

    [Header("Merge Settings (Optional)")]
    public string mergeJsonFilePath;

    [ContextMenu("Convert CSV To JSON")]
    public void ConvertCsvToJsonFile() 
    {
        string csvText = CsvUtility.LoadCsvText(csvFilePath);
        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogError($"[ConvertCsvToJson] CSV 데이터를 읽어오는 데 실패했습니다: {csvFilePath}");
            return;
        }

        // 줄 바꿈 단위 분할 (\r\n, \n 모두 대응)
        string[] lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (lines.Length == 0) return;

        // 1행 데이터 확인 및 헤더 위치 자동 감지
        var line0Tokens = CsvUtility.ParseCsvLine(lines[0]); 
        bool hasTypeRow = CsvUtility.IsTypeHeader(line0Tokens);

        int headerRowIndex = hasTypeRow ? 1 : 0;
        int dataStartRowIndex = hasTypeRow ? 2 : 1;

        if (lines.Length <= headerRowIndex)
        {
            Debug.LogError("[ConvertCsvToJson] CSV 파일의 데이터 행이 부족합니다.");
            return;
        }

        var headers = CsvUtility.ParseCsvLine(lines[headerRowIndex]);
        var newItems = new List<Dictionary<string, string>>();

        // 데이터 행 읽기
        for (int i = dataStartRowIndex; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var values = CsvUtility.ParseCsvLine(lines[i]);
            var itemData = new Dictionary<string, string>();

            for (int j = 0; j < headers.Count && j < values.Count; j++)
            {
                string key = headers[j].Trim();

                // 칼럼명이 비어있는 열(빈 A열 등)은 건너뜁니다.
                if (string.IsNullOrWhiteSpace(key)) continue;

                itemData[key] = values[j].Trim();
            }

            if (itemData.Count > 0)
            {
                newItems.Add(itemData);
            }
        }

        // 기존 파일 병합 처리
        var finalItems = new List<Dictionary<string, string>>();
        string fullMergePath = CsvUtility.ResolvePath(mergeJsonFilePath, ".txt");

        if (!string.IsNullOrEmpty(fullMergePath) && File.Exists(fullMergePath))
        {
            try
            {
                string existingJson = File.ReadAllText(fullMergePath);
                var existingRoot = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(existingJson);

                if (existingRoot != null && existingRoot.ContainsKey("items"))
                {
                    finalItems.AddRange(existingRoot["items"]);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ConvertCsvToJson] 기존 JSON 병합 중 읽기 실패 (새 데이터로 덮어씁니다): {ex.Message}");
            }
        }

        finalItems.AddRange(newItems);

        // JSON 직렬화 및 저장
        var rootData = new Dictionary<string, object>()
        {
            { "items", finalItems }
        };

        string jsonText = JsonConvert.SerializeObject(rootData, Formatting.Indented);
        string outputPath = CsvUtility.ResolveOutputPath(outputFolder, jsonOutputFileName, ".txt");

        // 저장할 폴더가 없으면 자동 생성
        string directoryPath = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(outputPath, jsonText);
        Debug.Log($"[ConvertCsvToJson] JSON 변환 성공! 경로: {outputPath}");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}