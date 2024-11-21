using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEditor;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkRunner runnerInstance { get; private set; }
    public Transform sessionListContentParent;
    public GameObject sessionListEntryPrefab;
    public Vector2 inputVec;
    public Dictionary<string, GameObject> sessionListUiDict = new Dictionary<string, GameObject>();
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    private int currentSpawnPointIndex = 0;

    public string lobbyName = "default";

    public string nickName;
    public GameObject NicknameObj;

    private string roomName;
    public GameObject roomNameObj;

    public GameObject[] playerPrefab;

    private RoomManager roomManager;
    public GameObject roomManagerPrefab;

    private string gameSceneName = "MultiGameScene2";
    private string loadingSceneName = "LoadingScene";
    
    private LoadingSceneManager loadingSceneManager;
    public GameObject loadingManagerPrefab;

    private void Awake()
    {
        runnerInstance = gameObject.GetComponent<NetworkRunner>();
        if (runnerInstance == null)
        {
            runnerInstance = gameObject.AddComponent<NetworkRunner>();
        }
    }

    private void Start()
    {
        if (runnerInstance == null)
        {
            runnerInstance = gameObject.AddComponent<NetworkRunner>();
        }

        runnerInstance.JoinSessionLobby(SessionLobby.ClientServer, lobbyName);

        // 씬에서 모든 SpawnPoint 컴포넌트를 찾아 리스트에 추가
        spawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());

        // SpawnPoint들을 위치에 따라 정렬 (선택사항)
        spawnPoints.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
    }

    public void onChangingNickname()
    {
        nickName = NicknameObj.GetComponent<TMP_InputField>().text;
    }

    public void onChangingRoomname()
    {
        roomName = roomNameObj.GetComponent<TMP_InputField>().text;
    }

    public void ReturnToLobby()
    {
        if (runnerInstance.IsPlayer)
        {
            runnerInstance.Despawn(runnerInstance.GetPlayerObject(runnerInstance.LocalPlayer));
        }

        runnerInstance.Shutdown(true, ShutdownReason.Ok);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SceneManager.LoadScene("Lobby");
    }

    public void CreateRandomSession()
    {
        int randomInt = Random.Range(1000, 9999);
        string randomSessionName = roomName + randomInt.ToString();

        var scene = SceneRef.FromIndex(GetSceneIndex("RoomScene"));
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        runnerInstance.StartGame(new StartGameArgs()
        {
            Scene = scene,
            SessionName = randomSessionName,
            GameMode = GameMode.Host,
            PlayerCount = 4,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void SpawnLoadingManager()
    {
        Debug.Log("NetworkManager - SpawnLoadingManager() - Attempting to spawn LoadingSceneManager");
        if (loadingManagerPrefab != null)
        {
            GameObject loadingManagerObj = Instantiate(loadingManagerPrefab, Vector3.zero, Quaternion.identity);
            if (loadingManagerObj != null)
            {
                loadingSceneManager = loadingManagerObj.GetComponent<LoadingSceneManager>();
                if (loadingSceneManager != null)
                {
                    Debug.Log($"NetworkManager - SpawnLoadingManager() - LoadingSceneManager spawned successfully: {loadingManagerObj.name}");
                    loadingSceneManager.Initialize(runnerInstance);
                }
                else
                {
                    Debug.LogError("NetworkManager - SpawnLoadingManager() - LoadingSceneManager component not found on spawned object");
                }
            }
            else
            {
                Debug.LogError("NetworkManager - SpawnLoadingManager() - Failed to instantiate LoadingSceneManager");
            }
        }
        else
        {
            Debug.LogError("NetworkManager - SpawnLoadingManager() - LoadingSceneManager prefab is not assigned");
        }
    }
    
    public int GetSceneIndex(string sceneName)
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

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player joined: {player}");
        if (runner.IsServer)
        {
            if (runner.ActivePlayers.Count() == 1)
            {
                var roomManagerObj = runner.Spawn(roomManagerPrefab, Vector3.zero, Quaternion.identity);
                roomManager = roomManagerObj.GetComponent<RoomManager>();
            }

            // 새로운 플레이어에게 현재 방 상태를 동기화
            if (roomManager != null)
            {
                string playerNickname = nickName.Length < 1 ? $"Player{player.PlayerId}" : nickName;
                Debug.Log($"NetworkManager - OnPlayerJoined - player : {player}, nickname : {playerNickname}");
                roomManager.SyncNewPlayer(player, playerNickname);
            }
        }

        // 로컬 플레이어의 PlayerSelection에 InputAuthority 할당
        if (player == runner.LocalPlayer)
        {
            StartCoroutine(AssignPlayerSelectionAuthorityCoroutine(player));
        }
    }

    private IEnumerator AssignPlayerSelectionAuthorityCoroutine(PlayerRef player)
    {
        PlayerSelection playerSelection = null;
        int attempts = 0;
        while (playerSelection == null && attempts < 10)
        {
            playerSelection = FindObjectOfType<PlayerSelection>();
            if (playerSelection != null)
            {
                Debug.Log($"PlayerSelection 찾음: {playerSelection}");
                if (playerSelection.Object != null)
                {
                    playerSelection.Object.AssignInputAuthority(player);
                    Debug.Log($"Local PlayerSelection에 InputAuthority 할당됨: {player}");
                }
                else
                {
                    Debug.LogError("PlayerSelection의 NetworkObject가 null입니다.");
                }

                break;
            }

            attempts++;
            yield return new WaitForSeconds(0.5f);
        }

        if (playerSelection == null)
        {
            Debug.LogError("PlayerSelection을 찾지 못했습니다.");
        }
    }

    private int sceneLoadCount = 0;

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        sceneLoadCount++;
        Debug.Log($"OnSceneLoadDone called {sceneLoadCount} times. Runner: {runner.name}");

        if (runner.GameMode == GameMode.Host || runner.GameMode == GameMode.Client)
        {
            if (SceneManager.GetActiveScene().name == gameSceneName)
            {
                Debug.Log("NetworkManager - OnSceneLoadDone() - GameScene Loaded, Spawn Player!");
                StartCoroutine(SpawnPlayersWhenSceneLoaded(runner));
                
                // LoadingSceneManager에 게임 씬 로딩 완료 알림
                LoadingSceneManager loadingSceneManager = FindObjectOfType<LoadingSceneManager>();
                if (loadingSceneManager != null)
                {
                    loadingSceneManager.OnGameSceneLoaded();
                }
                else
                {
                    Debug.LogWarning("LoadingSceneManager not found when game scene loaded.");
                }
            }
            
            else if (SceneManager.GetActiveScene().name == loadingSceneName)
            {
                Debug.Log("NetworkManager - OnSceneLoadDone() - LoadingScene Loaded");
                SpawnLoadingManager();
            }
        }
    }

    private IEnumerator SpawnPlayersWhenSceneLoaded(NetworkRunner runner)
    {
        yield return new WaitForSeconds(0.5f); // 씬 로드 완료 후 약간의 지연

        RoomManager roomManager = FindObjectOfType<RoomManager>();
        if (roomManager != null)
        {
            RoomManager.PlayerInfo[] playerInfos = roomManager.GetPlayerInfos();
            int playerCount = roomManager.GetPlayerCount();

            for (int i = 0; i < playerCount; i++)
            {
                RoomManager.PlayerInfo playerInfo = playerInfos[i];
                Debug.Log($"NetworkManager - SpawnPlayersWhenSceneLoaded() - Player Info: {playerInfo}");
                StartCoroutine(FindSpawnPointsAndSpawnPlayer(runner, playerInfo.PlayerRef, playerInfo.CharacterId,
                    playerInfo.WeaponId, playerInfo.Nickname.ToString()));
            }

            // 모든 플레이어 스폰이 완료될 때까지 기다림
            yield return new WaitUntil(() => _spawnedCharacters.Count == playerCount);

            // RoomManager 삭제
            runner.Despawn(roomManager.Object);
        }
        else
        {
            Debug.LogError("RoomManager not found!");
        }
    }

    private IEnumerator FindSpawnPointsAndSpawnPlayer(NetworkRunner runner, PlayerRef player, int characterId,
        int weaponId, string nickname)
    {
        // 플레이어 프리팹 설정
        GameObject playerPrefab = SelectPlayerPrefab(characterId, weaponId);

        // 스폰 포인트 찾기
        spawnPoints.Clear();
        spawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points found in the scene. Please add SpawnPoint components to your scene.");
            yield return new WaitForSeconds(0.5f); // 잠시 대기 후 다시 시도
            spawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());
        }

        if (spawnPoints.Count > 0)
        {
            Debug.Log($"Found {spawnPoints.Count} spawn points.");
            foreach (var sp in spawnPoints)
            {
                Debug.Log($"Spawn point: {sp.name} at position {sp.transform.position}");
            }
        }

        // 플레이어 스폰
        Vector3 spawnPosition = GetNextSpawnPosition();

        NetworkObject playerNetworkObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        runner.SetPlayerObject(player, playerNetworkObject);
        playerNetworkObject.AssignInputAuthority(player);

        _spawnedCharacters.Add(player, playerNetworkObject);

        Debug.Log($"Local player spawned: {playerNetworkObject.gameObject.name} at position {spawnPosition}");

        yield return StartCoroutine(NotifyEnemiesOfPlayerJoinWithDelay(true));
    }

    private GameObject SelectPlayerPrefab(int characterId, int weaponId)
    {
        // 캐릭터와 무기 ID에 따라 적절한 프리팹 선택
        // 캐릭터는 0 1 2 3
        // 무기는 0 1 2 3 4 5 6 7 이므로 현재는 무기에 맞춰서 스폰하도록 구현함
        Debug.Log("NetworkManager - SelectPlayerPrefab() - called");

        int prefabIndex = weaponId;

        return playerPrefab[prefabIndex];
    }

    private Vector3 GetNextSpawnPosition()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points available. Using default spawn position.");
            return Vector3.zero;
        }

        Vector3 spawnPosition = spawnPoints[currentSpawnPointIndex].transform.position;
        currentSpawnPointIndex = (currentSpawnPointIndex + 1) % spawnPoints.Count;
        return spawnPosition;
    }

    private IEnumerator NotifyEnemiesOfPlayerJoinWithDelay(bool isJoin)
    {
        yield return new WaitForSeconds(0.5f); // 1초 대기
        NotifyEnemiesOfPlayer(true);
    }

    private void NotifyEnemiesOfPlayer(bool isJoin)
    {
        MultiEnemy[] enemies = FindObjectsOfType<MultiEnemy>();
        Debug.Log($"Notifying {enemies.Length} enemies of player join");
        foreach (MultiEnemy enemy in enemies)
        {
            if (isJoin)
            {
                enemy.OnPlayerJoined();
            }
            else
            {
                enemy.OnPlayerLeft();
            }
        }
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("updated");
        DeleteOldSessionFromUI(sessionList);

        CompareLists(sessionList);
    }

    private void CompareLists(List<SessionInfo> sessionList)
    {
        foreach (SessionInfo session in sessionList)
        {
            if (!session.IsVisible || !session.IsOpen)
            {
                // 보이지 않거나 닫힌 세션 제거
                if (sessionListUiDict.ContainsKey(session.Name))
                {
                    GameObject uiToDelete = sessionListUiDict[session.Name];
                    sessionListUiDict.Remove(session.Name);
                    Destroy(uiToDelete);
                }
            }
            else
            {
                if (sessionListUiDict.ContainsKey(session.Name))
                {
                    UpdateEntryUI(session);
                    Debug.Log("update" + session.Name);
                }
                else
                {
                    CreateEntryUI(session);
                    Debug.Log("create" + session.Name);
                }
            }
        }
    }

    private void UpdateEntryUI(SessionInfo session)
    {
        sessionListUiDict.TryGetValue(session.Name, out GameObject newEntry);

        SessionListEntry entryScript = newEntry.GetComponent<SessionListEntry>();
    
        entryScript.roomName.text = session.Name.Substring(0, session.Name.Length - 4);
        entryScript.playerCount.text = session.PlayerCount.ToString() + "/" + session.MaxPlayers.ToString();
        entryScript.JoinButton.interactable = session.IsOpen;

        newEntry.SetActive(session.IsVisible);
    }

    private void CreateEntryUI(SessionInfo session)
    {
        GameObject newEntry = GameObject.Instantiate(sessionListEntryPrefab);
        newEntry.transform.SetParent(sessionListContentParent, false);


        SessionListEntry entryScript = newEntry.GetComponent<SessionListEntry>();
        sessionListUiDict.Add(session.Name, newEntry);

        entryScript.roomName.text = session.Name.Substring(0, session.Name.Length - 4);
        entryScript.roomId = session.Name;
        entryScript.playerCount.text = session.PlayerCount.ToString() + "/" + session.MaxPlayers.ToString();

        entryScript.JoinButton.interactable = session.IsOpen;

        newEntry.SetActive(session.IsVisible);
    }

    private void DeleteOldSessionFromUI(List<SessionInfo> sessionList)
    {
        bool isContained = false;
        GameObject uiToDelete = null;
        foreach (KeyValuePair<string, GameObject> kvp in sessionListUiDict)
        {
            string sessionKey = kvp.Key;

            foreach (SessionInfo sessionInfo in sessionList)
            {
                if (sessionInfo.Name == sessionKey)
                {
                    isContained = true;
                    break;
                }
            }

            if (!isContained)
            {
                uiToDelete = kvp.Value;
                sessionListUiDict.Remove(sessionKey);
                Destroy(uiToDelete);
            }
        }
    }

    void OnMove(InputValue value)
    {
        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            if (runnerInstance.IsPlayer)
            {
                inputVec = value.Get<Vector2>();
            }
        }
    }


    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player left: {player}");
        RoomManager roomManager = FindObjectOfType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.OnPlayerLeft(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            if (runnerInstance.IsPlayer)
            {
                var data = new NetworkInputData();
                data.direction = inputVec;
                data.isFiring = Input.GetMouseButton(0);
                if (data.isFiring)
                {
                    // Debug.Log("OnInput: Mouse button pressed");
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePosition.z = 0;
                    data.targetPosition = mousePosition;
                }


                input.Set(data);
            }
        }
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }


    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        ResetInstance();
        SceneManager.LoadScene("Lobby");
    }

    public void ResetInstance()
    {
        runnerInstance = null;
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }


    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}