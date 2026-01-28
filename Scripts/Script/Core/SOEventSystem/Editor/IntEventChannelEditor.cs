using UnityEditor;

[CustomEditor(typeof(IntEventChannelSO))]
public class IntEventChannelEditor : GenericEventChannelEditor<int, IntEventChannelSO>
{
    protected override int DrawTypeSpecificField(int value) => EditorGUILayout.IntField("Value", value);
}