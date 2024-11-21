using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MultiBullet : NetworkBehaviour
{
    public WeaponData weaponData;
    public BulletData bulletData;
    public Player playerScript;
    // 네트워크로 동기화할 데미지와 크리티컬 여부
    
    [Networked] public float BaseDamage { get; private set; }
    [Networked] public bool IsCritical { get; private set; }
    [Networked] public int FinalDamage { get; private set; }
    
    private Vector3 startPosition;
    private float distanceTraveled;
    
    private bool isInitialized = false;

    public virtual void Initialize(WeaponData weapon, BulletData bullet,Player player)
    {
        // 무기 및 총알 데이터를 초기화
        weaponData = weapon;
        bulletData = bullet;
        playerScript = player;
        // 서버에서만 데미지 계산
 
        
        startPosition = transform.position;
        distanceTraveled = 0f;
        
        isInitialized = true;
    }

    public virtual void CalculateDamage()
    {
        // 기본 데미지 계산
        BaseDamage = weaponData.Damage * bulletData.DamageMultiplier *playerScript.playerStat.damage;

        // 크리티컬 여부 결정
        // Random.value는 0.0 ~ 1.0
        // CriticalRate가 0.3이면 30% 확률로 크리티컬, 0.5면 50% 확률로 크리티컬
        IsCritical = Random.value < (weaponData.CriticalRate+playerScript.playerStat.criticalRate);
        
        // 크리티컬이면 크리티컬 데미지 적용
        float finalDamage = IsCritical? BaseDamage *= (weaponData.CriticalDamage+playerScript.playerStat.criticalDamage) : BaseDamage;

        // 최종 데미지를 정수로 반올림
        int roundedDamage = Mathf.RoundToInt(finalDamage);

        // 데미지가 최소 1은 되도록 보장
        Damage = Mathf.Max(1, roundedDamage);
    }

    public override void FixedUpdateNetwork()
    {
        // 여기서 필요시 네트워크 동기화된 데이터 사용 가능
        // 예를 들어 서버에서만 충돌 처리 등을 관리 가능
        // 클라이언트도 총알 위치 업데이트는 즉시 반영
        // 클라이언트도 총알 위치 업데이트는 즉시 반영
        //transform.position += (Vector3)Velocity * Runner.DeltaTime;
        
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
   
}
