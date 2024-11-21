using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiPlayerHPBar : MonoBehaviour
{
    public Player player;

    private void Start()
    {
        Initialize(player);
    }

    public void Initialize(Player player)
    {
        this.player = player;
        if(player.maxHP ==null || player.maxHP == 0){
            this.GetComponent<Slider>().maxValue = player.playerData.MaxHp;
        }
        else
        {
            this.GetComponent<Slider>().maxValue = player.maxHP;
        }
        
    }

    private void Update()
    {
        this.GetComponent<Slider>().value = player.currentHP;
    }
}
