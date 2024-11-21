using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Mission1 : MonoBehaviour
{
    // Mission 1 기믹 총괄 제어 스크립트입니다.

    public GameObject mission; // Mission Controller 참조 변수

    public List<GameObject> itemLists; // 수집 대상 아이템 리스트
    public int itemCount; // 수집 대상 아이템 개수

    public int currentCount = 0; // 현재 수집한 아이템 개수

    public GameObject SpawnerA;
    
    public AudioClip pickupSound; // 아이템 획득 사운드
    public AudioSource audioSource;  // 오디오 소스 추가

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Start()
    {
        // 수집 대상 아이템 개수 초기화
        itemCount = itemLists.Count;
        
        // 스포너 동작 시작
        SpawnerA.SetActive(true);

        // 수집 대상 아이템 활성화
        foreach (var item in itemLists)
        {
            item.SetActive(true);
        }
    }
    
    // 아이템 수집 인식
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void OnItemCollect()
    {
        currentCount++;
        GetComponent<Mission>().missionDescription = "주변에서 백신을 찾기 (" + currentCount + "/" + itemCount + ")";
        
        // pick up sound play
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // Mission UI 갱신
        GetComponentInParent<MissionManager>().UpdateMissionUI(0);

        // 수집한 아이템이 모두 수집되었을 경우
        if (currentCount == itemCount)
        {
            mission.GetComponent<Mission>().ClearMission();
            // 스포너 동작 종료
            SpawnerA.SetActive(false);
        }
    }
}