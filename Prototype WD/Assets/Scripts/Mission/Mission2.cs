using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class Mission2 : MonoBehaviour
{
    public Mission missionController;
    public Grid grid; // 그리드 불러오기
    public GameObject barricadePrefab; // 장애물 프리팹 참조
    public GameObject tilemapInstance;
    
    public GameObject SpawnerB;
    public GameObject missionTimer;
    public float surviveTime;
    
    public UnityEvent OnMission2Clear;

    #region MonoBehaviour Callbacks
    

    #endregion

    #region Public Methods
    
    // 디펜스 시작을 알리는 함수
    // 온라인 환경
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void StartMission()
    {
        // missionController 의 정보를 디펜스 성격으로 업데이트
        missionController.missionDescription = "좀비로부터 마트 입구를 지키기";
        
        // 타일맵 프리팹이 정상적으로 할당되었는지 확인
        if (barricadePrefab != null && grid != null)
        {
            // 프리팹 인스턴스 생성 및 Grid의 자식으로 설정
            tilemapInstance = Instantiate(barricadePrefab, grid.transform);

            // 프리팹을 활성화
            tilemapInstance.SetActive(true);

            Debug.Log("타일맵 프리팹이 Grid에 활성화되었습니다.");
        }
        else
        {
            Debug.LogWarning("TilemapPrefab 또는 Grid가 할당되지 않았습니다.");
        }

        // 미션 타이머 활성화
        missionTimer.SetActive(true);
        
        // spawner B 활성화
        SpawnerB.SetActive(true);
    }

    // 미션 클리어를 알리는 함수
    // 온라인 환경
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Mission2Clear()
    {
        // 프리팹을 비활성화
        tilemapInstance.SetActive(false);
        
        // 미션 클리어 처리
        missionController.isClear = true;
        // 미션 타이머 비활성화
        missionTimer.SetActive(false);
        // spawner B 비활성화
        SpawnerB.SetActive(false);
        // 클리어 함수 실행
        missionController.ClearMission();
    }

    

    #endregion
}