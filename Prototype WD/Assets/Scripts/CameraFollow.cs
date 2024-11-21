using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 카메라가 따라갈 타겟(플레이어)
    public Transform player;
    
    public float smoothTime = 0.3f; // 카메라 이동의 부드러움 정도
    
    // 스칼라
    public float scalar = 0.12f; 

    // 내부 변수
    private Camera mainCamera;
    private Vector3 currentVelocity;

    void Start()
    {

        // 메인 카메라 캐싱
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // 플레이어의 현재 위치를 변수로 저장
            Vector3 playerPosition = player.position;
            
            // 마우스 위치를 월드 좌표로 가져오기
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            
            // 마우스와 플레이어 위치의 gap
            Vector3 gap = playerPosition - mousePosition;
            
            
            // 카메라가 따라가야 할 새로운 위치
            Vector3 desiredPosition = playerPosition - gap * scalar;
            
            desiredPosition.z = -10.0f;
            
            // SmoothDamp를 사용하여 부드럽게 이동
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        }
    }
}