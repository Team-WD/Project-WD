// PlayerFireAttack.cs

using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Fusion;
using UnityEngine;

public class MultiPlayerFireAttack : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;

    private void Update()
    {
        if (HasInputAuthority && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            Vector3 direction = (mousePosition - transform.position).normalized;
            
            Debug.unityLogger.Log("sadqw");
            // 로컬에서 총알 생성 및 발사
            FireBulletLocally(direction);
            
            // 네트워크를 통해 다른 클라이언트에게 발사 신호 전송
            RPC_SignalBulletFired(direction);
        }
    }

    #region RPC Methods
    private void FireBulletLocally(Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;
        // 필요한 경우 총알의 수명 관리 로직 추가
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SignalBulletFired(Vector3 direction)
    {
        if (!HasStateAuthority) // 발사한 플레이어 외의 클라이언트에서만 실행
        {
            FireBulletLocally(direction);
        }
    }

    #endregion
}
