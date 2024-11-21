using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class Player : NetworkBehaviour
{
    [SerializeField] public PlayerData playerData; // 플레이어 데이터 (최대 HP, 방어력 등)
    [Networked] public int currentHP { get; set; } // 현재 HP
    [Networked] public int maxHP { get; set; }
    private PlayerController playerController; // 플레이어 이동 제어 스크립트

    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    public Color flashColor = Color.red; // 피격 시 색상
    public float flashDuration = 0.1f; // 피격 색상 유지 시간
    public float invincibilityDuration = 0.5f; // 무적 시간
    private bool isInvincible = false; // 무적 여부

    
    private Color originalColor; // 원래 색상

    public GameObject gravestonePrefab; // 묘비 프리팹
    public bool isDead = false; // 사망 여부
    private GameObject currentGravestone; // 현재 묘비

    public UnityEvent OnPlayerDie; // 사망 이벤트
    public UnityEvent OnPlayerRevive; // 부활 이벤트

    public AudioClip[] hitSounds; // 피격 소리
    public AudioClip deathSound;  // 사망 소리
    private AudioSource audioSource; // 오디오 소스

    public SurvivalTimer survivalTimer; // 생존 타이머 스크립트

    public GameObject revivalPrefab; // Revival 프리팹
    private GameObject currentRevival; // 현재 Revival 오브젝트

    private MultiPlayerController multiPlayerController;
    private Collider2D playerCollider; // Collider2D
    private Rigidbody2D playerRigidbody; // Rigidbody2D

    public LevelUpStat playerStat; //플레이어 기본 스탯
    private MultiPlayerHPBar floatingHpBar;
    private MultiPlayerHPBar statusHPBar;
    private MultiBulletLauncher _bulletLauncher;
    public PlayerData GetPlayerData()
    {
        return playerData;
    }

    void Start()
    {
        playerStatInit();
        multiPlayerController = GetComponent<MultiPlayerController>();
        _bulletLauncher = GetComponentInChildren<MultiBulletLauncher>();
        maxHP = playerData.MaxHp; // HP 초기화
        currentHP = playerData.MaxHp; // HP 초기화
        playerController = GetComponent<PlayerController>(); // PlayerController 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 가져오기
        originalColor = spriteRenderer.color; // 원래 색상 저장
        GameObject statusUI = GameObject.Find("Player Status UI Canvas");
        GameObject floatubgUI = transform.parent.Find("Player Floating UI Canvas").gameObject;
        floatingHpBar = floatubgUI.GetComponentInChildren<MultiPlayerHPBar>();
        statusHPBar = statusUI.GetComponentInChildren<MultiPlayerHPBar>();
        
        playerCollider = GetComponent<Collider2D>(); // Collider2D 가져오기
        playerRigidbody = GetComponent<Rigidbody2D>(); // Rigidbody2D 가져오기

        // AudioSource 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource 변수: AudioSource 이름의 컴포넌트가 할당되거나 존재하지 않습니다.");
        }
    }
    //플레이어 스탯 초기화
    void playerStatInit()
    {
        playerStat = new LevelUpStat();
        playerStat.damage = 1;
        playerStat.maxHealth = playerData.MaxHp;
        playerStat.armor = playerData.Armor;
        playerStat.moveSpeed = 1;
        playerStat.maxAmmo = 1;
        playerStat.criticalRate = 0.05f;
        playerStat.criticalDamage = 1.5f;
        
    }
    
    //레벨업 선택한 스탯 별로 플레이어 스탯을 올림
    public void AddLevelupStat(string addStatType, float value)
    {
        switch (addStatType)
        {
            //공격력이 올랐을 경우
            case "D":
                playerStat.damage += value;
                Debug.Log(addStatType + " " + playerStat.damage);
                break;
            //방어력이 올랐을 경우
            case "A":
                playerStat.armor += (int)value;
                Debug.Log(addStatType + " " + playerStat.armor);
                break;
            //최대 체력이 올랐을 경우
            case "H":
                playerStat.maxHealth += (int)value;

                if (!(currentHP <= 0))
                {
                    currentHP += (int)value;    
                }
                maxHP = playerStat.maxHealth;
                playerData.MaxHp = maxHP;
                RpcUpdateHpUI();
                Debug.Log(addStatType + " " + playerStat.maxHealth);
                break;
            //이동속도가 올랐을 경우
            case "S":
                Debug.Log(addStatType + " " + multiPlayerController.playerSpeed + " " + playerStat.moveSpeed + " " + value + " 1");
                playerStat.moveSpeed += value;
                multiPlayerController.playerSpeed = playerData.Speed * playerStat.moveSpeed;
                Debug.Log(addStatType + " " + multiPlayerController.playerSpeed + " " + playerStat.moveSpeed + " " + value + " 2");
                break;
            //최대 장전량이 올랐을 경우
            case "MA":
                playerStat.maxAmmo += value;
                _bulletLauncher.ammo = (int)(_bulletLauncher.weaponData.Ammo *playerStat.maxAmmo);
                RpcUpdateAmmoUI();
                Debug.Log(addStatType + " " + playerStat.maxAmmo + " " + _bulletLauncher.ammo + " " + value);
                break;
            //크리티컬 확률이 올랐을 경우
            case "CR":
                playerStat.criticalRate += value;
                Debug.Log(addStatType + " " + playerStat.criticalRate);
                break;
            //크리티컬 대미지가 올랐을 경우
            case "CD":
                playerStat.criticalDamage += value;
                Debug.Log(addStatType + " " + playerStat.criticalDamage);
                break;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcUpdateAmmoUI()
    {
        
        _bulletLauncher.initUI();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcUpdateHpUI()
    {
        if(HasInputAuthority){
            statusHPBar.Initialize(this);
        }
        floatingHpBar.Initialize(this);
    }
    // void OnEnable()
    // {
    //     // 동적 생성(활성화) 오브젝트 대상 이벤트 리스너 추가
    //     GameManager gameManager = FindObjectOfType<GameManager>();
    //     if (gameManager)
    //     {
    //         OnPlayerDie.AddListener(gameManager.OnPlayerDie);
    //     }
    // }
    //
    // void OnDisable()
    // {
    //     // 동적 생성(비활성화) 오브젝트 대상 이벤트 리스너 제거
    //     GameManager gameManager = FindObjectOfType<GameManager>();
    //     if (gameManager)
    //     {
    //         OnPlayerDie.RemoveListener(gameManager.OnPlayerDie);
    //     }
    // }

    void Update()
    {
        // HP가 0 이하이고 아직 죽지 않았을 때
        if (currentHP <= 0 && !isDead)
        {
            Debug.Log("Player HP is 0 or less. Player will die.");
            Die();
        }
    }

    public void Healing(int amount, bool isPercent)
    {
        if (HasStateAuthority)
        {
            ApplyHealing(amount, isPercent);
        }
        else
        {
            RequestHealingRpc(amount, isPercent);
        }
    }
    
    private void ApplyHealing(int amount, bool isPercent)
    {
        int healAmount = isPercent ? Mathf.RoundToInt(playerData.MaxHp * (amount / 100f)) : amount;
        currentHP = Mathf.Min(currentHP + healAmount, playerData.MaxHp);
        Debug.Log($"플레이어가 회복됨. 체력 : {currentHP} / {playerData.MaxHp}, {healAmount}만큼 회복됨");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RequestHealingRpc(int amount, bool isPercent)
    {
        ApplyHealing(amount, isPercent);
    }

    
    public void ApplyDamage(int damage)
    {
        if (currentHP > 0 && !isInvincible)
        {
            // 방어력을 고려하여 최소 1의 피해 적용
            currentHP -= Mathf.Max(1, damage - playerStat.armor);
            Debug.Log($"Player took {damage} damage. Current HP: {currentHP}");

            // 피격 효과음 재생
            PlayRandomHitSound();

            // 피격 시 색상 변경
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashRed());
            }

            // 무적 상태 시작
            StartCoroutine(BecomeInvincible());
        }
    }

    // 랜덤 피격 소리 재생
    private void PlayRandomHitSound()
    {
        if (hitSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, hitSounds.Length);
            audioSource.PlayOneShot(hitSounds[randomIndex]);
            Debug.Log($"Playing hit sound. Index: {randomIndex}");
        }
    }

    // 사망 처리
    void Die()
    {
        isDead = true;
        Debug.Log("Player Died!");

        // 타이머 중지
        if (survivalTimer != null)
        {
            survivalTimer.StopTimer();
            Debug.Log("Timer has been stopped.");
        }

        // 이동 멈춤
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("PlayerController has been disabled.");
        }

        // 태그 변경
        gameObject.tag = "DeadPlayer";
        transform.parent.tag = "DeadPlayer";
        Debug.Log("Player tag changed to DeadPlayer.");

        // 스프라이트 숨김
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            Debug.Log("SpriteRenderer disabled. Player sprite is no longer visible.");
        }

        // 하위 오브젝트 비활성화
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            Debug.Log($"Child object {child.name} has been deactivated.");
        }

        // 충돌 비활성화
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
            Debug.Log("Player Collider has been disabled.");
        }
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
            Debug.Log("Player Rigidbody has been set to kinematic (physics disabled).");
        }

        // 묘비 생성
        if (gravestonePrefab != null)
        {
            currentGravestone = Instantiate(gravestonePrefab, transform.position, Quaternion.identity);
            Debug.Log("Gravestone has been created.");
        }
        else
        {
            Debug.LogError("Gravestone prefab is not assigned.");
        }

        // Revival 오브젝트 생성 (자식으로 생성)
        if (revivalPrefab != null)
        {
            currentRevival = Instantiate(revivalPrefab, transform.position + new Vector3(0, 0.27f, 0), Quaternion.identity, transform);
            Debug.Log("Revival object has been created.");
        }
        else
        {
            Debug.LogError("Revival prefab is not assigned.");
        }

        // 사망 소리 재생
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
            Debug.Log("Death sound has been played.");
        }

        // 사망 이벤트 호출
        OnPlayerDie?.Invoke();
    }

    // 부활 처리
    public void Revive()
    {
        // 체력 회복
        currentHP = playerData.MaxHp;
        Debug.Log("Player's health has been restored to maximum.");

        // 사망 플래그 해제
        isDead = false;
        Debug.Log("isDead flag reset to false.");

        // 태그 복원
        gameObject.tag = "Player";
        transform.parent.tag = "Player";
        Debug.Log("Player's tag has been reverted to Player.");

        // 이동 및 스프라이트 렌더러 활성화
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("PlayerController has been enabled.");
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Debug.Log("SpriteRenderer has been enabled.");
        }

        // 하위 오브젝트 활성화
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
            Debug.Log($"Child object {child.name} has been reactivated.");
        }

        // 충돌 활성화
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
            Debug.Log("Player Collider has been enabled.");
        }
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
            Debug.Log("Player Rigidbody is no longer kinematic (physics enabled).");
        }

        // 묘비 제거
        if (currentGravestone != null)
        {
            Destroy(currentGravestone);
            Debug.Log("Gravestone has been destroyed after revival.");
        }

        // Revival 오브젝트 제거
        if (currentRevival != null)
        {
            Destroy(currentRevival);
            Debug.Log("Revival object has been destroyed after revival.");
        }

        // 부활 이벤트 호출
        OnPlayerRevive?.Invoke();

        Debug.Log("Player Revived!");
    }

    // 피격 시 색상 변경 코루틴
    IEnumerator FlashRed()
    {
        Debug.Log("Player is flashing red due to damage.");
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    // 무적 상태 코루틴
    IEnumerator BecomeInvincible()
    {
        Debug.Log("Player is now invincible.");
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        Debug.Log("Player is no longer invincible.");
    }
}
