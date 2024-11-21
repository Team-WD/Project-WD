using UnityEngine;
using UnityEngine.UI;

public class PlayerExpBar : MonoBehaviour
{
    private GameManager gameManager;
    private Slider slider;

    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        slider = GetComponent<Slider>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager를 찾을 수 없습니다!");
        }

        if (slider == null)
        {
            Debug.LogError("Slider 컴포넌트를 찾을 수 없습니다!");
        }

        UpdateMaxValue();
    }

    private void Update()
    {
        if (gameManager != null && slider != null)
        {
            slider.value = gameManager.exp;
            
            // 최대값이 변경되었는지 확인
            if (slider.maxValue != gameManager.expMax)
            {
                UpdateMaxValue();
            }
        }
    }

    public void UpdateMaxValue()
    {
        if (slider != null && gameManager != null)
        {
            slider.maxValue = gameManager.expMax;
        }
    }
}