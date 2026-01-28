using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RotationController))]
public class TravelManager : MonoBehaviour
{
    

    [SerializeField] private Transform m_currentPlace;

    [Header("--- Static Data & Scene Refs ---")]
    [SerializeField] private RotationController m_rotationController;
    [SerializeField] private PlaceRepository m_PlaceRepository;
    [SerializeField] private TutorialGuideManagerSO m_TutorialGuideManager;
    [SerializeField] private List<Transform> m_PlaceRoots = new List<Transform>();
    [SerializeField] private GameObject m_Marker;

    [Header("----- Event Channels (Publish) -----")]
    [SerializeField] private TravelStartEventChannelSO m_OnActiveTravelPopup;
    [SerializeField] private TravelStartEventChannelSO m_OnRequestReVisitPlace;
    [SerializeField] private IntEventChannelSO m_OnRequestChangeIsFirstRun;
    [SerializeField] private UnityObjectEventChannelSO m_OnArrivalPlace;
    [SerializeField] private UnityObjectEventChannelSO m_OnRequestAddTravelLog;
    [SerializeField] private GameStateEventChannelSO m_OnRequestChangeGameState;

    [Header("----- Travel Config -----")]
    public float costPerDistanceUnit = 100f;

    private Dictionary<string, PlaceDataSO> m_PlaceDataMap = new();

    private PlayerData m_CurrentPlayerData;
    private GameObject m_MarkerInstance;

    private string nowPlaceName;

    

    private void Awake()
    {
        m_rotationController = GetComponent<RotationController>();
        // 마커 생성 및 참조 저장
        if (m_Marker != null)
        {
            m_MarkerInstance = Instantiate(m_Marker, gameObject.transform);
            m_MarkerInstance.SetActive(false); // 처음에는 비활성화
        }

        InitializeDataFromRepository();
    }

    public void HandleUpdatePlayerData(object obj)
    {
        if (obj is PlayerData data)
        {
            Debug.Log($"<color=cyan>[TravelManager]</color> 플레이어 데이터 수신.");
            m_CurrentPlayerData = data;
            m_currentPlace = GetPlaceTransform(m_CurrentPlayerData.CurrentPlaceID);
            return;
        }
    }

    public void HandleOnStartGameLogic()
    {
        InitializeAllPlacePin();
    }

    public void HandleOnStartTutorial()
    {
        m_TutorialGuideManager.StartTutorial(m_PlaceRoots);
    }

    public void HandleOnLoadTutorial(object obj)
    {
        if (obj is not PlayerData data) return;
        m_TutorialGuideManager.LoadTutorial(m_PlaceRoots, data);
    }

    /// <summary>
    /// 외부 이벤트로 장소를 클릭하여 여행 요청이 들어오면, 현재 플레이어 데이터를 참조하여 여행 비용을 계산하고 팝업을 띄운다.
    /// 게임을 처음 시작하여서 시작 장소가 없거나, 현재 장소와 동일한 장소로 여행 요청이 들어오는 경우도 대응한다.
    /// </summary>
    /// <param name="targetPlace"></param>
    public void HandleTravelRequest(Transform targetPlace)
    {
        if (m_CurrentPlayerData == null) return;

        Transform startPlace = default;
        float distance; 
        int requiredCost = 0;
        int currentMoney;
        string targetName = string.Empty;

        
       

            // 데이터 준비
            if (m_currentPlace != null)
        {
            startPlace = GetPlaceTransform(m_CurrentPlayerData.CurrentPlaceID);
            distance = Vector3.Distance(startPlace.position, targetPlace.position);
            requiredCost = Mathf.RoundToInt(distance * costPerDistanceUnit);
            currentMoney = m_CurrentPlayerData.PlayerMoney;
            targetName = targetPlace.name;

            if (m_PlaceDataMap.TryGetValue(targetPlace.name, out PlaceDataSO data))
            {
                targetName = data.placeName_KR;
            }
        }


        
        Action callBack = delegate
        {
            m_currentPlace.parent.GetComponentInChildren<PinIndicator>(true).SetPinActive(true);
            targetPlace.parent.GetComponentInChildren<PinIndicator>().IconDisable();

        };

        TravelData travelData = new TravelData
        {
            StartPlace = startPlace,
            EndPlace = targetPlace,
            CurrentMoney = m_CurrentPlayerData.PlayerMoney,
            TravelCost = requiredCost,
            PlaceData = m_PlaceRepository.GetPlaceByPlaceID(targetPlace.gameObject.name),
            CountryData = m_PlaceRepository.GetCountryByPlaceID(targetPlace.gameObject.name),
            CallBack = callBack
        };

        // 로직 처리
        // 시작 장소가 없으면 경고 로그 출력 후 종료
        if (startPlace == null)
        {
            Debug.Log($"시작 장소가 할당되지 않았습니다.");
            // 시작 장소가 없으면 targetPlace로 바로 화면을 이동하고, 현재 장소로 설정후 퀴즈 이벤트 발생
            m_OnArrivalPlace.RaiseEvent(travelData.PlaceData);
            m_rotationController.StartTargeting(targetPlace);
            m_OnRequestChangeGameState.RaiseEvent(GameState.TRAVELING);
            return;
        }
        // 여행할 장소와 현재 장소가 같으면 재방문 이벤트 발생
        else if (startPlace.gameObject.name == targetPlace.gameObject.name)
        {
            Debug.Log($"여행할 장소와 현재장소가 같습니다");
            m_OnRequestAddTravelLog.RaiseEvent(travelData.PlaceData);
            m_rotationController.StartTargeting(targetPlace);
            m_OnRequestReVisitPlace.RaiseEvent(travelData);
            m_OnRequestChangeGameState.RaiseEvent(GameState.TRAVELING);
            return;
        }
        // 여행할 장소와 현재 장소가 다르면 여행 팝업 표시 이벤트 발생
        else
        {
            ShowTravelPopup(travelData, targetName, requiredCost);
        }
    }

