using UnityEngine;

public abstract class ManagerSO : DescriptionSO
{
    [Header("Base Settings")]
    [SerializeField]
    [Tooltip("체크 시 ManagersBootstraper의 초기화 리스트에 포함되어 게임 시작 시 Init()이 호출됩니다. " +
             "런타임에 시스템을 완전히 제외하고 테스트하고 싶을 때 사용하세요.")]
    private bool m_isEnabled = true;

    public bool IsEnabled => m_isEnabled;

    private bool m_isInitialized = false;
    /// <summary>
    /// 현재 매니저가 Init()을 거쳐 정상적으로 가동 중인지 나타냅니다.
    /// </summary>
    public bool IsInitialized => m_isInitialized;

    private void Awake()
    {
        m_isInitialized = false;
    }
    public abstract void Init();    // Awake 중에 호출됩니다.
    public abstract void Destroy();
    

    /// <summary>
    /// 부트스트래퍼에 의해 호출되어 매니저의 생명주기를 수동으로 제어합니다.
    /// </summary>
    public void SetActive(bool active)
    {
        if (active && !m_isInitialized)
        {
            Init();
            m_isInitialized = true;
            Debug.Log($"[{GetType().Name}] : 초기화 되었습니다.");
        }
        else if (!active && m_isInitialized)
        {
            Destroy();
            m_isInitialized = false;
            Debug.Log($"[{GetType().Name}] : 파괴 되었습니다.");
        }
    }
}