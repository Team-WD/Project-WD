using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
        playerTransform = gameObject.transform.parent.gameObject.transform.parent.transform;
    }
    
    void Update()
    {
        HandleMouseDirection();
    }

    void HandleMouseDirection()
    {
        // 마우스 위치를 월드 좌표로 가져오기
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z축을 0으로 설정하여 2D 환경에서의 마우스 위치를 정확히 맞춤

        // 플레이어와 마우스 간의 방향 벡터 계산
        Vector3 direction = mousePosition - playerTransform.position;

        // 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 무기 회전
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 플레이어의 로컬 스케일을 기준으로 무기 반전
        if (direction.x > 0)
        {
            // 마우스가 플레이어의 우측에 있을 때
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        }
        else
        {
            // 마우스가 플레이어의 좌측에 있을 때
            transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), -Mathf.Abs(initialScale.y), initialScale.z);
        }

        // 무기의 위치를 플레이어 위치로 설정
        // transform.position = playerTransform.position;
    }
}