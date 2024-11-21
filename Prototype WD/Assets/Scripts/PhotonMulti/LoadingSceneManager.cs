using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    public string gameSceneName = "MultiGameScene2";

    private float loadingProgress = 0f;
    private NetworkRunner runner;
    private bool isGameSceneLoaded = false;

    public void Initialize(NetworkRunner runner)
    {
        this.runner = runner;
        Debug.Log("LoadingSceneManager - Initialize() called");

        // Find UI elements if not assigned in inspector
        if (progressBar == null)
            progressBar = FindObjectOfType<Slider>();
        if (progressText == null)
            progressText = FindObjectOfType<TextMeshProUGUI>();

        if (progressBar == null || progressText == null)
        {
            Debug.LogError("UI elements not found in LoadingSceneManager");
        }

        StartCoroutine(SimulateLoading());
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator SimulateLoading()
    {
        Debug.Log("LoadingSceneManager - SimulateLoading() started");
        float elapsedTime = 0f;
        float loadingDuration = 3f; // 로딩 시간 (조정 가능)

        while (elapsedTime < loadingDuration && !isGameSceneLoaded)
        {
            elapsedTime += Time.deltaTime;
            loadingProgress = Mathf.Clamp01(elapsedTime / loadingDuration);
            UpdateProgressUI(loadingProgress);
            yield return null;
        }

        // 게임 씬 로딩이 완료될 때까지 대기
        while (!isGameSceneLoaded)
        {
            yield return null;
        }

        // 로딩 완료
        UpdateProgressUI(1f);
        yield return new WaitForSeconds(0.5f); // 완료 상태를 잠시 보여줌

        // 여기에 게임 씬으로 전환하는 로직 추가
    }

    private IEnumerator LoadGameScene()
    {
        if (runner != null && runner.IsServer)
        {
            SceneRef gameSceneRef = SceneRef.FromIndex(GetSceneIndex(gameSceneName));
            if (gameSceneRef.IsValid)
            {
                Debug.Log($"LoadingSceneManager - Loading game scene: {gameSceneName}");
                yield return runner.LoadScene(gameSceneRef);
                isGameSceneLoaded = true;
                Debug.Log("Game scene loaded successfully");
            }
            else
            {
                Debug.LogError($"Invalid scene reference for {gameSceneName}");
            }
        }
        else
        {
            Debug.Log("Waiting for server to load the game scene...");
            // 클라이언트는 서버가 씬을 로드할 때까지 대기
            while (!isGameSceneLoaded)
            {
                yield return null;
            }
        }
    }

    private void UpdateProgressUI(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"Loading... {(progress * 100):F0}%";
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

    // NetworkRunner의 OnSceneLoadDone 콜백에서 호출될 메소드
    public void OnGameSceneLoaded()
    {
        isGameSceneLoaded = true;
    }
}