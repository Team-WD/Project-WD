using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission : MonoBehaviour
{
    public string missionName;
    public string missionDescription;
    public bool isClear = false;

    #region Private Methods

    // 미션 클리어 여부를 갱신하는 메서드
    // 미션 클리어 할 때 호출하세요
    public void ClearMission()
    {
        isClear = true;
        Debug.Log("Mission Clear!");

        // 전부 클리어 했는지 확인
        if (transform.GetComponentInParent<MissionManager>().isAllClear())
        {
            Debug.Log("All Clear!");
            // 클리어 시 ResultScene으로 이동
            SceneManager.LoadScene("ResultScene");
        }
    }

    #endregion
}