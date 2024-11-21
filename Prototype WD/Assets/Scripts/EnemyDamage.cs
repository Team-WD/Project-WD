using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyDamage : MonoBehaviour
{
    public UnityEvent OnEnemyDie; // 적이 사망했을 때 발생하는 이벤트

    #region Private Fields

    private EnemyData enemyData;
    private Enemy enemyComponent;
    private Animator animator;
    private Collider2D enemyCollider; // 이름을 enemyCollider로 변경하여 상속된 멤버와 충돌 방지
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    private Color originalColor; // 원래 색상 저장 변수

    // Inspector에서 설정 가능한 플래시 색상과 지속 시간
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    // 경험치를 이미 추가했는지 확인하는 플래그
    private bool hasAddedExp = false;

    #endregion

    private void Start()
    {
        enemyComponent = GetComponent<Enemy>();
        enemyData = enemyComponent.enemyData;
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>(); // Collider2D 컴포넌트를 enemyCollider로 사용
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 컴포넌트 가져오기
        originalColor = spriteRenderer.color; // 원래 색상 저장
    }

    private void OnEnable()
    {
        hasAddedExp = false; // 객체가 다시 활성화될 때 플래그를 리셋
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }

        // 동적 생성(활성화) 오브젝트 대상 이벤트 리스너 추가
        // GameManager gameManager = FindObjectOfType<GameManager>();
        // if (gameManager != null)
        // {
        //     OnEnemyDie.AddListener(gameManager.OnEnemyDie);
        // }
    }

    private void OnDisable()
    {
        // 동적 생성(비활성화) 오브젝트 대상 이벤트 리스너 제거
        // GameManager gameManager = FindObjectOfType<GameManager>();
        // if (gameManager != null)
        // {
        //     OnEnemyDie.RemoveListener(gameManager.OnEnemyDie);
        // }
    }

    #region MonoBehaviour Callbacks

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Bullet bulletComponent = collision.gameObject.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                TakeDamage(bulletComponent);
            }
        }
    }

    private void Update()
    {
        if (enemyComponent.currentHp <= 0 && !hasAddedExp)
        {
            OnEnemyDie?.Invoke();
            animator.SetTrigger("IsDead");
            enemyCollider.enabled = false;
            AddExpToPlayer();
            hasAddedExp = true; // 경험치를 추가했음을 표시
            StartCoroutine(DisableAfterDeath());
        }
    }

    IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(0.5f);
        hasAddedExp = false; // 객체가 비활성화되기 전에 플래그를 리셋
        this.gameObject.SetActive(false);
    }

    #endregion

    #region Private Methods

    void TakeDamage(Bullet bullet)
    {
        int incomingDamage = bullet.Damage;
        int damageResult = Mathf.Max(1, incomingDamage - enemyData.Armor);

        enemyComponent.currentHp -= damageResult;

        // 피격 시 흰색으로 반짝이는 효과 추가
        StartCoroutine(FlashWhite());

        Debug.Log($"Zombie가 받은 데미지: {damageResult}, 남은 체력: {enemyComponent.currentHp}");
    }

    // 흰색 플래시 효과를 위한 코루틴
    IEnumerator FlashWhite()
    {
        spriteRenderer.color = flashColor; // 스프라이트 색상을 흰색으로 변경
        yield return new WaitForSeconds(flashDuration); // 플래시 지속 시간만큼 대기
        spriteRenderer.color = originalColor; // 원래 색상으로 복구
    }

    void AddExpToPlayer()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddExp(enemyData.ExpDrop);
            Debug.Log($"Enemy died, adding {enemyData.ExpDrop} exp to player.");
        }
    }

    #endregion
}