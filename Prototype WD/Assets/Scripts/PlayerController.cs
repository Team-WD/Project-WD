using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Player playerScript;
    private PlayerData playerData;
    private Rigidbody2D rb;
    private Vector3 initialScale;
    private Animator animator;

    void Start()
    {
        // Rigidbody2D 컴포넌트 가져오기z
        rb = GetComponent<Rigidbody2D>();

        // 플레이어의 초기 스케일 저장
        initialScale = this.transform.localScale;

        animator = GetComponent<Animator>();
        
        playerScript = GetComponent<Player>();
        if (playerScript != null)
        {
            playerData = playerScript.GetPlayerData();
        }
        else
        {
            Debug.LogError("Player script not found on this GameObject!");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleMouseDirection();
        UpdateAnimation();
    }

    void HandleMovement()
    {
        // 초기 속도를 0으로 설정
        Vector2 moveDirection = Vector2.zero;

        // WASD 키 입력에 따른 이동
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection.y = playerData.Speed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveDirection.y = -playerData.Speed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveDirection.x = -playerData.Speed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection.x = playerData.Speed;
        }

        // 이동 방향 벡터 정규화
        if (moveDirection != Vector2.zero)
        {
            moveDirection.Normalize();
        }

        // 정규화된 방향 벡터에 속도를 곱하여 최종 속도 계산
        Vector2 moveVelocity = moveDirection * playerData.Speed;

        // Rigidbody2D의 속도를 이동 속도로 설정
        rb.velocity = moveVelocity;
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
            // 마우스가 플레이어의 우측에 있을 때
            this.transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, 1);
        }
        else
        {
            // 마우스가 플레이어의 좌측에 있을 때
            this.transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, 1);
        }
    }

    void UpdateAnimation()
    {
        Vector2 velocity = rb.velocity;
        bool isMoving = velocity.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
    }
}