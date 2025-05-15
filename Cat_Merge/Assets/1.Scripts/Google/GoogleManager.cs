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
    public long timestamp;

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

// ���� �α��� & ���� �� �ε� ���� ��ũ��Ʈ
public class GoogleManager : MonoBehaviour
{


    #region Variables

    public static GoogleManager Instance { get; private set; }

    private Button deleteDataButton;                                // ���� ������ ���� ��ư (���߿� ���� ����)
    private const string SAVE_FILE_NAME = "GoogleCloudSaveState";   // ���� �̸�
    private const string GAME_SCENE = "GameScene-Han";              // GameScene �̸�
    private const float AUTO_SAVE_INTERVAL = 30f;                   // �ڵ� ���� ����
    private float autoSaveTimer = 0f;                               // �ڵ� ���� Ÿ�̸�

    [HideInInspector] public bool isLoggedIn = false;               // ���� �α��� ����
    [HideInInspector] public bool isDeleting = false;               // ���� ������ ������ ����
    [HideInInspector] public bool isPlayedGame = false;             // ù ���� ���� ����
    private bool isSaving = false;                                  // ���� ������ Ȯ���ϴ� �÷���
    private bool isGameStarting = false;                            // ���� ���� ������ Ȯ���ϴ� �÷���

    private Vector2 gameStartPosition;                              // ���� ���� ��ġ ��ġ�� ������ ����

    private const string encryptionKey = "CatMergeGame_EncryptionKey";  // ��ȣȭ Ű
    private const string FIRST_PLAY_KEY = "IsFirstPlay";                // ù ���� üũ�� Ű


    private const string SAVE_VERSION_KEY = "SaveVersion";          // ���� ���� Ű
    private const int CURRENT_SAVE_VERSION = 2;                     // ���� ���� ���� (1: ���� ����, 2: ���ο� ����)

    private const int MAX_BACKUP_COUNT = 3;                         // �ִ� ��� ���� ��
    private const int MAX_SAVE_SIZE_MB = 3;                         // �ִ� ���� �뷮 (MB)
    private const string BACKUP_FILE_PREFIX = "Backup_";            // ��� ���� ���λ�
    private const string NETWORK_STATUS_KEY = "NetworkStatus";      // ��Ʈ��ũ ���� Ű

    private bool isNetworkAvailable = false;                        // ��Ʈ��ũ ��� ���� ����
    private Queue<CompleteGameState> backupStates;                  // ��� ���� ť
    private DateTime lastNetworkCheck = DateTime.MinValue;          // ������ ��Ʈ��ũ üũ �ð�
    private const float NETWORK_CHECK_INTERVAL = 30f;               // ��Ʈ��ũ üũ ����

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
        StartCoroutine(InitializeGoogleLogin());

        // ��� �ý��� �ʱ�ȭ
        backupStates = new Queue<CompleteGameState>();

        // ��Ʈ��ũ ���� ����͸� ����
        StartCoroutine(MonitorNetworkStatus());

