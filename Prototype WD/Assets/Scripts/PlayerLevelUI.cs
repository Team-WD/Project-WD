using UnityEngine;
using TMPro;

public class PlayerLevelUI : MonoBehaviour
{
    private TextMeshProUGUI levelText;
    private GameManager gameManager;

    private void Start()
    {
        levelText = GetComponent<TextMeshProUGUI>();
        gameManager = FindObjectOfType<GameManager>();

        if (levelText == null)
        {
            Debug.LogError("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다!");
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager를 찾을 수 없습니다!");
        }

        UpdateLevelText();
    }

    public void UpdateLevelText()
    {
        if (levelText != null && gameManager != null)
        {
            levelText.text = $"{gameManager.level}";
        }
    }
}