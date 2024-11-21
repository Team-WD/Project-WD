using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    // 미션들을 담당하는 오브젝트 배열
    public GameObject[] missionObjects;
    // 현재 몇 번째 미션을 수행 중인지 Index를 확인하는 변수
    public int currentMissionIndex = 0;
    
    // 미션 정보를 표시하는 UI(제목, 내용) 오브젝트
    public GameObject missionName;
    public GameObject missionDescription;
    
    #region MonoBehaviour Callbacks

    private void Start()
    {
        // 게임 처음 켜면 첫 미션으로 정보 초기화
        UpdateMissionUI(0);
    }

    private void FixedUpdate()
    {
        UpdateMissionUI(currentMissionIndex);
    }

    #endregion

    #region Private Methods

    // 모든 미션을 클리어 했는지 확인하는 메서드
    public bool isAllClear()
    {
        // 모든 미션이 클리어되면 true 반환
        if (missionObjects.All(mission => mission.GetComponent<Mission>().isClear))
        {
            return true;
        }
        
        // 남은 미션이 있으면 그 다음 미션을 클리어하도록 유도
        currentMissionIndex++;
        UpdateMissionUI(currentMissionIndex);

        return false;
    }

    private string MissionName(int idx)
    {
        return missionObjects[idx].GetComponent<Mission>().missionName;
    }

    private string MissionDescription(int idx)
    {
        return missionObjects[idx].GetComponent<Mission>().missionDescription;
    }

    public void UpdateMissionUI(int idx)
    {
        missionName.GetComponent<TMP_Text>().text = MissionName(idx);
        missionDescription.GetComponent<TMP_Text>().text = MissionDescription(idx);
    }

    #endregion
}