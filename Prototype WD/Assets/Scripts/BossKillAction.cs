using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossKillAction : MonoBehaviour
{
    public Mission3 mission3;

    private void Start()
    {
        mission3 = FindObjectOfType<Mission3>();
    }

    public void BossKilled()
    {
        mission3.OnBossKilled.Invoke();
    }
}
