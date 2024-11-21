using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject gameoverUI; // 게임 오버 시 활성화할 UI 참조 변수
    public GameObject multiGameoverUI; // 멀티 플레이에서 게임 오버 시 활성화할 UI 참조 변수
    public GameObject playerStatusUI;
    public bool isPaused = false;  // 일시정지 상태를 네트워크로 동기화
    public MultiPlayerList multiPlayerList; // 플레이어 리스트 참조 변수

    public GameObject pauseMenu;
    // expMax의 경우, 레벨 경험치 테이블 데이터를 직접 참조하거나 요구 경험치 증가량 규칙을 완성하면 개편할 필요가 있습니다.
    public int level; // 파티 레벨
    [Networked] public int exp { get; set; } // 현재 경험치
    [Networked] public int expMax { get; set; } // 다음 레벨로 가기 위해 필요한 경험치
    [Networked] public int levelupSelect { get; set; } //레벨업 항목 선택한 유저 수
    public int killPoint; // 적 처치 수
    private bool isLevelup = false;
    private bool isSetting = false;
    public GameObject levelupUI;
    public GameObject settingUI;
    public GameObject watingUI;
    
  //  private ChangeDetector _changeDetector;
    
    #region MonoBehaviour Callbacks

    //c
    private void Awake()
    {
        //_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        level = 1;
    }
    // public override void Render()
    // {
    //     foreach (var change in _changeDetector.DetectChanges(this))
    //     {
    //         switch (change)
    //         {
    //             case nameof(exp):
    //                 exp = int.Parse(change);
    //                 break;
    //         }
    //     }
    // }
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(10f);
        level = 1;
        exp = 0;
        expMax = 5;
        killPoint = 0;
        // Player[] players = FindObjectsOfType<Player>();
        // foreach (Player player in players)
        // {
        //     player.OnPlayerDie.AddListener(OnPlayerDie);
        // }
        //
        // EnemyDamage[] enemyDamages = FindObjectsOfType<EnemyDamage>();
        // foreach (var enemyDamage in enemyDamages)
        // {
        //     enemyDamage.OnEnemyDie.AddListener(OnEnemyDie);
        // }
        

        
    }

    public override void FixedUpdateNetwork()
    {
        // Debug.Log(Runner.ActivePlayers.Count());
        // if (Input.GetKeyDown(KeyCode.Space) && HasStateAuthority)
        // {
        //     AddExp(5);
        // }
   
        
        if (levelupSelect >= Runner.ActivePlayers.Count() && HasStateAuthority)
        {
            levelupSelect = 0;
            watingUI = levelupUI.transform.parent.transform.Find("PlayerWaitingUI").gameObject;
            watingUI.SetActive(false);
            Debug.Log("isPaused Resume");
            RPC_SetPauseState(false);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
        // Debug.Log(multiPlayerList.players.Count);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetPauseState(bool pauseState,string status = "")
    {
        isPaused = pauseState;
        watingUI = levelupUI.transform.parent.transform.Find("PlayerWaitingUI").gameObject;
        watingUI.SetActive(false);
        if (isPaused)
        {
            PauseGameLocally(status);
        }
        else
        {
            ResumeGameLocally();
        }
    }

    private void PauseGameLocally(string status)
    {
        Time.timeScale = 0f;  // 로컬 클라이언트에서 시간 정지
        if (pauseMenu != null)
            pauseMenu.SetActive(true); // UI 표시

        if (status == "levelup")
        {
            levelupSelect = 0;
            levelupUI.GetComponent<LevelUpUI>().initilaize();
            //levelupUI.SetActive(true); // UI 표시
            
        }else if (status == "setting")
        {
            settingUI.SetActive(true);
            isSetting = false;
        }
    }

    private void ResumeGameLocally()
    {
        Time.timeScale = 1f;  // 로컬 클라이언트에서 시간 재개
        if (pauseMenu != null)
            pauseMenu.SetActive(false);  // UI 숨김
    }
    public void AddExp(int amount)
    {
        int previousExp = exp;
        exp += amount;
        Debug.Log($"경험치 추가: {amount}, 이전: {previousExp}, 현재: {exp}, 다음 레벨까지: {expMax - exp}");

        while (exp >= expMax)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        exp -= expMax;
        expMax = CalculateNextLevelExp(level);
        Debug.Log($"레벨 업! 현재 레벨: {level}");
        isLevelup = true;
        if(HasStateAuthority)
            RPC_SetPauseState(true,"levelup");
        
        // EXPBar 업데이트
        PlayerExpBar expBar = FindObjectOfType<PlayerExpBar>();
        if (expBar != null)
        {
            expBar.UpdateMaxValue();
        }

        // LevelText 업데이트
        PlayerLevelUI levelUI = FindObjectOfType<PlayerLevelUI>();
        if (levelUI != null)
        {
            levelUI.UpdateLevelText();
        }

        // 여기에 레벨업 시 추가할 로직을 넣을 수 있습니다.
        // 예: 플레이어 스탯 증가, 새로운 능력 해금 등
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestLevelUp(PlayerRef player, string statType, float value)
    {
        levelupSelect += 1;
        
        // 호출한 플레이어의 플레이어 스크립트 찾기
        Player levelUpPlayer = Runner.GetPlayerObject(player).gameObject.transform.GetChild(0).GetComponent<Player>();
        if (levelUpPlayer != null)
        {
            //선택된 레벨업 스탯을 해당 플레이어의 스탯에 더함
            levelUpPlayer.AddLevelupStat(statType,value);
        }
        else
        {
            Debug.Log("levelUpPlayer not found");
            
        }
        
        
        Debug.Log("StateAuthority가 levelupSelect 변경: " + levelupSelect);
    }
    private int CalculateNextLevelExp(int currentLevel)
    {
        // 간단한 경험치 증가 공식 예시
        return 10 * currentLevel * (currentLevel + 1) / 2;
    }
    // [Rpc(RpcSources.All, RpcTargets.All)]
    // public void OnEnemyDie()
    // {
    //     exp++;
    //     Debug.Log("Current Exp: " + exp + " / " + expMax);
    //     killPoint++;
    //     Debug.Log("Kill Point: " + killPoint);
    // }

    // 현재 원활하게 동작하지 않아서, 개선이 필요합니다.
    // EnemyDamage 스크립트에서 추가적인 개선이 필요한 것으로 추정됩니다.
    public void OnPlayerDie()
    {
        Debug.LogWarning("OnPlayerDie");
        
        // 멀티 플레이에서의 게임 오버 UI 매커니즘
        if (multiGameoverUI)
        {
            if (isEliminated())
            {
                Debug.LogWarning("전멸함");
                MultiGameover();
            }
        }
        else
        {
            // ====== 싱글 플레이 프로토타입을 위한 예외처리 ======
            // 개발 완료시 else 이하 삭제 바랍니다
            playerStatusUI.SetActive(false);
            gameoverUI.SetActive(true);
            // ====== 싱글 플레이 프로토타입 예외처리 끝 ======
        }
        
        Debug.LogWarning("OnPlayerDie End");
    }

    // [Rpc(RpcSources.All, RpcTargets.All)]
    // public void OnEnemyDie()
    // {
    //     exp++;
    //     Debug.Log("Current Exp: " + exp + " / " + expMax);
    //     killPoint++;
    //     Debug.Log("Kill Point: " + killPoint);
    // }

    #endregion

    #region Private Methods

    private bool isEliminated()
    {
        multiPlayerList.GetPlayersList();
        
        Debug.LogWarning(multiPlayerList.players.Count);
        
        return multiPlayerList.players.All(player => player.GetComponent<Player>().isDead == true);
    }


    private void MultiGameover()
    {
        // 멀티 플레이에서의 게임 오버 메커니즘
        // 1. 누군가 죽었을 때 체크 시작 (MultiGameover 호출)
        // 2. 모든 플레이어가 죽었을 때 (isEliminated) 게임 오버 판정
        // 3. 후속 조치 이행
        Debug.LogWarning("Game Over: All Players Dead");
        Instantiate(multiGameoverUI);
        // multiGameoverUI.SetActive(true);
    }

    #endregion

    #region Coroutines

    // IEnumerator WaitForSeconds(float seconds)
    // {
    //     yield return new WaitForSeconds(seconds);
    // }

    #endregion
}