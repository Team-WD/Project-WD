using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MultiBulletLauncher : NetworkBehaviour
{
    public BulletPool pool;
    public WeaponData weaponData;
    public BulletData bulletData;


    public LayerMask enemyLayer; // Inspector에서 설정

    public GameObject playerUI;
    public Transform playerUITransform;
    public GameObject reloadingBarUI;
    public GameObject ammoTextUI;
    private float offsetX = 0.2f;
    private float offsetY = 0f;
    private float offsetZ = 0f;
    private Animator reloadingAnimator;

    [Networked, Capacity(20)] private NetworkArray<ProjectileData> _projectiles => default;

    public Transform playerTransform;
    public Transform weaponTransform;
    public Transform muzzleTransform;
    public GameObject bulletPrefab;

    #region Private Fields

    private float timer; // 연사 간격 타이머
    private float fireRate; // 연사 속도
    [Networked] public int ammo { get; set; } // 탄창
    private float reloadSpeed; // 재장전 속도
    int bulletsPerShot; // 한번에 발사하는 탄환

    [Networked] public int currentAmmo { get; set; }

    private bool isFiring;
    private bool isReloading; // 재장전 중
    private bool isPlayerDead = false;
    private float minFiringDistance = 1f;
    public Player playerScript;

    // 총알 및 재장전 소리 관련 필드 추가
    public AudioClip fireSound; // 발사 소리 클립
    public AudioClip reloadSound; // 재장전 소리 클립
    private AudioSource audioSource; // 오디오 소스
    public MultiAmmoUI playerAmmoUI;
    public MultiAmmoUI floatingAmmoUI;
    public GameManager gameManager;
    [Networked] private Vector2 aimingDirection { get; set; }

    #endregion

    #region MonoBehaviour Callbacks

    public void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        Debug.Log("MultiBulletLauncher::Initialize");
        // 데이터 테이블에서 불러오기
        fireRate = weaponData.FireRate;
        ammo = weaponData.Ammo;
        reloadSpeed = weaponData.ReloadSpeed;

        if (weaponData.Id == 1003)
            bulletsPerShot = 5;
        else
            bulletsPerShot = 1;

        playerAmmoUI = GameObject.Find("Player Status UI Canvas/Ammo Text").GetComponent<MultiAmmoUI>();
        floatingAmmoUI = transform.parent.transform.parent.Find("Player Floating UI Canvas/Ammo Text")
            .GetComponent<MultiAmmoUI>();
        initUI();

        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        // 탄창 초기화
        currentAmmo = ammo;
        pool = GameObject.Find("Bullet Pool Manager").GetComponent<BulletPool>();
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
        weaponTransform = GetComponentInChildren<MultiWeapon>()?.transform;
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
            playerScript.OnPlayerRevive.AddListener(OnPlayerRevive); // 부활 이벤트 추가
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

    public void initUI()
    {
        // 플레이어 UI 오브젝트를 명확하게 지정

        if (HasInputAuthority)
        {
            if (playerAmmoUI != null)
            {
                playerAmmoUI.Initialize(this);
            }
            else
            {
                Debug.LogError("playerAmmoUI를 찾을 수 없습니다.");
            }
        }

        if (floatingAmmoUI != null)
        {
            floatingAmmoUI.Initialize(this);
        }
        else
        {
            Debug.LogError("floatingAmmoUI를 찾을 수 없습니다.");
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

    public void OnPlayerRevive()
    {
        Debug.Log("Player revived, restoring weapon state.");

        isPlayerDead = false; // 플레이어 상태 리셋
        isReloading = false; // 재장전 상태 초기화
        isFiring = false; // 발사 상태 초기화

        // 플레이어 UI 다시 활성화
        if (playerUI != null)
        {
            playerUI.SetActive(true);
        }

        // 재장전 UI 다시 활성화
        if (reloadingBarUI != null)
        {
            reloadingBarUI.SetActive(false); // 기본적으로 비활성화 (재장전 시작 시 활성화)
        }

        // 탄창 UI 다시 활성화
        if (ammoTextUI != null)
        {
            ammoTextUI.SetActive(true);
        }

        // 현재 탄약 상태를 다시 갱신
        currentAmmo = ammo;
        Debug.Log($"Current ammo restored to {currentAmmo}");
    }

    public override void FixedUpdateNetwork()
    {
        if (muzzleTransform == null)
        {
            Initialize();
            Debug.Log("muzzle!");
            return;
        }

        if (isPlayerDead || gameManager.isPaused)
            return;
        timer += Runner.DeltaTime;
        if (GetInput(out NetworkInputData data))
        {
            // Debug.Log("moustClicked!");
            // 마우스 왼쪽 버튼을 눌렀을 때 발사 가능 상태이면 발사
            if (data.isFiring && !isReloading)
            {
                isFiring = true;
                if (currentAmmo > 0)
                {
                    if (timer > (60 / fireRate))
                    {
                        if (HasStateAuthority)
                        {
                            Fire(data.targetPosition);
                        }
                        else
                        {
                            Debug.Log("FireRPC called");
                            FireRPC(data.targetPosition);
                        }

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
        }

        if (Input.GetKeyDown(KeyCode.R) && !isFiring && HasInputAuthority)
        {
            // R 키 누른 경우 즉시 재장전
            StartReload();
        }

        UpdateReloadingBarPosition();
        UpdateProjectiles();
    }

    #endregion

    #region Private Methods

    private void Fire(Vector3 targetPosition)
    {
        Debug.Log("Fire called");
        if (currentAmmo <= 0 || !HasStateAuthority) return;
        if (muzzleTransform == null)
        {
            Debug.Log("MuzzlePoint not found!");
            return;
        }

        if (pool == null)
        {
            Debug.LogError("Bullet pool is not initialized!");
            return;
        }

        for (int i = 0; i < bulletsPerShot; i++)
        {
            Vector2 direction = CalculateFireDirection(targetPosition);
            aimingDirection = direction; // 네트워크로 동기화될 값 설정    
            AddProjectile(muzzleTransform.position, direction);
        }

        currentAmmo--;
        
        // 총알 발사 소리 재생
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    private void AddProjectile(Vector3 position, Vector2 direction)
    {
        for (int i = 0; i < _projectiles.Length; i++)
        {
            ProjectileData projectile = _projectiles[i];
            if (projectile.LifeTime.ExpiredOrNotRunning(Runner))
            {
                projectile = new ProjectileData
                {
                    Position = position,
                    Velocity = direction * bulletData.ShootSpeed,
                    LifeTime = TickTimer.CreateFromSeconds(Runner, 5),
                    OwnerId = Object.Id,
                    DistanceTraveled = 0f,
                    PenetratingCount = weaponData.PenetratingPower
                };
                projectile = CalculateDamage(projectile);
                _projectiles.Set(i, projectile);
                break;
            }
        }
    }

    private ProjectileData CalculateDamage(ProjectileData projectile)
    {
        // 기본 데미지 계산
        projectile.BaseDamage = weaponData.Damage * bulletData.DamageMultiplier * playerScript.playerStat.damage;

        // 크리티컬 여부 결정
        projectile.IsCritical = Random.value < (weaponData.CriticalRate + playerScript.playerStat.criticalRate);

        // 크리티컬이면 크리티컬 데미지 적용
        float finalDamage = projectile.IsCritical
            ? projectile.BaseDamage * (weaponData.CriticalDamage + playerScript.playerStat.criticalDamage)
            : projectile.BaseDamage;

        // 최종 데미지를 정수로 반올림
        int roundedDamage = Mathf.RoundToInt(finalDamage);

        // 데미지가 최소 1은 되도록 보장
        projectile.FinalDamage = Mathf.Max(1, roundedDamage);

        return projectile;
    }

    private void UpdateProjectiles()
    {
        for (int i = 0; i < _projectiles.Length; i++)
        {
            ProjectileData projectile = _projectiles[i];
            if (!projectile.LifeTime.ExpiredOrNotRunning(Runner))
            {
                projectile.Position += projectile.Velocity * Runner.DeltaTime;
                projectile.DistanceTraveled += projectile.Velocity.magnitude * Runner.DeltaTime;

                if (projectile.DistanceTraveled >= weaponData.Range)
                {
                    projectile.LifeTime = TickTimer.None;
                }
                else
                {
                    CheckCollision(ref projectile);
                }

                _projectiles.Set(i, projectile);
            }
        }
    }

    private void CheckCollision(ref ProjectileData projectile)
    {
        RaycastHit2D hit = Physics2D.Raycast(projectile.Position, projectile.Velocity.normalized,
            projectile.Velocity.magnitude * Runner.DeltaTime, enemyLayer);
        if (hit.collider != null)
        {
            MultiEnemyDamage hitEnemy = hit.collider.GetComponent<MultiEnemyDamage>();
            if (hitEnemy != null)
            {
                hitEnemy.TakeDamage(projectile.FinalDamage, hit.collider.transform.position, projectile.IsCritical);
                projectile.PenetratingCount--;
                if (projectile.PenetratingCount <= 0)
                {
                    projectile.LifeTime = TickTimer.None;
                }
            }
            else
            {
                projectile.LifeTime = TickTimer.None;                                                                                                                         
            }
        }
    }

    public override void Render()
    {
        base.Render();
        for (int i = 0; i < _projectiles.Length; i++)
        {
            ProjectileData projectile = _projectiles[i];
            if (!projectile.LifeTime.ExpiredOrNotRunning(Runner))
            {
                RenderProjectile(projectile);
            }
        }
    }

    private void RenderProjectile(ProjectileData projectile)
    {
        GameObject bulletVisual = pool.GetBullet(bulletData.Id);
        if (bulletVisual != null)
        {
            bulletVisual.transform.position = projectile.Position;
            float angle = Mathf.Atan2(aimingDirection.y, aimingDirection.x) * Mathf.Rad2Deg;
            bulletVisual.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            StartCoroutine(ReturnBulletToPool(bulletVisual));
        }
    }

    private IEnumerator ReturnBulletToPool(GameObject bullet)
    {
        yield return new WaitForSeconds(Runner.DeltaTime);
        pool.ReturnBullet(bullet);
    }

    private Vector3 CalculateFireDirection(Vector3 targetPosition)
    {
        // 캐릭터 중심을 기준으로 방향 계산
        Vector3 direction = (targetPosition - playerTransform.position).normalized;
        // 최소 발사 거리 적용
        Vector3 adjustedMousePosition = playerTransform.position + direction *
            Mathf.Max(minFiringDistance, Vector3.Distance(playerTransform.position, targetPosition));
        Vector3 finalDirection = (adjustedMousePosition - muzzleTransform.position).normalized;

        // 탄퍼짐 적용
        float spread = weaponData.Spread; // 탄퍼짐 각도 (조절 가능)
        finalDirection = Quaternion.Euler(0, 0, Random.Range(-spread, spread)) * finalDirection;
        return finalDirection;
        //float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
        // bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        // bullet.GetComponent<Rigidbody2D>().velocity = finalDirection * bulletData.ShootSpeed;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void FireRPC(Vector3 targetPosition)
    {
        Fire(targetPosition);
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void UpdateAmmoRpc()
    {
        currentAmmo = ammo;
    }

    private IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(reloadSpeed);
        if (Runner.IsServer)
        {
            currentAmmo = ammo;
        }
        else
        {
            UpdateAmmoRpc();
        }

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
            playerScript.OnPlayerRevive.RemoveListener(OnPlayerRevive); // 리스너 제거
        }
    }

    #endregion
}