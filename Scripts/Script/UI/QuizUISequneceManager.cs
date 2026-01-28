using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizUISequneceManager : PlayerdataReceiver
{
    [Header("----- Dependency Injection -----")]
    [SerializeField] private WelcomeCardController m_welcomeCardController;
    [SerializeField] private NewQuizCardController m_quizCardController;
    [SerializeField] private GameObject m_WrongEffectPanel;
    [SerializeField] private SolutionPanelController m_SolutionPanelController;

    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private IntEventChannelSO m_OnRequestChangeCurrentDayProgress;
    [SerializeField] private IntEventChannelSO m_OnRequestChangeTotalCorrectQuizzes;
    [SerializeField] private IntEventChannelSO m_OnRequestChangePlayerMoney;
    [SerializeField] private VoidEventChannelSO m_OnRequestStartMiniGame;

    [Header("----- Quiz Sequnece Setting -----")]
    [SerializeField] private float m_welcomeCardDisplayDelay = 1.2f;
    [SerializeField] private int m_IncorrectAnswerPenalty = 100;

    [Header("----- Runtime Data -----")]
    [SerializeField] private QuizData m_currentQuizData;
    [SerializeField] private CardDataSO m_currentQuizCard;


    public void HandleOnRequestQuizSequence(object data)
    {
        if (data is not QuizData quizData)
        {
            Debug.LogWarning($"<color=#FF8000>[{GetType().Name}]</color> 퀴즈 시퀀스 요청 이벤트 수신 시 전달된 인자가 QuizData가 아닙니다.");
        }
        else
        {
            Debug.Log($"<color=#66FFFF>[{GetType().Name}]</color> 퀴즈 시퀀스 요청 이벤트를 수신받았습니다 called with argument: {quizData}");
            // 퀴즈 시퀀스 시작
            m_currentQuizData = quizData;
            StartCoroutine(StartQuizSequence());
        }
    }

    private IEnumerator StartQuizSequence()
    {
        yield return new WaitForSeconds(m_welcomeCardDisplayDelay);

        bool isWelcomeCardFinished = false;

        // 1. 웰컴 카드 오픈 (콜백으로 플래그를 true로 변경)
        OpenWelcomeCard(m_currentQuizData.PlaceData, () => {
            isWelcomeCardFinished = true;
        });

        // 2. 콜백이 실행될 때까지 대기
        yield return new WaitUntil(() => isWelcomeCardFinished);

        // ShowNextQuiz가 정답 여부(bool)를 받아 SubmittedQuizAnswer를 호출합니다.
        ShowNextQuiz(isCorrect =>
        {
            SubmittedQuizAnswer(isCorrect);
        });
    }

    private void OpenWelcomeCard(PlaceDataSO placeData, Action action)
    {
        m_welcomeCardController.Setup(placeData, action);
        m_welcomeCardController.gameObject.SetActive(true);
    }
    private void ShowNextQuiz(Action<bool> onAnswerSubmitted)
    {
        if (m_currentQuizData.QuizDeck.Count > 0)
        {
            m_currentQuizCard = m_currentQuizData.QuizDeck[UnityEngine.Random.Range(0, m_currentQuizData.QuizDeck.Count)];

            if (m_currentQuizCard.cardType == CardType.Quiz)
            {
                m_quizCardController.gameObject.SetActive(true);
                m_quizCardController.Setup(
                    m_currentQuizCard,
                    m_currentQuizData.PlaceData.regionName_KR,
                    m_currentQuizData.PlaceData.placeName_KR,
                    onAnswerSubmitted
                );
            }
            else ShowNextQuiz(onAnswerSubmitted);
        }
        else
        {
            Debug.Log("모든 퀴즈 완료 시퀀스 진입");
        }
    }
    private void SubmittedQuizAnswer(bool isCorrect)
    {
        m_currentQuizData.QuizDeck.Remove(m_currentQuizCard);
        m_OnRequestChangeCurrentDayProgress.RaiseEvent(1);

        if (isCorrect)
        {
            Debug.Log($"<color=#FF8000>[{GetType().Name}]</color> 정답입니다! 남은 퀴즈 수: {m_currentQuizData.QuizDeck.Count}");
            m_OnRequestChangeTotalCorrectQuizzes.RaiseEvent(1);
            m_quizCardController.CloseCard(() =>
            {
                m_quizCardController.gameObject.SetActive(false);
                ShowSolutionPopup(true);
            });
        }
        else
        {
            m_OnRequestChangePlayerMoney.RaiseEvent(-m_IncorrectAnswerPenalty);
            StartCoroutine(WrongAnswerRoutine());
        }
    }
    private IEnumerator WrongAnswerRoutine()
    {
        m_WrongEffectPanel.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        m_WrongEffectPanel.SetActive(false);
        m_quizCardController.CloseCard(() =>
        {
            m_quizCardController.gameObject.SetActive(false);
            CheckQuizProgress();
        });
    }
    private void ShowSolutionPopup(bool isCorrect)
    {
        m_SolutionPanelController.gameObject.SetActive(true);
        string expl = (m_currentQuizCard != null) ? m_currentQuizCard.explanation : "정답 설명이 없습니다";
        m_SolutionPanelController.Setup(isCorrect, expl, () => CheckQuizProgress());
    }
    private void CheckQuizProgress()
    {
        if (m_currentQuizData.QuizDeck.Count == 0)
        {
            Debug.Log($"<color=#FF8000>[{GetType().Name}]</color> 모든 퀴즈가 완료되었습니다. 미니게임을 시작합니다");
            m_OnRequestStartMiniGame.RaiseEvent();
        }
        else
        {
            Debug.Log($"<color=#66FFFF>[{GetType().Name}]</color> 다음 퀴즈로 이동합니다.");
            // 재귀적으로 다음 퀴즈를 부를 때도 똑같이 bool을 받도록 처리
            ShowNextQuiz(isCorrect => SubmittedQuizAnswer(isCorrect));
        }
    }

}