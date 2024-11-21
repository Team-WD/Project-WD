using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MultiSpawner : NetworkBehaviour
{
    // 풀 매니저
    public MultiPoolManager pool;
    
    // 스폰 위치들
    public Transform[] spawnPoints;

    // 좀비 스폰 타이머 
    private float timer;
     
    // 좀비 스폰 간격 
    [SerializeField] private float spawnTime = 0.2f;
    
    // 스폰 시작 간경 추가
    [SerializeField] private float startDelay;
    
    public GameManager gameManager;
    
    #region MonoBehaviour Callbacks
    
    void Start() 
    { 
        // 첫 번째 Transform은 스폰 매니저 자체의 Transform이므로 제외
        spawnPoints = GetComponentsInChildren<Transform>();
        timer = 0f; // 타이머 초기화
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    } 
     
    // 네트워크 동기화된 FixedUpdateNetwork 
    public override void FixedUpdateNetwork() 
    {
        if (gameManager.isPaused)
        {
            return;
        }

        // 서버에서만 스폰 로직을 실행 
        if (Runner.IsServer) 
        { 
            timer += Runner.DeltaTime; 

            // 처음에 10초 대기 후 스폰 시작
            if (timer >= startDelay) 
            {
                // 타이머가 스폰 간격을 초과하면 적을 스폰
                if (timer - startDelay >= spawnTime) 
                { 
                    timer -= spawnTime; // 타이머에서 spawnTime을 뺍니다.
                    Debug.Log($"Spawn timer: {timer}");
                    Spawn();
                }
            }
        } 
    }
    
    #endregion
    
    #region Private Methods
    
    // 적을 스폰하는 함수
    public void Spawn()
    {
        // 서버에서만 스폰 로직을 실행
        if (!Runner.IsServer) return;

        // 스폰 포인트 중 랜덤으로 선택 (첫 번째 Transform은 스폰 매니저 자체이므로 제외)
        int spawnIndex = UnityEngine.Random.Range(1, spawnPoints.Length);
        Vector3 spawnPosition = spawnPoints[spawnIndex].position;

        // 풀에서 NetworkObject를 가져옵니다
        NetworkObject enemyNetObj = pool.Get(0);
        if (enemyNetObj != null)
        {
            // 적 오브젝트의 위치를 설정
            enemyNetObj.transform.position = spawnPosition;

            // 적의 상태를 초기화 (체력 등)
            MultiEnemy enemyComponent = enemyNetObj.GetComponent<MultiEnemy>();
            if (enemyComponent != null)
            {
                // EnemyData에서 최대 체력을 가져와 적의 현재 체력으로 설정
                enemyComponent.currentHp = enemyComponent.enemyData.MaxHp;
                enemyComponent.isDead = false;
                
                // NavMeshAgent 속도 설정
                if (enemyComponent.agent != null)
                {
                    enemyComponent.agent.speed = enemyComponent.enemyData.Speed;
                }

                // 콜라이더 활성화
                if (enemyComponent.enemyCollider != null)
                {
                    enemyComponent.enemyCollider.enabled = true;
                }

                enemyNetObj.GetBehaviour<MultiEnemyDamage>().SetOriginalColor();
                
                // 네트워크 상에서 적 오브젝트를 활성화
                enemyNetObj.gameObject.SetActive(true);
                Debug.Log($"Spawned enemy at position: {spawnPosition}");
            }
            else
            {
                Debug.LogError("MultiEnemy component is missing on the spawned enemy.");
            }
        }
        else
        {
            Debug.LogError("Failed to retrieve enemy from pool.");
        }
    }
    
    #endregion
}