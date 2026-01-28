using UnityEngine;
using System;
using TMPro;
using System.Net;

/*
 * File : CheckServerStatusTask.cs
 * Desc : 서버 상태를 조회하고 상태에 따른 결과를 반환하는 비즈니스 로직 캡슐화 클래스
 * 
 * Functions :
 * [Public]
 * : ExecuteAsync() - 서버 상태를 비동기로 조회하고, 결과 메시지와 성공 여부를 튜플로 반환합니다.
 *                    서버 상태에 따라 UI 팝업도 생성합니다.
 */



public class CheckServerStatusTask : ITask
{
    private UIPopupBase m_Popup;
    private TextMeshProUGUI m_Text;
    public CheckServerStatusTask(UIPopupBase popup, TextMeshProUGUI text)
    {
        m_Popup = popup;
        m_Text = text;
    }

    public async Awaitable<bool> ExecuteAsync()
    {
        try
        {
            m_Text.text = "서버 연결 중...";
            Debug.Log($"{GetType()}::1) 서버 연결 체크 프로세스 시작");
            var result = await WebRequestManager.GetAsync<ServerCheckResponse>(API.URL + API.CHECK_SERVER_STATUS);
            switch (result.Data.status)
            {
                case 0: // 서버 접속 성공
                    Debug.Log($"{GetType()}::1) 서버 연결 체크 프로세스 완료 - 성공");
                    return true;
                case 1: // 서버 접속 실패 (오프라인)
                    m_Popup.Setup(
                        title: "서버 접속 실패",
                        content: result.Data.message,
                        onClose: () => Application.Quit());
                    m_Text.text = "서버 접속 실패";
                    m_Popup.gameObject.SetActive(true);
                    Debug.Log($"{GetType()}::1) 서버 연결 체크 프로세스 완료 - 실패");
                    return false;
                case 2: // 서버 접속 실패 (점검중)
                    m_Popup.Setup(
                        title: "서버 접속 실패",
                        content: result.Data.message,
                        onClose: () => Application.Quit());
                    m_Text.text = "서버 접속 실패";
                    m_Popup.gameObject.SetActive(true);
                    Debug.Log($"{GetType()}::1) 서버 연결 체크 프로세스 완료 - 실패");
                    return false;
                default: // 서버 접속 실패 (알수없는 이유)
                    m_Popup.Setup(
                        title: "서버 접속 실패",
                        content: "알 수 없는 이유로 서버 접속에 실패했습니다. 잠시 후 다시 시도해주세요.",
                        onClose: () => Application.Quit());
                    m_Text.text = "서버 접속 실패";
                    m_Popup.gameObject.SetActive(true);
                    Debug.Log($"{GetType()}::1) 서버 연결 체크 프로세스 완료 - 실패");
                    return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            return false;
        }
    }
}