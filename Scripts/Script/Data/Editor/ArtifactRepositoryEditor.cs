using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(ArtifactRepository))]
public class ArtifactRepositoryEditor : Editor
{
    // 데이터가 위치한 루트 경로 (상수로 유지하되 필요 시 인스펙터에서 수정 가능하도록 설계 가능)
    private const string ROOT_PATH = "Assets/AssetResources/Data/ArtifactData";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ArtifactRepository repository = (ArtifactRepository)target;

        GUILayout.Space(10);
        GUI.backgroundColor = Color.cyan;

        if (GUILayout.Button("Load All Artifact Data (Including Subfolders)", GUILayout.Height(30)))
        {
            LoadAllArtifacts(repository);
        }
    }

    private void LoadAllArtifacts(ArtifactRepository repository)
    {
        // 1. 경로 유효성 검사
        if (!AssetDatabase.IsValidFolder(ROOT_PATH))
        {
            Debug.LogError($"[ArtifactEditor] 해당 경로가 존재하지 않습니다: {ROOT_PATH}");
            return;
        }

        // 2. 검색 필터 설정
        // t:ArtifactDataSO -> ArtifactDataSO 타입을 상속받거나 해당 타입인 모든 에셋 검색
        // 하위 폴더는 FindAssets가 자동으로 포함하여 검색합니다.
        string filter = $"t:{typeof(ArtifactDataSO).Name}";
        string[] guids = AssetDatabase.FindAssets(filter, new[] { ROOT_PATH });

        List<ArtifactDataSO> artifactList = new List<ArtifactDataSO>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ArtifactDataSO asset = AssetDatabase.LoadAssetAtPath<ArtifactDataSO>(assetPath);

            if (asset != null)
            {
                artifactList.Add(asset);
            }
        }

        // 3. 데이터 할당 및 데이터 보존 (Undo 등록)
        Undo.RecordObject(repository, "Update Artifact Repository");

        // 중복 제거가 필요할 경우 LINQ의 Distinct 활용 가능
        repository.Artifacts = artifactList;

        // 4. 에셋 변경사항 저장
        EditorUtility.SetDirty(repository);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=lime>✔ 성공:</color> {ROOT_PATH} 하위 모든 폴더에서 {artifactList.Count}개의 유물 데이터를 로드했습니다.");
    }
}