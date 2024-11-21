using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpStat
{
    public string statType;
    //주는 대미지
    public float damage;
    //이동속도
    public float moveSpeed;
    //최대 장전 총탄
    public float maxAmmo;
    //최대 체력
    public int maxHealth;
    //크리티컬 확률
    public float criticalRate;
    //크리티컬 대미지
    public float criticalDamage;
    //방어력
    public int armor;
    
    public void initialize()
    {
        damage = 0;
        moveSpeed = 0;
        maxAmmo = 0;
        maxHealth = 0;
        criticalRate = 0;
        criticalDamage = 0;
        armor = 0;
    }
}
