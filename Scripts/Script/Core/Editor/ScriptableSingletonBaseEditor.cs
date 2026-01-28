using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableSingletonBase), true)]
public class ScriptableSingletonBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // (선택 사항) 테스트 편의를 위해 버튼은 남겨둘 수 있습니다.
        // 플레이 모드 중에도 수동으로 리셋하고 싶을 때 유용합니다.
        if (EditorApplication.isPlaying)
        {
            
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Manual Reset -> RuntimeData"))
        {
            var singleton = (ScriptableSingletonBase)target;
            singleton.ResetData();
            Debug.Log($"<color=orange>[Runtime Reset]</color> {singleton.name} 리셋됨");
        }
    }
}