using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class Mission1CollectObject : MonoBehaviour
{
    public UnityEvent OnItemCollect;
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnItemCollect.Invoke();
            gameObject.SetActive(false);
        }
    }
}