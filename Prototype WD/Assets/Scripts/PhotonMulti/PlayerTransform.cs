using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerTransfrom : NetworkBehaviour
{
    public Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void LateUpdate()
    {
        this.transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z);
    }
}
