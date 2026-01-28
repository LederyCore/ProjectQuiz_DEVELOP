using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CheckLatestVersionTask : ITask
{
    private TextMeshProUGUI m_text;
    private UIPopupBase m_popup;

    public CheckLatestVersionTask(UIPopupBase popup = null, TextMeshProUGUI text = null)
    {
        m_text = text;
        m_popup = popup;
    }

    public async Awaitable<bool> ExecuteAsync()
    {
        m_text.text = "버전 체크 중...";
        try
        {
            Debug.Log($"{GetType()}::2) 버전 체크 프로세스 시작");
            var result = await WebRequestManager.GetAsync<CheckLatestVersionResponse>(API.URL + API.CHECK_LATEST_VERSION);

            if (Application.version != result.Data.latestVersion)
            {
                m_popup.Setup(
                    title: "업데이트 필요",
                    content: "최신 버전이 출시되었습니다. 스토어로 이동하여 업데이트 해주세요.",
                    onClose: () =>
                    {
#if UNITY_ANDROID
                        Application.OpenURL(result.Data.updateUrlAnd);
                        Debug.Log("업데이트가 필요합니다. 마켓으로 이동합니다.");
#elif UNITY_IOS
                        Application.OpenURL(result.Data.updateUrlIos);
                        Debug.Log("업데이트가 필요합니다. 마켓으로 이동합니다.");
#else
#endif
                        Debug.Log("업데이트가 필요합니다. 마켓으로 이동합니다.");
                        Application.Quit();
                    });
                return false;
            }
            m_text.text = "클라이언트 버전 확인 완료";
            Debug.Log($"{GetType()}::2) 버전 체크 프로세스 완료");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
            return false;
        }
    }
}