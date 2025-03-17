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

    // �߰��� ����
    private bool isSaving = false;

    // �߰��� ��������Ʈ: ���� �Ϸ� �ݹ�
    public delegate void SaveCompletedCallback(bool success);

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

    // �� �ε� �Ϸ�� �����͸� �����ϴ� �Լ�
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� ���� �ε�Ǹ� ������ ����
        if (scene.name == gameScene)
        {
            ShowLoadingScreen(true);
            StartCoroutine(ApplyDataAndShowScreenCoroutine());
        }
    }

    // ������ ���� �� ȭ�� ǥ�ø� ������Ű�� �ڷ�ƾ
    private IEnumerator ApplyDataAndShowScreenCoroutine()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        ApplyDataAndShowScreen();
    }

    #endregion

    #region ���� �α��� �� UI

    // logText ã�Ƽ� �����ϴ� �Լ�
    private void UpdateLogText()
    {
        logText = GameObject.Find("Canvas/Title UI/Log Text")?.GetComponent<TextMeshProUGUI>();
    }

    // ���� �÷��� �α����� �õ��ϴ� �Լ�
    public void GPGS_LogIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
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

    // ��ü ���� ���¸� �����ϴ� �Լ�
    public void SaveGameState()
    {
        if (!isLoggedIn) return;

        // �̹� ���� ���̸� �ߺ� ���� ����
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
        if (!isLoggedIn)
        {
            if (callback != null) callback(false);
            return;
        }

        Debug.Log("����� ���� ����...");
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

        // ���� �Ϸ� �÷���
        bool saveCompleted = false;

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
                        saveCompleted = true;
                        bool success = saveStatus == SavedGameRequestStatus.Success;
                        if (success)
                        {
                            Debug.Log("����� Ŭ���� ���� ����: " + DateTime.Now.ToString());
                        }
                        else
                        {
                            Debug.LogWarning("����� Ŭ���� ���� ����: " + saveStatus);
                        }

                        if (callback != null) callback(success);
                    });
                }
                else
                {
                    Debug.LogError("����� ���� ���� ���� ����: " + status);
                    saveCompleted = true;
                    if (callback != null) callback(false);
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
        Debug.Log("Ŭ���� ���� ����...");

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
                        if (saveStatus == SavedGameRequestStatus.Success)
                        {
                            Debug.Log("Ŭ���� ���� ����: " + DateTime.Now.ToString());
                        }
                        else
                        {
                            Debug.LogWarning("Ŭ���� ���� ����: " + saveStatus);
                        }
                    });
                }
                else
                {
                    isSaving = false;
                    Debug.LogError("���� ���� ���� ����: " + status);
                }
            });
    }

    // ��ü ���� ���¸� �ε��ϴ� �Լ�
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

    // �ε�� �����͸� ĳ�ÿ� �����ϴ� �Լ�
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

    #region �ε� ȭ�� ����

    // �ε� ȭ���� ǥ���ϰų� ����� �Լ�
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

    // �����͸� �����ϰ� ȭ���� ǥ���ϴ� �Լ�
    private void ApplyDataAndShowScreen()
    {
        ApplyLoadedGameState();
        StartCoroutine(HideLoadingScreenCoroutine());
    }

    // �ε� ȭ���� ����� �ڷ�ƾ
    private IEnumerator HideLoadingScreenCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        ShowLoadingScreen(false);
    }

    #endregion

    #region OnApplication

    // ���� ���忡 ������ �ִ� ���� (Android)
    // ���������ư���� ������ = ���� O
    // Ȩ���� �����ٰ� �ٽ� ���ͼ� ���������ư���� ������ = ���� O
    // Ȩ���� �����ٰ� �ٽ� ���ͼ� �����ǹ�ư ������ �� ����� = ���� O
    // Ȩ���� �����ٰ� �����ǹ�ư ������ �� ����� = ���� X
    // ������ �����ǹ�ư ������ �� ����� = ���� X

    // ������ �����͵��� ����ɶ� �������ϴ� ������ �߰��ϴϱ� �ȵǴ��͵��� ������ ������ ����
    // ���� �������ڸ��� �ٷ� �����ǹ�ư ������ �� ����� ���� X
    // ���� �������ڸ��� �ٷ� Ȩ���� �����ΰ� �����ǹ�ư ������ �� ����� ���� X
    // ���� �����ϰ� 2~3�ʴ� ���ӿ� �ӹ����� ������� ������ ���� O

    // �� ����� ����� ������ �����ϴ� �Լ�
    private void OnApplicationQuit()
    {
        SaveGameStateSyncImmediate();
    }

    // Ȩ ��ư���� ������ �ڵ� ���� (��׶���� ��ȯ)
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGameStateSyncImmediate();
        }
    }

    // �ٸ� ������ ��ȯ�� �ڵ� ����
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
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
            try
            {
                MonoBehaviour mb = (MonoBehaviour)saveable;
                Type componentType = mb.GetType();
                string typeName = componentType.FullName;
                string data = saveable.GetSaveData();

                cachedData[componentType] = data;
                gameState.AddComponentData(typeName, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"���� �� ���� �߻�: {e.Message}");
            }
        }

        string jsonData = JsonUtility.ToJson(gameState);

        // ��� ������ ���� ���� ��� �õ�
        try
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
                            .WithUpdatedDescription("Emergency save: " + DateTime.Now.ToString())
                            .Build();

                        saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => {
                            if (saveStatus == SavedGameRequestStatus.Success)
                            {
                                Debug.Log("��� ���� ����: " + DateTime.Now.ToString());
                            }
                            else
                            {
                                Debug.LogWarning("��� ���� ����: " + saveStatus);
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("��� ���� ���� ���� ����: " + status);
                    }
                });
        }
        catch (Exception e)
        {
            Debug.LogError($"��� ���� �� ���� �߻�: {e.Message}");
        }
    }
    #endregion

}
