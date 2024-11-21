using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpCardUI : MonoBehaviour
{
    public LevelUpStat stat;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI UpgradeDescription;
    public TextMeshProUGUI FlavorText;
    public Image Icon;
    public Sprite iconSprite;
    void Awake()
    {
        stat = new LevelUpStat();
        //레벨업할 스탯클래스 초기화
        stat.initialize();
        //레벨업 선택카드의 제목
        Title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        //레벨업 선택카드의 설명
        UpgradeDescription = transform.Find("UpgradeDescription").GetComponent<TextMeshProUGUI>();
        //레벨업 선택카드의 플레이버 텍스트
        FlavorText = transform.Find("FlavorText").GetComponent<TextMeshProUGUI>();
        //레벨업 선택카드의 스탯 아이콘
        Icon = transform.Find("IconFrame").GetChild(0).gameObject.GetComponent<Image>();
    }
    //카드 내용 초기화
    public void InitCard(LevelUpData card)
    {
        
        //선택된 카드 데이터를 받아서 카드를 초기화한다.
        UpgradeDescription.text = card.upgradeDescription;
        FlavorText.text = card.flavorText;
        Icon.sprite = card.icon;
        stat.statType = card.type;
        //강화 타입마다 강화할 스탯 저장
        switch (card.type)
        {
            case "D":
                stat.damage = card.value;
                Title.text = "공격력 증가";
                break;
            case "A":
                stat.armor = (int)card.value;
                Title.text = "방어력 증가";
                break;
            case "H":
                stat.maxHealth = (int)card.value;
                Title.text = "최대체력 증가";
                break;
            case "S":
                stat.moveSpeed = card.value;
                Title.text = "이동속도 증가";
                break;
            case "MA":
                stat.maxAmmo = card.value;
                Title.text = "탄창 수 증가";
                break;
            case "CR":
                stat.criticalRate = card.value;
                Title.text = "치명타 확률 증가";
                break;
            case "CD":
                stat.criticalDamage = card.value;
                Title.text = "치명타 대미지 증가";
                break;
        }
    }
    
}
