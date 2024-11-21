using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;

public class BulletPenetrate : MultiBullet
{
    private float penetratingCount;

    public override void Initialize(WeaponData weapon, BulletData bullet,Player player)
    {
        base.Initialize(weapon, bullet,player);
        penetratingCount = weaponData.PenetratingPower;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            penetratingCount--;
            if (penetratingCount < 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
