using System.Collections;
using UnityEngine;
using Fusion;

public class BossSpawn : NetworkBehaviour
{
    public MultiPoolManager pool;               // Enemy Pool Manager 참조
    public Transform spawnPoint;
    // Grid 컴포넌트
    public Grid grid;// 보스가 소환될 위치
    public GameObject cagePrefab;               // Cage 프리팹 참조
    public GameObject bossUICanvas;             // BOSS Status UI Canvas 참조

    private bool isBossSpawned = false;         // 보스가 이미 소환되었는지 확인

    public Mission3 mission3;                   // 미션 목표 설정을 위한 Mission3 참조

    // 새로 추가된 클라이언트 동기화를 위한 변수
    [Networked] private NetworkObject bossInstance { get; set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어가 트리거에 진입하고 보스가 아직 소환되지 않은 경우
        if (other.CompareTag("Player") && !isBossSpawned) 
        {
            mission3.OnBossSpawned.Invoke(); // 보스 소환 이벤트 호출
            SpawnBoss();  // 보스 소환
        }
    }

    private void SpawnBoss()
    {
        // 보스 소환 플래그 설정 (중복 소환 방지)
        isBossSpawned = true;

        // 풀에서 보스 프리팹을 가져옴
        bossInstance = pool.Get(1);  // 1번 인덱스는 보스라고 가정

        if (bossInstance != null)
        {
            // 보스 위치 설정 및 활성화
            bossInstance.transform.position = spawnPoint.position; 
            bossInstance.gameObject.SetActive(true);

            // 보스에 'Target' 태그 설정 (ArrowController에서 인식 가능하게 함)
            bossInstance.gameObject.tag = "Target";

            // 모든 클라이언트에서 UI 캔버스를 활성화하도록 RPC 호출
            ActivateBossUI_RPC();

            // 타일맵 프리팹이 정상적으로 할당되었는지 확인
            if (cagePrefab != null && grid != null)
            {
                // 프리팹 인스턴스 생성 및 Grid의 자식으로 설정
                GameObject tilemapInstance = Instantiate(cagePrefab, grid.transform);

                // 프리팹을 활성화
                tilemapInstance.SetActive(true);

                Debug.Log("타일맵 프리팹이 Grid에 활성화되었습니다.");
            }
            else
            {
                Debug.LogWarning("TilemapPrefab 또는 Grid가 할당되지 않았습니다.");
            }

            // 미션 목표 설정
            GetComponentInParent<Mission>().missionDescription = "보스 처치하기";

            // 타겟 리스트 갱신 (보스가 생성된 후 바로 갱신)
            FindAndUpdateArrowTargets();

            // 스포너 오브젝트 비활성화
            DisableSpawner();
        }
        else
        {
            Debug.LogError("Boss prefab not found in pool.");
        }
    }

    // RPC로 모든 클라이언트에서 UI 캔버스를 활성화하는 함수
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void ActivateBossUI_RPC()
    {
        // 호스트와 클라이언트 모두에서 UI 캔버스를 활성화
        bossUICanvas.SetActive(true);
        Debug.Log("BOSS Status UI Canvas activated for all clients and the host.");
    }

    // ArrowController에서 타겟 리스트를 갱신하는 함수
    private void FindAndUpdateArrowTargets()
    {
        // ArrowController 스크립트를 찾아서 타겟 리스트를 갱신
        ArrowController arrowController = FindObjectOfType<ArrowController>();
        if (arrowController != null)
        {
            arrowController.FindAndAssignTargets();
        }
        else
        {
            Debug.LogWarning("ArrowController not found.");
        }
    }

    // 스포너 오브젝트 비활성화 함수
    private void DisableSpawner()
    {
        // 스포너 오브젝트를 비활성화 (self GameObject)
        this.gameObject.SetActive(false);
        Debug.Log("Spawner has been disabled.");
    }
}
