using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;
using System.Text;
using System;
using System.Linq;

// ����/�ε尡 �ʿ��� ������Ʈ�� ������ �������̽�
public interface ISaveable
{
    string GetSaveData();
    void LoadFromData(string data);
}

[System.Serializable]
public class CompleteGameState
{
    public Dictionary<string, string> componentData = new Dictionary<string, string>();
}

public class GoogleManager : MonoBehaviour
{
    public static GoogleManager Instance { get; private set; }

    private TextMeshProUGUI logText;                        // �ӽ� �α��� �α� �ؽ�Ʈ (���߿��� ������ ����)

    private const string fileName = "GameCompleteState";    // ������ ���ϸ�
    private bool isLoggedIn = false;                        // �α��� ���� Ȯ��

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
    }

    public void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        UpdateLogText();  // logText ã��
        GPGS_LogIn();
    }

    // �ӽ� logText ã�Ƽ� �����ϴ� �Լ�
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
            if (logText != null)
            {
                logText.text = "�α��� ����";
            }
            isLoggedIn = false;
        }
    }

    // ======================================================================================================================

    // ��ü ���� ���� ����
    public void SaveGameState()
    {
        if (!isLoggedIn) return;

        CompleteGameState gameState = new CompleteGameState();

        // Scene���� ISaveable �������̽��� ������ ��� ������Ʈ ã��
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();

        foreach (ISaveable saveable in saveables)
        {
            // �� ������Ʈ�� ���� �ĺ��ڷ� MonoBehaviour�� ��ü ��� ���
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string path = GetGameObjectPath(mb.gameObject);
            gameState.componentData[path] = saveable.GetSaveData();
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
                        .WithUpdatedDescription("Last saved: " + DateTime.Now.ToString())
                        .Build();

                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) =>
                    {
                        Debug.Log(saveStatus == SavedGameRequestStatus.Success ? "���� ���� ���� ����" : "���� ���� ���� ����");
                    });
                }
                else
                {
                    Debug.Log("������ �� �� ���� (���� ����)");
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
                            CompleteGameState gameState = JsonUtility.FromJson<CompleteGameState>(jsonData);

                            if (gameState != null)
                            {
                                // Scene�� ��� ISaveable ������Ʈ ã��
                                ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();

                                foreach (ISaveable saveable in saveables)
                                {
                                    MonoBehaviour mb = (MonoBehaviour)saveable;
                                    string path = GetGameObjectPath(mb.gameObject);

                                    if (gameState.componentData.TryGetValue(path, out string componentData))
                                    {
                                        saveable.LoadFromData(componentData);
                                    }
                                }
                                Debug.Log("���� ���� �ε� ����");
                            }
                        }
                        else
                        {
                            Debug.Log("������ �б� ����");
                        }
                    });
                }
                else
                {
                    Debug.Log("������ �� �� ���� (�ε� ����)");
                }
            });
    }

    // GameObject�� ��ü ��θ� ��� ���� �Լ�
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    // ���� ���� �� �ڵ� ����
    private void OnApplicationQuit()
    {
        SaveGameState();
    }

    // ���� ��׶���� ���� �ڵ� ����
    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveGameState();
    }

    // ======================================================================================================================


}