using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public PoolManager pool;
    public Transform[] spawnPoint;

    private float timer; // 몬스터 스폰 타이머
    private float spawnTime = 0.2f; // 몬스터 스폰 기준 시간

    private void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > spawnTime)
        {
            Spawn();
            timer = 0;
        }
    }

    #region private Methods

    void Spawn()
    {
        GameObject enemy = pool.Get(0);
        if (enemy != null)
        {
            // 좀비 체력 및 Collider 초기화
            enemy.GetComponent<Enemy>().currentHp = enemy.GetComponent<Enemy>().enemyData.MaxHp;

            // Collider도 자동으로 활성화됨 (EnemyDamage에서 OnEnable에 설정)
            enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        }
    }


    #endregion
}