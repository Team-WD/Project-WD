using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Mission2Start : MonoBehaviour
{
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 미션 시작
            GetComponentInParent<Mission2>().StartMission();

            // 미션 시작 지점 비활성화
            gameObject.SetActive(false);
        }
    }
}