        // ����� ��Ʈ��ũ ���� ����
        isNetworkAvailable = PlayerPrefs.GetInt(NETWORK_STATUS_KEY, 1) == 1;
    }

    private void Update()
    {
        if (CanSkipAutoSave())
        {
            return;
        }

        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
        {
            //Debug.Log($"[����] {AUTO_SAVE_INTERVAL}�� ����� �ڵ� ���� ����");
            SaveAllGameData();
            autoSaveTimer = 0f;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion


    #region Initialize

    // ���� �÷��� ���񽺸� �ʱ�ȭ�ϴ� �Լ�
    private void InitializeGooglePlay()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        CheckAndMigrateData();
    }

    // ���� ���� Ȯ�� �� ���̱׷��̼� �Լ�
    private void CheckAndMigrateData()
    {
        // PlayerPrefs�� ����� �����Ͱ� �ִ��� ���� Ȯ��
        bool hasExistingData = false;
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(typeName, "")))
            {
                hasExistingData = true;
                break;
            }
        }

        // ���� �����Ͱ� ���� ���� ���̱׷��̼� �˻�
        if (hasExistingData)
        {
            int savedVersion = PlayerPrefs.GetInt(SAVE_VERSION_KEY, 1);
            if (savedVersion < CURRENT_SAVE_VERSION)
            {
                //Debug.Log($"[���̱׷��̼�] ���� ����({savedVersion})�� ������ �߰�. ���̱׷��̼� ����");
                MigrateOldData();
                PlayerPrefs.SetInt(SAVE_VERSION_KEY, CURRENT_SAVE_VERSION);
                PlayerPrefs.Save();
                //Debug.Log("[���̱׷��̼�] ������ ���̱׷��̼� �Ϸ�");
            }
        }
        else
        {
            // ���ο� �����̹Ƿ� ���� �������� ����
            PlayerPrefs.SetInt(SAVE_VERSION_KEY, CURRENT_SAVE_VERSION);
            PlayerPrefs.Save();
        }
    }

    private void MigrateOldData()
    {
        try
        {
            // 1. ���� ������ ���
            Dictionary<string, string> oldData = new Dictionary<string, string>();
            var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();

            foreach (ISaveable saveable in saveables)
            {
                MonoBehaviour mb = (MonoBehaviour)saveable;
                string typeName = mb.GetType().FullName;
                string data = PlayerPrefs.GetString(typeName, "");

                if (!string.IsNullOrEmpty(data))
                {
                    oldData[typeName] = data;
                    //Debug.Log($"[���̱׷��̼�] {typeName} ������ ���");
                }
            }

            // 2. ���ο� �������� ��ȯ
            if (oldData.Count > 0)
            {
                CompleteGameState newGameState = new CompleteGameState();

                foreach (var kvp in oldData)
                {
                    string encryptedData = IsValidBase64(kvp.Value) ? kvp.Value : EncryptData(kvp.Value);
                    newGameState.AddComponentData(kvp.Key, encryptedData);
                }

                newGameState.UpdateTimestamp();

                // 3. ���ο� �������� ����
                if (isLoggedIn)
                {
                    SaveToCloud(JsonUtility.ToJson(newGameState));
                    //Debug.Log("[���̱׷��̼�] Ŭ���忡 ���ο� �������� ����");
                }
                else
                {
                    SaveToPlayerPrefs(newGameState);
                    //Debug.Log("[���̱׷��̼�] PlayerPrefs�� ���ο� �������� ����");
                }
            }
        }
        catch (Exception e)
        {
            //Debug.LogError($"[���̱׷��̼�] ������ ���̱׷��̼� �� ���� �߻�: {e.Message}");
        }
    }

    #endregion


    #region Scene Management

    // �� �ε� �� �ʿ��� ������ �ϴ� �Լ�
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndSetupDeleteButton();
        LoadingScreen.Instance?.UpdateLoadingScreenCamera();
    }

    #endregion


    #region Google Login & Game Start Setting

    // ���� �α����� �õ��ϴ� �ڷ�ƾ
    private IEnumerator InitializeGoogleLogin()
    {
        //Debug.Log("[�ʱ�ȭ] ���� �α��� �õ�");
        bool loginComplete = false;
        PlayGamesPlatform.Instance.Authenticate((status) => {
            ProcessAuthentication(status);
            loginComplete = true;
            //Debug.Log($"[�ʱ�ȭ] ���� �α��� ���: {status}");
        });

        float waitTime = 0;
        while (!loginComplete && waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        //// �ʱ� isPlayedGame ���¸� ���� ����� �������� ����
        //isPlayedGame = PlayerPrefs.HasKey(FIRST_PLAY_KEY);
        //Debug.Log($"[�ʱ�ȭ] ù ���� ���� Ȯ��: {!isPlayedGame}");
    }

    // ���� ���� ����� ó���ϴ� �Լ�
    internal void ProcessAuthentication(SignInStatus status)
    {
        isLoggedIn = (status == SignInStatus.Success);
    }

    // ���� ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ� (��ư�� ����)
    public void OnGameStartButtonClick()
    {
        if (isGameStarting) return;

        TitleManager titleManager = FindObjectOfType<TitleManager>();
        titleManager?.OnGameStart();

        Vector2 touchPosition = Input.mousePosition;
        StartCoroutine(StartGameWithLoad(touchPosition));
    }

    // ���� ���� �� �����͸� �ε��ϴ� �ڷ�ƾ
    private IEnumerator StartGameWithLoad(Vector2 touchPosition)
    {
        isGameStarting = true;
        gameStartPosition = touchPosition;

        // �ε� ȭ�� ǥ��
        LoadingScreen.Instance.Show(true, touchPosition);
        yield return new WaitForSecondsRealtime(LoadingScreen.Instance.animationDuration);

        // �� ��ȯ
        SceneManager.LoadScene(GAME_SCENE);
        yield return new WaitForEndOfFrame();

        // GameManager�� �ʱ�ȭ�� ������ ���
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
            //Debug.LogError("[�ε�] GameManager�� ã�� �� ����");
            yield break;
        }

        // ��� ������Ʈ�� �ʱ�ȭ�� ������ �߰� ���
        yield return new WaitForSecondsRealtime(0.5f);

        UnencryptedData();

        //Debug.Log("[�ε�] ���� ������ �ε� ����");

        CompleteGameState cloudState = null;
        CompleteGameState localState = null;

        // ���� ������ ���� �ε�
        if (PlayerPrefs.HasKey(FIRST_PLAY_KEY))
        {
            localState = LoadLocalStateFromPlayerPrefs();
            //Debug.Log($"[�ε�] ���� ������ Ÿ�ӽ�����: {localState?.timestamp}");
        }

        // ���� �α��� ���¸� Ŭ���� ������ �ε� �õ�
        if (isLoggedIn)
        {
            //Debug.Log("[�ε�] Google Cloud ������ �ε� �õ�");
            bool loadComplete = false;

            LoadFromCloud((success, state) => {
                if (success)
                {
                    cloudState = state;
                    //Debug.Log($"[�ε�] Ŭ���� ������ Ÿ�ӽ�����: {cloudState?.timestamp}");
                }
                loadComplete = true;
            });

            float waitTime = 0;
            while (!loadComplete && waitTime < 5f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }

        // ������ ����ȭ ����
        CompleteGameState finalState = null;

        if (cloudState != null && localState != null)
        {
            // �� �� �ִ� ��� - Ÿ�ӽ����� ��
            if (cloudState.timestamp >= localState.timestamp)
            {
                //Debug.Log("[�ε�] Ŭ���� �����Ͱ� �� �ֽ��̹Ƿ� Ŭ���� ������ ���");
                finalState = cloudState;
            }
            else
            {
                //Debug.Log("[�ε�] ���� �����Ͱ� �� �ֽ��̹Ƿ� ���� ������ ���");
                finalState = localState;
                // �� �ֽ� �����͸� Ŭ���忡 ����ȭ
                if (isLoggedIn && isNetworkAvailable)
                {
                    SaveToCloud(JsonUtility.ToJson(localState));
                }
            }
        }
        else
        {
            // �� �� �ϳ��� �ִ� ���
            finalState = cloudState ?? localState;
            //Debug.Log($"[�ε�] ��� ������ ������ ���: {(cloudState != null ? "Ŭ����" : "����")}");
        }

        // ���� ���õ� ������ ����
        if (finalState != null)
        {
            ApplyGameState(finalState);

            // ��ȿ�� �����Ͱ� �ε�Ǿ����Ƿ� ù ������ �ƴ��� ǥ��
            isPlayedGame = true;
            if (!PlayerPrefs.HasKey(FIRST_PLAY_KEY))
            {
                PlayerPrefs.SetInt(FIRST_PLAY_KEY, 1);
                PlayerPrefs.Save();
            }
            
            //Debug.Log($"[�ε�] ���� ������ ���� �Ϸ� (Ÿ�ӽ�����: {finalState.timestamp})");
        }
        else
        {
            isPlayedGame = false;
        }

        // ��� ���
        yield return new WaitForSecondsRealtime(0.5f);
        LoadingScreen.Instance.Show(false, gameStartPosition);

        // ù ���� ���� Ȯ�� �� ó��
        if (!isPlayedGame)
        {
            //Debug.Log("[�ʱ�ȭ] ù ���� ���� - Ʃ�丮�� ����");
            StartCoroutine(gameManager.ShowFirstGamePanel());
        }

        isGameStarting = false;
        //Debug.Log("[�ε�] ���� ������ �ε� �Ϸ�");
    }

    // PlayerPrefs���� ��ü ���� ���¸� �ε��ϴ� �Լ�
    private CompleteGameState LoadLocalStateFromPlayerPrefs()
    {
        CompleteGameState state = new CompleteGameState();
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();

        foreach (var saveable in saveables)
        {
            if (saveable == null) continue;

            string typeName = saveable.GetType().FullName;
            string encryptedData = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(encryptedData))
            {
                state.AddComponentData(typeName, encryptedData);
            }
        }

        string timestampStr = PlayerPrefs.GetString("LastSaveTime", "0");
        state.timestamp = long.Parse(timestampStr);

        return state.components.Count > 0 ? state : null;
    }

    // ���� ���¸� �����ϴ� �Լ�
    private void ApplyGameState(CompleteGameState state)
    {
        if (state == null) return;

        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();
        foreach (var saveable in saveables)
        {
            string typeName = saveable.GetType().FullName;
            if (state.TryGetValue(typeName, out string encryptedData))
            {
                string decryptedData = DecryptData(encryptedData);
                saveable.LoadFromData(decryptedData);
            }
        }
    }

    #endregion


    #region Save System

    // ������ ���� �����͸� �����ϴ� �Լ�
    public void ForceSaveAllData()
    {
        if (isDeleting) return;
        //Debug.Log("[����] ���� ���� ��û��");
        SaveAllGameData();
        autoSaveTimer = 0f;
    }

    // ��ü ���� �����͸� �����ϴ� �Լ�
    private void SaveAllGameData()
    {
        if (isSaving)
        {
            //Debug.Log("[����] �̹� ���� ���̹Ƿ� ��ŵ");
            return;
        }
        isSaving = true;

        try
        {
            CompleteGameState gameState = new CompleteGameState();
            var saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
            int componentCount = 0;

            foreach (var saveable in saveables)
            {
                if (saveable == null) continue;

                string typeName = saveable.GetType().FullName;
                string data = saveable.GetSaveData();

                if (!string.IsNullOrEmpty(data))
                {
                    string encryptedData = EncryptData(data);
                    gameState.AddComponentData(typeName, encryptedData);
                    componentCount++;
                }
            }

            gameState.UpdateTimestamp();

            // ������ ���Ἲ ����
            if (!ValidateGameState(gameState))
            {
                //Debug.LogError("[����] ������ ���Ἲ ���� ����");
                if (!RestoreFromBackup())
                {
                    //Debug.LogError("[����] ��� ���� ����");
                }
                return;
            }

            // ��� ����
            CreateBackup(gameState);

            // Ŭ���� ����
            if (isLoggedIn && isNetworkAvailable)
            {
                //Debug.Log($"[����] Google Cloud ���� ���� - ������Ʈ {componentCount}��");
                SaveToCloud(JsonUtility.ToJson(gameState));
            }
            else
            {
                //Debug.Log($"[����] PlayerPrefs ���� ���� - ������Ʈ {componentCount}��");
                SaveToPlayerPrefs(gameState);
            }

            // ������ ��� ����
            CleanupOldBackups();
        }
        catch (Exception e)
        {
            //Debug.LogError($"[����] ���� �� ���� �߻�: {e.Message}");
            if (!RestoreFromBackup())
            {
                //Debug.LogError("[����] ��� ���� ����");
            }
        }
        finally
        {
            isSaving = false;
        }

        //Debug.Log("[����] ���� �Ϸ�");
    }

    // PlayerPrefs�� ���� �����͸� �����ϴ� �Լ�
    private void SaveToPlayerPrefs(CompleteGameState gameState)
    {
        try
        {
            foreach (var component in gameState.components)
            {
                PlayerPrefs.SetString(component.path, component.data);
            }
            PlayerPrefs.SetString("LastSaveTime", gameState.timestamp.ToString());
            PlayerPrefs.Save();
            //Debug.Log("[����] PlayerPrefs ���� ����");
        }
        catch (Exception e)
        {
            //Debug.LogError($"[����] PlayerPrefs ���� ����: {e.Message}");
        }
    }

    // ���� Ŭ���忡 ���� �����͸� �����ϴ� �Լ�
    private void SaveToCloud(string jsonData)
    {
        if (!isLoggedIn) return;

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            SAVE_FILE_NAME,
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

                    saveGameClient.CommitUpdate(game, update, data, (commitStatus, savedGame) =>
                    {
                        //Debug.Log($"[����] Google Cloud ���� ���: {commitStatus}");
                    });
                }
                else
                {
                    //Debug.LogError($"[����] Google Cloud ���� ����: {status}");
                }
            });
    }

    #endregion


    #region Load System

    // ���� Ŭ���忡�� ���� �����͸� �ε��ϴ� �Լ�
    private void LoadFromCloud(Action<bool, CompleteGameState> onComplete)
    {
        if (!isLoggedIn)
        {
            onComplete?.Invoke(false, null);
            return;
        }

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            SAVE_FILE_NAME,
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
                                string jsonData = Encoding.UTF8.GetString(data);
                                CompleteGameState loadedState = JsonUtility.FromJson<CompleteGameState>(jsonData);

                                if (ValidateGameState(loadedState))
                                {
                                    onComplete?.Invoke(true, loadedState);
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                //Debug.LogError($"[�ε�] Ŭ���� ������ �Ľ� ����: {e.Message}");
                            }
                        }
                        onComplete?.Invoke(false, null);
                    });
                }
                else
                {
                    onComplete?.Invoke(false, null);
                }
            });
    }

    #endregion


    #region Encryption

    // �����͸� ��ȣȭ�ϴ� �Լ�
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

            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception e)
        {
            //Debug.LogError($"[��ȣȭ ����] {e.Message}");
            return data;
        }
    }

    // �����͸� ��ȣȭ�ϴ� �Լ�
    private string DecryptData(string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData)) return encryptedData;

        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];

            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception e)
        {
            //Debug.LogError($"[��ȣȭ ����] {e.Message}");
            return encryptedData;
        }
    }

    // ��ȣȭ���� ���� �����͸� ��ȣȭ�ϴ� �Լ�
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
                SaveToPlayerPrefs(new CompleteGameState { components = new List<ComponentData> { new ComponentData { path = typeName, data = EncryptData(data) } } });
            }
        }
    }

    // Base64 ������ ��ȿ���� Ȯ���ϴ� �Լ�
    private bool IsValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0)
            return false;

        foreach (char c in base64String)
        {
            if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && (c < '0' || c > '9') && c != '+' && c != '/' && c != '=')
                return false;
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

    #endregion


    #region Application Lifecycle

    // ���� �Ͻ������� �� ȣ��Ǵ� �Լ�
    private void OnApplicationPause(bool pause)
    {
        if (pause && !CanSkipAutoSave())
        {
            SaveAllGameData();
        }
    }

    // ���� ����� �� ȣ��Ǵ� �Լ�
    private void OnApplicationQuit()
    {
        if (!CanSkipAutoSave())
        {
            SaveAllGameData();
        }
    }

    // �ڵ� ������ �ǳʶ� �� �ִ��� Ȯ���ϴ� �Լ�
    private bool CanSkipAutoSave()
    {
        return isDeleting ||
            (GameManager.Instance != null && GameManager.Instance.isQuiting) ||
            (BattleManager.Instance != null && BattleManager.Instance.IsBattleActive);
    }

    #endregion


    #region Delete System

    // ���� ��ư�� ã�� �����ϴ� �Լ�
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

    // ���� �����͸� �����ϰ� ������ �����ϴ� �Լ�
    public void DeleteGameDataAndQuit()
    {
        isDeleting = true;
        Time.timeScale = 0f;

        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (ISaveable saveable in saveables)
        {
            saveable.LoadFromData(null);
        }

        StartCoroutine(DeleteDataWithConfirmation());
    }

    // ������ ������ Ȯ���ϰ� ó���ϴ� �ڷ�ƾ
    private IEnumerator DeleteDataWithConfirmation()
    {
        bool deleteCompleted = false;
        bool deleteSuccess = false;

        DeleteGameData((success) => {
            deleteCompleted = true;
            deleteSuccess = success;
        });

        float waitTime = 0;
        while (!deleteCompleted && waitTime < 3.0f)
        {
            waitTime += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        yield return new WaitForSecondsRealtime(1.0f);
        StartCoroutine(QuitGameAfterDelay());
    }

    // ���� �����͸� �����ϴ� �Լ�
    private void DeleteGameData(Action<bool> onComplete = null)
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (!isLoggedIn)
        {
            onComplete?.Invoke(true);
            return;
        }

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            SAVE_FILE_NAME,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    CompleteGameState emptyState = new CompleteGameState();
                    string emptyJson = JsonUtility.ToJson(emptyState);
                    byte[] emptyData = Encoding.UTF8.GetBytes(emptyJson);

                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription($"Data deleted: {DateTime.Now}")
                        .Build();

                    saveGameClient.CommitUpdate(game, update, emptyData, (saveStatus, savedGame) =>
                    {
                        onComplete?.Invoke(saveStatus == SavedGameRequestStatus.Success);
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
    }

    // ���� �� ������ �����ϴ� �ڷ�ƾ
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


    #region Network Monitoring

    // ��Ʈ��ũ ���¸� ����͸��ϴ� �ڷ�ƾ
    private IEnumerator MonitorNetworkStatus()
    {
        while (true)
        {
            if ((DateTime.Now - lastNetworkCheck).TotalSeconds >= NETWORK_CHECK_INTERVAL)
            {
                lastNetworkCheck = DateTime.Now;
                bool previousState = isNetworkAvailable;
                isNetworkAvailable = Application.internetReachability != NetworkReachability.NotReachable;

                if (previousState != isNetworkAvailable)
                {
                    PlayerPrefs.SetInt(NETWORK_STATUS_KEY, isNetworkAvailable ? 1 : 0);
                    PlayerPrefs.Save();

                    if (isNetworkAvailable)
                    {
                        StartCoroutine(SyncDataWithCloud());
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // Ŭ����� ������ ����ȭ�ϴ� �Լ�
    private IEnumerator SyncDataWithCloud()
    {
        if (!isLoggedIn || !isNetworkAvailable) yield break;

        //Debug.Log("[����ȭ] Ŭ���� ����ȭ ����");
        bool syncComplete = false;

        LoadFromCloud((success, state) => {
            if (success)
            {
                SaveAllGameData();
            }
            syncComplete = true;
        });

        while (!syncComplete)
        {
            yield return null;
        }

        //Debug.Log("[����ȭ] Ŭ���� ����ȭ �Ϸ�");
    }

    #endregion


    #region Data Integrity and Backup

    // ������ ���Ἲ ���� �Լ�
    private bool ValidateGameState(CompleteGameState state)
    {
        if (state == null) return false;

        // Ÿ�ӽ����� ����
        if (state.timestamp <= 0) return false;

        // �⺻ ������ ���� ���� ����
        if (state.components == null || state.components.Count == 0) return false;

        // ������ ũ�� ����
        string jsonData = JsonUtility.ToJson(state);
        float dataSizeMB = Encoding.UTF8.GetByteCount(jsonData) / (1024f * 1024f);
        if (dataSizeMB > MAX_SAVE_SIZE_MB) return false;

        return true;
    }

    // ��� ���� �Լ�
    private void CreateBackup(CompleteGameState state)
    {
        if (!ValidateGameState(state)) return;

        backupStates.Enqueue(state);
        while (backupStates.Count > MAX_BACKUP_COUNT)
        {
            backupStates.Dequeue();
        }

        // ���ÿ��� ��� ����
        string backupJson = JsonUtility.ToJson(state);
        string backupKey = $"{BACKUP_FILE_PREFIX}{state.timestamp}";
        PlayerPrefs.SetString(backupKey, backupJson);
        PlayerPrefs.Save();
    }

    // ������� �����ϴ� �Լ�
    private bool RestoreFromBackup()
    {
        if (backupStates.Count == 0) return false;

        CompleteGameState backupState = backupStates.Last();
        if (!ValidateGameState(backupState)) return false;

        //Debug.Log("[����] ������� ������ ���� ����");

        try
        {
            var saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
            foreach (var saveable in saveables)
            {
                string typeName = saveable.GetType().FullName;
                if (backupState.TryGetValue(typeName, out string encryptedData))
                {
                    string decryptedData = DecryptData(encryptedData);
                    saveable.LoadFromData(decryptedData);
                }
            }

            //Debug.Log("[����] ������� ������ ���� �Ϸ�");
            return true;
        }
        catch (Exception e)
        {
            //Debug.LogError($"[����] ��� ���� ����: {e.Message}");
            return false;
        }
    }

    // ������ ��� �����ϴ� �Լ�
    private void CleanupOldBackups()
    {
        var keys = new List<string>();
        var timestamps = new List<long>();

        // PlayerPrefs���� ��� Ű ã��
        foreach (string key in PlayerPrefs.GetString("").Split('\0'))
        {
            if (key.StartsWith(BACKUP_FILE_PREFIX))
            {
                keys.Add(key);
                long timestamp = long.Parse(key.Substring(BACKUP_FILE_PREFIX.Length));
                timestamps.Add(timestamp);
            }
        }

        // ������ ��� ����
        if (keys.Count > MAX_BACKUP_COUNT)
        {
            var sortedIndices = timestamps
                .Select((t, i) => new { Timestamp = t, Index = i })
                .OrderByDescending(x => x.Timestamp)
                .Skip(MAX_BACKUP_COUNT)
                .Select(x => x.Index);

            foreach (int index in sortedIndices)
            {
                PlayerPrefs.DeleteKey(keys[index]);
            }
            PlayerPrefs.Save();
        }
    }

    #endregion


}
