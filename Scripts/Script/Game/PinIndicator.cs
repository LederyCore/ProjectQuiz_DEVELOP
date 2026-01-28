using System;
using UnityEngine;

public class PinIndicator : MonoBehaviour
{
    [SerializeField] private PinStateData m_normalState;
    [SerializeField] private PinStateData m_lockedState;
    [SerializeField] private Vector3 m_offset = new Vector3(0f, 0f, -1f);

    private SphereCollider m_collider;
    private GameObject m_instance;

    private void Awake()
    {
        // 이 컴포넌트가 부착된 게임 오브젝트의 첫 번째 자식 오브젝트를 충돌체로 사용
        var colObj = transform.GetChild(0);
        m_collider = colObj.GetComponent<SphereCollider>();
        if (m_collider == null)
        {
            Debug.LogError("PinIndicator requires a SphereCollider on its first child object.");
        }
        m_collider.center = m_offset;

        // 처음 실행시 핀을 미리 생성해놓고 비활성화 상태로 둠
        m_instance = m_normalState.CreateInstance(m_collider.gameObject.transform);
        SetPinActive(false);
    }

    public void SetPinLocked(bool v)
    {
        // 핀의 잠금 상태에 따라 시각적 요소와 충돌체 설정
        var stateData = v ? m_lockedState : m_normalState;
        if (m_instance != null)
        {
            Destroy(m_instance);
        }
        m_instance = stateData.CreateInstance(m_collider.gameObject.transform);
        m_collider.radius = stateData.detectionRadius;
    }

    public void SetPinActive(bool v)
    {
        if (m_instance != null)
        {
            m_instance.SetActive(v);
            m_collider.enabled = v;
        }
    }
    
    public void IconDisable()
    {
        m_instance.SetActive(false);
    }
}