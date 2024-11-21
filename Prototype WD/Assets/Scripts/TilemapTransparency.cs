using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapTransparency : MonoBehaviour
{
    private Tilemap tilemap;

    void Start()
    {
        // Tilemap 컴포넌트를 가져옵니다.
        tilemap = GetComponent<Tilemap>();
    }

    // 플레이어가 트리거에 들어올 때 호출됩니다.
    void OnTriggerEnter2D(Collider2D other)
    {
        // 태그가 "Player"인지 확인합니다.
        if (other.CompareTag("Player"))
        {
            // Tilemap의 색상을 반투명하게 변경합니다.
            Color color = tilemap.color;
            color.a = 0.5f; // 투명도 값을 조정합니다 (0.0f ~ 1.0f).
            tilemap.color = color;
        }
    }

    // 플레이어가 트리거에서 나갈 때 호출됩니다.
    void OnTriggerExit2D(Collider2D other)
    {
        // 태그가 "Player"인지 확인합니다.
        if (other.CompareTag("Player"))
        {
            // Tilemap의 색상을 원래대로 돌립니다.
            Color color = tilemap.color;
            color.a = 1.0f; // 원래 투명도 값으로 복원합니다.
            tilemap.color = color;
        }
    }
}