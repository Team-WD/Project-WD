using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Game Data/Player Data")]
public class PlayerData : ScriptableObject
{
    public int Id;
    public string Name;
    public int Level;
    public int Exp;
    public int MaxHp;
    public int Heal;
    public int Armor;
    public float Speed;
}