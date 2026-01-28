using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class WebRequestManager
{
    public class WebResponse<T>
    {
        public bool IsSuccess;
        public T Data;
        public string ErrorMessage;
    }

    public static async Awaitable<WebResponse<T>> GetAsync<T>(string url, string authToken = null)
    {
        using UnityWebRequest request = UnityWebRequest.Get(url);
        SetHeaders(request, authToken);

        try
        {
            await request.SendWebRequest();
            return HandleResponse<T>(request);
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebRequest] Exception in GetAsync: {e.Message}");
            return new WebResponse<T> { IsSuccess = false, ErrorMessage = e.Message };
        }
    }

    public static async Awaitable<WebResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest body, string authToken = null)
    {
        string jsonPayload = JsonUtility.ToJson(body);
        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        SetHeaders(request, authToken);

        try
        {
            await request.SendWebRequest();
            return HandleResponse<TResponse>(request);
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebRequest] Exception in PostAsync: {e.Message}");
            return new WebResponse<TResponse> { IsSuccess = false, ErrorMessage = e.Message };
        }
    }

    private static void SetHeaders(UnityWebRequest request, string authToken)
    {
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(authToken))
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
    }

    private static WebResponse<T> HandleResponse<T>(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            if (string.IsNullOrEmpty(request.downloadHandler.text))
                return new WebResponse<T> { IsSuccess = true };

            try
            {
                T jsonData = JsonUtility.FromJson<T>(request.downloadHandler.text);
                return new WebResponse<T> { IsSuccess = true, Data = jsonData };
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebRequest] JSON Parse Error: {e.Message}");
                return new WebResponse<T> { IsSuccess = false, ErrorMessage = $"JSON Parse Error: {e.Message}" };
            }
        }

        Debug.LogError($"[WebRequest] Error: {request.error} | Response: {request.downloadHandler?.text ?? "No response"}");
        return new WebResponse<T> { IsSuccess = false, ErrorMessage = request.error };
    }
}