using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WheelMinigame : MiniGame
{
    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private VoidEventChannelSO m_OnRequestShowAd;  // 광고 재생 요청 이벤트 채널 나중에 추가

    [Header("----- UI 연결 -----")]
    [SerializeField] private Button m_ADBtn;
    [SerializeField] private Button m_SpinBtn;
    [SerializeField] private Button m_CloseBtn;
    [SerializeField] private Image m_SectionTempate;
    [SerializeField] private Transform m_WheelCircle;

    [Header("--- 섹션 데이터 설정 ---")]
    [SerializeField] private List<WheelSection> wheelSections = new List<WheelSection>();

    [Header("--- 회전 설정 ---")]
    [SerializeField] private float spinDuration = 3.0f;
    [SerializeField] private int minSpinCount = 5;

    private bool isSpinning = false;
    private List<Image> spawnedImages = new List<Image>();
    private float[] startAngles;
    private float[] endAngles;
    private int totalWeight = 0;
    private int adSpinCount = 0;

    private const int MAX_AD_SPINS = 1;

    private void OnEnable()
    {
        // 미니게임 시작 시 필요한 초기화 작업 수행
        m_ADBtn.onClick.AddListener(OnClickAdSpin);
        m_SpinBtn.onClick.AddListener(OnClickSpin);
        m_CloseBtn.onClick.AddListener(ClosePanel);

        ResetUI();
    }
    private void Start()
    {
        GenerateWheelVisuals();
        ResetUI();
    }
    private void OnDisable()
    {
        ResetUI();
        RequestChangeGameState(GameState.IDLE);
    }

    [ContextMenu("📷 휠 미리보기 생성 (Generate)")]
    private void GenerateWheelVisuals()
    {
        if (m_SectionTempate == null) return;

        foreach (var img in spawnedImages)
        {
            if (img != null)
            {
                if (Application.isPlaying) Destroy(img.gameObject);
                else DestroyImmediate(img.gameObject);
            }
        }
        spawnedImages.Clear();

        if (m_WheelCircle != null)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in m_WheelCircle) children.Add(child.gameObject);
            foreach (var child in children)
            {
                if (child != m_SectionTempate.gameObject)
                {
                    if (Application.isPlaying) Destroy(child);
                    else DestroyImmediate(child);
                }
            }
        }

        m_SectionTempate.gameObject.SetActive(false);
        if (wheelSections.Count == 0) return;

        totalWeight = 0;
        foreach (var sec in wheelSections) totalWeight += sec.weight;
        if (totalWeight <= 0) return;

        startAngles = new float[wheelSections.Count];
        endAngles = new float[wheelSections.Count];
        float currentAngle = 0f;

        for (int i = 0; i < wheelSections.Count; i++)
        {
            WheelSection data = wheelSections[i];
            float ratio = (float)data.weight / (float)totalWeight;
            float angleSize = ratio * 360f;

            GameObject newObj = Instantiate(m_SectionTempate.gameObject, m_WheelCircle);
            newObj.name = $"Section_{i}_{data.sectionName}";
            newObj.SetActive(true);

            Image newImg = newObj.GetComponent<Image>();
            newImg.sprite = m_SectionTempate.sprite;
            newImg.type = Image.Type.Filled;
            newImg.fillMethod = Image.FillMethod.Radial360;
            newImg.fillOrigin = (int)Image.Origin360.Top;
            newImg.fillClockwise = true;
            newImg.color = data.sectionColor;
            newImg.fillAmount = ratio;

            newObj.transform.localRotation = Quaternion.Euler(0, 0, -currentAngle);
            spawnedImages.Add(newImg);

            startAngles[i] = currentAngle;
            endAngles[i] = currentAngle + angleSize;
            currentAngle += angleSize;
        }
    }
    private void ResetUI()
    {
        adSpinCount = 0;
        isSpinning = false;

        m_SpinBtn.gameObject.SetActive(true);
        m_ADBtn.gameObject.SetActive(true);
        m_CloseBtn.gameObject.SetActive(true);

        SetButtonState(m_SpinBtn, true);
        SetButtonState(m_ADBtn, false);
        SetButtonState(m_CloseBtn, false);
    }
    private void OnClickSpin()
    {
        if (isSpinning) return;
        SetAllButtons(false);
        StartCoroutine(SpinRoutine());
    }
    private void OnClickAdSpin()
    {
        if (isSpinning) return;
        if (adSpinCount >= MAX_AD_SPINS) return; // 횟수 초과 시 차단

        adSpinCount++;
        SetAllButtons(false);
        // 광고 재생 요청 (광고 재생 후 콜백에서 SpinRoutine 호출하도록 변경 가능)
        // TODO: 광고 시스템 연동 필요
        // 지금을 일단 한번 더돌림
        OnClickSpin();
        m_OnRequestShowAd?.RaiseEvent();
    }
    private IEnumerator SpinRoutine()
    {
        if (wheelSections.Count == 0 || totalWeight <= 0) yield break;
        isSpinning = true;

        int selectedIndex = GetRandomSectionIndex();
        WheelSection selectedData = wheelSections[selectedIndex];

        float centerAngle = (startAngles[selectedIndex] + endAngles[selectedIndex]) / 2f;
        float range = (endAngles[selectedIndex] - startAngles[selectedIndex]) * 0.4f;
        float randomOffset = UnityEngine.Random.Range(-range, range);
        float finalAngle = (minSpinCount * 360f) + centerAngle + randomOffset;

        float timer = 0f;
        float startZ = m_WheelCircle.eulerAngles.z;

        while (timer < spinDuration)
        {
            timer += Time.deltaTime;
            float t = timer / spinDuration;
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            float currentAngle = Mathf.Lerp(0, finalAngle, easeOut);
            m_WheelCircle.rotation = Quaternion.Euler(0, 0, startZ - currentAngle);
            yield return null;
        }

        isSpinning = false;
        yield return new WaitForSeconds(0.5f);
        ApplyResult(selectedData);
    }
    private void ApplyResult(WheelSection data)
    {
        Debug.Log($"🎯 결과: {data.sectionName} ({data.actionType})");
        EventDataRepository.ExecuteStrategy(data.actionType);
        if (adSpinCount < MAX_AD_SPINS)
        {
            HandleFailState();
        }
        else
        {
            ClosePanel();
        }
    }

    private void HandleFailState()
    {
        SetButtonState(m_SpinBtn, false);

        // 2. 광고 스핀 버튼: 횟수 남았으면 활성화!
        if (adSpinCount < MAX_AD_SPINS)
        {
            SetButtonState(m_ADBtn, true);
        }
        else
        {
            SetButtonState(m_ADBtn, false);
        }

        // 3. 나가기 버튼 활성화 (재도전 안하고 나갈 수도 있으니까)
        SetButtonState(m_CloseBtn, true);
    }

    private int GetRandomSectionIndex()
    {
        int rnd = UnityEngine.Random.Range(0, totalWeight);
        int current = 0;
        for (int i = 0; i < wheelSections.Count; i++)
        {
            current += wheelSections[i].weight;
            if (rnd < current) return i;
        }
        return wheelSections.Count - 1;
    }
    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
    private void SetButtonState(Button btn, bool isInteractable) => btn.interactable = isInteractable;
    private void SetAllButtons(bool v)
    {
        SetButtonState(m_SpinBtn, v);
        SetButtonState(m_ADBtn, v);
        SetButtonState(m_CloseBtn, v);
    }
}



[System.Serializable]
public class WheelSection
{
    public string sectionName;
    public EventActionType actionType;
    [Range(1, 100)] 
    public int weight = 10;
    public Color sectionColor = Color.white;
}

