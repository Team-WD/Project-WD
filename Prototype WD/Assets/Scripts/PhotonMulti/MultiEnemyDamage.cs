using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class MultiEnemyDamage : NetworkBehaviour
{
    private EnemyData enemyData;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private MultiEnemy _multiEnemy;
    public GameObject damagePopupPrefab;
    
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    public Canvas damageCanvas;
    public UnityEvent OnEnemyDie;

    private void Start()
    {
        _multiEnemy = GetComponent<MultiEnemy>();
        if (_multiEnemy == null)
        {
            Debug.LogError("MultiEnemy component is missing.");
            return;
        }
        damageCanvas = GameObject.Find("DamageCanvas").GetComponent<Canvas>();
        enemyData = _multiEnemy.enemyData;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Vector3 hitPosition = collision.transform.position;
            MultiBullet bulletComponent = collision.gameObject.GetComponent<MultiBullet>();
            bulletComponent.CalculateDamage();
            if (bulletComponent != null)
            {
                TakeDamage(bulletComponent.Damage,hitPosition, bulletComponent.IsCritical);
            }
        }
    }

    private void LateUpdate()
    {
        if (_multiEnemy.isDead)
        {
            return; 
        }

        if (_multiEnemy.currentHp <= 0 && !_multiEnemy.isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetTrigger("IsDead"); 
        Debug.Log("Enemy has died. Starting death process.");
        
        if (_multiEnemy != null)
        {
            _multiEnemy.Die();
            Debug.Log("MultiEnemy.Die() called.");
        }
        else
        {
            Debug.LogError("MultiEnemy component is missing.");
        }

        OnEnemyDie?.Invoke(); 
        StartCoroutine(DisableAfterAnimation());
    }

    IEnumerator DisableAfterAnimation()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float waitTime = stateInfo.length;

        yield return new WaitForSeconds(waitTime);
        
        spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
        Debug.Log("Disabling enemy object");
        gameObject.SetActive(false); 
    }

    public void TakeDamage(int damage, Vector3 hitPoint, Boolean isCritical)
    {
        int damageResult = damage - enemyData.Armor;
        if (damageResult <= 0)
            _multiEnemy.currentHp -= 1;
        else 
            _multiEnemy.currentHp -= damageResult;

        GameObject damagePopup = Instantiate(damagePopupPrefab,  damageCanvas.transform);
        damagePopup.GetComponent<DamagePopup>().Setup(damageResult,this.transform.position, isCritical);
        StartCoroutine(FlashWhite());
    }

    private IEnumerator FlashWhite()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    public void SetOriginalColor()
    {
        spriteRenderer.color = originalColor;
    }
}
