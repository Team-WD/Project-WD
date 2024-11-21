using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LevelUp Data", menuName = "Game Data/LevelUp Data")]
public class LevelUpData : ScriptableObject
{
    public int id;
    public Sprite icon;
    public string upgradeDescription;
    public string flavorText;
    public string type;
    public float value;
}
