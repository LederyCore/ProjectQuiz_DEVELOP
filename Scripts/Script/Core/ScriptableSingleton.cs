using UnityEngine;

public abstract class ScriptableSingleton<T> : ScriptableSingletonBase where T : ScriptableSingleton<T>
{
    private static T m_instance;
    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                T[] asset = Resources.LoadAll<T>("");
                if (asset == null || asset.Length < 1)
                {
                    throw new System.Exception($"Could not find {typeof(T).Name} in Resources.");
                }
                
                m_instance = asset[0];
            }
            return m_instance;
        }
    }
}