using System.Collections;
using UnityEngine;

public class PlayerTriggerArrowUpdate : MonoBehaviour
{
    private ArrowController arrowController;  // 에로우 컨트롤러 참조 (인스펙터에서 지정하지 않아도 됨)

    private bool isTriggered = false;  // 중복 처리를 막기 위한 플래그

    private void Start()
    {
        // ArrowController를 자동으로 찾기
        arrowController = FindObjectOfType<ArrowController>();

        if (arrowController == null)
        {
            Debug.LogWarning("ArrowController could not be found automatically.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어가 트리거에 진입하고 아직 처리되지 않은 경우
        if (other.CompareTag("Player") && !isTriggered)
        {
            // 에로우 가이드 업데이트
            UpdateArrowGuide();

            // 중복 처리를 막기 위해 플래그 설정
            isTriggered = true;

            // 스포너 오브젝트 비활성화 (필요에 따라 삭제 가능)
            Destroy(gameObject);
        }
    }

    private void UpdateArrowGuide()
    {
        // 에로우 컨트롤러가 할당된 경우 타겟을 갱신
        if (arrowController != null)
        {
            arrowController.FindAndAssignTargets();
            Debug.Log("Arrow guide updated successfully.");
        }
        else
        {
            Debug.LogWarning("ArrowController reference is missing.");
        }
    }
}
