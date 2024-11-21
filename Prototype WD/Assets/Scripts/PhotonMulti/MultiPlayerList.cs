using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class MultiPlayerList : MonoBehaviour
{
    public List<GameObject> players = new List<GameObject>();
    
    // 기능 구현 우선을 위해서 FixedUpdateNetwork 이벤트 상에서 GetPlayersList 함수를 호출하였습니다.
    // 향후 최적화를 위해 플레이어 스폰시에 GetPlayersList 함수를 호출하도록 수정해야 할 필요가 있습니다.
    void FixedUpdate()
    {
        // GetPlayersList();
    }

    // 플레이어 리스트를 가져오는 함수
    public void GetPlayersList()
    {
        players.Clear();

        Player[] playerComponents = FindObjectsOfType<Player>();
        foreach (Player player in playerComponents)
        {
            players.Add(player.gameObject);
        }
    }
}
