using System.Collections;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [Space(10), Header("----- Event Channels (Listen) -----")]
    [SerializeField] private TravelStartEventChannelSO m_OnStartTravel;

    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private UnityObjectEventChannelSO m_OnArrivalPlace;
    [SerializeField] private VoidEventChannelSO m_OnRequestActiveMarker;
    [SerializeField] private VoidEventChannelSO m_OnRequestInActiveMarker;
    [SerializeField] private GameStateEventChannelSO m_OnRequestChangeGameState;

    [Header("--- 필수 설정---")]
    [SerializeField] private Transform planetCenter;
    [SerializeField] private RotationController earthRotationController;

    [Header("--- 비행 설정---")]
    [SerializeField] private float travelDuration = 3.0f;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float flightHeight = 1f;
    [Range(0f, 2f)][SerializeField] private float peakHeightOffset = 1f;

    [Header("--- 거리별 비행기 크기 조절---")]
    [SerializeField] private float minPlaneScale = 0.2f;
    [SerializeField] private float maxPlaneScale = 2.0f;
    [SerializeField] private float minDistanceLimit = 10f;
    [SerializeField] private float maxDistanceLimit = 20f;

    [Header("--- 근거리 비행 연출 ---")]
    [SerializeField] private float shortDistanceThreshold = 2f;
    [SerializeField] private float closeFlightZoom = 3.5f;
    [SerializeField] private float normalFlightZoom = 6f;
    [SerializeField] private float arrivalZoomOutDistance = 80f;

    [Header("--- 궤적 설정 ---")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float flightLineWidth = 0.5f;
    [SerializeField] private Color flightLineColor = Color.cyan;

    [SerializeField] private PlaceDataSO currentPlaceData;
    // 내부 변수
    private GameObject flightLineObj;
    private LineRenderer flightTrail;

    private Vector3 currentTripTargetScale;
    private WaitForSeconds wait05 = new WaitForSeconds(0.5f);
    private Transform currentPlace;
    

    private void Awake() => m_OnStartTravel.OnEventRaised += HandleOnStartTravel;

    private void Start()
    {
        currentTripTargetScale = transform.localScale;

        if (planetCenter != null)
        {
            transform.SetParent(planetCenter);
        }

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    private void OnDestroy() => m_OnStartTravel.OnEventRaised -= HandleOnStartTravel;

    public void HandleOnStartTravel(TravelData data)
    {
        if (data.StartPlace == null || data.EndPlace == null)
        {
            Debug.LogError("[AirplaneController] 시작/도착 장소가 없습니다.");
            return;
        }

        gameObject.SetActive(true);
        currentPlace = data.EndPlace;
        currentPlaceData = data.PlaceData;
        StartCoroutine(TravelRoutine(data.StartPlace, data.EndPlace));
    }

    private void CreateFlightLineObject()
    {
        // 이미 존재하면 새로 만들지 않음 (비행 중복 방지)
        if (flightLineObj != null) return;

        flightLineObj = new GameObject("FlightLine_Trajectory");

        // ★ 핵심: 지구본을 부모로 설정해야 지구가 돌 때 선도 같이 돕니다.
        flightLineObj.transform.SetParent(planetCenter);
        flightLineObj.transform.localPosition = Vector3.zero;
        flightLineObj.transform.localRotation = Quaternion.identity;
        flightLineObj.transform.localScale = Vector3.one;

        flightTrail = flightLineObj.AddComponent<LineRenderer>();
        flightTrail.useWorldSpace = false; // ★ 핵심: 로컬 좌표 사용
        flightTrail.positionCount = 0;
        flightTrail.startWidth = flightLineWidth / 3f;
        flightTrail.endWidth = flightLineWidth; // 꼬리가 얇아지는 연출
        flightTrail.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        flightTrail.startColor = flightLineColor;
        flightTrail.endColor = flightLineColor;
        flightTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        flightTrail.receiveShadows = false;
    }

    private void SetCurrentPlace(Transform place)
    {
        currentPlace = place;
        if (planetCenter != null && place != null)
        {
            Vector3 localPos = GetLocalPositionOnPlanet(place);
            transform.localPosition = localPos;
            transform.LookAt(planetCenter.position);
            transform.localScale = Vector3.zero;
        }
    }

    private IEnumerator TravelRoutine(Transform startPlace, Transform targetPlace)
    {
        m_OnRequestInActiveMarker.RaiseEvent();

        // 1. 라인 오브젝트 생성 및 초기화
        CreateFlightLineObject();
        flightLineObj.SetActive(true);

        Vector3 startPosLocal = GetLocalPositionOnPlanet(startPlace);
        Vector3 endPosLocal = GetLocalPositionOnPlanet(targetPlace);

        float distance = Vector3.Distance(startPosLocal, endPosLocal);
        float targetZoom = (distance < shortDistanceThreshold) ? closeFlightZoom : normalFlightZoom;
        float scaleRatio = Mathf.InverseLerp(minDistanceLimit, maxDistanceLimit, distance);
        float targetScaleValue = Mathf.Lerp(minPlaneScale, maxPlaneScale, scaleRatio);
        currentTripTargetScale = Vector3.one * targetScaleValue;

        transform.localPosition = startPosLocal;
        transform.localScale = Vector3.zero;

        // 라인 시작점 찍기
        flightTrail.positionCount = 1;
        flightTrail.SetPosition(0, startPosLocal);

        // 2. 비행기 등장 (Scale Up)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, currentTripTargetScale, timer / fadeDuration);
            yield return null;
        }
        transform.localScale = currentTripTargetScale;

        if (earthRotationController == null) Debug.LogWarning("[AirplaneController] Rotation Controller Missing");
        else earthRotationController.StartFollowing(this.transform);

        // 3. 비행 이동 Loop
        float currentTime = 0f;
        float startZoom = Vector3.Distance(Camera.main.transform.position, planetCenter.position);

        while (currentTime < travelDuration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / travelDuration;

            // 베지어 곡선/파라볼라 이동 계산
            Vector3 baseLocalPos = Vector3.Slerp(startPosLocal, endPosLocal, t);
            float parabolaHeight = 4.0f * t * (1.0f - t) * peakHeightOffset;
            Vector3 finalLocalPos = baseLocalPos + (baseLocalPos.normalized * parabolaHeight);

            transform.localPosition = finalLocalPos;

            // 회전 처리
            if (t < 0.99f)
            {
                float nextT = t + 0.01f;
                Vector3 nextBase = Vector3.Slerp(startPosLocal, endPosLocal, nextT);
                float nextH = 4.0f * nextT * (1.0f - nextT) * peakHeightOffset;
                Vector3 nextPos = nextBase + (nextBase.normalized * nextH);

                Vector3 forward = (nextPos - finalLocalPos).normalized;
                Vector3 up = finalLocalPos.normalized;

                if (forward != Vector3.zero)
                {
                    Quaternion rot = Quaternion.LookRotation(forward, up);
                    transform.localRotation = rot;
                }
            }

            // 카메라 줌 처리
            if (earthRotationController != null)
            {
                float currentZoom = Mathf.Lerp(startZoom, targetZoom, t);
                earthRotationController.SetFollowDistance(currentZoom);
            }

            // ★ 라인 그리기 (이전 점과 위치가 다를 때만 추가 - 최적화)
            if (flightTrail.positionCount > 0)
            {
                Vector3 lastPos = flightTrail.GetPosition(flightTrail.positionCount - 1);
                if (Vector3.Distance(lastPos, finalLocalPos) > 0.01f)
                {
                    flightTrail.positionCount++;
                    flightTrail.SetPosition(flightTrail.positionCount - 1, finalLocalPos);
                }
            }

            yield return null;
        }

        transform.localPosition = endPosLocal;

        // 4. 도착 후 타겟팅
        if (earthRotationController != null)
            earthRotationController.StartTargeting(targetPlace);

        yield return wait05; // 도착 후 잠시 대기

        // 5. ★ 궤적 처리: 떼어내고(Detach), 서서히 사라지게(Fade) 위임
        if (flightLineObj != null)
        {
            // Fader 스크립트 붙여서 자동 파괴 위임
            PlaneTrailFader fader = flightLineObj.AddComponent<PlaneTrailFader>();
            fader.StartFadeAndDestroy(fadeDuration);
        }

        // 컨트롤러는 이제 궤적을 잊습니다 (참조 해제)
        flightLineObj = null;
        flightTrail = null;

        // 6. 비행기 퇴장 (Scale Down)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(currentTripTargetScale, Vector3.zero, timer / fadeDuration);
            yield return null;
        }

        transform.localScale = Vector3.zero;


        // 도착 후 줌 아웃
        if (earthRotationController != null)
        {
            earthRotationController.SetFollowDistance(arrivalZoomOutDistance);
        }

        SetCurrentPlace(targetPlace);

        m_OnArrivalPlace.RaiseEvent(currentPlaceData);
        m_OnRequestActiveMarker.RaiseEvent();
        m_OnRequestChangeGameState.RaiseEvent(GameState.EVENT);
        gameObject.SetActive(false);
    }

    private Vector3 GetLocalPositionOnPlanet(Transform target)
    {
        Vector3 rawLocal = planetCenter.InverseTransformPoint(target.position);
        float radius = rawLocal.magnitude + flightHeight;
        return rawLocal.normalized * radius;
    }
}