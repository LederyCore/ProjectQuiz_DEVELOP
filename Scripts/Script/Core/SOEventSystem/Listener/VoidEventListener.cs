using System;
using UnityEngine;
using UnityEngine.Events;

public class VoidEventListener : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO m_channel = default;

    public UnityEvent OnEventRaised;

    private void OnEnable() => m_channel.OnEventRaised += Respond;
    private void OnDisable() => m_channel.OnEventRaised -= Respond;

    private void Respond() => OnEventRaised?.Invoke();
}