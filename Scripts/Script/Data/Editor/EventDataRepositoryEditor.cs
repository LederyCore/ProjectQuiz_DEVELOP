using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(EventRepository))]
public class EventDataRepositoryEditor : Editor
{
    private const string ROOT_PATH = "Assets/AssetResources/Data/EventData";

    public override void OnInspectorGUI()
    {
        // 리스트 데이터를 인스펙터에서 직접 확인할 수 있도록 기본 UI 출력
        DrawDefaultInspector();

        EventRepository repository = (EventRepository)target;

        GUILayout.Space(15);
        GUI.backgroundColor = Color.yellow;

        if (GUILayout.Button("Auto-Classify & Load All Event Lists", GUILayout.Height(35)))
        {
            LoadAndClassifyEvents(repository);
        }
    }

    private void LoadAndClassifyEvents(EventRepository repository)
    {
        if (!AssetDatabase.IsValidFolder(ROOT_PATH))
        {
            Debug.LogError($"[EventEditor] 경로를 찾을 수 없습니다: {ROOT_PATH}");
            return;
        }

        // 1. Undo 등록
        Undo.RecordObject(repository, "Classify Event Data Lists");

        // 2. 해당 경로의 모든 EventDataSO 로드
        List<EventDataSO> allEvents = LoadAssetsByType<EventDataSO>(ROOT_PATH);

        // 3. 키워드 기반 리스트 분류 (Where 사용)
        // 대소문자 구분 없이 파일 이름에 키워드가 포함되어 있는지 확인하여 할당합니다.
        repository.CommonDangerDeck = allEvents
            .Where(e => e.name.IndexOf("Danger", System.StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        repository.CommonLuckDeck = allEvents
            .Where(e => e.name.IndexOf("Luck", System.StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        repository.CommonEventDeck = allEvents
            .Where(e => e.name.IndexOf("Event", System.StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        repository.FailEventData = allEvents
            .Where(e => e.name.IndexOf("Fail", System.StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        // 4. 변경사항 저장
        EditorUtility.SetDirty(repository);
        AssetDatabase.SaveAssets();

        // 5. 결과 로그 출력
        Debug.Log($"<color=cyan><b>[EventRepository]</b></color> 자동 분류 완료!\n" +
                  $"Danger: {repository.CommonDangerDeck.Count}개, " +
                  $"Luck: {repository.CommonLuckDeck.Count}개, " +
                  $"Common: {repository.CommonEventDeck.Count}개, " +
                  $"Fail: {repository.FailEventData.Count}개");
    }

    private List<T> LoadAssetsByType<T>(string path) where T : ScriptableObject
    {
        string filter = $"t:{typeof(T).Name}";
        string[] guids = AssetDatabase.FindAssets(filter, new[] { path });

        return guids.Select(guid =>
            AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))
        ).Where(asset => asset != null).ToList();
    }
}