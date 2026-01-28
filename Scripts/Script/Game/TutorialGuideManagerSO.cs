using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialGuideManagerSO", menuName = "Game/TutorialGuideManagerSO")]
public class TutorialGuideManagerSO : ScriptableObject
{
    [Header("----- Tutorial Guide Settings -----")]
    [SerializeField] private List<string> m_guidePlaces = new();

    [Header("----- Event Channels (Listen) -----")]
    [SerializeField] private CSharpObjectEventChannelSO m_OnChangedTravelLog;

    [Header("----- Event Channels (Publish) -----")]
    [SerializeField] private IntEventChannelSO m_OnRequestChangeIsGuideFinished;

    // 런타임 상태 관리
    // LinkedList 대신 특정 요소 제거가 직관적인 List 사용
    private List<string> m_remainingGuides = new();

    // 마지막으로 방문한 장소의 원본 리스트 내 인덱스 (순환 탐색용)
    private int m_lastVisitedIndex = -1;

    private Dictionary<string, PinIndicator> m_pinMap = new();

    public void StartTutorial(List<Transform> places)
    {
        InitializePinMap(places);

        // 리스트 복사 (남은 방문지 관리용)
        m_remainingGuides = new List<string>(m_guidePlaces);
        m_lastVisitedIndex = -1; // 초기화: 아직 아무것도 방문 안함

        m_OnChangedTravelLog.OnEventRaised += CheckTutorialProgress;

        // 최초 시작: 모든 가이드 핀 해제
        SetInitialGuideState();
    }

    public void LoadTutorial(List<Transform> places, PlayerData data)
    {
        InitializePinMap(places);
        m_remainingGuides = new List<string>(m_guidePlaces);
        m_lastVisitedIndex = -1;
        m_OnChangedTravelLog.OnEventRaised += CheckTutorialProgress;

        // 저장된 로그를 순회하며 방문한 곳만 '콕 집어서' 제거
        foreach (var log in data.TravelLog)
        {
            if (m_remainingGuides.Contains(log))
            {
                m_remainingGuides.Remove(log);

                // 방문한 장소의 원본 인덱스 갱신 (마지막 방문 위치 기억)
                UpdateLastVisitedIndex(log);
            }
        }

        if (m_remainingGuides.Count == 0)
        {
            CompleteTutorial();
        }
        else
        {
            // 방문 기록이 하나라도 있다면 진행 상태로 업데이트
            // (만약 아무것도 방문 안했다면 StartTutorial과 동일하게 전체가 열려있어야 하는지, 
            // 아니면 첫번째부터 가리켜야 하는지에 따라 다르지만, 
            // 보통 로드 시에는 진행 중인 상태인 UpdateProgressState를 호출하는 것이 안전합니다.)
            // 단, 방문 기록이 아예 없다면 초기 상태(전체 오픈)를 유지해야 할 수도 있으므로 조건 추가
            if (m_remainingGuides.Count == m_guidePlaces.Count)
                SetInitialGuideState();
            else
                UpdateProgressState();
        }
    }

    private void CheckTutorialProgress(object obj)
    {
        if (obj is not List<string> logs || logs.Count == 0) return;
        if (m_remainingGuides.Count == 0) return;

        string latestLog = logs[^1];

        // 이번에 방문한 곳이 가이드 목록(남은 목록)에 있는가?
        if (m_remainingGuides.Contains(latestLog))
        {
            // 1. 남은 목록에서 해당 장소만 제거 (앞의 장소를 지우지 않음)
            m_remainingGuides.Remove(latestLog);

            // 2. 마지막 방문 위치 인덱스 갱신
            UpdateLastVisitedIndex(latestLog);

            // 3. 완료 여부 체크
            if (m_remainingGuides.Count == 0)
            {
                CompleteTutorial();
            }
            else
            {
                UpdateProgressState();
            }
        }
    }

    // --- Helper Logic ---

    private void UpdateLastVisitedIndex(string placeName)
    {
        // 원본 리스트에서 인덱스를 찾아 저장 (순환 계산을 위해)
        int idx = m_guidePlaces.IndexOf(placeName);
        if (idx != -1)
        {
            m_lastVisitedIndex = idx;
        }
    }

    /// <summary>
    /// 순환 구조로 다음 타겟 찾기
    /// 마지막 방문 인덱스부터 시작해서 리스트를 돌며, 아직 '남은 목록'에 있는 첫 번째 장소를 찾음
    /// </summary>
    private string GetNextTargetCyclic()
    {
        if (m_remainingGuides.Count == 0) return null;

        // 마지막 방문 위치 다음부터 탐색 시작
        int count = m_guidePlaces.Count;
        int startIdx = (m_lastVisitedIndex + 1) % count;

        // 원본 리스트를 순환하며 탐색
        for (int i = 0; i < count; i++)
        {
            int currentIdx = (startIdx + i) % count;
            string placeName = m_guidePlaces[currentIdx];

            // 이 장소가 아직 안 가본 곳(남은 리스트에 존재)이라면 이게 바로 다음 타겟
            if (m_remainingGuides.Contains(placeName))
            {
                return placeName;
            }
        }
        return null;
    }

    // --- Visual Updates ---

    private void InitializePinMap(List<Transform> places)
    {
        m_pinMap.Clear();
        foreach (var root in places)
        {
            var pinIndicator = root.GetComponentInChildren<PinIndicator>();
            if (pinIndicator != null)
            {
                if (!m_pinMap.ContainsKey(root.name))
                    m_pinMap.Add(root.name, pinIndicator);
            }
        }
    }

    private void SetInitialGuideState()
    {
        foreach (var kvp in m_pinMap)
        {
            // 가이드 장소면 켜고(잠금해제), 아니면 끔
            if (m_guidePlaces.Contains(kvp.Key))
            {
                kvp.Value.SetPinActive(true);
                kvp.Value.SetPinLocked(false);
            }
            else
            {
                kvp.Value.SetPinActive(false);
            }
        }
    }

    private void UpdateProgressState()
    {
        string nextTarget = GetNextTargetCyclic();

        foreach (var kvp in m_pinMap)
        {
            string placeName = kvp.Key;
            PinIndicator pin = kvp.Value;

            if (placeName == nextTarget)
            {
                // 다음 타겟: 활성 & 잠금해제
                pin.SetPinActive(true);
                pin.SetPinLocked(false);
            }
            else if (m_remainingGuides.Contains(placeName))
            {
                // 아직 방문 안 했지만 지금 갈 차례는 아님: 활성 & 잠금
                pin.SetPinActive(true);
                pin.SetPinLocked(true);
            }
            else
            {
                // 이미 방문했거나(remaining에 없음), 애초에 가이드 장소가 아님: 비활성
                pin.SetPinActive(false);
            }
        }
    }

    private void CompleteTutorial()
    {
        Debug.Log("<color=cyan>[TutorialGuideManagerSO]</color> 튜토리얼 가이드 완료.");
        m_OnChangedTravelLog.OnEventRaised -= CheckTutorialProgress;
        m_OnRequestChangeIsGuideFinished.RaiseEvent(1);

        foreach (var pin in m_pinMap.Values)
        {
            pin.SetPinActive(true);
            pin.SetPinLocked(false);
        }

        m_pinMap.Clear();
        m_remainingGuides.Clear();
    }
}