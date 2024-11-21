using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    // 총알 데이터 가져오기
    private BulletLauncher bulletLauncher;

    // 총알 변수
    private int ammo;
    private int currentAmmo;

    private void Start()
    {
        bulletLauncher = FindObjectOfType<BulletLauncher>();
        ammo = bulletLauncher.weaponData.Ammo;
    }

    private void Update()
    {
        currentAmmo = bulletLauncher.currentAmmo;
        this.GetComponent<TMP_Text>().text = currentAmmo + "/" + ammo;
    }
}