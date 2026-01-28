using UnityEngine;


public class GenericSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_instance;
    public static T Instance
    {
        get
        {
            if (m_instance == null)
                m_instance = FindFirstObjectByType<T>();
            return m_instance;
        }
    }

    protected virtual void Awake()
    {
        if (m_instance != null && m_instance != this)
            Destroy(gameObject);
        else
            m_instance = this as T;

        DontDestroyOnLoad(gameObject);
    }
}