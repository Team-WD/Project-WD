using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Game Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public int Id;
    public string Name;
    public int Ammo;
    public float Damage;
    public float CriticalRate;
    public float CriticalDamage;
    public float FireRate;
    public float ReloadSpeed;
    public float PenetratingPower;
    public float Range;
    public float Spread;
    public int Tier;
}