using System.Collections;
using UnityEngine;

public class Revival : MonoBehaviour
{
    public Sprite[] revivalSprites; // Revival 게이지 스프라이트 배열
    public SpriteRenderer revivalRenderer; // Revival UI를 표시할 SpriteRenderer
    public float fillInterval = 0.5f; // 게이지 채우기 간격
    private readonly float revivalRange = 0.5f; // Revival 범위

    private int currentIndex = 0; // 현재 게이지 인덱스
    private Coroutine fillCoroutine = null; // 진행 중인 코루틴
    private bool isFilling = false; // 게이지 채우기 여부

    private Player deadPlayer; // 죽은 플레이어의 Player 컴포넌트
    private Transform parentTransform; // 부모의 Transform

    void Start()
    {
        if (revivalRenderer != null)
        {
            revivalRenderer.enabled = false; // Revival UI 숨김
        }

        // 부모 오브젝트에서 Player 컴포넌트 가져오기
        deadPlayer = GetComponentInParent<Player>();
        if (deadPlayer == null)
        {
            Debug.LogError("Dead Player component not found in parent!");
        }

        // 부모의 Transform 가져오기
        parentTransform = transform.parent;
        if (parentTransform == null)
        {
            Debug.LogError("Parent transform not found!");
        }
    }

    void Update()
    {
        if (deadPlayer == null || !deadPlayer.isDead)
        {
            // 죽은 플레이어가 없거나 이미 부활한 경우 Revival 오브젝트 제거
            Destroy(gameObject);
            return;
        }

        // 살아있는 플레이어 탐지
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        bool livingPlayerInRange = false; // 범위 내 살아있는 플레이어 존재 여부

        foreach (GameObject playerObject in playerObjects)
        {
            // 죽은 플레이어 제외
            if (playerObject == deadPlayer.gameObject)
                continue;

            // Player 컴포넌트 가져오기
            Player playerComponent = playerObject.GetComponent<Player>();
            if (playerComponent == null)
                continue;

            // 살아있는 플레이어인지 확인
            if (playerComponent.isDead)
                continue;

            // 거리 계산
            float distance = Vector3.Distance(transform.position, playerObject.transform.position);

            // 범위 내에 있는지 확인
            if (distance <= revivalRange)
            {
                livingPlayerInRange = true;
                break;
            }
        }

        if (livingPlayerInRange)
        {
            if (!isFilling)
            {
                // Revival 시작
                StartRevival();
            }
        }
        else
        {
            if (isFilling)
            {
                // Revival 중지
                StopRevival();
            }
        }

        // 부모의 좌우 반전 상태에 따라 Revival 오브젝트의 방향 조정
        AdjustRevivalDirection();
    }

    // Revival 오브젝트의 방향을 조정하는 메서드
    private void AdjustRevivalDirection()
    {
        if (parentTransform != null)
        {
            Vector3 parentScale = parentTransform.localScale;
            Vector3 revivalScale = transform.localScale;

            // 부모가 반전되었을 경우 Revival 오브젝트의 X 스케일을 반전시켜 보정
            revivalScale.x = parentScale.x < 0 ? -Mathf.Abs(revivalScale.x) : Mathf.Abs(revivalScale.x);
            transform.localScale = revivalScale;

            // Revival 오브젝트의 회전을 고정하여 방향을 일정하게 유지
            transform.rotation = Quaternion.identity;
        }
    }

    // Revival 시작 메서드
    private void StartRevival()
    {
        if (revivalRenderer != null)
        {
            revivalRenderer.enabled = true; // Revival UI 표시
            revivalRenderer.sprite = revivalSprites[currentIndex]; // 현재 스프라이트 설정
        }

        isFilling = true;
        fillCoroutine = StartCoroutine(FillRevival()); // 게이지 채우기 코루틴 시작
        Debug.Log($"Started revival for {deadPlayer.gameObject.name}");
    }

    // Revival 중지 메서드
    private void StopRevival()
    {
        isFilling = false;

        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }

        if (revivalRenderer != null)
        {
            revivalRenderer.enabled = false; // Revival UI 숨김
        }

        currentIndex = 0; // 게이지 초기화

        Debug.Log($"Stopped revival for {deadPlayer.gameObject.name}");
    }

    // Revival 게이지 채우기 코루틴
    private IEnumerator FillRevival()
    {
        if (revivalRenderer == null || revivalSprites == null || revivalSprites.Length == 0)
        {
            Debug.LogError("Revival Renderer or sprites are not assigned!");
            yield break;
        }

        while (currentIndex < revivalSprites.Length && isFilling)
        {
            if (revivalSprites[currentIndex] != null)
            {
                revivalRenderer.sprite = revivalSprites[currentIndex];
            }

            Debug.Log($"Revival progress for {deadPlayer.gameObject.name}: {currentIndex + 1}/{revivalSprites.Length}");

            currentIndex++;
            yield return new WaitForSeconds(fillInterval);

            if (!isFilling)
            {
                yield break;
            }
        }

        if (currentIndex >= revivalSprites.Length)
        {
            RevivalComplete();
        }
    }

    // Revival 완료 메서드
    private void RevivalComplete()
    {
        if (deadPlayer != null)
        {
            deadPlayer.Revive(); // 플레이어 부활
            Debug.Log($"{deadPlayer.gameObject.name} has been revived.");
        }

        if (revivalRenderer != null)
        {
            revivalRenderer.enabled = false;
        }

        // Revival 오브젝트 제거
        Destroy(gameObject);
    }

    // Revival 범위 Gizmo 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, revivalRange);
    }
}
