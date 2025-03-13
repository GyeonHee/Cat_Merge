using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// ����/�ε尡 �ʿ��� ������Ʈ�� ������ �������̽�
public interface ISaveable
{
    string GetSaveData();
    void LoadFromData(string data);
}

[System.Serializable]
public class ComponentData
{
    public string path;
    public string data;
}

[System.Serializable]
public class CompleteGameState
{
    public List<ComponentData> components = new List<ComponentData>();

    // Dictionary�� List�� ��ȯ�ϴ� �޼���
    public void AddComponentData(string path, string data)
    {
        components.Add(new ComponentData { path = path, data = data });
    }

    // List���� Dictionaryó�� ������ ��ȸ
    public bool TryGetValue(string path, out string data)
    {
        foreach (var component in components)
        {
            if (component.path == path)
            {
                data = component.data;
                return true;
            }
        }
        data = null;
        return false;
    }
}

public class GoogleManager : MonoBehaviour
{
    #region ������

    public static GoogleManager Instance { get; private set; }

    // ��� �� ����
    private const string fileName = "GameCompleteState";
    private const string gameScene = "GameScene-Han";
    private TextMeshProUGUI logText;
    private GameObject loadingScreen;
    private bool isLoggedIn = false;
    private bool isDataLoaded = false;
    private CompleteGameState loadedGameState;
    private Dictionary<Type, string> cachedData = new Dictionary<Type, string>();
    private float autoSaveInterval = 30f;
    private float autoSaveTimer = 0f;

    #endregion

    #region �ʱ�ȭ �� �̺�Ʈ ó��

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        UpdateLogText();
        GPGS_LogIn();

        loadingScreen = GameObject.Find("LoadingScreen");
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            DontDestroyOnLoad(loadingScreen);
        }
    }

    private void Update()
    {
        // �ֱ��� �ڵ� ���� ó��
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            SaveGameState();
            autoSaveTimer = 0f;
        }
    }

    // �� �ε� �Ϸ� �� ȣ��Ǵ� �޼���
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� ���� �ε�Ǹ� ������ ����
        if (scene.name == gameScene)
        {
            ShowLoadingScreen(true);
            StartCoroutine(ApplyDataAndShowScreenCoroutine());
        }
    }

    private System.Collections.IEnumerator ApplyDataAndShowScreenCoroutine()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        ApplyDataAndShowScreen();
    }

    // ���� ���� �� �ڵ� ����
    private void OnApplicationQuit()
    {
        SaveGameState();
    }

    // ���� ��׶���� ���� �ڵ� ����
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGameState();
        }
    }

    #endregion

    #region ���� �α��� �� UI

    // logText ã�Ƽ� �����ϴ� �Լ�
    private void UpdateLogText()
    {
        logText = GameObject.Find("Canvas/Title UI/Log Text")?.GetComponent<TextMeshProUGUI>();
    }

    public void GPGS_LogIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    // ���� �α���
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            isLoggedIn = true;
            string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            string userID = PlayGamesPlatform.Instance.GetUserId();

            if (logText != null)
            {
                logText.text = "�α��� ���� : " + displayName + " / " + userID;
            }

            // �α��� ���� �� �ڵ����� ������ �ε�
            LoadGameState();
        }
        else
        {
            isLoggedIn = false;
            if (logText != null)
            {
                logText.text = "�α��� ����";
            }
        }
    }

    #endregion

    #region ������ ���� �� �ε�

    // ��ü ���� ���� ����
    public void SaveGameState()
    {
        if (!isLoggedIn) return;

        CompleteGameState gameState = new CompleteGameState();
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            Type componentType = mb.GetType();
            string typeName = componentType.FullName;
            string data = saveable.GetSaveData();

            cachedData[componentType] = data;
            gameState.AddComponentData(typeName, data);
        }

        string jsonData = JsonUtility.ToJson(gameState);
        SaveToCloud(jsonData);
    }

    // Ŭ���忡 ������ ����
    private void SaveToCloud(string jsonData)
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    byte[] data = Encoding.UTF8.GetBytes(jsonData);
                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("Last saved: " + DateTime.Now.ToString())
                        .Build();

                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => { });
                }
            });
    }

    // ��ü ���� ���� �ε�
    public void LoadGameState()
    {
        if (!isLoggedIn) return;

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    saveGameClient.ReadBinaryData(game, (readStatus, data) =>
                    {
                        if (readStatus == SavedGameRequestStatus.Success)
                        {
                            string jsonData = Encoding.UTF8.GetString(data);
                            loadedGameState = JsonUtility.FromJson<CompleteGameState>(jsonData);
                            isDataLoaded = true;
                            CacheLoadedData();

                            if (SceneManager.GetActiveScene().name == gameScene)
                            {
                                ApplyLoadedGameState();
                            }
                        }
                    });
                }
            });
    }

    // �ε�� �����͸� ĳ�ÿ� ����
    private void CacheLoadedData()
    {
        if (!isDataLoaded || loadedGameState == null) return;

        cachedData.Clear();

        foreach (var component in loadedGameState.components)
        {
            try
            {
                Type componentType = Type.GetType(component.path);
                if (componentType != null)
                {
                    cachedData[componentType] = component.data;
                }
            }
            catch (Exception) { }
        }
    }

    // �ε�� ���� ���� ����
    public void ApplyLoadedGameState()
    {
        if (!isDataLoaded) return;

        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            Type componentType = mb.GetType();

            if (cachedData.TryGetValue(componentType, out string componentData))
            {
                saveable.LoadFromData(componentData);
            }
        }
    }

    #endregion

    #region �ε� ȭ�� ����

    // �ε� ȭ�� ǥ��/���� ó��
    public void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(show);

            // �ε� ȭ�� ǥ�� �߿��� ���� �ð� ����
            Time.timeScale = show ? 0f : 1f;

            if (show)
            {
                DontDestroyOnLoad(loadingScreen);
            }
        }
    }

    // ������ ���� �� ȭ�� ǥ��
    private void ApplyDataAndShowScreen()
    {
        ApplyLoadedGameState();
        StartCoroutine(HideLoadingScreenCoroutine());
    }

    private System.Collections.IEnumerator HideLoadingScreenCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        ShowLoadingScreen(false);
    }

    #endregion

}
