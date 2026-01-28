#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomEditor(typeof(ManagersBootstraper))]
public class ManagersBootstraperEditor : Editor
{
    private SerializedProperty _testTargetProp;
    private SerializedProperty _newManagerProp;

    private void OnEnable()
    {
        _testTargetProp = serializedObject.FindProperty("m_TestTargetManager");
        _newManagerProp = serializedObject.FindProperty("m_NewManagerSO");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ManagersBootstraper script = (ManagersBootstraper)target;
        serializedObject.Update();

        GUILayout.Space(20);
        EditorGUILayout.LabelField("🛠 Manager Replacement Tool", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(_testTargetProp, new GUIContent("Target (Old)", "현재 리스트에서 제어하거나 교체될 대상"));
            EditorGUILayout.PropertyField(_newManagerProp, new GUIContent("New Manager", "새로 교체하여 주입할 매니저"));

            serializedObject.ApplyModifiedProperties();

            ManagerSO oldSO = _testTargetProp.objectReferenceValue as ManagerSO;
            ManagerSO newSO = _newManagerProp.objectReferenceValue as ManagerSO;

            if (oldSO != null)
            {
                DrawManagerControls(script, oldSO, newSO);
            }
            else
            {
                EditorGUILayout.HelpBox("교체 대상(Target)을 먼저 할당해주세요.", MessageType.Info);
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Re-Initialize All Managers", GUILayout.Height(30)))
        {
            if (Application.isPlaying) InitializeAllViaReflection(script);
            else Debug.LogWarning("재초기화는 Play Mode에서만 가능합니다.");
        }
    }

    private void DrawManagerControls(ManagersBootstraper script, ManagerSO oldSO, ManagerSO newSO)
    {
        EditorGUILayout.Space(5);

        // --- Status Indicator 섹션 ---
        using (new EditorGUILayout.HorizontalScope())
        {
            // 작은 네모 인디케이터 그리기
            Rect rect = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
            rect.y += 2; // 중앙 정렬 보정
            Color statusColor = oldSO.IsInitialized ? new Color(0.4f, 1f, 0.4f) : new Color(0.7f, 0.7f, 0.7f);
            EditorGUI.DrawRect(rect, statusColor);

            GUILayout.Space(5);
            EditorGUILayout.LabelField($"Status: {(oldSO.IsInitialized ? "Active" : "Inactive")}", EditorStyles.miniLabel);
        }

        EditorGUILayout.BeginHorizontal();

        // 1. 활성/비활성 토글 (버튼 색상 제거)
        string btnText = oldSO.IsInitialized ? "Deactivate" : "Activate";
        if (GUILayout.Button(btnText, GUILayout.Height(25)))
        {
            oldSO.SetActive(!oldSO.IsInitialized);
            EditorUtility.SetDirty(oldSO);
        }

        // 2. 실제 교체 버튼
        GUI.enabled = newSO != null;
        string replaceBtnText = newSO != null ? $"Replace with {newSO.name}" : "Select New Manager";
        if (GUILayout.Button(replaceBtnText, GUILayout.Height(25)))
        {
            if (Application.isPlaying)
            {
                MethodInfo method = typeof(ManagersBootstraper).GetMethod("ReplaceManager");
                MethodInfo generic = method.MakeGenericMethod(oldSO.GetType());
                generic.Invoke(script, new object[] { newSO });

                Debug.Log($"<color=yellow>[Editor]</color> Replaced {oldSO.name} with {newSO.name}");
            }
            else
            {
                Debug.LogWarning("Replace는 Play Mode에서만 가능합니다.");
            }
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void InitializeAllViaReflection(ManagersBootstraper script)
    {
        var field = typeof(ManagersBootstraper).GetField("m_ManagersSO", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (System.Collections.Generic.List<ManagerSO>)field.GetValue(script);
        var initMethod = typeof(ManagersBootstraper).GetMethod("InitializeManager", BindingFlags.NonPublic | BindingFlags.Instance);

        if (list == null || initMethod == null) return;

        for (int i = 0; i < list.Count; i++)
        {
            initMethod.Invoke(script, new object[] { list[i], i });
        }
    }
}
#endif