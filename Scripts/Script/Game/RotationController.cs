
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class RotationController : MonoBehaviour
{
    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private TransformEventChannelSO m_OnPlaceSelected;

    [Space(10), Header("----- 레이캐스트 및 조작 설정 -----")]
    private bool m_CanInput = true;
    [SerializeField] private LayerMask placeLayerMask = ~0;

    [Space(10), Header("----- 회전 설정 -----")]
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float minInertialSpeed = 0.05f;

    [Space(10), Header("----- 줌(확대/축소) 설정 -----")]
    [SerializeField] private float zoomSpeed = 1;
    [SerializeField] private float minZoomDistance = 36f;
    [SerializeField] private float maxZoomDistance = 20f;
    [SerializeField] private float zoomDamping = 5f;
    [SerializeField] private float zoomThreshold = 1;
    [SerializeField] private bool isProcessingZoom = false;
    [SerializeField] private float zoomInputCooldown = 0.6f;

    [Space(10), Header("----- 자동 이동 설정 -----")]
    [SerializeField] private float transitionSpeed = 3f;
    [SerializeField] private float zoomInDistance = 20f;

    public enum RotationState { STOPPED, DRAGGING, INERTIA, TARGETING, FOLLOWING }
    public RotationState CurrentState = RotationState.STOPPED;

    private Vector3 lastMousePosition;
    private Vector3 inertiaRotationAxis = Vector3.up;
    private float currentInertiaSpeed;
    private Transform mainCamera;

    private float targetZoomDistance;
    private Quaternion targetRotation;
    private Transform followingTarget;

    private Vector3 dragOrigin;
    private bool wasInertiaStopping = false;
    private float inertiaDamping = 8.0f;
    private float minInertiaSpeed = 0.05f;
    private const float CLICK_THRESHOLD = 10f;

    private bool controlFlag = true;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
        if (mainCamera == null)
        {
            Debug.LogError($"[RotationController]Main Camera가 없음.");
            return;
        }

        float currentDist = Vector3.Distance(mainCamera.position, transform.position);
        targetZoomDistance = Mathf.Clamp(currentDist, minZoomDistance, maxZoomDistance);
    }

    private void Update()
    {
        HandleInput();
        HandleStateLogic();

        if (CurrentState != RotationState.TARGETING && CurrentState != RotationState.FOLLOWING)
            HandleSmoothZoom();
    }


    /// <summary>
    /// 카메라의 목표 줌 거리를 직접 설정합니다.
    /// </summary>
    /// <param name="distance">설정할 거리 값</param>
    public void SetFollowDistance(float distance)
    {
        // 인스펙터에서 설정한 최소/최대 줌 범위를 벗어나지 않도록 클램핑합니다.
        // 현재 설정: minZoomDistance(36f, 멀리), maxZoomDistance(20f, 가까이)
        // Mathf.Clamp는 (값, 최소기준, 최대기준) 순서이므로 논리적 크기에 맞춰 정렬합니다.
        targetZoomDistance = Mathf.Clamp(distance, maxZoomDistance, minZoomDistance);

        // 로그 확인용 (필요 시 주석 해제)
        //Debug.Log($"[RotationController] Target Zoom Distance Updated: {targetZoomDistance}");
    }
    /// <summary>
    /// 특정 타겟 오브젝트를 바라보도록 카메라와 지구를 회전시킵니다.
    /// </summary>
    /// <param name="targetObject">바라볼 타겟 트랜스폼</param>
    public void StartTargeting(Transform targetObject)
    {
        CurrentState = RotationState.TARGETING;
        CalculateTargetRotation(targetObject);
        targetZoomDistance = zoomInDistance;
    }
    /// <summary>
    /// 움직이는 타겟을 지속적으로 따라가도록 설정합니다.
    /// </summary>
    /// <param name="movingTarget">추적할 타겟 트랜스폼</param>
    public void StartFollowing(Transform movingTarget)
    {
        followingTarget = movingTarget;
        CurrentState = RotationState.FOLLOWING;
    }

    public void HandleStateChanged(GameState newState)
    {
        // IDLE 상태일 때만 드래그/줌 입력을 허용
        m_CanInput = (newState == GameState.IDLE);

        // 이동 중이거나 이벤트 중이면 즉시 관성 정지
        if (!m_CanInput)
        {
            currentInertiaSpeed = 0f;
            if (CurrentState == RotationState.DRAGGING || CurrentState == RotationState.INERTIA)
                CurrentState = RotationState.STOPPED;
        }
    }

    public void HandleOnDisableGlobControl(bool flag)
    {
        controlFlag = flag;
    }


    private void HandleInput()
    {
        if (!controlFlag) return;
        if (!m_CanInput) return;
        if (CurrentState == RotationState.TARGETING || CurrentState == RotationState.FOLLOWING) return;
        if (isProcessingZoom) return;

        // 모바일 핀치 줌
        if (Input.touchCount == 2)
        {
            HandlePinchZoom();
            return;
        }

#if UNITY_EDITOR
        // 마우스 휠
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f) ZoomCamera(-scroll * zoomSpeed * 5f);
#endif

        // 드래그 및 클릭 처리
        if (Input.GetMouseButtonDown(0)) StartDrag();
        if (Input.GetMouseButton(0)) UpdateDrag();
        if (Input.GetMouseButtonUp(0)) EndDrag();
    }
    private void StartDrag()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        wasInertiaStopping = (CurrentState == RotationState.INERTIA);
        CurrentState = RotationState.DRAGGING;
        lastMousePosition = Input.mousePosition;
        dragOrigin = Input.mousePosition;
        currentInertiaSpeed = 0;
    }
    private void UpdateDrag()
    {
        if (CurrentState != RotationState.DRAGGING) return;
        if (Input.touchCount >= 2) return;

        Vector3 delta = Input.mousePosition - lastMousePosition;
        if (delta.sqrMagnitude > 0.01f)
        {
            Vector3 rotationVector = new Vector3(delta.y, -delta.x, 0f);
            float rotationAngle = rotationVector.magnitude * rotationSpeed * Time.deltaTime;
            Vector3 axis = rotationVector.normalized;

            transform.Rotate(axis, rotationAngle, Space.World);
            inertiaRotationAxis = axis;
            currentInertiaSpeed = delta.magnitude / Time.deltaTime * 0.01f;
        }
        lastMousePosition = Input.mousePosition;
    }
    private void EndDrag()
    {
        if (CurrentState != RotationState.DRAGGING) return;

        float dragDistance = (Input.mousePosition - dragOrigin).magnitude;
        if (dragDistance < CLICK_THRESHOLD)
        {
            if (wasInertiaStopping) CurrentState = RotationState.STOPPED;
            else PerformRayCast();
        }
        else
        {
            if (currentInertiaSpeed > minInertialSpeed) CurrentState = RotationState.INERTIA;
            else CurrentState = RotationState.STOPPED;
        }
    }
    private void PerformRayCast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, placeLayerMask))
        {
            if (hit.transform.CompareTag("Place"))
            {
                m_OnPlaceSelected?.RaiseEvent(hit.transform);
                Debug.Log($"[RotationController] Place Selected: {hit.transform.name}");
            }
        }
        else CurrentState = RotationState.STOPPED;
    }
    private void CalculateTargetRotation(Transform target)
    {
        Vector3 camToEarth = transform.position - mainCamera.position;
        Vector3 targetLocalForward = transform.InverseTransformDirection(target.position - transform.position);
        Vector3 targetLocalUp = transform.InverseTransformDirection(target.up);

        Quaternion targetWorldRot = Quaternion.LookRotation(-camToEarth, mainCamera.up);
        Quaternion targetLocalRot = Quaternion.LookRotation(targetLocalForward, targetLocalUp);

        targetRotation = targetWorldRot * Quaternion.Inverse(targetLocalRot);
    }
    private void HandleStateLogic()
    {
        if (CurrentState == RotationState.TARGETING)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
            Vector3 targetCameraPosition = transform.position - mainCamera.forward * targetZoomDistance;
            mainCamera.position = Vector3.Lerp(mainCamera.position, targetCameraPosition, Time.deltaTime * transitionSpeed);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                CurrentState = RotationState.STOPPED;
            }
        }
        else if (CurrentState == RotationState.FOLLOWING)
        {
            if (followingTarget != null)
            {
                CalculateTargetRotation(followingTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
                Vector3 targetCameraPosition = transform.position - mainCamera.forward * targetZoomDistance;
                mainCamera.position = Vector3.Lerp(mainCamera.position, targetCameraPosition, Time.deltaTime * transitionSpeed);
            }
        }
        else if (CurrentState == RotationState.INERTIA)
        {
            currentInertiaSpeed -= inertiaDamping * Time.deltaTime;
            if (currentInertiaSpeed < minInertiaSpeed)
            {
                currentInertiaSpeed = 0f;
                CurrentState = RotationState.STOPPED;
            }
            else
            {
                transform.Rotate(inertiaRotationAxis, currentInertiaSpeed * rotationSpeed * Time.deltaTime, Space.World);
            }
        }
    }
    private void ZoomCamera(float increment) => targetZoomDistance = Mathf.Clamp(targetZoomDistance + increment, minZoomDistance, maxZoomDistance);
    private void HandleSmoothZoom()
    {
        float currentDist = Vector3.Distance(mainCamera.position, transform.position);
        if (Mathf.Abs(currentDist - targetZoomDistance) < 0.01f) return;

        float nextDist = Mathf.Lerp(currentDist, targetZoomDistance, Time.deltaTime * zoomDamping);
        mainCamera.position = transform.position + (mainCamera.position - transform.position).normalized * nextDist;
    }
    private void HandlePinchZoom()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);
        float prevMag = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
        float currentMag = (t0.position - t1.position).magnitude;
        float diff = prevMag - currentMag;

        if (Mathf.Abs(diff) > zoomThreshold)
        {
            ZoomCamera(-diff * zoomSpeed * 0.05f);
            isProcessingZoom = true;
            Invoke(nameof(ResetZoomFlag), zoomInputCooldown);
        }
    }
    private void ResetZoomFlag() => isProcessingZoom = false;
}