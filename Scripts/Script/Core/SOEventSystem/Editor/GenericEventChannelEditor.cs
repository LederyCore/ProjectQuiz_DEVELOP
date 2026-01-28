// 모든 제네릭 이벤트 채널 에디터의 부모가 될 클래스
using UnityEditor;
using UnityEngine;

public abstract class GenericEventChannelEditor<T, TChannel> : EventChannelEditor
    where TChannel : EventChannelSO<T>
{
    private T m_testValue;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;
        EditorGUILayout.Space();

        // 타입별 입력 필드 그리기 (추상 메서드로 자식에게 위임)
        m_testValue = DrawTypeSpecificField(m_testValue);

        if (GUILayout.Button($"Raise Event with {typeof(T).Name}"))
        {
            ((TChannel)target).RaiseEvent(m_testValue);
        }

        GUI.enabled = true;
    }

    protected abstract T DrawTypeSpecificField(T value);
}