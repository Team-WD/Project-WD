using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;


public class LevelUpUI : NetworkBehaviour
{
    
    public int Index;
    public LevelUpStat stat;
    public GameObject[] levelupOptions;
    public GameManager gameManager;
    public GameObject WaitingUI;
    public List<UpgradeCardData> upgradeList = new List<UpgradeCardData>();

    public LevelUpData[] levelUpDataArray;


    private void Awake()
    {
        Init();
    }
    void Init()
    {

        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        WaitingUI = transform.parent.transform.Find("PlayerWaitingUI").gameObject;
        levelupOptions = new GameObject[3];
        // LoadCardData("Assets/Resources/UpgradeData/UpgradeList.csv");
        
        for (int i = 0; i < 3; i++)
        {
            levelupOptions[i] = transform.GetChild(i).gameObject;
            Debug.Log("levelupOption init");
        }
    }

    void LoadCardData(string filePath)
    {
        using (StreamReader sr = new StreamReader(filePath))
        {
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] values = line.Split(',');
                
                UpgradeCardData cardData = new UpgradeCardData
                {
                    iconPath = values[1],
                    upgradeDescription = values[2],
                    flavorText = values[3],
                    type = values[4],
                    value = values[5],
                };

                upgradeList.Add(cardData);
            }
        }
        Debug.Log("load card data");
    }
 
    public void initilaize()
    {
        this.gameObject.SetActive(true);
        Shuffle(levelUpDataArray);
        for (int i = 0; i < levelupOptions.Length; i++)
        {
            Debug.Log(levelUpDataArray[i].upgradeDescription);
            levelupOptions[i].GetComponent<LevelUpCardUI>().InitCard(levelUpDataArray[i]);
        }
        
    }


    void Shuffle(LevelUpData[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            LevelUpData temp = array[i];
            int randomIndex = Random.Range(i, array.Length);
            array[i] = array[randomIndex];
            array[randomIndex] = temp;

        }
        Debug.Log("Shuffle");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickLevelUp()
    {
        //클릭된 카드 가져오기
        GameObject clickedCard = EventSystem.current.currentSelectedGameObject;
        LevelUpStat selectedLevelUpStat = clickedCard.GetComponent<LevelUpCardUI>().stat;
        float value = 0f;
        //선택한 카드 종류에 따라 값을 저장
        switch (selectedLevelUpStat.statType)
        {
            case "D":
                value = selectedLevelUpStat.damage;
                break;
            case "A":
                value = selectedLevelUpStat.armor;
                break;
            case "H":
                value = selectedLevelUpStat.maxHealth;
                break;
            case "S":
                value = selectedLevelUpStat.moveSpeed;
                break;
            case "MA":
                Debug.Log("MA Selected " + selectedLevelUpStat.maxAmmo);
                value = selectedLevelUpStat.maxAmmo;
                break;
            case "CR":
                value = selectedLevelUpStat.criticalRate;
                break;
            case "CD":
                value = selectedLevelUpStat.criticalDamage;
                break;
        }
        // 버튼 클릭 시 바로 RPC 호출
        gameManager.RpcRequestLevelUp(Runner.LocalPlayer,selectedLevelUpStat.statType,value);
        Debug.Log("Level Up Selected, current value: " + gameManager.levelupSelect);

        this.gameObject.SetActive(false);
        if (gameManager.levelupSelect < Runner.ActivePlayers.Count())
        {
            WaitingUI.SetActive(true);
        }
    }

    
}

public class UpgradeCardData
{
    public string iconPath;
    public string upgradeDescription;
    public string flavorText;
    public string type;
    public string value;

    public string toString()
    {
        return upgradeDescription +", "+iconPath+", " + type + "," + flavorText + "," + value + ",";
    }
}