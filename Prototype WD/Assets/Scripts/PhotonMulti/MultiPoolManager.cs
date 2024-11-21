using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MultiPoolManager : NetworkBehaviour
{
    // prefab을 저장할 배열
    public NetworkObject[] prefabs; // index 0은 일반 좀비, index 1은 보스 등
    // pool을 담당하는 리스트
    private List<NetworkObject>[] pools;

    private void Awake()
    {
        // pool을 초기화
        pools = new List<NetworkObject>[prefabs.Length];
        
        // pool 배열 내부 리스트 초기화
        for (int index = 0; index < pools.Length; index++)
        {
            pools[index] = new List<NetworkObject>();
        }
    }

    #region Public Methods

    // 일반적인 객체를 가져오는 메서드
    public NetworkObject Get(int index)
    {
        if (!Runner.IsServer) return null;

        // pool내부 object 최대 개수 제한
        if (ActiveChildCount() >= 50)
        {
            Debug.Log("Too Many Objects: Skip Object Pooling");
            return null;
        }

        NetworkObject select = null;
        // 선택한 pool에 유휴 상태(비활성화된)의 object에 접근
        foreach (NetworkObject item in pools[index])
        {
            if (!item.gameObject.activeSelf)
            {
                // 발견하면 select 변수에 할당
                select = item;
                select.gameObject.SetActive(true);
                break;
            }
        }

        // 없으면
        if (!select)
        {
            // 새로 생성하여 select 변수에 할당
            select = Runner.Spawn(prefabs[index], transform.position, Quaternion.identity);
            pools[index].Add(select);
        }

        return select;
    }

    // 보스 프리팹을 가져오는 메서드
    public NetworkObject GetBoss(string bossName)
    {
        if (!Runner.IsServer) return null;

        NetworkObject select = null;

        // 풀에서 보스 프리팹을 검색하여 비활성화된 보스를 찾기
        foreach (NetworkObject item in pools[1]) // Boss가 있는 풀을 사용
        {
            if (item.gameObject.name == bossName && !item.gameObject.activeSelf)
            {
                select = item;
                select.gameObject.SetActive(true);
                break;
            }
        }

        // 풀에 보스가 없으면 새로 생성
        if (select == null)
        {
            select = Runner.Spawn(prefabs[1], transform.position, Quaternion.identity); // 보스 스폰
            select.gameObject.name = bossName; // 이름을 설정하여 보스 구분
            pools[1].Add(select);
        }

        // 스폰 후 보스의 스케일 설정 (예시로 스케일 5배 적용)
        select.transform.localScale = new Vector3(5f, 5f, 5f); // 원하는 스케일로 설정

        // **Animator 상태 초기화** - 보스의 상태와 애니메이터를 초기 상태로 설정
        Animator bossAnimator = select.GetComponent<Animator>(); // Animator를 직접 가져옴
        if (bossAnimator != null)
        {
            bossAnimator.Rebind(); // 애니메이터 상태를 강제로 초기화
            bossAnimator.SetBool("IsDead", false); // 죽음 애니메이션 비활성화
        }

        return select;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RequestSpawnRpc(int index)
    {
        // 서버에서 스폰 후 모든 클라이언트에 동기화
        NetworkObject spawnedObject = Get(index);
        if (spawnedObject != null)
        {
            Runner.Spawn(spawnedObject, inputAuthority: Runner.LocalPlayer);
        }
    }

    #endregion

    #region Private Methods

    // 활성화된 poolManager의 자식 개수를 반환하는 함수
    private int ActiveChildCount()
    {
        int count = 0;

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                count++;
            }
        }

        return count;
    }

    #endregion
}
