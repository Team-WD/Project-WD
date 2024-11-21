using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    private Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        this.GetComponent<Slider>().maxValue = player.playerData.MaxHp;
    }

    private void Update()
    {
        this.GetComponent<Slider>().value = player.currentHP;
    }
}
