using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class Mission2Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // 타이머 텍스트를 표시할 UI
    public Mission2 mission2Controller; // mission2 에서 데이터 가져오기 

    private float missionTime;
    private float triggerTime;

    public UnityEvent OnSpawnTime;

    private void OnEnable()
    {
        // 미션 생존 시간 초기화
        missionTime = mission2Controller.surviveTime;
        // 웨이브 발생 시간 초기화
        triggerTime = mission2Controller.surviveTime;
    }

    private void Update()
    {
        // 시간이 유효할 때만 작동

        if (missionTime >= 0)
        {
            // 시간 깎기
            missionTime -= Time.deltaTime;
            // 타이머 텍스트 표시
            DisplayTime(missionTime);
            // 웨이브 발생 시간이 되면 이벤트 호출
            if (missionTime <= triggerTime)
            {
                for (int i = 0; i < 100; i++)
                {
                    OnSpawnTime.Invoke();
                }

                // OnSpawnTime.Invoke();
                triggerTime -= 30;
            }
        }
        else
        {
            // 시간이 다 되면 미션 클리어
            mission2Controller.Mission2Clear();
            // 미션 클리어 이벤트 호출
            mission2Controller.OnMission2Clear.Invoke();
        }
    }
    
    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); // 분
        float seconds = Mathf.FloorToInt(timeToDisplay % 60); // 초
        float milliseconds = (timeToDisplay % 1) * 1000; // 밀리초

        // 분:초:밀리초 형식으로 타이머 텍스트 표시
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, Mathf.FloorToInt(milliseconds / 10));
    }
}


// public void StopTimer()
// {
//     // 타이머를 멈추지만, 텍스트는 화면에 남김
//     timerIsRunning = false;
//     Debug.Log("Timer has been stopped.");
// }