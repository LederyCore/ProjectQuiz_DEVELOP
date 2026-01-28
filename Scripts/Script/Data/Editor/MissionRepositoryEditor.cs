using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(MissionRepository))]
public class MissionRepositoryEditor : Editor
{
    // 데이터가 위치한 루트 경로
    private const string ROOT_MISSION = "Assets/AssetResources/Data/MissionData";

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 표시 (m_missions 리스트 등)
        DrawDefaultInspector();

        MissionRepository repository = (MissionRepository)target;

        GUILayout.Space(10);
        GUI.backgroundColor = Color.cyan;

        if (GUILayout.Button("Load All Mission Data Assets", GUILayout.Height(30)))
        {
            LoadMissions(repository);
        }
    }

    private void LoadMissions(MissionRepository repository)
    {
        // 1. 해당 경로가 존재하는지 확인
        if (!Directory.Exists(ROOT_MISSION))
        {
            Debug.LogError($"경로를 찾을 수 없습니다: {ROOT_MISSION}");
            return;
        }

        // 2. AssetDatabase를 사용하여 MissionDataSO 타입의 에셋 GUID들을 검색
        // t:MissionDataSO 는 해당 타입을 가진 에셋만 필터링합니다.
        string[] guids = AssetDatabase.FindAssets("t:MissionDataSO", new[] { ROOT_MISSION });

        List<MissionDataSO> missionList = new List<MissionDataSO>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            MissionDataSO asset = AssetDatabase.LoadAssetAtPath<MissionDataSO>(assetPath);

            if (asset != null)
            {
                missionList.Add(asset);
            }
        }

        // 3. 찾은 데이터를 Repository에 할당
        // Undo 시스템에 등록하여 실행 취소가 가능하게 만듭니다 (유지보수성)
        Undo.RecordObject(repository, "Update Mission Repository");
        repository.Missions = missionList;

        // 4. 에셋 변경사항 저장
        EditorUtility.SetDirty(repository);
        AssetDatabase.SaveAssets();

        Debug.Log($"성공: {ROOT_MISSION} 경로에서 {missionList.Count}개의 미션 데이터를 불러왔습니다.");
    }
}