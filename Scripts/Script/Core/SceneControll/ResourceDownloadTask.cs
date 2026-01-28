using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/*
 * File : ResourceDownloadTask.cs
 * Desc : Addressables resource download task.
 * Handles initialization, size checking, and asynchronous downloading.
 */

public class ResourceDownloadTask : ITask
{
    private readonly TextMeshProUGUI m_progessText;
    private TextMeshProUGUI m_infoText;
    private readonly Slider m_Slider;
    private UIConfirmBase m_confirm;
    public ResourceDownloadTask(TextMeshProUGUI infotext, TextMeshProUGUI progresstext = null, Slider slider = null, UIConfirmBase confirm = null)
    {
        m_progessText = progresstext;
        m_Slider = slider;
        m_confirm = confirm;
        m_infoText = infotext;
    }

    public async Awaitable<bool> ExecuteAsync()
    {
        try
        {
            m_infoText.text = "리소스 점검 중...";
            Debug.Log("1) Starting Addressables initialization");
            await Addressables.InitializeAsync().Task;

            Debug.Log("2) Checking for catalog updates");
            // Check if there are new catalogs (changes) on the server.
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            var catalogs = await checkHandle.Task;

            if (catalogs != null && catalogs.Count > 0)
            {
                Debug.Log("3) Updating catalogs...");
                var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                await updateHandle.Task;
                Addressables.Release(updateHandle);
            }
            Addressables.Release(checkHandle);

            // 4) Check resources based on updated information
            return await CheckResources();
        }
        catch (Exception e)
        {
            Debug.LogError($"Resource process failed: {e.Message}");
            return false;
        }
    }

    private async Awaitable<bool> CheckResources()
    {
        long totalDownloadSize = 0;
        List<string> groupsToDownload = new List<string>();

       IEnumerable<IResourceLocator> resourceLocators = Addressables.ResourceLocators;
       
       foreach (var resourceLocator in resourceLocators)
       {
           Debug.Log($"{GetType()}::Resource Locator: {resourceLocator.LocatorId}");
       }


        foreach (Define.RemoteResouceGroup group in Enum.GetValues(typeof(Define.RemoteResouceGroup)))
        {
            var handle = Addressables.GetDownloadSizeAsync(group.ToString());
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{GetType()}::Download size for {group}: {handle.Result} bytes");
                if (handle.Result > 0)
                {
                    totalDownloadSize += handle.Result;
                    groupsToDownload.Add(group.ToString());
                }
            }
            else
            {
                Debug.LogError($"{GetType()}::Failed to retrieve download size for {group}");
            }

            Addressables.Release(handle);
        }

        if (totalDownloadSize > 0)
        {
            Debug.Log($"{GetType()}::Total download size: {totalDownloadSize / 1_000_000f:F0}MB");

            var acs = new AwaitableCompletionSource<bool>();

            m_confirm.Setup(
                title: "새로운 패치 발견",
                description: $"새로운 패치 버전이 있습니다.\n" +
                         $"{totalDownloadSize / 1_000_000f:F1} MB\n" +
                         $"(Wi-fi 상태 권장)\n" +
                         $"다운로드 하시겠습니까?",
                onShow: null,
                onClose: null,
                onConfirm: () => acs.SetResult(true),
                onCancel: () => acs.SetResult(false));

            bool confirmed = await acs.Awaitable;
            if (!confirmed)
                return false;

            return await DownloadResources(groupsToDownload);
        }
        else
        {
            Debug.Log($"{GetType()}::No resource download required.");
            return true;
        }
    }

    private async Awaitable<bool> DownloadResources(List<string> groups)
    {
        m_Slider.gameObject.SetActive(true);
        foreach (var group in groups)
        {
            Debug.Log($"{GetType()}::Starting download -> {group}");
            var handle = Addressables.DownloadDependenciesAsync(group);

            while (!handle.IsDone)
            {
                float progress = handle.PercentComplete;
                m_Slider.value = progress;
                m_progessText.text = $"{progress * 100:F0}%";
                await Awaitable.NextFrameAsync();
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"{GetType()}::Download failed -> {group}");
                Addressables.Release(handle);
                return false;
            }

            Debug.Log($"{GetType()}::Download completed -> {group}");
            Addressables.Release(handle);
        }
        await Awaitable.WaitForSecondsAsync(2.5f);
        m_Slider.gameObject.SetActive(false);
        m_progessText.text = null;
        return true;
    }
}