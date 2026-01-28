using TMPro;
using UnityEngine;

public class DataLoadTask : ITask
{
    private TextMeshProUGUI m_text;

    public DataLoadTask(TextMeshProUGUI text)
    {
        m_text = text;
    }


    public async Awaitable<bool> ExecuteAsync()
    {
        try
        {
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
            return false;
        }

        return true;
    }
}