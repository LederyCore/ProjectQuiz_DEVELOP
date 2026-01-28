using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ErrorLog : MonoBehaviour
{
    private static ErrorLog instance;
    private const int maxLine = 1000;
    private string path;

    void Awake() // 싱글톤은 Start보다 Awake가 적합합니다.
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 핵심 수정: 안드로이드에서는 persistentDataPath를 사용해야 합니다.
#if UNITY_EDITOR
        path = Path.Combine(Application.dataPath, "ErrorLog.txt");
#else
        path = Path.Combine(Application.persistentDataPath, "ErrorLog.txt");
#endif

        Application.logMessageReceived += ErrorHandle;
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        Application.logMessageReceived -= ErrorHandle;
    }

    void ErrorHandle(string condition, string stackTrace, LogType type)
    {
        // 에러와 예외 상황만 기록
        if (type == LogType.Error || type == LogType.Exception)
        {
            // 네트워크 응답 에러 등 특정 키워드 제외 로직
            if (condition.Contains("Received")) return;

            SaveLog(condition, stackTrace);
        }
    }

    void SaveLog(string condition, string stackTrace)
    {
        try
        {
            List<string> list = new List<string>();

            // 기존 파일이 있으면 읽어옴
            if (File.Exists(path))
            {
                list.AddRange(File.ReadAllLines(path));
            }

            // 로그 내용 추가
            list.Add($"[{DateTime.Now:yyyy.MM.dd // HH:mm:ss}]");
            list.Add($"Condition: {condition}");
            list.Add($"StackTrace: {stackTrace}");
            list.Add("-------------------------------------------");

            // 최대 라인 수 관리 (오버플로 방지)
            while (list.Count > maxLine)
            {
                list.RemoveAt(0);
            }

            // 파일 쓰기
            File.WriteAllLines(path, list.ToArray());
        }
        catch (Exception e)
        {
            // 로그 저장 중 발생한 에러는 콘솔에만 찍음 (무한 루프 방지)
            Debug.LogWarning($"Failed to save log: {e.Message}");
        }
    }
}