    public void AlignView()
    {
        if (m_currentPlace == null) return;
        m_rotationController.StartTargeting(m_currentPlace);
    }

    public void HandleOnRequestActiveMarker()
    {
        if (m_MarkerInstance == null || m_currentPlace == null)
        {
            Debug.LogWarning("<color=yellow>[TravelManager]</color> 마커 인스턴스나 현재 장소가 할당되지 않았습니다.");
            return;
        }

        // 1. 마커를 현재 장소의 자식으로 변경
        m_MarkerInstance.transform.SetParent(m_currentPlace);

        // 2. 마커의 위치를 자식 기준 (0, 0, 0)으로 초기화 (장소 정중앙에 배치)
        m_MarkerInstance.transform.localPosition = Vector3.zero;
        m_MarkerInstance.transform.localRotation = Quaternion.identity;

        // 3. 마커 활성화
        m_MarkerInstance.SetActive(true); 

        Debug.Log($"<color=cyan>[TravelManager]</color> 마커 이동 완료: {m_currentPlace.name}");
    }

    public void HandleOnRequestInActiveMarker()
    {
        m_MarkerInstance.gameObject.SetActive(false);
    }

    public Transform GetPlaceTransform(string placeID)
    {
        if (string.IsNullOrEmpty(placeID)) return null;
        foreach (var root in m_PlaceRoots)
        {
            if (root != null && root.name.Contains(placeID))
            {
                foreach (Transform child in root)
                    if (child.name.Contains(placeID))
                    {
                        nowPlaceName = root.name;
                        return child;
                    }
            }
        }
        return null;
    }

    private void ShowTravelPopup(TravelData data, string name, int cost)
    {
        Debug.Log($"<color=cyan>[TravelManager]</color> 여행 팝업 장소 : {name} 금액: {cost})");
        m_OnRequestChangeGameState.RaiseEvent(GameState.EVENT);
        
        m_OnActiveTravelPopup?.RaiseEvent(data);
    }

    private void InitializeDataFromRepository()
    {
        if (m_PlaceRepository == null) return;
        m_PlaceDataMap.Clear();
        foreach (var p in m_PlaceRepository.Places)
        {
            if (p != null) m_PlaceDataMap[p.placeID] = p;
        }
    }

    private void InitializeAllPlacePin()
    {
        foreach (var root in m_PlaceRoots)
        {
            if (!root.name.Contains(nowPlaceName))
            {
                var pinIndicator = root.GetComponentInChildren<PinIndicator>();
                if (pinIndicator != null)
                {
                    pinIndicator.SetPinActive(true);
                }
            }
            else
            {
                var pinIndicator = root.GetComponentInChildren<PinIndicator>();
                if (pinIndicator != null)
                {
                    pinIndicator.IconDisable();
                }
            }

            
        }
    }
}