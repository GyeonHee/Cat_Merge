using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public void AddComponentData(string path, string data)
    {
        components.Add(new ComponentData { path = path, data = data });
    }

    public bool TryGetValue(string path, out string data)
    {
        var component = components.FirstOrDefault(c => c.path == path);
        if (component != null)
        {
            data = component.data;
            return true;
        }
        data = null;
        return false;
    }
}

public class GoogleManager : MonoBehaviour
{


    #region Variables

    public static GoogleManager Instance { get; private set; }

    private TextMeshProUGUI logText;                        // �α� �ؽ�Ʈ (���߿� ���ٰ���)
    private GameObject loadingScreen;                       // �ε� ��ũ�� (���߿� ���ְų� �����ҵ�)
    private Button deleteDataButton;                        // ���� ������ ���� ��ư

    private const string fileName = "GameCompleteState";        // ���� �̸�
    private const string gameScene = "GameScene-Han";           // GameScene �̸�
    private const string loadingScreenName = "LoadingScreen";   // �ε� ��ũ�� �̸�
    private const float autoSaveInterval = 30f;                 // �ֱ��� �ڵ� ���� �ð�
    private float autoSaveTimer = 0f;                           // �ڵ� ���� �ð� ��� Ÿ�̸�

    private bool isLoggedIn = false;                        // ���� �α��� ����
    private bool isDataLoaded = false;                      // ������ �ε� ����
    private bool isSaving = false;                          // ���� ������ ������ ����
    private bool isDeletingData = false;                    // ���� ������ ������ ����

    private CompleteGameState loadedGameState;
    private Dictionary<Type, string> cachedData = new Dictionary<Type, string>();

    public delegate void SaveCompletedCallback(bool success);

    private Canvas loadingScreenCanvas;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Start()
    {
        Application.targetFrameRate = 60;

        InitializeGooglePlay();
        InitializeLoadingScreen();

        StartCoroutine(GPGS_Login());
    }

