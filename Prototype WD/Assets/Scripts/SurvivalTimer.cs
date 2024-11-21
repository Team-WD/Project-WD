using UnityEngine;
using TMPro;

public class SurvivalTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;  // 타이머 텍스트를 표시할 UI
    public bool timerIsRunning = true; // 타이머는 게임이 시작되면 자동으로 동작
    private float elapsedTime = 0f;  // 경과 시간

    void Update()
    {
        if (timerIsRunning)
        {
            elapsedTime += Time.deltaTime;
            DisplayTime(elapsedTime);
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);  // 분
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);  // 초
        float milliseconds = (timeToDisplay % 1) * 1000;  // 밀리초

        // 분:초:밀리초 형식으로 타이머 텍스트 표시
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, Mathf.FloorToInt(milliseconds / 10));
    }

    public void StopTimer()
    {
        // 타이머를 멈추지만, 텍스트는 화면에 남김
        timerIsRunning = false;
        Debug.Log("Timer has been stopped.");
    }
}