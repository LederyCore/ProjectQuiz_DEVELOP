using UnityEditor;

[CustomEditor(typeof(StringEventChannelSO))]
public class StringEventChannelEditor : GenericEventChannelEditor<string, StringEventChannelSO>
{
    protected override string DrawTypeSpecificField(string value) => EditorGUILayout.TextField("Value", value);
}