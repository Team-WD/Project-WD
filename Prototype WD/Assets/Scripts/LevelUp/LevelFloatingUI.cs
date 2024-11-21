using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelFloatingUI : MonoBehaviour
{
    GameManager gameManager;
    public TextMeshProUGUI level;
    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        level.text = "1";
    }

    // Update is called once per frame
    void Update()
    {
        level.text = gameManager.level.ToString();
    }
}
