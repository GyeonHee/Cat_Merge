using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
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
    public long timestamp;  // Ÿ�ӽ����� �߰�

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

    public void UpdateTimestamp()
    {
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

public class GoogleManager : MonoBehaviour
{


    #region Variables

    public static GoogleManager Instance { get; private set; }

    private Button deleteDataButton;                        // ���� ������ ���� ��ư (���߿� ���� ����)

    private const string fileName = "GoogleCloudSaveState"; // ���� �̸�
    private const string gameScene = "GameScene-Han";       // GameScene �̸�
    private const float autoSaveInterval = 30f;             // �ֱ��� �ڵ� ���� �ð�
    private float autoSaveTimer = 0f;                       // �ڵ� ���� �ð� ��� Ÿ�̸�

    [HideInInspector] public bool isLoggedIn = false;       // ���� �α��� ����
    [HideInInspector] public bool isDeleting = false;       // ���� ������ ������ ����
    private bool isSaving = false;                          // ���� ������ Ȯ���ϴ� �÷���
    private bool isGameStarting = false;                    // ���� ���� ������ Ȯ���ϴ� �÷���

    private Vector2 gameStartPosition;                      // ���� ���� ��ġ ��ġ�� ������ ����

    private const string encryptionKey = "CatMergeGame_EncryptionKey";  // ��ȣȭ Ű

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

    private void Start()
    {
        InitializeGooglePlay();
        UnencryptedData();
        StartCoroutine(GPGS_Login());
    }

    private void Update()
    {
        if (CanSkipAutoSave()) return;

        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            SaveToCloudWithLocalData();
            autoSaveTimer = 0f;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion


    #region Initialize

    // Google Play ���� ���� �Լ�
    private void InitializeGooglePlay()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    #endregion


    #region Scene Management

    // �� �ε� �Ϸ�� �����͸� �����ϴ� �Լ�
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� �ε�� �� ���� ��ư ã��
        FindAndSetupDeleteButton();

        // ī�޶� ������Ʈ�� �׻� ����
        LoadingScreen.Instance?.UpdateLoadingScreenCamera();
    }

    #endregion


    #region Google Login & Game Start Setting

    // ���� �α��� �ڷ�ƾ
    private IEnumerator GPGS_Login()
    {
        // ���� �α��� �õ�
        bool loginComplete = false;
        PlayGamesPlatform.Instance.Authenticate((status) => {
            ProcessAuthentication(status);
            loginComplete = true;
        });

        // �α��� �Ϸ� ��� (�ִ� 5��)
        float waitTime = 0;
        while (!loginComplete && waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        // ������ ����ȭ �� �ε�
        if (isLoggedIn)
        {
            bool syncComplete = false;
            SynchronizeData((success) => {
                if (success)
                {
                    LoadFromLocalPlayerPrefs(gameObject);
                }
                else
                {
                    Debug.LogWarning("Ŭ���� ����ȭ ����. ���� �����͸� ����մϴ�.");
                    LoadFromLocalPlayerPrefs(gameObject);
                }
                syncComplete = true;
            });

            // ����ȭ �Ϸ� ��� (�ִ� 5��)
            waitTime = 0;
            while (!syncComplete && waitTime < 5f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // ��α��� ����: ���� �����͸� �ε�
            LoadFromLocalPlayerPrefs(gameObject);
        }
    }

    // ���� �α��� ����� ó���ϴ� �Լ�
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            isLoggedIn = true;
        }
        else
        {
            isLoggedIn = false;
        }
    }

    // ���� ���� ��ư�� ������ public �Լ�
    public void OnGameStartButtonClick()
    {
        // �̹� ���� ���� ���̸� ����
        if (isGameStarting) return;

        // TitleManager ã�Ƽ� ���� ���� �˸�
        TitleManager titleManager = FindObjectOfType<TitleManager>();
        titleManager?.OnGameStart();

        // ��ġ ��ġ ��������
        Vector2 touchPosition = Input.mousePosition;
        StartCoroutine(StartGameWithLoad(touchPosition));
    }

    // ���� ���� ��ư�� ������ ���۵Ǵ� ���� ���� �ڷ�ƾ
    private IEnumerator StartGameWithLoad(Vector2 touchPosition)
    {
        isGameStarting = true;
        gameStartPosition = touchPosition;

        // 1. �ε� ȭ�� ����
        LoadingScreen.Instance.Show(true, touchPosition);

        // 2. �ε� �ִϸ��̼� �Ϸ� ���
        yield return new WaitForSecondsRealtime(LoadingScreen.Instance.animationDuration);

        // 3. �� ��ȯ
        SceneManager.LoadScene(gameScene);

        // 4. �� �ε� �Ϸ� ���
        yield return new WaitForEndOfFrame();

        // 4-1. GameManager ã�� ������ ���
        int maxAttempts = 10;
        int attempts = 0;
        GameManager gameManager = null;

        while (gameManager == null && attempts < maxAttempts)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                attempts++;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        if (gameManager == null)
        {
            yield break;
        }

        // 5. ������ �ε� �� ����
        bool loadComplete = false;
        if (isLoggedIn)
        {
            // �α��� ����: Ŭ���� ������ �ε� �õ�
            LoadFromCloud((success) => {
                if (!success)
                {
                    // Ŭ���� �ε� ���н� ���� ������ ���
                    LoadFromLocalPlayerPrefs(gameManager.gameObject);
                }
                loadComplete = true;
            });
        }
        else
        {
            // ��α��� ����: ���� �����͸� �ε�
            LoadFromLocalPlayerPrefs(gameManager.gameObject);
            loadComplete = true;
        }

        // 5-1. �ε� �Ϸ� ���
        float waitTime = 0;
        while (!loadComplete && waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        // 5-2. ������ ����
        ApplyLoadedGameState(gameManager.gameObject);

        // 6. �ε� ȭ�� �����
        yield return new WaitForSecondsRealtime(0.5f);
        LoadingScreen.Instance.Show(false, gameStartPosition);

        // 7. ù���� �Ǻ�
        bool hasAnyData = CheckForAnyData(gameManager);
        if (!hasAnyData)
        {
            StartCoroutine(gameManager.ShowFirstGamePanel());
        }

        isGameStarting = false;
    }

    // � �����Ͷ� �����ϴ��� Ȯ���ϴ� �Լ� (ù �������� �Ǻ�)
    private bool CheckForAnyData(GameManager gameManager)
    {
        var saveables = gameManager.gameObject.GetComponents<MonoBehaviour>().OfType<ISaveable>();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(data))
            {
                return true;
            }
        }

        return false;
    }

    #endregion


    #region Data Save and Load System

    // �ڵ� ������ ��ŵ�ص� �Ǵ��� �Ǻ��ϴ� �Լ�
    private bool CanSkipAutoSave()
    {
        return isDeleting || (GameManager.Instance != null && GameManager.Instance.isQuiting);
    }

    // ���ڿ� ��ȣȭ �Լ�
    private string EncryptData(string data)
    {
        if (string.IsNullOrEmpty(data)) return data;

        try
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] encryptedBytes = new byte[dataBytes.Length];

            for (int i = 0; i < dataBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(dataBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            string result = Convert.ToBase64String(encryptedBytes);

            // ����� ������ ������ ����
            if (result == data)
            {
                Debug.LogError("[��ȣȭ ����] ��ȣȭ�� ����� ������ �����մϴ�!");
            }

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"[��ȣȭ ����] {e.Message}\n{e.StackTrace}");
            return data;
        }
    }

    // ���ڿ� ��ȣȭ �Լ�
    private string DecryptData(string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData)) return encryptedData;

        try
        {
            // �����Ͱ� Base64 �������� Ȯ�� (��ȣȭ�� ���������� Ȯ��)
            if (!IsValidBase64(encryptedData))
            {
                return encryptedData;
            }

            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];

            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            string result = Encoding.UTF8.GetString(decryptedBytes);

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"[��ȣȭ ����] {e.Message}\n{e.StackTrace}");
            return encryptedData;
        }
    }

    // ���ڿ��� ��ȿ�� Base64 �������� Ȯ���ϴ� �Լ�
    private bool IsValidBase64(string base64String)
    {
        // Base64 ���ڿ��� ���̰� 4�� ������� �ϸ�, Ư�� ���ڸ� �����ؾ� ��
        if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0)
        {
            return false;
        }

        // Base64�� ���Ǵ� ���ڸ� �����ϴ��� Ȯ�� (A-Z, a-z, 0-9, +, /, =)
        foreach (char c in base64String)
        {
            if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && (c < '0' || c > '9') && c != '+' && c != '/' && c != '=')
            {
                return false;
            }
        }

        try
        {
            Convert.FromBase64String(base64String);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ��� ��ȣȭ���� ���� �����͸� ��ȣȭ�ϴ� �Լ�
    private void UnencryptedData()
    {
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(data) && !IsValidBase64(data))
            {
                SaveToPlayerPrefs(typeName, data);
            }
        }
    }

    // PlayerPrefs�� ��ȣȭ�� ������ �����ϴ� �Լ�
    public void SaveToPlayerPrefs(string key, string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            // ���� �����Ϳ� ��ȣȭ�� ������
            string encryptedData = EncryptData(data);

            // ��ȣȭ�� ����� �Ǿ����� Ȯ��
            bool isEncrypted = IsValidBase64(encryptedData) && encryptedData != data;

            // ��ȣȭ�� �����ߴٸ� �ٽ� �õ�
            if (!isEncrypted)
            {
                encryptedData = ForcedEncrypt(data);
            }

            PlayerPrefs.SetString(key, encryptedData);
            PlayerPrefs.Save();
        }
    }

    // ���� ��ȣȭ �Լ� (���� �ذ��)
    private string ForcedEncrypt(string data)
    {
        if (string.IsNullOrEmpty(data)) return data;

        try
        {
            // ��������� �ܰ躰�� ��ȣȭ ����
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] encryptedBytes = new byte[dataBytes.Length];

            // XOR ��ȣȭ
            for (int i = 0; i < dataBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(dataBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            // Base64�� ���ڵ�
            string result = Convert.ToBase64String(encryptedBytes);
            Debug.Log($"���� ��ȣȭ ���: ����={result.Length}, ����={result.Substring(0, Math.Min(10, result.Length))}");
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ��ȣȭ ����: {e.Message}");
            // ��ȣȭ ���� �� �⺻ Base64 ���ڵ��̶� ����
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }
    }

    // PlayerPrefs���� ������ �ε��ϴ� �Լ�
    private void LoadFromLocalPlayerPrefs(GameObject gameManagerObject)
    {
        // GameManager ������Ʈ���� ��� ������Ʈ ��������
        var allComponents = gameManagerObject.GetComponents<MonoBehaviour>();

        List<ISaveable> saveables = new List<ISaveable>();
        foreach (var component in allComponents)
        {
            if (component is ISaveable saveable)
            {
                saveables.Add(saveable);
            }
        }

        if (saveables.Count == 0)
        {
            return;
        }

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            Type componentType = mb.GetType();
            string typeName = componentType.FullName;

            // PlayerPrefs���� ���� ��ȣȭ�� ���� ������ ��������
            string rawData = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(rawData))
            {
                // ��ȣȭ ����
                string decryptedData = DecryptData(rawData);
                saveable.LoadFromData(decryptedData);
            }
        }
    }

    // �ε�� �����͸� �����ϴ� �Լ�
    private void ApplyLoadedGameState(GameObject gameManagerObject)
    {
        var saveables = gameManagerObject.GetComponents<MonoBehaviour>().OfType<ISaveable>();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;

            // PlayerPrefs���� ���� ��ȣȭ�� ���� ������ ��������
            string rawData = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(rawData))
            {
                // ��ȣȭ ����
                string decryptedData = DecryptData(rawData);
                saveable.LoadFromData(decryptedData);
            }
        }
    }

    // ��� PlayerPrefs �����͸� ���� Ŭ���忡 �����ϴ� �Լ�
    // 30�ʸ��� �ڵ�����, ���� �����ϱ��ư, ���� ����, ���������� ����
    public void SaveToCloudWithLocalData()
    {
        if (!isLoggedIn || isSaving) return;

        isSaving = true;
        CompleteGameState gameState = new CompleteGameState();
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = saveable.GetSaveData();

            if (!string.IsNullOrEmpty(data))
            {
                gameState.AddComponentData(typeName, data);
            }
        }

        gameState.UpdateTimestamp();
        SaveLocalTimestamp(gameState.timestamp);

        string jsonData = JsonUtility.ToJson(gameState);
        SaveToCloud(EncryptData(jsonData));
        isSaving = false;
    }

    // ���� Ŭ���忡 ������ �����ϴ� �Լ�
    private void SaveToCloud(string jsonData)
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
                    byte[] data = Encoding.UTF8.GetBytes(jsonData);
                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription($"Last saved: {DateTime.Now}")
                        .Build();

                    saveGameClient.CommitUpdate(game, update, data, null);
                }
            });
    }

    // ���� Ŭ���忡�� ������ �ε��ϴ� �Լ�
    private void LoadFromCloud(Action<bool> onComplete)
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
                    saveGameClient.ReadBinaryData(game, (readStatus, data) =>
                    {
                        if (readStatus == SavedGameRequestStatus.Success && data != null && data.Length > 0)
                        {
                            string encryptedJsonData = Encoding.UTF8.GetString(data);
                            string decryptedJsonData = DecryptData(encryptedJsonData);
                            CompleteGameState loadedState = JsonUtility.FromJson<CompleteGameState>(decryptedJsonData);

                            foreach (var component in loadedState.components)
                            {
                                SaveToPlayerPrefs(component.path, component.data);
                            }

                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            onComplete?.Invoke(false);
                        }
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
    }

    // ���� ISaveable ������Ʈ�� �� ���� �����ϴ� �Լ�
    public void SaveAllSaveables(ISaveable[] saveables)
    {
        foreach (ISaveable saveable in saveables)
        {
            if (saveable == null) continue;

            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = saveable.GetSaveData();

            if (!string.IsNullOrEmpty(data))
            {
                SaveToPlayerPrefs(typeName, data);
            }
        }
    }

    // ��� Ŭ���� ���� ���� �Լ� (�����)
    public void SaveGameStateSyncImmediate()
    {
        SaveToCloudWithLocalData();
    }

    #endregion


    #region Data Remove System

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
        isDeleting = true;
        Time.timeScale = 0f;

        // ���� ���� ���� ��� ������Ʈ �ʱ�ȭ
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (ISaveable saveable in saveables)
        {
            saveable.LoadFromData(null);
        }

        // Ŭ���� �� ���� ������ ����
        StartCoroutine(DeleteDataWithConfirmation());
    }

    // ������ ���� Ȯ�� �ڷ�ƾ
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

        // ���� �Ϸ� ��� �� ���� ����
        yield return new WaitForSecondsRealtime(1.0f);
        StartCoroutine(QuitGameAfterDelay());
    }

    // ����� ���� �����͸� �����ϴ� �Լ�
    private void DeleteGameData(Action<bool> onComplete = null)
    {
        // PlayerPrefs ������ ����
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // �α��� ���°� �ƴϸ� ���� ������ �ϰ� ����
        if (!isLoggedIn)
        {
            onComplete?.Invoke(true);
            return;
        }

        // Ŭ���� ������ ����
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
                            //cachedData.Clear();
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

    // ���� ������ �ڵ� ����
    private void OnApplicationQuit()
    {
        if (!CanSkipAutoSave())
        {
            SaveToCloudWithLocalData();
        }
    }

    // Ȩ ��ư���� ������ �ڵ� ���� (��׶���� ��ȯ)
    private void OnApplicationPause(bool pause)
    {
        if (pause && !CanSkipAutoSave())
        {
            SaveToCloudWithLocalData();
        }
    }

    // �ٸ� ������ ��ȯ�� �ڵ� ����
    private void OnApplicationFocus(bool focus)
    {
        if (!focus && !CanSkipAutoSave())
        {
            SaveToCloudWithLocalData();
        }
    }

    #endregion


    #region Data Synchronization

    private const string LOCAL_TIMESTAMP_KEY = "LocalDataTimestamp";
    private CompleteGameState cachedLocalState;

    // ���� �������� Ÿ�ӽ����� ����
    private void SaveLocalTimestamp(long timestamp)
    {
        PlayerPrefs.SetString(LOCAL_TIMESTAMP_KEY, timestamp.ToString());
        PlayerPrefs.Save();
    }

    // ���� �������� Ÿ�ӽ����� �ε�
    private long LoadLocalTimestamp()
    {
        string timestampStr = PlayerPrefs.GetString(LOCAL_TIMESTAMP_KEY, "0");
        return long.Parse(timestampStr);
    }

    // ���� �����͸� CompleteGameState�� ��ȯ
    private CompleteGameState GetLocalGameState()
    {
        if (cachedLocalState != null)
            return cachedLocalState;

        CompleteGameState localState = new CompleteGameState();
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(data))
            {
                localState.AddComponentData(typeName, data);
            }
        }

        localState.timestamp = LoadLocalTimestamp();
        cachedLocalState = localState;
        return localState;
    }

    // Ŭ���� �����͸� ���ÿ� ����
    private void ApplyCloudToLocal(CompleteGameState cloudState)
    {
        foreach (var component in cloudState.components)
        {
            SaveToPlayerPrefs(component.path, component.data);
        }
        SaveLocalTimestamp(cloudState.timestamp);
        cachedLocalState = null; // ĳ�� ��ȿȭ
    }

    // ���� �����͸� Ŭ���忡 ����
    private void ApplyLocalToCloud()
    {
        var localState = GetLocalGameState();
        localState.UpdateTimestamp();
        SaveLocalTimestamp(localState.timestamp);
        string jsonData = JsonUtility.ToJson(localState);
        SaveToCloud(EncryptData(jsonData));
        cachedLocalState = null; // ĳ�� ��ȿȭ
    }

    // ������ ����ȭ ó��
    public void SynchronizeData(Action<bool> onComplete)
    {
        if (!isLoggedIn)
        {
            onComplete?.Invoke(true);
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
                        if (readStatus == SavedGameRequestStatus.Success && data != null && data.Length > 0)
                        {
                            try
                            {
                                string encryptedJsonData = Encoding.UTF8.GetString(data);
                                string decryptedJsonData = DecryptData(encryptedJsonData);
                                CompleteGameState cloudState = JsonUtility.FromJson<CompleteGameState>(decryptedJsonData);
                                CompleteGameState localState = GetLocalGameState();

                                // Ÿ�ӽ����� �� �� ����ȭ
                                if (cloudState.timestamp > localState.timestamp)
                                {
                                    // Ŭ���� �����Ͱ� �� �ֽ�
                                    ApplyCloudToLocal(cloudState);
                                }
                                else if (cloudState.timestamp < localState.timestamp)
                                {
                                    // ���� �����Ͱ� �� �ֽ�
                                    ApplyLocalToCloud();
                                }
                                onComplete?.Invoke(true);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"������ ����ȭ �� ���� �߻�: {e.Message}");
                                onComplete?.Invoke(false);
                            }
                        }
                        else
                        {
                            // Ŭ���忡 �����Ͱ� ������ ���� �����͸� ���ε�
                            ApplyLocalToCloud();
                            onComplete?.Invoke(true);
                        }
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
    }

    #endregion

}
