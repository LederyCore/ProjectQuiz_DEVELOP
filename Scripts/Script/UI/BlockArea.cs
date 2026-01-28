using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlockArea : MonoBehaviour
{
    [SerializeField] private Color m_EventColor;
    [SerializeField] private Color m_TravelingColor;
    private Image m_blockImage;



    private void Awake()
    {
        if (m_blockImage == null)
        {
            m_blockImage = GetComponent<Image>();
        }
    }

    public void HandleOnChangeGameState(GameState state)
    {
        if (state == GameState.IDLE)
        {
            m_blockImage.raycastTarget = false;
            m_blockImage.enabled = false;
        }
        else if (state == GameState.EVENT)
        {
            m_blockImage.color = m_EventColor;
            m_blockImage.raycastTarget = true;
            m_blockImage.enabled = true;
        }
        else if (state == GameState.TRAVELING)
        {
            m_blockImage.color = m_TravelingColor;
            m_blockImage.raycastTarget = true;
            m_blockImage.enabled = true;
        }
    }
}
