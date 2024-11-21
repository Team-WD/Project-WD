using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public int Id;
    public string Name;
    public int MaxHp;
    public int Damage;
    public float Speed;
    public int Armor;
    public bool IsRanged;
    public float AttackSpeed;
    public float AttackRange;
    public int ExpDrop;
}