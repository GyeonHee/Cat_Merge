using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;

public class GoogleManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logText;

    private void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        GPGS_LogIn();
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
            string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            string userID = PlayGamesPlatform.Instance.GetUserId();

            logText.text = "�α��� ���� : " + displayName + " / " + userID;
            //Debug.Log("Success");
        }
        else
        {
            logText.text = "�α��� ����";
            //Debug.Log("Fail");
            //GameManager.instance.DataSave_Scr.LoadUserDataFun();
        }
    }

    /////////////////////////////////////////////////////////////////////////////

    // ������ ����
    /*
    public void SaveData() // �ܺο��� ������ �Լ�
    {
        OpenSaveGame();
    }

    private void OpenSaveGame()
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        // ������ ����
        saveGameClient.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood,
            onsavedGameOpend);
    }


    private void onsavedGameOpend(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        if (status == SavedGameRequestStatus.Success)
        {
            var update = new SavedGameMetadataUpdate.Builder().Build();

            //json
            var json = JsonUtility.ToJson("�����Ϸ��� ������!");
            byte[] data = Encoding.UTF8.GetBytes(json);

            // ���� �Լ� ����
            saveGameClient.CommitUpdate(game, update, data, OnSavedGameWritten);
        }
        else
        {
            Debug.Log("Save No.....");
        }
    }

    // ���� Ȯ�� 
    private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // ����Ϸ�κ�
            Debug.Log("Save End");
        }
        else
        {
            Debug.Log("Save nonononononono...");
        }
    }
    */

    /////////////////////////////////////////////////////////////////////////////

    // ������ �ε�
    /*
    public void LoadData()
    {
        OpenLoadGame();
    }

    private void OpenLoadGame()
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        saveGameClient.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood,
            LoadGameData);
    }

    private void LoadGameData(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        if (status == SavedGameRequestStatus.Success)
        {
            Debug.Log("!! GoodLee");

            // ������ �ε�
            saveGameClient.ReadBinaryData(data, onSavedGameDataRead);
        }
        else
        {
            Debug.Log("?? no");
        }
    }

    // �ҷ��� ������ ó�� 
    private void onSavedGameDataRead(SavedGameRequestStatus status, byte[] loadedData)
    {
        string data = System.Text.Encoding.UTF8.GetString(loadedData);

        if (data == "")
        {
            SaveData();
        }
        else
        {
            // �ҷ��� �����͸� ���� ó�����ִ� �κ� �ʿ�!
        }
    }
    */

    /////////////////////////////////////////////////////////////////////////////

    // ������ ����
    /*
    public void DeleteData()
    {
        DeleteGameData();
    }

    private void DeleteGameData()
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        savedGameClient.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood,
            DeleteSaveGame);
    }


    private void DeleteSaveGame(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        if (status == SavedGameRequestStatus.Success)
        {
            savedGameClient.Delete(data);

        }
        else
        {
        }
    }
    */

    /////////////////////////////////////////////////////////////////////////////


}