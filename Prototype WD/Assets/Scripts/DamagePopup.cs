using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI damageText;
    public float disappearTime = 1f;
    public float scaleUpDuration = 0.2f;
    public float moveDuration = 1f;
    
    // 랜덤 오프셋 범위 설정
    public float randomOffsetX = 0.3f;
    public float randomOffsetY = 0.3f;

    public void Setup(int damageAmount, Vector3 hitPosition, bool isCritical)
    {
        // 랜덤 오프셋 생성
        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-randomOffsetX, randomOffsetX),
            UnityEngine.Random.Range(-randomOffsetY, randomOffsetY),
            0
        );

        // 충돌 지점에 랜덤 오프셋을 더한 위치 설정
        transform.position = hitPosition + randomOffset;

        // 데미지 값 설정
        damageText.text = damageAmount.ToString();
        
        // 텍스트 팝업 스케일 애니메이션
        transform.localScale = Vector3.zero;
        transform.DOScale(new Vector3(0.015f, 0.015f, 0.015f), scaleUpDuration).SetEase(Ease.OutBack);

        if (isCritical)
        {
            damageText.color = Color.yellow;
            damageText.fontStyle = FontStyles.Bold;
            damageText.outlineWidth = 0.2f;
            damageText.outlineColor = Color.black;
        }
        else
        {
            damageText.color = Color.white; 
        }

        // 텍스트를 위로 이동시키고 사라지게 하는 트윈 애니메이션
        transform.DOMove(transform.position + new Vector3(0, 0.5f, 0), moveDuration).SetEase(Ease.OutCubic);

        // 텍스트의 알파값을 서서히 줄여서 사라지게 함
        damageText.DOFade(0, disappearTime).SetDelay(moveDuration).OnComplete(() => Destroy(gameObject));
    }
}