using Google;
using UnityEngine;
using System;
using TMPro;

public class LoginTask : ITask
{
    private AwaitableCompletionSource<bool> _completionSource;
    private TextMeshProUGUI m_text;
    private GoogleLogin _googleLogin;
    private GoogleSignInUser _mUser;
    private UIPopupBase m_popup;


    public LoginTask(TextMeshProUGUI text, GoogleLogin login, UIPopupBase popup)
    {
        m_text = text;
        _googleLogin = login;
        m_popup = popup;
    }

    public async Awaitable<bool> ExecuteAsync()
    {
        if (_googleLogin == null)
        {
            Debug.LogError("GoogleLogin 인스턴스가 null입니다.");
            return false;
        }

        _completionSource = new AwaitableCompletionSource<bool>();
        _googleLogin.onLoginSuccess.AddListener(HandleGoogleLoginSuccess);

        try
        {
            m_text.text = "로그인 중...";
            _googleLogin.OnSignIn();

            // 1. Google 로그인 완료 대기
            bool isGoogleLoginSuccess = await _completionSource.Awaitable;

            Debug.Log("Google Login Success");

            if (!isGoogleLoginSuccess || _mUser == null) return false;

            // 2. 서버 로그인 시도 (요청 클래스 생성)
            var dto = new ServerLoginRequest
            {
                user_id = _mUser.UserId ?? "test1",
                display_name = _mUser.DisplayName ?? "test",
                email = _mUser.Email ?? "test",
                device = SystemInfo.deviceUniqueIdentifier,
                culture = Application.systemLanguage.ToString()
            };

            return await SendLoginRequestToServer(dto);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LoginTask] 치명적 오류: {ex.Message}");
            return false;
        }
        finally
        {
            _googleLogin.onLoginSuccess.RemoveListener(HandleGoogleLoginSuccess);
        }
    }

    private async Awaitable<bool> SendLoginRequestToServer(ServerLoginRequest dto)
    {
        var result = await WebRequestManager.PostAsync<ServerLoginRequest, ServerLoginResponse>(API.URL + API.TRY_LOGIN, dto);

        if (result.Data.success)
        {
            Debug.Log($"{GetType()}::3) 유저 인증 완료");
            ServerManagerSO.UserId = dto.user_id;
            Debug.Log($"User ID: {ServerManagerSO.UserId}");
            return true;
        }
        else
        {
            m_popup.Setup(
                title: "유저 인증 실패",
                content: $"유저 인증에 실패했습니다",
                onClose: () => { Application.Quit(); });
            return false;
        }
    }

    public void HandleGoogleLoginSuccess(GoogleSignInUser user)
    {
        if (user != null)
        {
            _mUser = user;
            _completionSource?.TrySetResult(true);
        }
        else
        {
            _completionSource?.TrySetResult(false);
        }
    }
}

[Serializable]
public class ServerLoginRequest
{
    public string user_id;
    public string display_name;
    public string email;
    public string device;
    public string culture;
}

[Serializable] 
public class ServerLoginResponse
{
    public bool success;
    public int user_id;
    public string message;
}