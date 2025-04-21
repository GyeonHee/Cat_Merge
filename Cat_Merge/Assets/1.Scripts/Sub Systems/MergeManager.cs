using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// ����� ���� ��ũ��Ʈ
[DefaultExecutionOrder(-1)]
public class MergeManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static MergeManager Instance { get; private set; }

    [Header("---[Merge On/Off System]")]
    [SerializeField] private Button openMergePanelButton;           // ���� �г� ���� ��ư
    [SerializeField] private GameObject mergePanel;                 // ���� On/Off �г�
    [SerializeField] private Button closeMergePanelButton;          // ���� �г� �ݱ� ��ư
    [SerializeField] private Button mergeStateButton;               // ���� ���� ��ư
    private bool isMergeEnabled;                                    // ���� Ȱ��ȭ ����
    private bool previousMergeState;                                // ���� ���� ����

    [Header("---[UI Color]")]
    private const string activeColorCode = "#FFCC74";               // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";             // ��Ȱ��ȭ���� Color


    private bool isDataLoaded = false;                              // ������ �ε� Ȯ��
    
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
        // GoogleManager���� �����͸� �ε����� ���� ��쿡�� �ʱ�ȭ
        if (!isDataLoaded)
        {
            isMergeEnabled = true;
            previousMergeState = isMergeEnabled;
        }

        InitializeButtonListeners();
        UpdateMergeButtonColor();
    }

    #endregion


    #region Button System

    // ��ư ������ �ʱ�ȭ �Լ�
    private void InitializeButtonListeners()
    {
        openMergePanelButton.onClick.AddListener(OpenMergePanel);
        closeMergePanelButton.onClick.AddListener(CloseMergePanel);
        mergeStateButton.onClick.AddListener(ToggleMergeState);
    }

    // ���� �г� ���� �Լ�
    private void OpenMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(true);
        }
    }

    // ���� �г� �ݴ� �Լ�
    public void CloseMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
    }

    // ���� ���� ��� �Լ�
    private void ToggleMergeState()
    {
        isMergeEnabled = !isMergeEnabled;
        UpdateMergeButtonColor();
        CloseMergePanel();

        SaveToLocal();
    }

    #endregion


    #region Battle System

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleMergeState()
    {
        previousMergeState = isMergeEnabled;
        isMergeEnabled = false;

        openMergePanelButton.interactable = false;
        if (mergePanel.activeSelf)
        {
            mergePanel.SetActive(false);
        }

        SaveToLocal();
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleMergeState()
    {
        isMergeEnabled = previousMergeState;
        openMergePanelButton.interactable = true;

        SaveToLocal();
    }

    #endregion


    #region UI System

    // ���� ��ư ���� ������Ʈ �Լ�
    private void UpdateMergeButtonColor()
    {
        if (openMergePanelButton != null)
        {
            string colorCode = !isMergeEnabled ? activeColorCode : inactiveColorCode;
            if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
            {
                openMergePanelButton.GetComponent<Image>().color = color;
            }
        }
    }

    // ���� ���� ��ȯ �Լ�
    public bool IsMergeEnabled()
    {
        return isMergeEnabled;
    }

    #endregion


    #region Merge System

    // ����� Merge �Լ�
    public Cat MergeCats(Cat cat1, Cat cat2)
    {
        if (cat1.CatGrade != cat2.CatGrade)
        {
            //Debug.LogWarning("����� �ٸ�");
            return null;
        }

        Cat nextCat = GetCatByGrade(cat1.CatGrade + 1);
        if (nextCat != null)
        {

            //Debug.Log($"�ռ� ����");
            DictionaryManager.Instance.UnlockCat(nextCat.CatGrade - 1);
            QuestManager.Instance.AddMergeCount();

            // �����Ǵ� ������� ����ġ ���� (2)
            FriendshipManager.Instance.AddExperience(cat1.CatGrade, 2);

            // �����Ǵ� ���� ��� ������� ����ġ ���� (1)
            FriendshipManager.Instance.AddExperience(nextCat.CatGrade, 1);

            return nextCat;
        }
        else
        {
            //Debug.LogWarning("�� ���� ����� ����̰� ����");
            return null;
        }
    }

    // ����� ��� ��ȯ �Լ�
    public Cat GetCatByGrade(int grade)
    {
        GameManager gameManager = GameManager.Instance;
        foreach (Cat cat in gameManager.AllCatData)
        {
            if (cat.CatGrade == grade)
            {
                return cat;
            }
        }
        return null;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool isMergeEnabled;         // ���� Ȱ��ȭ ����
        public bool previousMergeState;     // ���� ����
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            isMergeEnabled = this.isMergeEnabled,
            previousMergeState = this.previousMergeState
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        this.isMergeEnabled = savedData.isMergeEnabled;
        this.previousMergeState = savedData.previousMergeState;

        UpdateMergeButtonColor();

        isDataLoaded = true;
    }

    private void SaveToLocal()
    {
        string data = GetSaveData();
        string key = this.GetType().FullName;
        GoogleManager.Instance?.SaveToPlayerPrefs(key, data);
    }

    #endregion


}
