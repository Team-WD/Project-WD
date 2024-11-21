using System;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerSelection : NetworkBehaviour
{
    private Boolean isReady = false;
    
    public Button[] characterButtons;
    public GameObject weaponFloatingUI;
    public Button[] weaponButtons; // 항상 2개
    public Image[] weaponButtonImages; // 무기 버튼의 이미지
    public TextMeshProUGUI[] weaponName; // 무기 이름
    public Button readyButton;

    public NetworkRunner Runner;
    public RoomManager roomManager;

    public String[] weaponNames;
    public Sprite[] weaponSprites; // 모든 무기 스프라이트 (8개)

    public GameObject comingSoon;
    
    private int selectedCharacterId = -1;
    private int selectedWeaponId = -1;

    private bool isInitialized = false;

    public void SetRoomManager(RoomManager manager)
    {
        roomManager = manager;
        Debug.Log($"RoomManager set for PlayerSelection. HasInputAuthority: {Object.HasInputAuthority}");
    }

    private void Awake()
    {
        readyButton.interactable = false;
    }

    public override void Spawned()
    {
        base.Spawned();
        Runner = Object.Runner;
        Debug.Log($"PlayerSelection Spawned. HasInputAuthority: {Object.HasInputAuthority}");
        StartCoroutine(WaitForInputAuthorityAndInitialize());
    }

    private IEnumerator WaitForInputAuthorityAndInitialize()
    {
        yield return new WaitUntil(() => Object.HasInputAuthority);

        Debug.Log(
            $"PlayerSelection initialized. HasInputAuthority: {Object.HasInputAuthority}, HasStateAuthority: {Object.HasStateAuthority}");
        StartCoroutine(FindRoomManager());
        // InitializeButtons();
    }

    private IEnumerator FindRoomManager()
    {
        float timeOut = 5f;
        float elapsedTime = 0f;

        while (roomManager == null && elapsedTime < timeOut)
        {
            roomManager = FindObjectOfType<RoomManager>();
            if (roomManager != null)
            {
                Debug.Log("RoomManager를 찾았습니다: " + roomManager.name);
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (roomManager == null)
        {
            Debug.LogError("RoomManager를 찾을 수 없습니다!");
        }
    }

    private void UpdateWeaponUI(int characterId)
    {
        if (characterId < 0)
        {
            weaponFloatingUI.SetActive(false);
            return;
        }

        int startWeaponId = characterId * 2;
        for (int i = 0; i < 2; i++)
        {
            if (characterId >= 2)
            {
                weaponButtons[1].interactable = false;
                comingSoon.SetActive(true);
            }
            else
            {
                weaponButtons[1].interactable = true;
                comingSoon.SetActive(false);
            }
                
            weaponName[i].text = weaponNames[startWeaponId + i];
            
            weaponButtonImages[i].sprite = weaponSprites[startWeaponId + i];
            weaponButtonImages[i].SetNativeSize(); // Set native size
        }

        weaponFloatingUI.SetActive(true);
    }

    public void SelectCharacter(int characterId)
    {
        Debug.Log($"SelectCharacter called: {characterId}");
        selectedCharacterId = characterId;
        UpdateWeaponUI(characterId);

        Debug.Log($"Calling RPC_SelectCharacter: {Runner.LocalPlayer}, {characterId}");
        roomManager.RPC_SelectCharacter(Runner.LocalPlayer, characterId);
    }

    public void SelectWeapon(int buttonIndex)
    {
        Debug.Log($"SelectWeapon called: {buttonIndex}");
        int weaponId = (selectedCharacterId * 2) + buttonIndex;
        selectedWeaponId = weaponId;
        UpdatePlayerStatus();

        Debug.Log($"Calling RPC_SelectWeapon: {Runner.LocalPlayer}, {weaponId}");
        roomManager.RPC_SelectWeapon(Runner.LocalPlayer, weaponId);

        weaponFloatingUI.SetActive(false);
    }

    private void UpdatePlayerStatus()
    {
        if (selectedCharacterId != -1 && selectedWeaponId != -1)
        {
            readyButton.interactable = true;
        }
    }

    public void ReadyUp()
    {
        if (isReady)
        {
            roomManager.RPC_PlayerReady(Runner.LocalPlayer);
            
            for (int i = 0; i < characterButtons.Length; i++)
            {
                characterButtons[i].interactable = true;   
            }
            
            isReady = false;
        }
        else
        {
            roomManager.RPC_PlayerReady(Runner.LocalPlayer);

            for (int i = 0; i < characterButtons.Length; i++)
            {
                characterButtons[i].interactable = false;   
            }
            
            isReady = true;
        }
    }

    public void Exit()
    {
        if (Runner != null)
        {
            if (Runner.IsServer) // Host인 경우
            {
                // 모든 클라이언트에게 세션 종료를 알림
                roomManager.RPC_EndSession();
            }
            else // Client인 경우
            {
                // 현재 클라이언트만 세션에서 나감
                roomManager.RPC_RequestDisconnect(Runner.LocalPlayer);
            }
        }
        else
        {
            Debug.LogError("NetworkRunner is null in PlayerSelection");
        }
        // 로비 씬으로 돌아가기 전에 정리 작업 수행
        CleanUpNetworkManager();
    }

    private void CleanUpNetworkManager()
    {
        if (NetworkManager.runnerInstance != null)
        {
            NetworkManager.runnerInstance.Shutdown();
        }
        
        SceneManager.LoadScene("Lobby");
    }
}