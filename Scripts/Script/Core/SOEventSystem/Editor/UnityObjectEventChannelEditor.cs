using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityObjectEventChannelSO))]
public class UnityObjectEventChannelEditor : GenericEventChannelEditor<Object, UnityObjectEventChannelSO>
{
    protected override Object DrawTypeSpecificField(Object value)
    {
        // ObjectField를 사용하여 인스펙터에서 에셋이나 씬 오브젝트를 드래그 앤 드롭할 수 있게 합니다.
        // 첫 번째 인자: 라벨 이름
        // 두 번째 인자: 현재 값
        // 세 번째 인자: 허용할 타입 (UnityEngine.Object)
        // 네 번째 인자: 씬 오브젝트 허용 여부 (true)
        return EditorGUILayout.ObjectField("Debug Unity Object Value", value, typeof(Object), true);
    }
}