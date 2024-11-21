using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class MultiAmmoUI : MonoBehaviour
{
    // 총알 데이터 가져오기
    public MultiBulletLauncher bulletLauncher;

    // 총알 변수
    private int ammo;
    private int currentAmmo;

    private void Start()
    {
        Initialize(bulletLauncher);
    }

    // bulletLauncher를 초기화하는 메서드
    public void Initialize(MultiBulletLauncher launcher)
    {
        bulletLauncher = launcher;

        if (bulletLauncher != null)
        {
            ammo = bulletLauncher.ammo;
        }
    }

    private void Update()
    {
        if (bulletLauncher != null)
        {
            currentAmmo = bulletLauncher.currentAmmo;
            GetComponent<TMP_Text>().text = currentAmmo + "/" + ammo;
        }
    }
}