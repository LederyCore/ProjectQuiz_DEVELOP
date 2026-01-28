using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

public class CSVReader
{
    // CSV 파싱을 위한 정규식
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    /// <summary>
    /// 지정된 파일 경로(Assets/...)에서 CSV를 읽어 Dictionary 리스트로 반환합니다.
    /// </summary>
    public static List<Dictionary<string, object>> Read(string filePath)
    {
        var list = new List<Dictionary<string, object>>();

        // [변경] Resources.Load 대신 File.Exists와 ReadAllText 사용
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[CSVReader] 파일을 찾을 수 없습니다: {filePath}");
            return null;
        }

        // [변경] 실제 경로에서 텍스트 데이터 로드
        string csvData = File.ReadAllText(filePath);

        var lines = Regex.Split(csvData, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);

        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                // 데이터 정제 및 타입 변환 (int 판별)
                object finalvalue = value;
                int n;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }

                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }
}