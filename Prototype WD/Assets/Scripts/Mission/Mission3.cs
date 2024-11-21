using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Mission3 : MonoBehaviour
{
    public Mission missionController;

    public UnityEvent OnBossSpawned;
    public UnityEvent OnBossKilled;
    
    public GameObject SpawnerC;

    public void startMission()
    {
        missionController.missionDescription = "보스를 처치하기";
        missionController.GetComponentInParent<MissionManager>().UpdateMissionUI(2);
        
        SpawnerC.SetActive(true);
    }
}
