using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewQuizCardController : MonoBehaviour
{
    [Header("--- [공통] 문제 정보 ---")]
    public TextMeshProUGUI txtRegion;
    public TextMeshProUGUI txtPlace;
    public Image imgQuestion;
    public TextMeshProUGUI txtQuestion;

    [Header("--- 이미지 없을시 대체 이미지 ---")]
    public Sprite[] defaultImage;

    [Header("--- [패널 1] 4지선다 퀴즈 ---")]
    public GameObject standardPanel;
    public Button[] answerButtons;
    public TextMeshProUGUI[] answerTexts;

    [Header("--- [패널 2] OX 퀴즈 ---")]
    public GameObject oxPanel;
    public Button btnO;
    public Button btnX;

    [Header("--- [신규] 연출 설정 ---")]
    public float animDuration = 0.5f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Action<bool> m_AnswerSubmittedCallback;

    private class TempOption
    {
        public string text;
        public bool isCorrect;
        public TempOption(string t, bool c) { text = t; isCorrect = c; }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(CardDataSO card, string regionName, string placeName, System.Action<bool> callback)
    {
        if (card == null) return;
        txtRegion.text = regionName;
        txtPlace.text = placeName;
        txtQuestion.text = card.question;
        m_AnswerSubmittedCallback = callback;
        if (card.cardImage != null)
        {
            imgQuestion.sprite = card.cardImage;
            imgQuestion.gameObject.SetActive(true);
        }
        else
        {
            imgQuestion.sprite = defaultImage[UnityEngine.Random.Range(0, defaultImage.Length)];
        }

        bool isOXQuiz = (card.correctAnswer == "O" || card.correctAnswer == "X");
        if (isOXQuiz) SetupOXPanel(card);
        else SetupStandardPanel(card);

        StartCoroutine(AnimateEntrance());
    }

    private IEnumerator AnimateEntrance()
    {
        float startX = 1000f;
        rectTransform.anchoredPosition = new Vector2(startX, 0);
        rectTransform.localRotation = Quaternion.Euler(0, 90, 0);
        canvasGroup.alpha = 0f;

        float timer = 0f;
        while (timer < animDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animDuration;
            float tEase = 1f - Mathf.Pow(1f - t, 3f);

            float currentX = Mathf.Lerp(startX, 0f, tEase);
            rectTransform.anchoredPosition = new Vector2(currentX, 0);

            float currentYRot = Mathf.Lerp(90f, 0f, tEase);
            rectTransform.localRotation = Quaternion.Euler(0, currentYRot, 0);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, tEase);

            yield return null;
        }

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        canvasGroup.alpha = 1f;
    }

    private void SetupStandardPanel(CardDataSO card)
    {
        oxPanel.SetActive(false);
        standardPanel.SetActive(true);
        List<TempOption> mixList = new List<TempOption>();
        if (!string.IsNullOrEmpty(card.correctAnswer)) mixList.Add(new TempOption(card.correctAnswer, true));
        if (!string.IsNullOrEmpty(card.wrongAnswer1)) mixList.Add(new TempOption(card.wrongAnswer1, false));
        if (!string.IsNullOrEmpty(card.wrongAnswer2)) mixList.Add(new TempOption(card.wrongAnswer2, false));
        if (!string.IsNullOrEmpty(card.wrongAnswer3)) mixList.Add(new TempOption(card.wrongAnswer3, false));

        for (int i = 0; i < mixList.Count; i++)
        {
            TempOption temp = mixList[i];
            int rand = UnityEngine.Random.Range(i, mixList.Count);
            mixList[i] = mixList[rand];
            mixList[rand] = temp;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].onClick.RemoveAllListeners();
            if (i < mixList.Count)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerTexts[i].text = mixList[i].text;
                bool isCorrect = mixList[i].isCorrect;
                answerButtons[i].onClick.AddListener(() => OnAnswerClicked(isCorrect));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetupOXPanel(CardDataSO card)
    {
        standardPanel.SetActive(false);
        oxPanel.SetActive(true);
        btnO.onClick.RemoveAllListeners();
        btnX.onClick.RemoveAllListeners();
        bool isAnswerO = (card.correctAnswer == "O");
        btnO.onClick.AddListener(() => OnAnswerClicked(isAnswerO));
        btnX.onClick.AddListener(() => OnAnswerClicked(!isAnswerO));
    }

    private void OnAnswerClicked(bool isAnswerO)
    {
        foreach (var btn in answerButtons) btn.interactable = false;
        btnO.interactable = false;
        btnX.interactable = false;

        // 답변 제출 처리
        m_AnswerSubmittedCallback?.Invoke(isAnswerO);

        Invoke(nameof(EnableButtons), 1.0f);
    }

    private void EnableButtons()
    {
        foreach (var btn in answerButtons) btn.interactable = true;
        btnO.interactable = true;
        btnX.interactable = true;
    }

    public void CloseCard(Action onClosed)
    {
        StartCoroutine(AnimateExit(onClosed));
    }

    IEnumerator AnimateExit(System.Action onClosed)
    {
        float startX = 0f;
        float endX = -1000f;

        float timer = 0f;
        while (timer < animDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animDuration;
            float tEase = t * t;

            float currentX = Mathf.Lerp(startX, endX, tEase);
            rectTransform.anchoredPosition = new Vector2(currentX, 0);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, tEase);

            yield return null;
        }
        onClosed?.Invoke();
    }
}