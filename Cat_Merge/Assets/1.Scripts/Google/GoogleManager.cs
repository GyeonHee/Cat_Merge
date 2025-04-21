using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
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
        InitializeGooglePlay();
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

    #endregion


    #region Initialize

    // Google Play ���� ����  �Լ�
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


    #region ���� �α��� �� UI

    // ���� �α��� �ڷ�ƾ
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
        }
        else
        {
            isLoggedIn = false;
        }
    }

    // ���� ���� ��ư�� ������ public �޼���
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

    // � �����Ͷ� �����ϴ��� Ȯ���ϴ� �Լ�
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


    #region Save System

    // �ڵ� ������ ��ŵ�ص� �Ǵ��� �Ǻ��ϴ� �Լ�
    private bool CanSkipAutoSave()
    {
        return isDeleting || (GameManager.Instance != null && GameManager.Instance.isQuiting);
    }

    // PlayerPrefs�� ������ �����ϴ� �Լ�
    public void SaveToPlayerPrefs(string key, string data)
    {
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();
    }

    // PlayerPrefs���� ������Ʈ ������ �ε��ϴ� �Լ�
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
            string data = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(data))
            {
                saveable.LoadFromData(data);
            }
        }
    }

    // �ε�� �����͸� ������Ʈ�� �����ϴ� �Լ�
    private void ApplyLoadedGameState(GameObject gameManagerObject)
    {
        var saveables = gameManagerObject.GetComponents<MonoBehaviour>().OfType<ISaveable>();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(data))
            {
                saveable.LoadFromData(data);
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

        string jsonData = JsonUtility.ToJson(gameState);
        SaveToCloud(jsonData);
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
                            string jsonData = Encoding.UTF8.GetString(data);
                            CompleteGameState loadedState = JsonUtility.FromJson<CompleteGameState>(jsonData);

                            foreach (var component in loadedState.components)
                            {
                                PlayerPrefs.SetString(component.path, component.data);
                            }
                            PlayerPrefs.Save();

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

    #endregion


    #region ������ ����

    // ����� ���� �����͸� �����ϴ� �Լ�
    public void DeleteGameData(Action<bool> onComplete = null)
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

        // ���� �Ϸ� ��� �� ���� ����
        yield return new WaitForSecondsRealtime(1.0f);
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


}
