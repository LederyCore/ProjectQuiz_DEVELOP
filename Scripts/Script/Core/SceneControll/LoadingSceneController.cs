using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders; // SceneInstance를 위해 필요
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    public bool m_sessionStart = true;

    [Header("----- UI 연결 -----")]
    public TextMeshProUGUI m_infoText;
    public TextMeshProUGUI m_progressText;
    public Slider m_slider;
    public UIConfirmBase m_confirm;
    public UIPopupBase m_popup;

    public GoogleLogin m_googleLogin;

    private void Awake()
    {
        m_infoText.text = "게임 초기화 중...";
        ExecuteProcess();
    }

    public async void ExecuteProcess()
    {
        await Awaitable.WaitForSecondsAsync(0.5f);
        if (!m_sessionStart) return;

        // 인터페이스 기반 태스크 리스트
        var processes = new List<ITask>
        {
            new CheckServerStatusTask(m_popup, m_infoText),
            new CheckLatestVersionTask(m_popup, m_infoText),
            new LoginTask(m_infoText, m_googleLogin, m_popup),
            new ResourceDownloadTask(m_infoText, m_progressText, m_slider, m_confirm),
            new DataLoadTask(m_infoText)
        };

        foreach (var process in processes)
        {
            var success = await process.ExecuteAsync();
            if (!success)
            {
                // 실패 시 팝업 띄우고 중단
                m_popup.Setup(
                    title: "게임 초기화 오류",
                    content: "게임을 초기화 하는데 실패했습니다.",
                    onClose: () => Application.Quit());
                m_popup.gameObject.SetActive(true);
                return;
            }
        }

        await LoadMainSceneAsync();
    }

    public  string GetSceneKeyByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;

        // 1. 모든 ResourceLocators 순회
        foreach (var locator in Addressables.ResourceLocators)
        {
            // 2. 로케이터 내의 모든 Keys 중 조건에 맞는 것을 탐색
            // - 이름이 포함되어 있는가
            // - 실제 SceneInstance 타입을 가리키는 키인가
            foreach (var key in locator.Keys)
            {
                string keyStr = key.ToString();

                // 씬 이름 포함 여부 및 확장자 확인 (이미지의 [2]번 형태 대응)
                if (keyStr.Contains(sceneName) && keyStr.EndsWith(".unity"))
                {
                    // 3. 해당 키가 씬 리소스 정보를 가지고 있는지 확인
                    if (locator.Locate(key, typeof(SceneInstance), out _))
                    {
                        return keyStr;
                    }
                }
            }
        }

        Debug.LogWarning($"[AddressableKeyFinder] '{sceneName}'에 해당하는 씬 키를 찾을 수 없습니다.");
        return null;
    }

    private async Task LoadMainSceneAsync()
    {
        const string scenePath = "Assets/_P2/Scene/MainScene.unity";
        m_infoText.text = "메인 로비로 이동 중...";


        // 1. 씬 로드 시작 (즉시 활성화 방지)
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(scenePath, LoadSceneMode.Single, false);

        try
        {
            // 2. 프로그레스 업데이트 루프
            // handle.IsDone은 로드가 100% 되거나 에러가 확정되면 true가 됩니다.
            while (!handle.IsDone)
            {
                if (m_slider != null)
                    m_slider.value = handle.PercentComplete;

                if (m_progressText != null)
                    m_progressText.text = $"{(handle.PercentComplete * 100f):F0}%";

                await Awaitable.NextFrameAsync();
            }

            // 3. Task 완료 대기 (RanToCompletion 상태 보장)
            await handle.Task;

            // 4. 어드레서블 성공 여부 확인
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (m_slider != null) m_slider.value = 1.0f;
                m_infoText.text = "로딩 완료";

                await Awaitable.WaitForSecondsAsync(0.5f);

                // 5. 실제 씬 활성화
                // handle.Result(SceneInstance)가 유효한지 확인 후 활성화
                await handle.Result.ActivateAsync();
            }
            else
            {
                // 앞서 발생한 'Unable to open archive file' 등의 에러가 여기서 잡힙니다.
                throw new Exception($"Addressables Scene Load Failed: {handle.OperationException}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneLoadError] {e.Message}");

            // 에러 발생 시 핸들 메모리 해제 (유지보수 및 리소스 관리)
            if (handle.IsValid())
                Addressables.Release(handle);

            //m_popup.Setup("로딩 오류", "씬을 불러오는 중 오류가 발생했습니다.\n네트워크 상태를 확인해주세요.", () => ExecuteProcess());
            m_popup.gameObject.SetActive(true);
        }
    }
}