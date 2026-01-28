using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[CustomEditor(typeof(EventChannelBaseSO), true)]
public class EventChannelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Debug Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Raise Event (Debug)", GUILayout.Height(30)))
        {
            RaiseEventOnTarget();
        }

        EditorGUILayout.Space(5);
        DrawSubscribersList();

        GUI.enabled = true;
    }

    private void DrawSubscribersList()
    {
        EditorGUILayout.LabelField("Active Subscribers:", EditorStyles.miniBoldLabel);

        // 'OnEventRaised' 필드 탐색 (제네릭 상속 구조를 고려하여 하위/상위 클래스 모두 검색)
        FieldInfo fieldInfo = target.GetType().GetField("OnEventRaised",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (fieldInfo == null) return;

        var eventDelegate = fieldInfo.GetValue(target) as MulticastDelegate;

        if (eventDelegate != null)
        {
            Delegate[] invocationList = eventDelegate.GetInvocationList();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var del in invocationList)
            {
                EditorGUILayout.BeginHorizontal();

                // 1. Target이 유니티 객체(MonoBehaviour, SO 등)인 경우
                if (del.Target is UnityEngine.Object unityObj)
                {
                    // 왼쪽에는 객체 필드를 그려서 클릭 시 위치를 찾을 수 있게 함
                    EditorGUILayout.ObjectField(unityObj, unityObj.GetType(), true, GUILayout.Width(170));
                    // 오른쪽에는 메서드 이름 표시
                    EditorGUILayout.LabelField($"➔ {del.Method.Name}", EditorStyles.label);
                }
                // 2. 일반 C# 클래스거나 Static 메서드인 경우
                else
                {
                    string ownerName = del.Target != null ? del.Target.ToString() : "Static Class";
                    EditorGUILayout.LabelField($"[C#] {ownerName}", GUILayout.Width(120));
                    EditorGUILayout.LabelField($"➔ {del.Method.Name}", EditorStyles.label);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("현재 활성화된 구독자가 없습니다 (Runtime Only)", MessageType.Info);
        }
    }

    private void RaiseEventOnTarget()
    {
        var method = target.GetType().GetMethod("RaiseEvent");
        if (method != null)
        {
            // 매개변수가 있는 경우 기본값(null/0)을 넣어 호출하거나 경고 출력
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
                method.Invoke(target, null);
            else
                Debug.LogWarning($"[Debug] '{target.name}'은 매개변수가 필요한 이벤트입니다. 인스펙터 직접 호출은 취소 되었습니다.");
        }
    }
}