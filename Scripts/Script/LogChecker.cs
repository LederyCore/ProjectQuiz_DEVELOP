using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogChecker : MonoBehaviour
{
    float deltaTime = 0.0f;

    GUIStyle style;
    Rect rect;
    float msec;
    float fps;
    float worstFps = 100f;
    public string text;
    float t;

    
    void Awake()
    {
        int w = Screen.width, h = Screen.height;

        rect = new Rect(0 , h * 4 / 100 , w, h * 4 / 50);

        style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 4 / 400;
        style.normal.textColor = Color.red;

        Application.logMessageReceived += ErrorHandle;
        StartCoroutine(ErrorReset());
    }


    IEnumerator ErrorReset() //코루틴으로 15초 간격으로 최저 프레임 리셋해줌.
    {
        t = 0;
        while (true)
        {
            t += Time.deltaTime;
            yield return null;
            if (t > 15)
            {
                text = "";
                t = 0;
            }
        }
    }


    void ErrorHandle(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                SaveLog(condition, stackTrace);
                break;
            case LogType.Assert:
                break;
            case LogType.Warning:
                break;
            case LogType.Log:
                //SaveLog(condition, stackTrace);
                break;
            case LogType.Exception:
                SaveLog(condition, stackTrace);
                break;
            default:
                break;
        }
    }
    void SaveLog(string condition, string stackTrace)
    {
        t = 0;
        text += condition + "\n" + stackTrace + "\n";
    }


    void OnGUI()//소스로 GUI 표시.
    {
        GUI.Label(rect, text, style);
    }


}
