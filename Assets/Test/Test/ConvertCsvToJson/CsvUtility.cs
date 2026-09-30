using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Text.RegularExpressions;

public static class CsvUtility
{
    // CSV 한 줄을 파싱
    public static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false; //순회하고있는 문자열이 따음표 안인지, 밖인지 확인용 bool 변수 inQuotes: false == 따음표 밖에 있는 상태, isQuotes가 true -> 따음표 안쪽에 있는 상태
        string currentToken = ""; //시트 속 문자열을 읽어올 변수

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"') 
            {
                inQuotes = !inQuotes; 
            }
            else if (c == ',' && !inQuotes) //값이 , 이고, isQuotes가 true면 -> 셀 하나가 끝난것.
            {
                result.Add(currentToken); //result에 currentToken 값 추가 후 다시 공백으로 초기화
                currentToken = "";
            }
            else
            {
                currentToken += c; //line을 순회하면서 문자열을 currentToken에 저장함
            }
        }
        result.Add(currentToken); //csv 마지막 데이터 뒤에는 ,가 붙지 않기때문에 for문 종료 후 아직 result에 저장되지 않은 마지막 값 추가
        return result;
    }

    //첫 번째 행이 int, string 등의 데이터 타입 정의행인지 검사
    public static bool IsTypeHeader(List<string> tokens) 
    {
        if (tokens == null || tokens.Count == 0) return false; //tokens가 null이면 false 반환하고 종료
        int typeCount = 0;
        int validTokenCount = 0;

        foreach (var token in tokens)
        {
            string t = token.Trim().ToLower();
            if (string.IsNullOrEmpty(t)) continue;

            validTokenCount++;
            if (t == "int" || t == "string" || t == "float" || t == "bool" || t == "double" || t == "long")
            {
                typeCount++;
            }
        }

        if (validTokenCount == 0) return false;
        return (float)typeCount / validTokenCount >= 0.4f;
    }

    // 구글 시트 웹 URL을 CSV 다운로드 전용 URL로 자동 변환

    public static string ConvertGoogleSheetUrlToExportCsv(string url) //ConvertGoogleSheetUrlToExportCsv() 메서드를 사용해 사용할 수 있는 csv URL로 변환함
    {
        var docMatch = Regex.Match(url, @"/d/([a-zA-Z0-9-_]+)");
        if (!docMatch.Success) return url;

        string docId = docMatch.Groups[1].Value;
        string gid = "0";

        var gidMatch = Regex.Match(url, @"[?&]gid=([0-9]+)");
        if (!gidMatch.Success) gidMatch = Regex.Match(url, @"#gid=([0-9]+)");
        if (gidMatch.Success) gid = gidMatch.Groups[1].Value;

        return $"https://docs.google.com/spreadsheets/d/{docId}/export?format=csv&gid={gid}";
    }


    // 웹 URL 또는 로컬 경로에서 CSV 텍스트를 읽어옴
    public static string LoadCsvText(string inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath)) return "";

        inputPath = inputPath.Trim();

        if (inputPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            inputPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            string exportUrl = ConvertGoogleSheetUrlToExportCsv(inputPath);
            try
            {
                using (var client = new HttpClient())
                {
                    return client.GetStringAsync(exportUrl).Result;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CsvUtility] 구글 시트 다운로드 실패: {ex.Message}");
                return "";
            }
        }

        string localPath = ResolvePath(inputPath, ".csv");
        return File.Exists(localPath) ? File.ReadAllText(localPath) : "";
    }

    // 입력 경로의 상대/절대 여부 및 확장자를 보완하여 파일 절대 경로를 반환

    public static string ResolvePath(string inputPath, string defaultExtension)
    {
        if (string.IsNullOrWhiteSpace(inputPath)) return "";
        inputPath = inputPath.Replace('\\', '/').Trim();

        string fullPath = inputPath;
        if (!Path.IsPathRooted(fullPath))
        {
            if (fullPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                fullPath = Path.Combine(projectRoot, fullPath);
            }
            else
            {
                fullPath = Path.Combine(Application.dataPath, fullPath);
            }
        }

        if (File.Exists(fullPath)) return fullPath;
        if (!Path.HasExtension(fullPath) && File.Exists(fullPath + defaultExtension))
            return fullPath + defaultExtension;

        if (fullPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            string txtPath = Path.ChangeExtension(fullPath, ".txt");
            if (File.Exists(txtPath)) return txtPath;
        }
        else if (fullPath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            string csvPath = Path.ChangeExtension(fullPath, ".csv");
            if (File.Exists(csvPath)) return csvPath;
        }

        return Path.HasExtension(fullPath) ? fullPath : fullPath + defaultExtension;
    }

    // 저장될 폴더 및 파일명을 조합해 출력 절대 경로를 생성
    public static string ResolveOutputPath(string outputFolder, string fileName, string extension)
    {
        if (string.IsNullOrWhiteSpace(outputFolder)) outputFolder = "SetData";

        if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        {
            fileName += extension;
        }

        string relativePath = Path.Combine("Assets", outputFolder, fileName);
        return Path.GetFullPath(relativePath);
    }
}