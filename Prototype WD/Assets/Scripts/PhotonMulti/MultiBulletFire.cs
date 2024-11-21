using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MultiBulletFire : NetworkBehaviour
{
    public MultiPoolManager pool;
    public WeaponData weaponData;
    public BulletData bulletData;

    public GameObject reloadingUI;
    public Transform reloadingBarTransform;
    private float offsetX = 0f;
    private float offsetY = 0.4f;
    private float offsetZ = 0f;
    private Animator reloadingAnimator;

    private Transform playerTransform;
    private Transform weaponTransform;
    private Transform muzzleTransform;

    #region Private Fields

    private float timer; // 연사 간격 타이머

    private float fireRate; // 연사 속도
    public int ammo; // 탄창
    private float reloadSpeed; // 재장전 속도
    public int currentAmmo; // 현재 탄창

    private bool isFiring;
    private bool isReloading; // 재장전 중
    private bool isPlayerDead = false;
    private Player playerScript;

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        // 데이터 테이블에서 불러오기
        fireRate = weaponData.FireRate;
        ammo = weaponData.Ammo;
        reloadSpeed = weaponData.ReloadSpeed;
        pool = GameObject.Find("Bullet Pool Manager").GetComponent<MultiPoolManager>();
        reloadingUI = GameObject.Find("Reloading Canvas").transform.GetChild(0).gameObject;
        reloadingBarTransform = reloadingUI.transform;
        // 탄창 초기화
        currentAmmo = ammo;

        // 재장전 꺼두기
        reloadingUI.SetActive(false);
        // 재장전 애니메이션
        reloadingAnimator = reloadingUI.GetComponent<Animator>();

        // 플레이어 Transform 찾기
        playerTransform = GetComponentInParent<Player>()?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found in parents!");
        }

        // Weapon Transform 찾기
        weaponTransform = GetComponentInChildren<Weapon>()?.transform;
        if (weaponTransform == null)
        {
            Debug.LogError("Weapon not found in children!");
        }

        muzzleTransform = transform.Find("Weapon/MuzzlePoint");
        if (muzzleTransform == null)
        {
            Debug.LogError("MuzzlePoint not found in children!");
        }

        // Player 스크립트 참조 가져오기 및 이벤트 구독
        playerScript = GetComponentInParent<Player>();
        if (playerScript != null)
        {
            playerScript.OnPlayerDie.AddListener(OnPlayerDie);
        }
        else
        {
            Debug.LogError("Player script not found in parents!");
        }
    }

    private void OnPlayerDie()
    {
        isPlayerDead = true;

        // 재장전 UI 비활성화
        if (reloadingUI != null)
        {
            reloadingUI.SetActive(false);
        }

        // 추가적인 정리 작업이 필요하다면 여기에 구현
    }

    public override void FixedUpdateNetwork()
    {
        if (isPlayerDead)
        {
            return; // 플레이어가 사망했다면 업데이트를 수행하지 않음
        }

        timer += Time.deltaTime;

        // 마우스 왼쪽 버튼을 눌렀을 때 발사 가능 상태이면 발사
        // 연사 간격 = 60초 / RPM
        if (GetInput(out NetworkInputData data) &&!isReloading)
        {
            isFiring = true;
            if (currentAmmo > 0)
            {
                if (timer > (60 / fireRate))
                {
                    
                    Fire(data.targetPosition);
                    
                    timer = 0;
                }
            }
            else
            {
                // 탄창이 다 떨어졌는데 발사하려는 경우 재장전
                StartReload();
            }
        }
        else
        {
            isFiring = false;
        }

        if (HasStateAuthority&&Input.GetKeyDown(KeyCode.R) && !isFiring)
        {
            // R 키 누른 경우 즉시 재장전
            // 빠른 재장전 어드벤티지 확장 가능성 고려해서 함수 input 열어놨던거 닫혔네요...
            StartReload();
        }

        UpdateReloadingBarPosition();
    }

    #endregion

    #region Private Methods
    
    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void Fire(Vector2 target)
    {
        if (muzzleTransform == null) return;
        
       
            // 총알 pooling 및 초기화
        NetworkObject bulletObj = pool.Get(0);
        MultiBullet bullet = bulletObj.GetComponent<MultiBullet>();
        if (bullet == null)
        {
            bullet = bulletObj.gameObject.AddComponent<MultiBullet>();
        }
        //bullet.Initialize(weaponData, bulletData,player);
        
        // 잔탄량 - 1
        currentAmmo--;

        bullet.transform.position = muzzleTransform.position; // 총 위치에 총알 생성
        // 총알 방향 설정
        Vector3 direction = ((Vector3)target - bullet.transform.position).normalized;
        
        
        
        // 총알 회전 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 총알 속도 부여
        bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletData.ShootSpeed;
        
    }

    private void StartReload()
    {
        if (isPlayerDead || isReloading)
        {
            return; // 플레이어가 사망했거나 이미 재장전 중이라면 재장전을 시작하지 않음
        }

        isReloading = true;
        reloadingUI.SetActive(true);
        if (reloadingAnimator != null)
        {
            reloadingAnimator.speed = 1f / reloadSpeed; // 애니메이션 속도를 재장전 시간에 맞춤
            reloadingAnimator.Play("ReloadingAnim", 0, 0f);
        }

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(reloadSpeed);

        currentAmmo = ammo;
        isReloading = false;
        reloadingUI.SetActive(false);
    }

    private void UpdateReloadingBarPosition()
    {
        if (reloadingBarTransform != null)
        {
            reloadingBarTransform.position = playerTransform.position + new Vector3(offsetX, offsetY, offsetZ);
        }
    }

    private void OnDestroy()
    {
        // 스크립트가 파괴될 때 이벤트 구독 해제
        if (playerScript != null)
        {
            playerScript.OnPlayerDie.RemoveListener(OnPlayerDie);
        }
    }
    
    
    #endregion
}