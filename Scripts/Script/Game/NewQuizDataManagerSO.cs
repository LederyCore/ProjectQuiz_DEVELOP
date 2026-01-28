using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuizDataManager", menuName = "ManagerSO/NewQuizDataManager")]
public class NewQuizDataManagerSO : ManagerSO
{
    [Space(10), Header("----- Repository Inject -----")]
    [SerializeField] private EventRepository m_eventDataRepository;
    [SerializeField] private PlaceRepository m_placeDataRepository;

    [Space(10), Header("----- Event Channels (Listen) -----")]
    [SerializeField] private UnityObjectEventChannelSO m_OnStartQuizEvent;
    [SerializeField] private TravelStartEventChannelSO m_OnRequestReVisitPlace;

    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private CSharpObjectEventChannelSO m_OnRequestQuizSequence;

    [Space(10), Header("----- Quiz Setting -----")]
    [SerializeField] private int m_TotalQuizCountPerSession = 10;

    [Space(10), Header("----- Runtime Quiz Data -----")]
    [SerializeField] private List<CardDataSO> m_QuizDeck;

    
    public override void Init()
    {
        m_QuizDeck = new List<CardDataSO>();
        m_OnStartQuizEvent.OnEventRaised += HandleOnStartQuizEvent;
        m_OnRequestReVisitPlace.OnEventRaised += HandleOnStartQuizEvent;

        Debug.Log($"<color=#FF8000>[{GetType().Name}]</color> Initialized.");
    }
    public override void Destroy()
    {
        m_OnStartQuizEvent.OnEventRaised -= HandleOnStartQuizEvent;
        m_OnRequestReVisitPlace.OnEventRaised -= HandleOnStartQuizEvent;
        m_QuizDeck.Clear();
    }

    private void HandleOnStartQuizEvent(UnityEngine.Object arg0)
    {
        if (arg0 == null || arg0 is not PlaceDataSO)
        {
            Debug.LogWarning($"<color=#FF8000>[{GetType().Name}]</color> 퀴즈 시작 이벤트 수신 시 전달된 인자가 null 이거나 PlaceDataSO가 아닙니다.");
            return;
        }
        Debug.Log($"<color=#FF8000>[{GetType().Name}]</color> 퀴즈 시작 이벤트를 수신받았습니다 called with argument: {arg0}");

        // 전체 퀴즈 풀 생성
        PrepareQuizList(arg0 as PlaceDataSO);
        
        // 전체 풀에서 설정된 개수만큼 랜덤 추출
        List<CardDataSO> selectedQuizDeck = GetRandomQuizzes(m_TotalQuizCountPerSession);

        var quizData = new QuizData
        {
            QuizDeck = selectedQuizDeck,
            PlaceData = arg0 as PlaceDataSO,
            TotalQuizCountPerSession = m_TotalQuizCountPerSession,
        };
        m_OnRequestQuizSequence.RaiseEvent(quizData);
    }

    private void HandleOnStartQuizEvent(TravelData arg0)
    {
        Debug.Log($"<color=#FF8000>[{GetType().Name}]</color> 퀴즈 시작 이벤트를 수신받았습니다 called with argument: {arg0}");
        PrepareQuizList(arg0.PlaceData);
        // 전체 풀에서 설정된 개수만큼 랜덤 추출
        List<CardDataSO> selectedQuizDeck = GetRandomQuizzes(m_TotalQuizCountPerSession);
        var quizData = new QuizData
        {
            QuizDeck = selectedQuizDeck,
            PlaceData = arg0.PlaceData,
            TotalQuizCountPerSession = m_TotalQuizCountPerSession
        };
        m_OnRequestQuizSequence.RaiseEvent(quizData);
    }

    private List<CardDataSO> GetRandomQuizzes(int count)
    {
        // 셔플 알고리즘 (Fisher-Yates) 또는 LINQ 활용
        List<CardDataSO> randomDeck = new List<CardDataSO>(m_QuizDeck);

        // 퀴즈 풀이 요청량보다 적을 경우를 대비한 클램핑
        int targetCount = Mathf.Min(count, randomDeck.Count);

        // 랜덤 셔플
        for (int i = 0; i < randomDeck.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, randomDeck.Count);
            CardDataSO temp = randomDeck[i];
            randomDeck[i] = randomDeck[randomIndex];
            randomDeck[randomIndex] = temp;
        }

        // 앞에서부터 targetCount만큼만 잘라서 반환
        return randomDeck.GetRange(0, targetCount);
    }

    private void PrepareQuizList(PlaceDataSO data)
    {
        m_QuizDeck.Clear();
        m_QuizDeck.AddRange(data.placeQuizDeck);

        // 추가로, 나라 데이터가 지정되었을 경우 나라별 퀴즈를 공통 퀴즈 덱에 추가로 채움
        if (!string.IsNullOrEmpty(data.countryID))
        {
            CountryDataSO countryData = m_placeDataRepository.GetCountryByCountryID(data.countryID);
            if (countryData != null && countryData.countryQuizDeck != null)
                m_QuizDeck.AddRange(countryData.countryQuizDeck);
        }
    }
}

[Serializable]
public struct QuizData
{
    public List<CardDataSO> QuizDeck;
    public PlaceDataSO PlaceData;
    public int TotalQuizCountPerSession;
}