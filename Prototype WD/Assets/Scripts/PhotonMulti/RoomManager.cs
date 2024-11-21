using System;
using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManager : NetworkBehaviour
{
    public GameObject playerInfoUIPrefab;
    public TextMeshProUGUI roomName;
    public Transform gridParent;
    public Button startGameButton;
    public Button readyGameButton;

    private string roomNamePath = "Canvas/Top/Room Name";
    private string gridParentPath = "Canvas/Middle/Player Scroll View/UserGridViewport";
    private string startGameButtonPath = "Canvas/Bottom/StartButton";
    private string readyGameButtonPath = "Canvas/Bottom/ReadyButton";

    [Networked, Capacity(4)] private NetworkArray<PlayerInfo> PlayerInfos => default;
    [Networked] public int PlayerCount { get; private set; }
    [Networked] public PlayerRef hostRef { get; set; }

    [SerializeField] private string gameSceneName = "MultiGameScene2";
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [ScenePath] [SerializeField] private string gameScenePath;
    [ScenePath] [SerializeField] private string loadingScenePath;
    private NetworkSceneManagerDefault networkSceneManager;

    [Header("Character and Weapon Sprites")]
    public Sprite[] characterSprites;
    public Sprite[] weaponSprites;
    
    private bool isStartGameListenerAdded = false;

    public struct PlayerInfo : INetworkStruct
    {
        public PlayerRef PlayerRef;
        public NetworkString<_16> Nickname;
        public int CharacterId;
        public int WeaponId;
        public NetworkBool IsReady;
    }
    
    // PlayerInfos에 대한 public 접근자 추가
    public PlayerInfo[] GetPlayerInfos()
    {
        PlayerInfo[] infos = new PlayerInfo[PlayerCount];
        for (int i = 0; i < PlayerCount; i++)
        {
            infos[i] = PlayerInfos[i];
        }
        return infos;
    }

    public int GetPlayerCount()
    {
        return PlayerCount;
    }

    private PlayerSelection playerSelection;
    
    private bool isInitialized = false;

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            // 실제 연결된 플레이어 수와 동기화
            int connectedPlayers = Runner.ActivePlayers.Count();
            if (PlayerCount != connectedPlayers)
            {
                PlayerCount = connectedPlayers;
                RPC_UpdatePlayerInfoUI();
            }
        }
    }

    public override void Spawned()
    {
        base.Spawned();
        DontDestroyOnLoad(gameObject);
        Debug.Log($"RoomManager Spawned. HasStateAuthority: {Object.HasStateAuthority}");

        if (Object.HasStateAuthority)
        {
            hostRef = Runner.LocalPlayer;
            Debug.Log($"This client is the host. HostRef: {hostRef}");
        }
        else
        {
            Debug.Log("This client is not the host.");
        }

        if (!isInitialized)
        {
            isInitialized = true;
            StartCoroutine(InitializeAfterDelay());
        }
        
        // InitializePlayerSelections();
    }

    public void InitializePlayerSelections()
    {
        PlayerSelection[] playerSelections = FindObjectsOfType<PlayerSelection>();
        foreach (PlayerSelection playerSelection in playerSelections)
        {
            if (playerSelection.Object.InputAuthority == PlayerRef.None)
            {
                playerSelection.Object.AssignInputAuthority(Runner.LocalPlayer);
                Debug.Log($"PlayerSelection에 InputAuthority 할당됨: {Runner.LocalPlayer}");
            }

            playerSelection.SetRoomManager(this);
        }
    }

    private IEnumerator InitializeAfterDelay()
    {
        int attempts = 0;
        while (attempts < 10)
        {
            FindUIElements();
            if (roomName != null && gridParent != null && startGameButton != null && readyGameButton != null)
            {
                Debug.Log("UI 요소를 모두 찾았습니다.");
                break;
            }

            attempts++;
            yield return new WaitForSeconds(0.5f);
        }

        if (roomName == null || gridParent == null || startGameButton == null || readyGameButton == null)
        {
            Debug.LogError("UI 요소를 찾지 못했습니다. 초기화에 실패했습니다.");
            yield break;
        }

        InitializePlayerSelections();

        networkSceneManager = Runner.GetComponent<NetworkSceneManagerDefault>();
        if (networkSceneManager == null)
        {
            Debug.LogError("NetworkSceneManagerDefault를 NetworkRunner에서 찾을 수 없습니다!");
        }

        if (Runner.IsPlayer)
        {
            NetworkManager networkManager = FindObjectOfType<NetworkManager>();
            string playerNickname = (networkManager != null && !string.IsNullOrEmpty(networkManager.nickName))
                ? networkManager.nickName
                : "Player" + Runner.LocalPlayer.PlayerId;

            Debug.Log($"플레이어 추가: {Runner.LocalPlayer}, 닉네임: {playerNickname}");
            RPC_SetPlayerNickname(Runner.LocalPlayer, playerNickname);
            SetRoomName();
        }
    }

    private void SetRoomName()
    {
        Debug.Log($"RoomManager - SetRoomname() - SessionInfo.Name : {Runner.SessionInfo.Name}");
        roomName.text = Runner.SessionInfo.Name.Substring(0, Runner.SessionInfo.Name.Length - 4);
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerNickname(PlayerRef playerRef, string newNickname)
    {
        if (Object.HasStateAuthority)
        {
            for (int i = 0; i < PlayerCount; i++)
            {
                var info = PlayerInfos[i];
                if (info.PlayerRef == playerRef)
                {
                    info.Nickname = newNickname;
                    PlayerInfos.Set(i, info);
                    RPC_UpdatePlayerInfoUI();
                    break;
                }
            }
        }
    }
    
    private void FindUIElements()
    {
        if(roomName == null)
            roomName = GameObject.Find(roomNamePath).GetComponent<TextMeshProUGUI>();
        
        if (gridParent == null)
        {
            GameObject gridParentObj = GameObject.Find(gridParentPath);
            if (gridParentObj != null)
            {
                gridParent = gridParentObj.transform;
            }
            else
            {
                Debug.LogError($"UI 요소를 찾을 수 없습니다: {gridParentPath}");
            }
        }

        if (startGameButton == null)
            startGameButton = GameObject.Find(startGameButtonPath)?.GetComponent<Button>();

        if (readyGameButton == null)
            readyGameButton = GameObject.Find(readyGameButtonPath)?.GetComponent<Button>();

        if (gridParent == null || startGameButton == null || readyGameButton == null)
        {
            Debug.LogError(
                $"UI 요소를 찾지 못했습니다. GridParent: {gridParent != null}, StartButton: {startGameButton != null}, ReadyButton: {readyGameButton != null}");
        }
    }

    public void SyncNewPlayer(PlayerRef playerRef, string nickName)
    {
        if (Object.HasStateAuthority)
        {
            if (PlayerCount < PlayerInfos.Length)
            {
                PlayerInfos.Set(PlayerCount, new PlayerInfo
                {
                    PlayerRef = playerRef,
                    Nickname = nickName,
                    CharacterId = -1,
                    WeaponId = -1,
                    IsReady = false
                });

                Debug.Log($"RoomManager - SyncNewPlayer() - {PlayerInfos[PlayerCount].Nickname}가 추가됨, 현재 인원 수 {PlayerCount}, {PlayerInfos.Length}");

                PlayerCount++;

                RPC_UpdatePlayerInfoUI();
            }
            else
            {
                Debug.LogWarning("플레이어 목록이 가득 찼습니다.");
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdatePlayerInfoUI()
    {
        Debug.Log($"RPC_UpdatePlayerInfoUI 시작: {PlayerCount}명의 플레이어");

        if (gridParent == null)
        {
            Debug.LogError("gridParent가 null입니다. UI를 업데이트할 수 없습니다.");
            return;
        }

        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < PlayerCount; i++)
        {
            var playerInfo = PlayerInfos[i];
            Debug.Log($"플레이어 UI 생성 중: {playerInfo.Nickname}");
            LogPlayerInfo(playerInfo);

            if (playerInfoUIPrefab != null)
            {
                GameObject playerInfoUI = Instantiate(playerInfoUIPrefab, gridParent);
                PlayerInfoUIController uiController = playerInfoUI.GetComponent<PlayerInfoUIController>();

                if (uiController == null)
                {
                    Debug.LogError("생성된 프리팹에서 PlayerInfoUIController 컴포넌트를 찾을 수 없습니다");
                    continue;
                }

                uiController.SetNickName(playerInfo.Nickname.ToString());
                uiController.SetCharacterImage(GetCharacterSprite(playerInfo.CharacterId));
                uiController.SetWeaponImage(GetWeaponSprite(playerInfo.WeaponId));

                if (playerInfo.PlayerRef == hostRef)
                {
                    uiController.SetAsLeader();
                }
                else
                {
                    uiController.SetReadyStatus(playerInfo.IsReady);
                }
            }
            else
            {
                Debug.LogError("playerInfoUIPrefab이 null입니다.");
            }

            UpdateSessionInfo();
        }

        if (Runner.LocalPlayer == hostRef)
        {
            if (startGameButton != null) startGameButton.gameObject.SetActive(true);
            if (readyGameButton != null) readyGameButton.gameObject.SetActive(false);
        }
        else
        {
            if (startGameButton != null) startGameButton.gameObject.SetActive(false);
            if (readyGameButton != null) readyGameButton.gameObject.SetActive(true);
        }

        if (Runner.LocalPlayer == hostRef)
        {
            CheckStartGameCondition();
        }
    }

    public void CheckStartGameCondition()
    {
        bool allPlayersReady = true;
        PlayerInfo hostInfo = default;

        for (int i = 0; i < PlayerCount; i++)
        {
            var info = PlayerInfos[i];
            if (info.PlayerRef == hostRef)
            {
                hostInfo = info;
            }
            else if (!info.IsReady)
            {
                allPlayersReady = false;
                break;
            }
        }

        if (!isStartGameListenerAdded)
        {
            startGameButton.onClick.AddListener(StartGame);
            isStartGameListenerAdded = true;
        }

        startGameButton.interactable = allPlayersReady &&
                                       hostInfo.CharacterId != -1 &&
                                       hostInfo.WeaponId != -1;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerReady(PlayerRef playerRef)
    {
        if (Object.HasStateAuthority)
        {
            for (int i = 0; i < PlayerCount; i++)
            {
                var info = PlayerInfos[i];
                if (info.PlayerRef == playerRef && playerRef != hostRef)
                {
                    info.IsReady = !info.IsReady;
                    PlayerInfos.Set(i, info);
                    RPC_UpdatePlayerInfoUI();
                    break;
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SelectCharacter(PlayerRef playerRef, int characterId)
    {
        Debug.Log($"RPC_SelectCharacter called: {playerRef}, {characterId}");
        if (Object.HasStateAuthority)
        {
            for (int i = 0; i < PlayerCount; i++)
            {
                var info = PlayerInfos[i];
                if (info.PlayerRef == playerRef)
                {
                    info.CharacterId = characterId;
                    PlayerInfos.Set(i, info);
                    Debug.Log($"Updated character for player {info.Nickname}: {characterId}");
                    RPC_UpdatePlayerInfoUI();
                    break;
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SelectWeapon(PlayerRef playerRef, int weaponId)
    {
        Debug.Log($"RPC_SelectWeapon called: {playerRef}, {weaponId}");
        if (Object.HasStateAuthority)
        {
            for (int i = 0; i < PlayerCount; i++)
            {
                var info = PlayerInfos[i];
                if (info.PlayerRef == playerRef)
                {
                    info.WeaponId = weaponId;
                    PlayerInfos.Set(i, info);
                    Debug.Log($"Updated weapon for player {info.Nickname}: {weaponId}");
                    RPC_UpdatePlayerInfoUI();
                    break;
                }
            }
        }
    }

    private void LogPlayerInfo(PlayerInfo info)
    {
        Debug.Log($"플레이어 정보: " +
                  $"PlayerRef={info.PlayerRef}, " +
                  $"Nickname={info.Nickname}, " +
                  $"CharacterId={info.CharacterId}, " +
                  $"WeaponId={info.WeaponId}, " +
                  $"IsReady={info.IsReady}");
    }

    public void StartGame()
    {
        if (Runner.IsServer)
        {
            RPC_StartGame();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartGame()
    {
        SceneRef loadingSceneRef = SceneRef.FromIndex(GetSceneIndex(loadingSceneName));
        Debug.Log($"RoomManager - RPC_StartGame() - {loadingSceneRef.ToString()}");
        if (loadingSceneRef.IsValid && Runner.SessionInfo.IsValid)
        {
            // 세션 상태를 변경
            Runner.SessionInfo.IsVisible = false;
            Runner.SessionInfo.IsOpen = false;
            
            Runner.LoadScene(loadingSceneRef);
        }
        else
        {
            Debug.LogError($"{loadingSceneName}에 대한 씬 참조가 잘못되었거나 세션이 유효하지 않습니다");
        }
    }

    private int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (name == sceneName)
            {
                return i;
            }
        }

        return -1;
    }


    public void OnPlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority)
        {
            // PlayerInfos에서 해당 플레이어 정보 제거
            for (int i = 0; i < PlayerCount; i++)
            {
                if (PlayerInfos[i].PlayerRef == player)
                {
                    // 마지막 플레이어의 정보를 현재 위치로 이동
                    PlayerInfos.Set(i, PlayerInfos[PlayerCount - 1]);
                    PlayerCount--;
                    break;
                }
            }

            // UI 업데이트
            RPC_UpdatePlayerInfoUI();

            // 세션 정보 업데이트
            if (Runner.SessionInfo != null)
            {
                Runner.SessionInfo.IsOpen = true;
                Runner.SessionInfo.IsVisible = true;
            }
        }
    }

    public void UpdateSessionInfo()
    {
        if (Runner.IsServer && Runner.SessionInfo != null)
        {
            Runner.SessionInfo.IsOpen = PlayerCount < Runner.SessionInfo.MaxPlayers;
            Runner.SessionInfo.IsVisible = true;
        }
    }

    private Sprite GetCharacterSprite(int characterId)
    {
        if (characterId >= 0 && characterId < characterSprites.Length)
        {
            return characterSprites[characterId + 1];
        }
        else
        {
            Debug.LogWarning($"Invalid character ID: {characterId}");
            return characterSprites[0];
        }
    }

    private Sprite GetWeaponSprite(int weaponId)
    {
        if (weaponId >= 0 && weaponId < weaponSprites.Length)
        {
            return weaponSprites[weaponId];
        }
        else
        {
            Debug.LogWarning($"Invalid weapon ID: {weaponId}");
            return null;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_EndSession()
    {
        Runner.Shutdown(true, ShutdownReason.GameClosed);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestDisconnect(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            Runner.Disconnect(player);
        }
    }
}