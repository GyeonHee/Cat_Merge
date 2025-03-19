using UnityEngine;
using UnityEngine.UI;
using System;

// ����� �ڵ��̵� ��ũ��Ʈ
[DefaultExecutionOrder(-1)]
public class AutoMoveManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static AutoMoveManager Instance { get; private set; }

    [Header("---[AutoMove On/Off System]")]
    [SerializeField] private Button openAutoMovePanelButton;        // �ڵ� �̵� �г� ���� ��ư
    [SerializeField] private GameObject autoMovePanel;              // �ڵ� �̵� On/Off �г�
    [SerializeField] private Button closeAutoMovePanelButton;       // �ڵ� �̵� �г� �ݱ� ��ư
    [SerializeField] private Button autoMoveStateButton;            // �ڵ� �̵� ���� ��ư
    private const float autoMoveTime = 10f;                         // �ڵ� �̵� �ð�
    private bool isAutoMoveEnabled;                                 // �ڵ� �̵� Ȱ��ȭ ����
    private bool previousAutoMoveState;                             // ���� ���� ����

    private bool isDataLoaded = false;

    [Header("---[UI Color]")]
    private const string activeColorCode = "#FFCC74";               // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";             // ��Ȱ��ȭ���� Color

    #endregion

    

    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("AutoMoveManager ȣ��");

        // GoogleManager���� �����͸� �ε����� ���� ��쿡�� �ʱ�ȭ
        if (!isDataLoaded)
        {
            isAutoMoveEnabled = true;
            previousAutoMoveState = isAutoMoveEnabled;
        }

        InitializeButtonListeners();
        UpdateAutoMoveButtonColor();
    }

    #endregion

    

    #region Button System

    // ��ư ������ �ʱ�ȭ
    private void InitializeButtonListeners()
    {
        openAutoMovePanelButton.onClick.AddListener(OpenAutoMovePanel);
        closeAutoMovePanelButton.onClick.AddListener(CloseAutoMovePanel);
        autoMoveStateButton.onClick.AddListener(ToggleAutoMoveState);
    }

    // �ڵ��̵� �г� ���� �Լ�
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // �ڵ��̵� �г� �ݴ� �Լ�
    public void CloseAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // �ڵ��̵� ���� ��� �Լ�
    public void ToggleAutoMoveState()
    {
        isAutoMoveEnabled = !isAutoMoveEnabled;
        ApplyAutoMoveStateToAllCats();
        UpdateAutoMoveButtonColor();
        CloseAutoMovePanel();

        GoogleSave();
    }

    #endregion

    

    #region Auto Move System

    // ��� ����̿��� �ڵ��̵� ���� ����
    private void ApplyAutoMoveStateToAllCats()
    {
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned)
            {
                continue;
            }
            cat.SetAutoMoveState(isAutoMoveEnabled);
        }
    }

    // �ڵ��̵� ��ư ���� ������Ʈ �Լ�
    private void UpdateAutoMoveButtonColor()
    {
        if (openAutoMovePanelButton == null)
        {
            return;
        }

        string colorCode = !isAutoMoveEnabled ? activeColorCode : inactiveColorCode;
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            openAutoMovePanelButton.GetComponent<Image>().color = color;
        }
    }

    // ���� �ڵ��̵� ���� ��ȯ �Լ�
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

    // �ڵ��̵� �ð� ��ȯ �Լ�
    public float AutoMoveTime()
    {
        return autoMoveTime;
    }

    #endregion

    

    #region Battle System

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ �Լ�
    public void StartBattleAutoMoveState()
    {
        SaveAndDisableAutoMoveState();
        DisableAutoMoveUI();

        GoogleSave();
    }

    // �ڵ��̵� ���� ���� �� ��Ȱ��ȭ �Լ�
    private void SaveAndDisableAutoMoveState()
    {
        previousAutoMoveState = isAutoMoveEnabled;
        isAutoMoveEnabled = false;
        ApplyAutoMoveStateToAllCats();
    }

    // �ڵ��̵� UI ��Ȱ��ȭ �Լ�
    private void DisableAutoMoveUI()
    {
        openAutoMovePanelButton.interactable = false;
        if (autoMovePanel.activeSelf)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleAutoMoveState()
    {
        RestoreAutoMoveState();
        EnableAutoMoveUI();

        GoogleSave();
    }

    // �ڵ��̵� ���� ���� �Լ�
    private void RestoreAutoMoveState()
    {
        isAutoMoveEnabled = previousAutoMoveState;
        ApplyAutoMoveStateToAllCats();
    }

    // �ڵ��̵� UI Ȱ��ȭ �Լ�
    private void EnableAutoMoveUI()
    {
        openAutoMovePanelButton.interactable = true;
    }

    #endregion

    

    #region Sort System

    // ��� ����� �̵� ���� �Լ�
    public void StopAllCatsMovement()
    {
        StopAllCoroutines();

        // ��� ����� �̵� �ʱ�ȭ
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.StopAllMovement();
        }
    }

    #endregion

    

    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool isAutoMoveEnabled;          // �ڵ� �̵� Ȱ��ȭ ����
        public bool previousAutoMoveState;      // ���� ����
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            isAutoMoveEnabled = this.isAutoMoveEnabled,
            previousAutoMoveState = this.previousAutoMoveState,
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        this.isAutoMoveEnabled = savedData.isAutoMoveEnabled;
        this.previousAutoMoveState = savedData.previousAutoMoveState;

        UpdateAutoMoveButtonColor();
        ApplyAutoMoveStateToAllCats();

        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            Debug.Log("���� ����");
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}
