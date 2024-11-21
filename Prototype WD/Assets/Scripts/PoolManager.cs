using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
  
    // prefab을 저장할 배열
    public GameObject[] prefabs;
    // pool을 담당하는 리스트
    private List<GameObject>[] pools;

    private void Awake()
    {
        // pool을 초기화
        pools = new List<GameObject>[prefabs.Length];
        
        // pool 배열 내부 리스트 초기화
        for (int index = 0; index < pools.Length; index++)
        {
            pools[index] = new List<GameObject>();
        }
    }

    #region Public Methods

    public GameObject Get(int index)
    {
        // pool내부 object 최대 개수 제한
        if (ActiveChildCount() >= 300)
        {
            Debug.Log("Too Many Objects: Skip Object Pooling");
            return null;
        }
        
        GameObject select = null;

        // 선택한 pool에 유휴 상태(비활성화된)의 object에 접근
        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                // 발견하면 select 변수에 할당
                select = item;
                select.SetActive(true);
                break;
            }
        }
            
        // 없으면
        if (!select)
        {
            // 새로 생성하여 select 변수에 할당
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
            
        }
        
        return select;
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
