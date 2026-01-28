using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PlaceRepository))]
public class PlaceRepositoryEditor : Editor
{
    // 데이터가 위치한 루트 경로 (Artifact와 구분하여 설정)
    private const string ROOT_PATH = "Assets/AssetResources/Data";

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 속성 출력 (m_places, m_countries 리스트 표시)
        DrawDefaultInspector();

        PlaceRepository repository = (PlaceRepository)target;

        GUILayout.Space(15);
        GUI.backgroundColor = Color.cyan;

        // 버튼 클릭 시 데이터 수집 로직 실행
        if (GUILayout.Button("Load All Place & Country Data", GUILayout.Height(35)))
        {
            LoadAllPlaceData(repository);
        }
    }

    private void LoadAllPlaceData(PlaceRepository repository)
    {
        // 1. 경로 유효성 검사
        if (!AssetDatabase.IsValidFolder(ROOT_PATH))
        {
            Debug.LogError($"[PlaceEditor] 해당 경로가 존재하지 않습니다: {ROOT_PATH}");
            return;
        }

        // 2. Undo 등록 (Ctrl+Z 지원 및 에디터 변경사항 추적)
        Undo.RecordObject(repository, "Update Place Repository");

        // 3. 프로퍼티를 통해 데이터 로드 및 할당
        // LoadAssetsByType 제네릭 메서드를 활용하여 중복 로직 제거
        repository.Places = LoadAssetsByType<PlaceDataSO>();
        repository.Countries = LoadAssetsByType<CountryDataSO>();

        // 4. 에셋 저장 및 Dirty 표시 (인스펙터 갱신 및 데이터 보존)
        EditorUtility.SetDirty(repository);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=lime>✔ 성공:</color> {ROOT_PATH} 하위에서 " +
                  $"장소 {repository.Places.Count}개, 국가 {repository.Countries.Count}개를 로드했습니다.");
    }

    /// <summary>
    /// 특정 경로 하위에서 제네릭 타입에 해당하는 모든 에셋을 찾아 리스트로 반환합니다.
    /// </summary>
    private List<T> LoadAssetsByType<T>() where T : ScriptableObject
    {
        // 타입 이름으로 필터 생성 (예: t:PlaceDataSO)
        string filter = $"t:{typeof(T).Name}";

        // AssetDatabase를 통해 모든 해당 에셋의 GUID 검색
        string[] guids = AssetDatabase.FindAssets(filter, new[] { ROOT_PATH });
        List<T> assetList = new List<T>();

        foreach (string guid in guids)
        {
            // GUID를 실제 에셋 경로로 변환
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 해당 경로의 에셋 로드
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (asset != null)
            {
                assetList.Add(asset);
            }
        }
        return assetList;
    }
}