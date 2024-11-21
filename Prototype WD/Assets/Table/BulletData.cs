using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game Data/Enemy Data")]
public class BulletData : ScriptableObject
{
    public int Id;
    public string Name;
    public float DamageMultiplier;
    public float ShootSpeed;
}