    private void Update()
    {
        if (CanSkipAutoSave()) return;

        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            SaveGameState();
            autoSaveTimer = 0f;
        }
    }

    #endregion


    #region Initialize

    private void InitializeGooglePlay()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        UpdateLogText();
    }

    private void InitializeLoadingScreen()
    {
        loadingScreen = GameObject.Find(loadingScreenName);
        if (loadingScreen != null)
        {
            loadingScreenCanvas = loadingScreen.GetComponent<Canvas>();
            loadingScreen.SetActive(false);
            DontDestroyOnLoad(loadingScreen);
        }
    }

    private bool CanSkipAutoSave()
    {
        return isDeletingData || (GameManager.Instance != null && GameManager.Instance.isQuiting);
    }

    #endregion


    #region Scene Management

    // �� �ε� �Ϸ�� �����͸� �����ϴ� �Լ�
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // LoadingScreen�� ī�޶� ������Ʈ
        UpdateLoadingScreenCamera();

        // ���� �ε�� ������ ���� ��ư ã��
        FindAndSetupDeleteButton();

        // ���� ���� �ε�Ǹ� ������ �ε� ����
        if (scene.name == gameScene)
        {
            ShowLoadingScreen(true);
            StartCoroutine(LoadDataAndInitializeGame());
        }
    }

    private IEnumerator LoadDataAndInitializeGame()
    {
        bool dataApplied = false;

        // �α��ε� ��쿡�� ������ �ε�
        if (isLoggedIn)
        {
            bool loadComplete = false;
            LoadGameState(() => {
                loadComplete = true;
                // ������ �ε� �Ϸ� ���� �� ���� ����
                if (isDataLoaded && !dataApplied)
                {
                    ApplyLoadedGameState();
                    dataApplied = true;
                }
            });

            // �ε� �Ϸ� ���
            float waitTime = 0;
            while (!loadComplete && waitTime < 5f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }

        // �ε� ȭ�� ����� (�ణ�� ���� ��)
        yield return new WaitForSecondsRealtime(0.5f);
        ShowLoadingScreen(false);
    }

    #endregion


    #region ���� �α��� �� UI

    // logText ã�Ƽ� �����ϴ� �Լ�
    private void UpdateLogText()
    {
        logText = GameObject.Find("Canvas/Title UI/Log Text")?.GetComponent<TextMeshProUGUI>();
    }

    private IEnumerator GPGS_Login()
    {
        // �α���
        bool loginComplete = false;
        PlayGamesPlatform.Instance.Authenticate((status) => {
            ProcessAuthentication(status);
            loginComplete = true;
        });

        // �α��� �Ϸ� ���
        float waitTime = 0;
        while (!loginComplete && waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
    }

    // ���� �α��� ����� ó���ϴ� �Լ�
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            isLoggedIn = true;
            string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            string userID = PlayGamesPlatform.Instance.GetUserId();

            if (logText != null)
            {
                logText.text = $"�α��� ���� : {displayName}";
            }
        }
        else
        {
            isLoggedIn = false;
            if (logText != null)
            {
                logText.text = $"�α��� ����";
            }
        }
    }

    // ���� ���� ��ư�� ������ public �޼���
    public void OnGameStartButtonClick()
    {
        SceneManager.LoadScene(gameScene);
    }

    // ���� ���� ��ư���� ȣ���� �޼���
    public IEnumerator StartGameWithLoad(Action onLoadComplete = null)
    {
        ShowLoadingScreen(true);

        if (isLoggedIn)
        {
            bool loadComplete = false;
            LoadGameState(() => {
                loadComplete = true;
            });

            // �ε� �Ϸ� ���
            float waitTime = 0;
            while (!loadComplete && waitTime < 5f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }

        // �� ��ȯ
        SceneManager.LoadScene(gameScene);

        if (onLoadComplete != null)
        {
            onLoadComplete();
        }
    }

    #endregion


    #region �ε� ȭ�� ����

    private void UpdateLoadingScreenCamera()
    {
        if (loadingScreenCanvas != null)
        {
            // ���� ���� ���� ī�޶� ã��
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                loadingScreenCanvas.worldCamera = mainCamera;
                loadingScreenCanvas.planeDistance = 1f; // �ʿ��� ��� �Ÿ� ����
            }
        }
    }

    // �ε� ȭ���� ǥ���ϰų� ����� �Լ�
    public void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(show);

            // �ε� ȭ�� ǥ�� �߿��� ���� �ð� ����
            if (!isDeletingData)
            {
                Time.timeScale = show ? 0f : 1f;
            }

            if (show)
            {
                DontDestroyOnLoad(loadingScreen);
            }
        }
    }

    #endregion


    #region ������ ���� �� �ε�

    // ��ü ���� ���¸� �����ϴ� �Լ�
    public void SaveGameState()
    {
        // ������ ���� ���̰ų� ���� ���� ���� ���� ���� ����
        if (!isLoggedIn || isDeletingData || (GameManager.Instance != null && GameManager.Instance.isQuiting)) return;
        if (isSaving) return;

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

    // ����� ���� �Լ� (���� �� ���)
    public void SaveGameStateSync(SaveCompletedCallback callback = null)
    {
        if (!isLoggedIn || isDeletingData)
        {
            callback?.Invoke(false);
            return;
        }

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
                        .WithUpdatedDescription($"Last saved: {DateTime.Now.ToString()}")
                        .Build();

                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => {
                        bool success = saveStatus == SavedGameRequestStatus.Success;

                        callback?.Invoke(success);
                    });
                }
                else
                {
                    callback?.Invoke(false);
                }
            });

        // ������ �ʹ� ���� �ɸ��� ��츦 ����� Ÿ�Ӿƿ� ó��
        StartCoroutine(SaveTimeout(callback));
    }

    // ���� Ÿ�Ӿƿ��� ó���ϴ� �ڷ�ƾ
    private IEnumerator SaveTimeout(SaveCompletedCallback callback)
    {
        // �ִ� 3�� ���
        yield return new WaitForSeconds(2.0f);

        // ���� �ݹ��� ȣ����� �ʾҴٸ� ȣ��
        if (callback != null) callback(true);
    }

    // Ŭ���忡 �����͸� �����ϴ� �Լ�
    private void SaveToCloud(string jsonData)
    {
        isSaving = true;

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

                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => {
                        isSaving = false;
                    });
                }
                else
                {
                    isSaving = false;
                }
            });
    }

    // ��ü ���� ���¸� �ε��ϴ� �Լ�
    public void LoadGameState(Action onComplete = null)
    {
        if (!isLoggedIn || isDeletingData)
        {
            onComplete?.Invoke();
            return;
        }

        // �̹� �����Ͱ� �ε�� ���¸� �ݹ鸸 ȣ��
        if (isDataLoaded)
        {
            onComplete?.Invoke();
            return;
        }

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
                            if (data == null || data.Length == 0)
                            {
                                loadedGameState = new CompleteGameState();
                                isDataLoaded = true;
                                cachedData.Clear();
                            }
                            else
                            {
                                string jsonData = Encoding.UTF8.GetString(data);
                                loadedGameState = JsonUtility.FromJson<CompleteGameState>(jsonData);
                                isDataLoaded = true;
                                CacheLoadedData();
                            }
                        }
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    onComplete?.Invoke();
                }
            });
    }

    // �ε�� �����͸� ĳ�ÿ� �����ϴ� �Լ�
    private void CacheLoadedData()
    {
        if (!isDataLoaded || loadedGameState == null) return;

        cachedData.Clear();
        foreach (var component in loadedGameState.components)
        {
            Type componentType = Type.GetType(component.path);
            if (componentType != null)
            {
                cachedData[componentType] = component.data;
            }
        }
    }

    // �ε�� ���� ���¸� �����ϴ� �Լ�
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


    #region ������ ����

    // ����� ���� �����͸� �����ϴ� �Լ�
    public void DeleteGameData(Action<bool> onComplete = null)
    {
        if (!isLoggedIn)
        {
            onComplete?.Invoke(false);
            return;
        }

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    // �� �����ͷ� �����
                    CompleteGameState emptyState = new CompleteGameState();
                    string emptyJson = JsonUtility.ToJson(emptyState);
                    byte[] emptyData = Encoding.UTF8.GetBytes(emptyJson);

                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription($"Data deleted: {DateTime.Now.ToString()}")
                        .Build();
                    
                    saveGameClient.CommitUpdate(game, update, emptyData, (saveStatus, savedGame) =>
                    {
                        bool success = saveStatus == SavedGameRequestStatus.Success;
                        if (success)
                        {
                            // ĳ�õ� �����͵� �ʱ�ȭ
                            loadedGameState = null;
                            isDataLoaded = false;
                            cachedData.Clear();
                        }
                        onComplete?.Invoke(success);
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
    }

    // ���� ��ư ã�Ƽ� �����ϴ� �Լ�
    private void FindAndSetupDeleteButton()
    {
        GameObject buttonObj = GameObject.Find("Canvas/Main UI Panel/Top Simple Button Panel/Delete Data Button");
        if (buttonObj != null)
        {
            deleteDataButton = buttonObj.GetComponent<Button>();
            if (deleteDataButton != null)
            {
                deleteDataButton.onClick.RemoveAllListeners();
                deleteDataButton.onClick.AddListener(DeleteGameDataAndQuit);
            }
        }
        else
        {
            deleteDataButton = null;
        }
    }

    // ���� ������ ���� �� �� �����ϴ� �Լ� (��ư�� ������ �Լ�)
    public void DeleteGameDataAndQuit()
    {
        if (!isLoggedIn) return;

        isDeletingData = true;
        Time.timeScale = 0f;

        // ���� ���� ���� ��� ������Ʈ �ʱ�ȭ
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (ISaveable saveable in saveables)
        {
            saveable.LoadFromData(null);
        }

        // Ŭ���� ������ ����
        StartCoroutine(DeleteDataWithConfirmation());
    }

    // ������ ���� Ȯ�� �ڷ�ƾ �߰�
    private IEnumerator DeleteDataWithConfirmation()
    {
        bool deleteCompleted = false;
        bool deleteSuccess = false;

        // ���� �õ�
        DeleteGameData((success) => {
            deleteCompleted = true;
            deleteSuccess = success;
        });

        // ���� �Ϸ� ��� (�ִ� 3��)
        float waitTime = 0;
        while (!deleteCompleted && waitTime < 3.0f)
        {
            waitTime += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // ���� �� ���� ������ �ʱ�ȭ Ȯ��
        loadedGameState = null;
        isDataLoaded = false;
        cachedData.Clear();

        // ���� Ȯ���� ���� �߰� ���� (�� ������)
        CompleteGameState emptyState = new CompleteGameState();
        string emptyJson = JsonUtility.ToJson(emptyState);
        SaveToCloud(emptyJson);

        // ���� �Ϸ� ��� �� ���� ����
        yield return new WaitForSecondsRealtime(2.0f);
        StartCoroutine(QuitGameAfterDelay());
    }

    // ���� �� �� �����ϴ� �ڷ�ƾ
    private IEnumerator QuitGameAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    #endregion


    #region OnApplication

    private void OnApplicationQuit()
    {
        if (!CanSkipAutoSave())
        {
            SaveGameStateSyncImmediate();
        }
    }

    // Ȩ ��ư���� ������ �ڵ� ���� (��׶���� ��ȯ)
    private void OnApplicationPause(bool pause)
    {
        if (pause && !CanSkipAutoSave())
        {
            SaveGameStateSyncImmediate();
        }
    }

    // �ٸ� ������ ��ȯ�� �ڵ� ����
    private void OnApplicationFocus(bool focus)
    {
        if (!focus && !CanSkipAutoSave())
        {
            SaveGameStateSyncImmediate();
        }
    }

    // ��� ����� ���� �Լ� (������ ���� ���)
    private void SaveGameStateSyncImmediate()
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

        // ��� ������ ���� ���� ��� �õ�
        string jsonData = JsonUtility.ToJson(gameState);
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
                        .WithUpdatedDescription($"Emergency save: {DateTime.Now.ToString()}")
                        .Build();
                   
                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => { });
                }
            });
    }

    #endregion


}
