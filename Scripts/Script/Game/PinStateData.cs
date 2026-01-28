using UnityEngine;

[CreateAssetMenu(fileName = "PinStateData", menuName = "Game/PinStateData")]
public class PinStateData : ScriptableObject
{
    public GameObject visualPrefab;
    public float detectionRadius;
    public GameObject CreateInstance(Transform parent)
    {
        if (visualPrefab == null) return null;

        // X축으로 -90도 회전된 쿼터니언 생성
        // 부모의 회전값에 상관없이 월드 좌표계 기준으로 -90도를 바라보게 합니다.
        Quaternion fixedRotation = Quaternion.Euler(-90f, 0f, 0f);

        var obj = Instantiate(visualPrefab, parent.position, parent.rotation, parent);
        obj.transform.localRotation = fixedRotation;

        return obj;
    }
}