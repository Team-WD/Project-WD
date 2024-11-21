using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{
    public Player player;
    
    public AudioClip healSkillSound; // 고정된 스킬 사운드
    public AudioSource audioSource;  // 오디오 소스 추가

    public Image cooldownImage;
    public TextMeshProUGUI cooldownText;
    public float maxCooldown = 30f;

    private float currentCooldown;
    public bool isOnCooldown = false;
    
    public void Initialize(Player player)
    {
        this.player = player;
        cooldownImage.fillAmount = 0;
        cooldownText.text = "";

        currentCooldown = 0;
        isOnCooldown = false;
    }
    
    void Update()
    {
        if (isOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            
            if (currentCooldown <= 0)
            {
                isOnCooldown = false;
                cooldownImage.fillAmount = 0;
                cooldownText.text = "";
            }
            else
            {
                // Update visual cooldown
                cooldownImage.fillAmount = currentCooldown / maxCooldown;

                // Update text cooldown
                string cooldownString;
                if (currentCooldown < 10f)
                {
                    cooldownString = currentCooldown.ToString("F1");
                }
                else
                {
                    cooldownString = Mathf.CeilToInt(currentCooldown).ToString();
                }
                cooldownText.text = cooldownString;
            }
        }
    }
    
    public void ActivateSkill()
    {
        if (!isOnCooldown)
        {
            // skill sound play
            if (healSkillSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(healSkillSound);
            }
            // Skill logic here
            Debug.Log("Skill activated!");

            // Start cooldown
            isOnCooldown = true;
            currentCooldown = maxCooldown;
        }
    }
}
