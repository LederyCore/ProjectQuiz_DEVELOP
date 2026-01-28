using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransformEventChannelSO))]
public class TransformEventChannelEditor : GenericEventChannelEditor<Transform, TransformEventChannelSO>
{
    // Transform은 Object이므로 ObjectField를 사용하여 구현합니다.
    protected override Transform DrawTypeSpecificField(Transform value)
    {
        return (Transform)EditorGUILayout.ObjectField("Debug Transform to Raise", value, typeof(Transform), true);
    }
}