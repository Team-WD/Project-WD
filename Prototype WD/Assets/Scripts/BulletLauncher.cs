using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLauncher : MonoBehaviour
{
    public PoolManager pool;
    public WeaponData weaponData;
    public BulletData bulletData;

    public GameObject playerUI;
    public Transform playerUITransform;
    public GameObject reloadingBarUI;
    public GameObject ammoTextUI;
    private float offsetX = 0.2f;
    private float offsetY = 0f;
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
    private float minFiringDistance = 1f;
    private Player playerScript;

    // 총알 및 재장전 소리 관련 필드 추가
    public AudioClip fireSound;  // 발사 소리 클립
    public AudioClip reloadSound; // 재장전 소리 클립
    private AudioSource audioSource; // 오디오 소스

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        // 데이터 테이블에서 불러오기
        fireRate = weaponData.FireRate;
        ammo = weaponData.Ammo;
        reloadSpeed = weaponData.ReloadSpeed;

        // 탄창 초기화
        currentAmmo = ammo;

        // 재장전 꺼두기
        reloadingBarUI.SetActive(false);
        // 재장전 애니메이션
        reloadingAnimator = reloadingBarUI.GetComponent<Animator>();

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

        // AudioSource 컴포넌트 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found!");
        }
    }

    private void OnPlayerDie()
    {
        isPlayerDead = true;

        if (playerUI != null)
        {
            playerUI.SetActive(false);
        }
        
        // 재장전 UI 비활성화
        if (reloadingBarUI != null)
        {
            reloadingBarUI.SetActive(false);
        }

        // 추가적인 정리 작업이 필요하다면 여기에 구현
    }

    private void Update()
    {
        if (isPlayerDead)
        {
            return; // 플레이어가 사망했다면 업데이트를 수행하지 않음
        }

        timer += Time.deltaTime;

        // 마우스 왼쪽 버튼을 눌렀을 때 발사 가능 상태이면 발사
        if (Input.GetMouseButton(0) && !isReloading)
        {
            isFiring = true;
            if (currentAmmo > 0)
            {
                if (timer > (60 / fireRate))
                {
                    Fire();
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

        if (Input.GetKeyDown(KeyCode.R) && !isFiring)
        {
            // R 키 누른 경우 즉시 재장전
            StartReload();
        }

        UpdateReloadingBarPosition();
    }

    #endregion

    #region Private Methods

    private void Fire()
    {
        if (muzzleTransform == null) return;

        // 총알 pooling 및 초기화
        GameObject bulletObj = pool.Get(0);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet == null)
        {
            bullet = bulletObj.AddComponent<Bullet>();
        }
        bullet.Initialize(weaponData, bulletData);

        // 잔탄량 - 1
        currentAmmo--;

        bullet.transform.position = muzzleTransform.position;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = playerTransform.position.z;

        // 캐릭터 중심을 기준으로 방향 계산
        Vector3 direction = (mousePosition - playerTransform.position).normalized;

        // 최소 발사 거리 적용
        Vector3 adjustedMousePosition = playerTransform.position + direction * Mathf.Max(minFiringDistance, Vector3.Distance(playerTransform.position, mousePosition));
        Vector3 finalDirection = (adjustedMousePosition - muzzleTransform.position).normalized;

        float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        bullet.GetComponent<Rigidbody2D>().velocity = finalDirection * bulletData.ShootSpeed;

        // 총알 발사 소리 재생
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    private void StartReload()
    {
        if (isPlayerDead || isReloading)
        {
            return; // 플레이어가 사망했거나 이미 재장전 중이라면 재장전을 시작하지 않음
        }

        isReloading = true;
        ammoTextUI.SetActive(false);
        reloadingBarUI.SetActive(true);

        // 재장전 소리 재생
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

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
        ammoTextUI.SetActive(true);
        reloadingBarUI.SetActive(false);
    }

    private void UpdateReloadingBarPosition()
    {
        if (playerUITransform != null)
        {
            playerUITransform.position = playerTransform.position + new Vector3(offsetX, offsetY, offsetZ);
        }
    }

    private void OnDestroy()
    {
        if (playerScript != null)
        {
            playerScript.OnPlayerDie.RemoveListener(OnPlayerDie);
        }
    }

    #endregion
}
