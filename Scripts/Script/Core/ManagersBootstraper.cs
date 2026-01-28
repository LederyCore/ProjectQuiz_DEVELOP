using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagersBootstraper : GenericSingleton<ManagersBootstraper>
{
    [SerializeField] private List<ManagerSO> m_ManagersSO;
    [SerializeField] private GameManagerSO m_GameManagerSO;
    [SerializeField] private bool m_IsCompleteLoadData;
    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private VoidEventChannelSO m_OnApplicationPauseOrQuit;

    // 에디터 툴에서 접근하기 위해 필요한 변수들입니다.
    // HideInInspector를 사용하여 평소에는 숨기고 에디터 스크립트에서만 제어합니다.
    [Header("Editor Debug Tool"), HideInInspector]
    [SerializeField] private ManagerSO m_TestTargetManager;
    [HideInInspector]
    [SerializeField] private ManagerSO m_NewManagerSO;

    private Scene m_currentScene;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        Application.targetFrameRate = 60;

        m_currentScene = SceneManager.GetActiveScene();
    }

    private void Start()
    {
        foreach (var manager in m_ManagersSO)
        {
            InitializeManager(manager);
        }
    }

    private void OnEnable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }
        for (int i = m_ManagersSO.Count - 1; i >= 0; i--)
        {
            if (m_ManagersSO[i] != null && m_ManagersSO[i].IsInitialized)
            {
                m_ManagersSO[i].SetActive(false);
            }
        }
        m_GameManagerSO.Destroy();
    }
    private void OnApplicationQuit() => m_OnApplicationPauseOrQuit?.RaiseEvent();

    private void OnApplicationPause(bool pause)
    {
#if UNITY_EDITOR
#else
        if (pause) m_OnApplicationPauseOrQuit?.RaiseEvent();
#endif
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (m_currentScene != scene)
        {
            m_currentScene = scene;
            ExecuteGameStart();
        }
    }

    private void ExecuteGameStart()
    {
        if (m_GameManagerSO != null)
        {
            m_GameManagerSO.GameStart();
        }
    }

    public void HandleOnCompleteLoadData()
    {
        m_IsCompleteLoadData = true;
        ExecuteGameStart();
    }

    // 에디터 리플렉션에서 찾을 수 있도록 private 유지
    private void InitializeManager(ManagerSO manager)
    {
        if (manager != null)
        {
            manager.SetActive(true);
        }
    }

    // 에디터 툴에서 호출하는 제네릭 교체 메서드
    public void ReplaceManager<T>(T newManagerSO) where T : ManagerSO
    {
        if (newManagerSO == null) return;

        int index = m_ManagersSO.FindIndex(m => m is T);

        if (index != -1)
        {
            if (m_ManagersSO[index].IsInitialized)
            {
                m_ManagersSO[index].SetActive(false);
            }

            m_ManagersSO[index] = newManagerSO;
            InitializeManager(newManagerSO);
            Debug.Log($"<color=yellow>[Managers]</color> {typeof(T).Name} replaced with {newManagerSO.name}.");
        }
        else
        {
            m_ManagersSO.Add(newManagerSO);
            InitializeManager(newManagerSO);
        }
    }
}