using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TravelStartEventChannelSO))]
public class TravelStartEventChannelEditor : GenericEventChannelEditor<TravelData, TravelStartEventChannelSO>
{
    protected override TravelData DrawTypeSpecificField(TravelData value)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Debug Travel Data", EditorStyles.boldLabel);

        // 1. 출발지 설정
        value.StartPlace = (Transform)EditorGUILayout.ObjectField(
            "Start Place", value.StartPlace, typeof(Transform), true);

        // 2. 목적지 설정
        value.EndPlace = (Transform)EditorGUILayout.ObjectField(
            "End Place", value.EndPlace, typeof(Transform), true);

        // 3. 비용 설정
        value.TravelCost = EditorGUILayout.IntField("Travel Cost", value.TravelCost);

        EditorGUILayout.EndVertical();

        return value;
    }
}