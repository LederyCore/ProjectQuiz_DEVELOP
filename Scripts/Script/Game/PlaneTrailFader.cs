using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlaneTrailFader : MonoBehaviour
{
    private LineRenderer lineRenderer;

    /// <summary>
    /// 외부에서 호출하면 꼬리를 줄여가며 삭제를 시작합니다.
    /// </summary>
    /// <param name="duration">완전히 사라지는 데 걸리는 시간</param>
    public void StartFadeAndDestroy(float duration)
    {
        lineRenderer = GetComponent<LineRenderer>();
        StartCoroutine(RetractAndDestroyRoutine(duration));
    }

    private IEnumerator RetractAndDestroyRoutine(float duration)
    {
        // 1. 현재 라인의 모든 점을 리스트로 가져옵니다. (수정이 용이하도록)
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);
        List<Vector3> pointList = new List<Vector3>(positions);

        // 점이 너무 적으면 바로 파괴
        if (pointList.Count < 2)
        {
            Destroy(gameObject);
            yield break;
        }

        // 2. 전체 라인의 길이를 계산합니다. (삭제 속도를 결정하기 위해)
        float totalLength = 0f;
        for (int i = 0; i < pointList.Count - 1; i++)
        {
            totalLength += Vector3.Distance(pointList[i], pointList[i + 1]);
        }

        // 3. 삭제 속도 계산 (초당 이동 거리)
        float retractSpeed = totalLength / duration;

        // 4. 루프: 꼬리(Index 0)를 다음 점(Index 1)으로 이동시키며 줄임
        while (pointList.Count > 1)
        {
            float moveDistance = retractSpeed * Time.deltaTime;

            // 한 프레임에 이동해야 할 거리만큼 점들을 처리
            while (moveDistance > 0 && pointList.Count > 1)
            {
                // 현재 꼬리점(0)과 다음 점(1) 사이의 거리
                float distToNext = Vector3.Distance(pointList[0], pointList[1]);

                if (moveDistance >= distToNext)
                {
                    // 이동해야 할 거리가 다음 점까지의 거리보다 멀다면
                    // 0번 점을 완전히 삭제하고, 이동 거리를 차감한 뒤 다음 루프(다음 점) 처리
                    moveDistance -= distToNext;
                    pointList.RemoveAt(0);
                }
                else
                {
                    // 다음 점에 도달하지 못했다면, 그 방향으로 이동만 시킴
                    pointList[0] = Vector3.MoveTowards(pointList[0], pointList[1], moveDistance);
                    moveDistance = 0f; // 이동 완료
                }
            }

            // 변경된 점들을 라인 렌더러에 반영
            lineRenderer.positionCount = pointList.Count;
            lineRenderer.SetPositions(pointList.ToArray());

            yield return null;
        }

        // 5. 모든 점이 사라지면 오브젝트 파괴
        Destroy(gameObject);
    }
}