using System;
using System.Collections.Generic;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    [Space(10), Header("----- Repository Inject -----")]
    [SerializeField] private MinigameRepository m_MinigameRepo;
    [SerializeField] private EventRepository m_EventRepo;


    private void Start()
    {
        m_MinigameRepo.CreateInstanceAllMinigame(this.transform, m_EventRepo);
    }

    public void HandleOnRequestStartMinigame()
    {
        m_MinigameRepo.OnActiveRandomMinigame();
    }

    
}