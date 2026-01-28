using UnityEditor;

[CustomEditor(typeof(BooleanEventChannelSO))]
public class BooleanEventChannelEditor : GenericEventChannelEditor<bool, BooleanEventChannelSO>
{
    // bool 타입에 맞는 Toggle 필드로 수정
    protected override bool DrawTypeSpecificField(bool value)
    {
        return EditorGUILayout.Toggle("Value to Raise", value);
    }
}