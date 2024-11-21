using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    protected WeaponData weaponData;
    protected BulletData bulletData;
    
    private float baseDamage;
    private bool isCritical;
    private Vector3 startPosition;
    private float distanceTraveled;
    
    private bool isInitialized = false;

    public virtual void Initialize(WeaponData weapon, BulletData bullet)
    {
        weaponData = weapon;
        bulletData = bullet;
        CalculateDamage();
        
        startPosition = transform.position;
        distanceTraveled = 0f;
        
        isInitialized = true;
    }

    protected virtual void CalculateDamage()
    {
        // 기본 데미지 계산
        baseDamage = weaponData.Damage * bulletData.DamageMultiplier;

        // 크리티컬 여부 결정
        // Random.value는 0.0 ~ 1.0
        // CriticalRate가 0.3이면 30% 확률로 크리티컬, 0.5면 50% 확률로 크리티컬
        isCritical = Random.value < weaponData.CriticalRate;
        
        // 크리티컬이면 크리티컬 데미지 적용
        float finalDamage = isCritical? baseDamage *= weaponData.CriticalDamage : baseDamage;

        // 최종 데미지를 정수로 반올림
        int roundedDamage = Mathf.RoundToInt(finalDamage);

        // 데미지가 최소 1은 되도록 보장
        Damage = Mathf.Max(1, roundedDamage);
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return; // 초기화되지 않았다면 Update 로직을 실행하지 않음
        }
        
        // 이동 거리 계산
        distanceTraveled += bulletData.ShootSpeed * Time.deltaTime;

        // 사거리를 초과하면 총알 비활성화
        if (distanceTraveled >= weaponData.Range)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // 총알이 비활성화될 때 초기화
        distanceTraveled = 0f;
        
        isInitialized = false;
    }
    
    public int Damage { get; private set; }
    public bool IsCritical => isCritical;
}
