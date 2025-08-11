using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

public class SpreadSheetJson : EditorWindow
{
    [Serializable]
    public class TemplateData
    {
        public string templateName;
        public int typesX;
        public int typesY;
        public string spreadsheetId;
        public int startX;
        public int startY;
        public string saveFileName;
    }

    private List<TemplateData> templates = new();
    private int selectedTemplateIndex = 0;

    private int typesX = 0;
    private int typesY = 0;
    private string spreadsheetId = "";
    private int startX = 0;
    private int startY = 0;
    private string saveFileName = "game_data.json";

    private string templateNameInput = "";
    private static string TemplateFilePath =>
        Path.Combine(Application.persistentDataPath, "spreadsheet_templates.json");

    [MenuItem("Tools/Spreadsheet JSON")]
    public static void ShowWindow()
    {
        GetWindow<SpreadSheetJson>("Spreadsheet JSON");
    }

    private void OnEnable()
    {
        LoadTemplates();
    }

    private void OnDisable()
    {
        EditorPrefs.SetInt("ss_json_typesX", typesX);
        EditorPrefs.SetInt("ss_json_typesY", typesY);
        EditorPrefs.SetString("ss_json_spreadsheetId", spreadsheetId);
        EditorPrefs.SetInt("ss_json_startX", startX);
        EditorPrefs.SetInt("ss_json_startY", startY);
        EditorPrefs.SetString("ss_json_saveFileName", saveFileName);
    }

    private void OnGUI()
    {
        GUILayout.Label("템플릿 관리", EditorStyles.boldLabel);

        if (templates.Count > 0)
        {
            string[] names = templates.Select(t => t.templateName).ToArray();
            selectedTemplateIndex = EditorGUILayout.Popup("템플릿 선택", selectedTemplateIndex, names);
        }
        else
        {
            EditorGUILayout.LabelField("저장된 템플릿 없음");
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("불러오기") && templates.Count > 0)
        {
            LoadTemplate(templates[selectedTemplateIndex]);
        }

        if (GUILayout.Button("삭제") && templates.Count > 0)
        {
            templates.RemoveAt(selectedTemplateIndex);
            SaveTemplates();
            selectedTemplateIndex = Mathf.Clamp(selectedTemplateIndex - 1, 0, templates.Count - 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("새 템플릿 이름:");
        templateNameInput = EditorGUILayout.TextField(templateNameInput);

        if (GUILayout.Button("현재 설정 저장") && !string.IsNullOrWhiteSpace(templateNameInput))
        {
            var newTemplate = new TemplateData
            {
                templateName = templateNameInput,
                typesX = typesX,
                typesY = typesY,
                spreadsheetId = spreadsheetId,
                startX = startX,
                startY = startY,
                saveFileName = saveFileName,
            };
            templates.Add(newTemplate);
            SaveTemplates();
            templateNameInput = "";
        }

        GUILayout.Space(15);
        GUILayout.Label("📄 Spreadsheet 설정", EditorStyles.boldLabel);

        spreadsheetId = EditorGUILayout.TextField("스프레드시트 ID", spreadsheetId);

        EditorGUILayout.LabelField("타입 / 필드명 위치 (행 기반)");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("가로 시작 (X)", GUILayout.Width(80));
        typesX = EditorGUILayout.IntField(typesX);
        EditorGUILayout.LabelField("세로 시작 (Y)", GUILayout.Width(80));
        typesY = EditorGUILayout.IntField(typesY);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("본문 데이터 시작 위치");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("가로 (X)", GUILayout.Width(80));
        startX = EditorGUILayout.IntField(startX);
        EditorGUILayout.LabelField("세로 (Y)", GUILayout.Width(80));
        startY = EditorGUILayout.IntField(startY);
        EditorGUILayout.EndHorizontal();

        saveFileName = EditorGUILayout.TextField("저장파일 이름", saveFileName);

        GUILayout.Space(10);
        if (GUILayout.Button("스프레드시트에서 다운로드"))
            FetchSheet();
    }

    private void LoadTemplate(TemplateData t)
    {
        typesX = t.typesX;
        typesY = t.typesY;
        spreadsheetId = t.spreadsheetId;
        startX = t.startX;
        startY = t.startY;
        saveFileName = t.saveFileName;
    }

    private void SaveTemplates()
    {
        string json = JsonConvert.SerializeObject(templates, Formatting.Indented);
        File.WriteAllText(TemplateFilePath, json);
    }

    private void LoadTemplates()
    {
        if (File.Exists(TemplateFilePath))
        {
            string json = File.ReadAllText(TemplateFilePath);
            templates = JsonConvert.DeserializeObject<List<TemplateData>>(json) ?? new List<TemplateData>();
        }
    }

    private void FetchSheet()
    {
        if (string.IsNullOrWhiteSpace(spreadsheetId))
        {
            Debug.LogError("Spreadsheet ID를 입력하세요.");
            return;
        }

        string url = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv";

        try
        {
            using WebClient client = new();
            string csvText = client.DownloadString(url);
            string[] lines = csvText.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            List<List<string>> grid = lines.Select(line => ParseCsvLine(line.Trim())).ToList();

            // 필드 타입과 이름 추출
            List<string> types = grid[typesY].Skip(typesX).ToList();
            List<string> names = grid[typesY + 1].Skip(typesX).ToList();
            int fieldCount = Mathf.Min(types.Count, names.Count);

            var fields = new List<(string name, string type)>();
            for (int i = 0; i < fieldCount; i++)
                fields.Add((names[i], types[i]));

            List<Dictionary<string, object>> allRows = new();
            for (int y = startY; y < grid.Count; y++)
            {
                var columns = grid[y];
                var row = new Dictionary<string, object>();

                for (int i = 0; i < fields.Count; i++)
                {
                    int colIndex = startX + i;
                    if (colIndex >= columns.Count)
                        continue;

                    string raw = columns[colIndex];
                    row[fields[i].name] = ParseValue(raw, fields[i].type);
                }

                allRows.Add(row);
            }

            SaveJson(allRows);
            Debug.Log($"저장 완료: {saveFileName}");
        }
        catch (Exception ex)
        {
            Debug.LogError("오류 발생: " + ex.Message);
        }
    }

    private void SaveJson(List<Dictionary<string, object>> data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        string path = Path.Combine(Application.dataPath, saveFileName);
        File.WriteAllText(path, json, Encoding.UTF8);
        AssetDatabase.Refresh();
    }
    private object ParseValue(string raw, string type)
    {
        try
        {
            return type.ToLower() switch
            {
                "int" => int.Parse(raw),
                "float" => float.Parse(raw),
                "bool" => bool.Parse(raw),
                "string" => raw,
                _ => raw
            };
        }
        catch
        {
            Debug.LogWarning($"파싱 실패: '{raw}' 를 {type}로 변환할 수 없음");
            return null;
        }
    }

    private List<string> ParseCsvLine(string line)
    {
        List<string> fields = new();
        bool inQuotes = false;
        StringBuilder current = new();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
