using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MultiPlayerController : NetworkBehaviour
{
    private CharacterController _controller;
    public Camera cam;
    public float playerSpeed = 5f;
    private Vector3 initialScale; // 초기 플레이어 스케일
    private Vector3 nameScale;
    private Animator animator;

    
    private Rigidbody2D rb;

    [Networked, OnChangedRender(nameof(isMovingAnimation))]
    private bool isMoving { get; set; }

    public GameObject playerUI;
    private Player player; // Player 스크립트 참조
    public NetworkManager networkManager;
    public GameObject nickNameObject;
    [Networked] public string nickName { get; set; }

    public GameManager gameManager;
    
    public SkillCooldown skillCooldown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 충돌 감지 모드 설정

        // 물리 소재 생성 및 적용 (마찰력을 제거)
        PhysicsMaterial2D noFrictionMaterial = new PhysicsMaterial2D();
        noFrictionMaterial.friction = 0; // 마찰력 제거
        Collider2D collider = GetComponent<Collider2D>();
        collider.sharedMaterial = noFrictionMaterial; // 물리 소재 적용

        initialScale = transform.localScale;
        nameScale = transform.GetChild(0).localScale;
        animator = GetComponent<Animator>();

        player = GetComponent<Player>(); // Player 스크립트 참조
        playerSpeed = player.playerData.Speed;
    }

    public void Start()
    {
        UpdateNickNameUI();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority) // 로컬 플레이어만 처리
        {
            findCamera();
            initStatusUI();

            // 로컬 플레이어의 닉네임 설정 요청
            networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            if (nickNameObject == null)
            {
                nickNameObject = transform.Find("Player Floating UI Canvas/NickName").gameObject;
            }

            RPC_RequestUpdateNickName(networkManager.nickName);
        }

        initWeapon();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestUpdateNickName(string newNickName)
    {
        RPC_UpdateNickName(Object.InputAuthority, newNickName);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateNickName(PlayerRef playerRef, string newNickName)
    {
        if (Object.InputAuthority == playerRef)
        {
            nickName = newNickName;
            UpdateNickNameUI();
        }
    }

    private void UpdateNickNameUI()
    {
        if (nickNameObject != null)
        {
            nickNameObject.GetComponent<TextMeshProUGUI>().text = nickName;
        }
    }

    public void findCamera()
    {
        cam = Camera.main;

        if (cam != null)
        {
            CameraFollow cameraFollow = cam.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.player = transform; // 로컬 플레이어만 카메라가 따라감
            }
            else
            {
                Debug.LogWarning("CameraFollow 스크립트가 카메라에 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("메인 카메라를 찾을 수 없습니다.");
        }
    }

    public void initStatusUI()
    {
        // 자식에서 bulletLauncher 컴포넌트 찾기
        MultiBulletLauncher bulletLauncher = GetComponentInChildren<MultiBulletLauncher>();
        GameObject statusUI = GameObject.Find("Player Status UI Canvas");

        MultiAmmoUI playerAmmoUi = statusUI.GetComponentInChildren<MultiAmmoUI>();

        MultiPlayerHPBar playerHPBarS = statusUI.GetComponentInChildren<MultiPlayerHPBar>();

        Player player = this.GetComponent<Player>();
        
        skillCooldown = statusUI.GetComponentInChildren<SkillCooldown>();
        
        GameObject playerImageObj = GameObject.Find("Player Image");
        if (player != null)
        {
            playerHPBarS.Initialize(player);
        }
        else
        {
            Debug.Log("playerHPBar is null");
        }

        if (bulletLauncher != null)
        {
            if (playerAmmoUi != null)
            {
                playerAmmoUi.Initialize(bulletLauncher);
            }
        }

        if (playerImageObj != null)
        {
            playerImageObj.GetComponent<Image>().sprite = this.GetComponent<SpriteRenderer>().sprite; 
        }

        if (skillCooldown != null)
        {
            skillCooldown.Initialize(player);
        }
        else
        {
            Debug.Log("skillCooldown is null");
        }
    }
    
    public void initWeapon()
    {
        MultiBulletLauncher bulletLauncher = GetComponentInChildren<MultiBulletLauncher>();
        if (bulletLauncher != null)
        {
            bulletLauncher.Initialize();
        }
        else
        {
            Debug.Log("bulletLauncher is null");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (nickName != nickNameObject.GetComponent<TextMeshProUGUI>().text)
        {
            UpdateNickNameUI();
        }

        if (gameManager.isPaused)
        {
            return;
        }

        if (HasStateAuthority) // 네트워크 권한이 있는지 확인 (로컬 플레이어만 이동 처리)
        {
            if (player.currentHP > 0) // HP가 0보다 클 때만 이동
            {
                // Debug.Log("이동이동합니다");
                HandleMovement();
            }
            else
            {
                // HP가 0이하일 경우 이동을 멈추고 속도를 0으로 설정
                rb.velocity = Vector2.zero; // 이동을 멈춤
                isMoving = false; // 이동 상태를 false로 설정
            }
        }

        if (HasInputAuthority)
        {
            HandleMouseDirection();

            if (Input.GetKey(KeyCode.F))
            {
                if (player.currentHP <= 0)
                    return;
                
                // Debug.Log($"MultiPlayerController - Input.GetKey(KeyCode.F) - 회복버튼 눌림");
                if (skillCooldown.isOnCooldown)
                    return;

                player.Healing(5, false);
                skillCooldown.ActivateSkill();
            }
        }
    }

    public void HandleMovement()
    {
        if (GetInput(out NetworkInputData data))
        {
            Vector2 movement = data.direction * playerSpeed * Runner.DeltaTime;
            rb.MovePosition(rb.position + movement); // transform.position 대신 MovePosition 사용
            isMoving = data.direction.x != 0 || data.direction.y != 0;
        }
    }

    void HandleMouseDirection()
    {
        // 마우스 위치를 월드 좌표로 가져오기
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z축을 0으로 설정하여 2D 환경에서의 마우스 위치를 정확히 맞춤

        // 플레이어와 마우스 간의 방향 벡터 계산
        Vector3 direction = mousePosition - this.transform.position;

        // 플레이어의 스케일을 좌우 반전
        if (direction.x > 0)
        {
            //transform.Rotate(0f, 180f, 0f);
            this.transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, 1);
        }
        else
        {
            //transform.Rotate(0f, 0, 0f);
            this.transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, 1);
        }
    }

    void isMovingAnimation()
    {
        animator.SetBool("IsMoving", isMoving);
    }
}