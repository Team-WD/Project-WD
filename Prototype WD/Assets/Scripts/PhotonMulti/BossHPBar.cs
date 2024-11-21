using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHPBar : MonoBehaviour
{
    private MultiEnemy boss;  // 보스의 체력과 상태를 가져오기 위해 MultiEnemy 참조
    private Slider hpSlider;  // 체력 바 슬라이더

    private void Start()
    {
        hpSlider = GetComponent<Slider>();
        StartCoroutine(FindBossCoroutine());
    }

    private IEnumerator FindBossCoroutine()
    {
        while (boss == null)
        {
            // 씬 내에서 모든 MultiEnemy 객체를 찾습니다.
            MultiEnemy[] enemies = FindObjectsOfType<MultiEnemy>();
            foreach (var enemy in enemies)
            {
                // enemy의 이름이 'NetBoss' 또는 'NetBoss(Clone)'인지 확인합니다.
                if (enemy.gameObject.name == "NetBoss" || enemy.gameObject.name == "NetBoss(Clone)")
                {
                    boss = enemy;
                    break;
                }
            }

            if (boss != null)
            {
                // 보스를 찾았으면 초기화
                Initialize(boss);

                Debug.Log("Boss found and HP Bar initialized.");

                yield break;
            }

            // 보스를 찾지 못했으면 잠시 대기 후 다시 시도
            yield return new WaitForSeconds(0.1f);
        }
    }

    // 보스가 소환될 때 체력 바를 초기화하는 함수
    public void Initialize(MultiEnemy spawnedBoss)
    {
        boss = spawnedBoss;

        // 보스의 최대 체력을 슬라이더의 maxValue로 설정
        hpSlider.maxValue = boss.enemyData.MaxHp;

        // 현재 체력을 슬라이더에 반영
        hpSlider.value = boss.currentHp;
    }

    private void Update()
    {
        if (boss != null)
        {
            if (!boss.isDead)
            {
                // 보스의 현재 체력을 슬라이더에 반영
                hpSlider.value = boss.currentHp;
            }
            else
            {
                // 보스가 죽으면 캔버스 전체를 비활성화
                transform.root.gameObject.SetActive(false);  // BOSS Status UI Canvas 전체 비활성화
            }
        }
    }
}