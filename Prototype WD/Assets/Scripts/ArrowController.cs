using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ArrowController : NetworkBehaviour
{
    public Transform[] targets;  // 여러 타겟을 담는 배열
    public Transform player;     // 플레이어 오브젝트의 위치
    private int currentTargetIndex = 0;  // 현재 타겟 인덱스
    private float radius = 0.65f; // 가상의 원 반지름

    private void Start()
    {
        // 로컬 플레이어인지 확인하여 화살표를 보이게 할지 결정
        if (!Object.HasInputAuthority)
        {
            // 다른 플레이어의 화살표는 비활성화
            this.gameObject.SetActive(false);
            return; // 로컬 플레이어가 아니면 더 이상 처리하지 않음
        }

        // 플레이어를 자동으로 찾기 (다른 스크립트에서 할당되지 않았다면)
        if (player == null)
        {
            player = transform.parent; // 부모 오브젝트에 플레이어가 있다고 가정 (필요시 상황에 맞게 변경)
        }

        // 타겟 리스트가 비어있을 경우에만 태그를 통해 동적으로 설정
        if (targets == null || targets.Length == 0)
        {
            FindAndAssignTargets();
        }

        // 첫 번째 타겟 설정 (존재하는 경우)
        if (targets.Length > 0)
        {
            currentTargetIndex = 0; // 첫 번째 타겟으로 설정
        }
        else
        {
            Debug.LogWarning("No targets found at the start!");
        }
    }

    void Update()
    {
        // 타겟 리스트를 주기적으로 갱신
        UpdateTargets();

        // 로컬 플레이어와 최소 하나 이상의 타겟이 있을 때 화살표 업데이트
        if (player != null && targets.Length > 0)
        {
            UpdateArrowPosition();
        }
    }

    // 타겟 리스트를 주기적으로 갱신하는 함수
    void UpdateTargets()
    {
        // 항상 타겟 리스트를 갱신하여 최신 상태 유지
        FindAndAssignTargets();
    }

    void UpdateArrowPosition()
    {
        // 현재 활성화된 타겟이 있는지 확인
        Transform currentTarget = GetCurrentActiveTarget();

        if (currentTarget != null)
        {
            // 플레이어와 타겟 사이의 방향 벡터 계산
            Vector3 direction = (currentTarget.position - player.position).normalized;

            // 플레이어 기준으로 방향 벡터를 반지름만큼 곱해 교차점 위치 계산
            Vector3 arrowPosition = player.position + direction * radius;
            
            // X축으로 -0.35만큼 이동
            arrowPosition.x -= 0.25f;

            // 화살표의 위치 업데이트 (Z축 고정, 2D 평면에서 동작)
            arrowPosition.z = transform.position.z;
            transform.position = arrowPosition;

            // 화살표의 회전을 타겟 방향으로 설정
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));  
        }
        else
        {
            // 활성화된 타겟이 없을 경우 처리 (필요시 다른 로직 추가)
            Debug.LogWarning("No active targets available!");
        }
    }

    // 타겟을 자동으로 찾아서 배열에 할당하는 함수
    public void FindAndAssignTargets()
    {
        // 특정 태그를 사용해 타겟 오브젝트를 찾음 (예: "Target" 태그)
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag("Target");

        // 각 GameObject의 Transform을 가져와서 배열에 저장
        targets = new Transform[targetObjects.Length];
        for (int i = 0; i < targetObjects.Length; i++)
        {
            targets[i] = targetObjects[i].transform;  // GameObject에서 Transform 추출
        }

        // 미리 정해둔 이름 순서대로 타겟을 정렬하는 방법
        string[] predefinedOrder = new string[] {
            "Vaccine (1)", "Vaccine (2)", "Vaccine (3)", "Vaccine (4)", "TG1", "TG2",
            "Start Position", "TG3", "TG4", "TGBoss", "NetBoss(Clone)"
        };

        System.Array.Sort(targets, (a, b) =>
        {
            int indexA = System.Array.IndexOf(predefinedOrder, a.name);
            int indexB = System.Array.IndexOf(predefinedOrder, b.name);

            // 이름이 predefinedOrder에 없으면 나중 순서로 배치
            if (indexA == -1) indexA = int.MaxValue;
            if (indexB == -1) indexB = int.MaxValue;

            return indexA.CompareTo(indexB);
        });

        // if (targets.Length > 0)
        // {
        //     Debug.Log("Targets automatically assigned and sorted by predefined name order.");
        // }
        // else
        // {
        //     Debug.LogWarning("No targets found with the 'Target' tag!");
        // }
    }

    // 현재 활성화된 타겟을 반환하는 함수
    Transform GetCurrentActiveTarget()
    {
        currentTargetIndex = 0;

        // 순차적으로 타겟을 검사하여 활성화된 타겟 반환
        while (currentTargetIndex < targets.Length)
        {
            if (targets[currentTargetIndex] != null && targets[currentTargetIndex].gameObject.activeInHierarchy)
            {
                return targets[currentTargetIndex];  // 우선순위가 높은 타겟을 먼저 반환
            }

            // 다음 타겟으로 넘어감
            currentTargetIndex++;
        }

        // 활성화된 타겟이 없으면 null 반환
        return null;
    }
}
