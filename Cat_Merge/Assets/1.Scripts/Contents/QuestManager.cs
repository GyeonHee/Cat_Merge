using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

// ����Ʈ Script
public class QuestManager : MonoBehaviour
{
    // Singleton Instance
    public static QuestManager Instance { get; private set; }

    public class QuestUI
    {
        public TextMeshProUGUI questName;           // ����Ʈ �̸�
        public Slider questSlider;                  // Slider
        public TextMeshProUGUI countText;           // "?/?" Text
        public TextMeshProUGUI plusCashText;        // ���� ��ȭ ���� Text
        public Button rewardButton;                 // ���� ��ư
        public TextMeshProUGUI rewardText;          // ���� ȹ�� Text
        public GameObject rewardDisabledBG;         // ���� ��ư ��Ȱ��ȭ BG

        public class QuestData
        {
            public int currentCount;                // ���� ��ġ
            public int targetCount;                 // ��ǥ ��ġ
            public int rewardCash;                  // ���� ĳ��
            public bool isComplete;                 // �Ϸ� ����
        }
        public QuestData questData = new QuestData();
    }

    // ======================================================================================================================

    [Header("---[QuestManager]")]
    [SerializeField] private Button questButton;                                // ����Ʈ ��ư
    [SerializeField] private Image questButtonImage;                            // ����Ʈ ��ư �̹���
    [SerializeField] private GameObject questMenuPanel;                         // ����Ʈ �޴� Panel
    [SerializeField] private Button questBackButton;                            // ����Ʈ �ڷΰ��� ��ư
    private ActivePanelManager activePanelManager;                              // ActivePanelManager

    [SerializeField] private GameObject[] mainQuestMenus;                       // ���� ����Ʈ �޴� Panels
    [SerializeField] private Button[] subQuestMenuButtons;                      // ���� ����Ʈ �޴� ��ư �迭

    [SerializeField] private GameObject questSlotPrefab;                        // Quest Slot Prefab
    [SerializeField] private Transform[] questSlotParents;                      // ���Ե��� ��ġ�� �θ� ��ü�� (����, �ְ�, �ݺ�)

    private Dictionary<string, QuestUI> dailyQuestDictionary = new Dictionary<string, QuestUI>();       // Daily Quest Dictionary
    private Dictionary<string, QuestUI> weeklyQuestDictionary = new Dictionary<string, QuestUI>();      // Weekly Quest Dictionary
    private Dictionary<string, QuestUI> repeatQuestDictionary = new Dictionary<string, QuestUI>();      // Repeat Quest Dictionary

    [SerializeField] private ScrollRect repeatQuestScrollRects;                 // �ݺ�����Ʈ�� ��ũ�Ѻ�

    // ======================================================================================================================

    [Header("---[Daily Special Reward UI]")]
    [SerializeField] private Slider dailySpecialRewardSlider;                   // Daily Special Reward Slider
    [SerializeField] private TextMeshProUGUI dailySpecialRewardCountText;       // "?/?" �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI dailySpecialRewardPlusCashText;    // Daily Special Reward ���� ��ȭ ���� Text
    [SerializeField] private Button dailySpecialRewardButton;                   // Daily Special Reward ��ư
    [SerializeField] private TextMeshProUGUI dailySpecialRewardText;            // Daily Special Reward ���� ȹ�� Text
    [SerializeField] private GameObject dailySpecialRewardDisabledBG;           // Daily Special Reward ���� ��ư ��Ȱ��ȭ BG
    private int dailySpecialRewardTargetCount;                                  // Daily ��ǥ Ƚ��
    private int dailySpecialRewardQuestRewardCash = 500;                        // Daily Special Reward ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isDailySpecialRewardQuestComplete;                             // Daily Special Reward ����Ʈ �Ϸ� ���� ����

    [Header("---[Weekly Special Reward UI]")]
    [SerializeField] private Slider weeklySpecialRewardSlider;                  // Weekly Special Reward Slider
    [SerializeField] private TextMeshProUGUI weeklySpecialRewardCountText;      // "?/?" �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI weeklySpecialRewardPlusCashText;   // Weekly Special Reward ���� ��ȭ ���� Text
    [SerializeField] private Button weeklySpecialRewardButton;                  // Weekly Special Reward ��ư
    [SerializeField] private TextMeshProUGUI weeklySpecialRewardText;           // Weekly Special Reward ���� ȹ�� Text
    [SerializeField] private GameObject weeklySpecialRewardDisabledBG;          // Weekly Special Reward ���� ��ư ��Ȱ��ȭ BG
    private int weeklySpecialRewardTargetCount;                                 // Weekly ��ǥ Ƚ��
    private int weeklySpecialRewardQuestRewardCash = 5000;                      // Weekly Special Reward ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isWeeklySpecialRewardQuestComplete;                            // Weekly Special Reward ����Ʈ �Ϸ� ���� ����

    // ======================================================================================================================

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject questButtonNewImage;                    // Main ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject dailyQuestButtonNewImage;               // Daily ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject weeklyQuestButtonNewImage;              // Weekly ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject repeatQuestButtonNewImage;              // Repeat ����Ʈ ��ư�� New Image

    [Header("---[All Reward Button]")]
    [SerializeField] private Button dailyAllRewardButton;                       // Daily All RewardButton
    [SerializeField] private Button weeklyAllRewardButton;                      // Weekly All RewardButton
    [SerializeField] private Button repeatAllRewardButton;                      // Repeat All RewardButton

    [Header("---[Text UI Color]")]
    private string activeColorCode = "#5f5f5f";                                 // Ȱ��ȭ���� Color
    private string inactiveColorCode = "#FFFFFF";                               // ��Ȱ��ȭ���� Color

    // ======================================================================================================================
    // [����Ʈ ���� ����]

    private float playTimeCount;                                        // �÷���Ÿ�� ī��Ʈ
    public float PlayTimeCount { get => playTimeCount; set => playTimeCount = value; }

    private int mergeCount;                                             // ����� ���� Ƚ��
    public int MergeCount { get => mergeCount; set => mergeCount = value; }

    private int spawnCount;                                             // ����� ���� Ƚ��(���� �� Ƚ��)
    public int SpawnCount { get => spawnCount; set => spawnCount = value; }

    private int purchaseCatsCount;                                      // ����� ���� Ƚ��
    public int PurchaseCatsCount { get => purchaseCatsCount; set => purchaseCatsCount = value; }

    private int battleCount;                                            // ���� Ƚ��
    public int BattleCount { get => battleCount; set => battleCount = value; }

    private int dailySpecialRewardCount;                                // Daily ���� ����Ʈ ���� Ƚ��
    public int DailySpecialRewardCount { get => dailySpecialRewardCount; set => dailySpecialRewardCount = value; }

    private int weeklySpecialRewardCount;                               // Weekly ���� ����Ʈ ���� Ƚ��
    public int WeeklySpecialRewardCount { get => weeklySpecialRewardCount; set => weeklySpecialRewardCount = value; }

    // ======================================================================================================================

    // Enum���� �޴� Ÿ�� ���� (���� �޴��� �����ϱ� ���� ���)
    private enum QuestMenuType
    {
        Daily,                  // ���� ����Ʈ �޴�
        Weekly,                 // �ְ� ����Ʈ �޴�
        Repeat,                 // �ݺ� ����Ʈ �޴�
        End                     // Enum�� ��
    }
    private QuestMenuType activeMenuType;                               // ���� Ȱ��ȭ�� �޴� Ÿ��

    // ======================================================================================================================

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
        questMenuPanel.SetActive(false);
        questButtonNewImage.SetActive(false);
        activeMenuType = QuestMenuType.Daily;

        InitializeQuestManager();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("QuestMenu", questMenuPanel, questButtonImage);
    }

    private void Update()
    {
        AddPlayTimeCount();
        UpdateQuestUI();
        UpdateAllDailyRewardButtonState();
        UpdateAllWeeklyRewardButtonState();
    }

    // ======================================================================================================================

    // ��� QuestManager ���� �Լ��� ����
    private void InitializeQuestManager()
    {
        InitializeQuestButton();
        InitializeSubMenuButtons();

        InitializeDailyQuestManager();
        InitializeWeeklyQuestManager();
        InitializeRepeatQuestManager();
    }

    // QuestButton ���� �Լ�
    private void InitializeQuestButton()
    {
        questButton.onClick.AddListener(() => activePanelManager.TogglePanel("QuestMenu"));
        questBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("QuestMenu"));
    }
    
    // Daily Quest ���� �Լ�
    private void InitializeDailyQuestManager()
    {
        InitializeQuest("PlayTime", 10, 5, QuestMenuType.Daily);
        InitializeQuest("Merge Cats", 1, 5, QuestMenuType.Daily);
        InitializeQuest("Spawn Cats", 1, 5, QuestMenuType.Daily);
        InitializeQuest("Purchase Cats", 1, 5, QuestMenuType.Daily);
        InitializeQuest("Battle", 1, 5, QuestMenuType.Daily);

        InitializeDailySpecialReward();

        // Daily AllReward ��ư ���
        dailyAllRewardButton.onClick.AddListener(ReceiveAllDailyRewards);
    }

    // Weekly Quest ���� �Լ�
    private void InitializeWeeklyQuestManager()
    {
        InitializeQuest("PlayTime", 20, 50, QuestMenuType.Weekly);
        InitializeQuest("Merge Cats", 10, 50, QuestMenuType.Weekly);
        InitializeQuest("Spawn Cats", 10, 50, QuestMenuType.Weekly);
        InitializeQuest("Purchase Cats", 10, 50, QuestMenuType.Weekly);

        InitializeWeeklySpecialReward();

        // AllReward ��ư ���
        weeklyAllRewardButton.onClick.AddListener(ReceiveAllWeeklyRewards);
    }

    // Repeat Quest ���� �Լ�
    private void InitializeRepeatQuestManager()
    {
        // �ʱ� ��ũ�� ��ġ �ʱ�ȭ
        repeatQuestScrollRects.verticalNormalizedPosition = 1f;

        //// AllReward ��ư ���
        //repeatAllRewardButton.onClick.AddListener(ReceiveAllRewards);
    }

    // ======================================================================================================================

    // ��� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateQuestUI()
    {
        UpdateNewImageStatus();

        UpdateQuestProgress("PlayTime");
        UpdateQuestProgress("Merge Cats");
        UpdateQuestProgress("Spawn Cats");
        UpdateQuestProgress("Purchase Cats");
        UpdateQuestProgress("Battle");

        UpdateDailySpecialRewardUI();
        UpdateWeeklySpecialRewardUI();


        //UpdateDailyQuestUI();
        //UpdateWeeklyQuestUI();
        //UpdateRepeatQuestUI();
    }

    //// Daily Quest UI ������Ʈ �Լ�
    //private void UpdateDailyQuestUI()
    //{
    //    UpdateDailySpecialRewardUI();
    //}

    //// Weekly Quest UI ������Ʈ �Լ�
    //private void UpdateWeeklyQuestUI()
    //{
    //    UpdateWeeklySpecialRewardUI();
    //}

    //// Repeat Quest UI ������Ʈ �Լ�
    //private void UpdateRepeatQuestUI()
    //{

    //}

    // ĳ�� �߰� �Լ�
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    // ======================================================================================================================
    // [����Ʈ ����]

    // ����Ʈ �ʱ�ȭ
    private void InitializeQuest(string questName, int targetCount, int rewardCash, QuestMenuType menuType)
    {
        // Quest Slot ����
        GameObject newQuestSlot = Instantiate(questSlotPrefab, questSlotParents[(int)menuType]);

        // QuestUI ����
        QuestUI questUI = new QuestUI
        {
            questName = newQuestSlot.transform.Find("Quest Name").GetComponent<TextMeshProUGUI>(),
            questSlider = newQuestSlot.transform.Find("Slider").GetComponent<Slider>(),
            countText = newQuestSlot.transform.Find("Slider/Count Text").GetComponent<TextMeshProUGUI>(),
            plusCashText = newQuestSlot.transform.Find("Plus Cash Text").GetComponent<TextMeshProUGUI>(),
            rewardButton = newQuestSlot.transform.Find("Reward Button").GetComponent<Button>(),
            rewardText = newQuestSlot.transform.Find("Reward Button/Reward Text").GetComponent<TextMeshProUGUI>(),
            rewardDisabledBG = newQuestSlot.transform.Find("Reward Button/DisabledBG").gameObject,
            questData = new QuestUI.QuestData
            {
                currentCount = 0,
                targetCount = targetCount,
                rewardCash = rewardCash,
                isComplete = false
            }
        };

        // UI �ؽ�Ʈ �ʱ�ȭ
        questUI.questName.text = questName;
        questUI.plusCashText.text = $"x {rewardCash}";
        questUI.rewardText.text = "Accept";

        // ���� ��ư ������ ���
        questUI.rewardButton.onClick.AddListener(() => ReceiveQuestReward(questName, menuType));

        // ����Ʈ �����͸� Dictionary�� �߰� (�޴� Ÿ�Կ� ���� �ٸ�)
        if (menuType == QuestMenuType.Daily)
        {
            dailyQuestDictionary[questName] = questUI;
        }
        else if (menuType == QuestMenuType.Weekly)
        {
            weeklyQuestDictionary[questName] = questUI;
        }

        // UI ������Ʈ
        UpdateQuestUI(questName, menuType);
    }

    // ����Ʈ UI ������Ʈ
    private void UpdateQuestUI(string questName, QuestMenuType menuType)
    {
        // �޴� Ÿ�Կ� ���� �ش� ��ųʸ� ����
        QuestUI questUI = null;
        if (menuType == QuestMenuType.Daily && dailyQuestDictionary.ContainsKey(questName))
        {
            questUI = dailyQuestDictionary[questName];
        }
        else if (menuType == QuestMenuType.Weekly && weeklyQuestDictionary.ContainsKey(questName))
        {
            questUI = weeklyQuestDictionary[questName];
        }

        // ����Ʈ UI�� ���ٸ� ����
        if (questUI == null) return;

        QuestUI.QuestData questData = questUI.questData;

        // Slider �� ����
        questUI.questSlider.maxValue = questData.targetCount;
        questUI.questSlider.value = questData.currentCount;

        // �ؽ�Ʈ ������Ʈ
        questUI.countText.text = $"{questData.currentCount} / {questData.targetCount}";

        // �Ϸ� ���� Ȯ��
        bool isComplete = questData.currentCount >= questData.targetCount && !questData.isComplete;
        questUI.rewardButton.interactable = isComplete;
        questUI.rewardDisabledBG.SetActive(!isComplete);

        if (questData.isComplete)
        {
            questUI.rewardText.text = "Complete";
        }
    }

    // ����Ʈ ���� ������Ʈ
    public void UpdateQuestProgress(string questName)
    {
        // ���� ����Ʈ ������Ʈ
        if (dailyQuestDictionary.ContainsKey(questName))
        {
            QuestUI questUI = dailyQuestDictionary[questName];
            QuestUI.QuestData questData = questUI.questData;

            questData.currentCount = Mathf.Min(questData.currentCount, questData.targetCount);

            UpdateQuestUI(questName, QuestMenuType.Daily);
        }
        // �ְ� ����Ʈ ������Ʈ
        if (weeklyQuestDictionary.ContainsKey(questName))
        {
            QuestUI questUI = weeklyQuestDictionary[questName];
            QuestUI.QuestData questData = questUI.questData;

            questData.currentCount = Mathf.Min(questData.currentCount, questData.targetCount);

            UpdateQuestUI(questName, QuestMenuType.Weekly);
        }
    }

    // ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveQuestReward(string questName, QuestMenuType menuType)
    {
        // �ش� �޴� Ÿ�Կ� �´� ����Ʈ ��ųʸ� ����
        QuestUI questUI = null;
        if (menuType == QuestMenuType.Daily && dailyQuestDictionary.ContainsKey(questName))
        {
            questUI = dailyQuestDictionary[questName];
        }
        else if (menuType == QuestMenuType.Weekly && weeklyQuestDictionary.ContainsKey(questName))
        {
            questUI = weeklyQuestDictionary[questName];
        }

        // ����Ʈ UI�� ���ٸ� ����
        if (questUI == null) return;

        QuestUI.QuestData questData = questUI.questData;

        // ���� ��ư�� ��Ȱ��ȭ�Ǿ� �ְų� ����Ʈ�� �̹� �Ϸ�Ǿ����� ����
        if (!questUI.rewardButton.interactable || questData.isComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref questData.isComplete, questData.rewardCash, questUI.rewardButton, questUI.rewardDisabledBG, menuType);

        // UI ������Ʈ
        UpdateQuestUI(questName, menuType);
    }

    // ======================================================================================================================
    // [PlayTime Quest]

    // �÷���Ÿ�� ���� �Լ�
    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
        dailyQuestDictionary["PlayTime"].questData.currentCount = (int)PlayTimeCount;
        weeklyQuestDictionary["PlayTime"].questData.currentCount = (int)PlayTimeCount;
    }

    // �÷���Ÿ�� ���� �Լ�
    public void ResetPlayTimeCount()
    {
        PlayTimeCount = 0;
    }

    // ======================================================================================================================
    // [Merge Quest]

    // ����� ���� ���� �Լ�
    public void AddMergeCount()
    {
        MergeCount++;
        dailyQuestDictionary["Merge Cats"].questData.currentCount = MergeCount;
        weeklyQuestDictionary["Merge Cats"].questData.currentCount = MergeCount;
    }

    // ����� ���� ���� �Լ�
    public void ResetMergeCount()
    {
        MergeCount = 0;
    }

    // ======================================================================================================================
    // [Spawn Quest]

    // ����� ���� ���� �Լ�
    public void AddSpawnCount()
    {
        SpawnCount++;
        dailyQuestDictionary["Spawn Cats"].questData.currentCount = SpawnCount;
        weeklyQuestDictionary["Spawn Cats"].questData.currentCount = SpawnCount;
    }

    // ����� ���� ���� �Լ�
    public void ResetSpawnCount()
    {
        SpawnCount = 0;
    }

    // ======================================================================================================================
    // [Purchase Cats Quest]

    // ����� ���� ���� �Լ�
    public void AddPurchaseCatsCount()
    {
        PurchaseCatsCount++;
        dailyQuestDictionary["Purchase Cats"].questData.currentCount = PurchaseCatsCount;
        weeklyQuestDictionary["Purchase Cats"].questData.currentCount = PurchaseCatsCount;
    }

    // ����� ���� ���� �Լ�
    public void ResetPurchaseCatsCount()
    {
        PurchaseCatsCount = 0;
    }

    // ======================================================================================================================
    // [Battle Count Quest]

    // ��Ʋ ���� �Լ�
    public void AddBattleCount()
    {
        BattleCount++;
        dailyQuestDictionary["Battle"].questData.currentCount = BattleCount;
    }

    // ��Ʋ ���� �Լ�
    public void ResetBattleCount()
    {
        BattleCount = 0;
    }

    // ======================================================================================================================
    // [Special Reward Quest - Daily]

    // Daily Special Reward Quest �ʱ� ���� �Լ�
    private void InitializeDailySpecialReward()
    {
        dailySpecialRewardTargetCount = dailyQuestDictionary.Count;

        dailySpecialRewardButton.onClick.AddListener(ReceiveDailySpecialReward);
        isDailySpecialRewardQuestComplete = false;
        dailySpecialRewardButton.interactable = false;
        dailySpecialRewardDisabledBG.SetActive(true);
        dailySpecialRewardPlusCashText.text = $"x {dailySpecialRewardQuestRewardCash}";
        dailySpecialRewardText.text = "Accept";
    }

    // Daily Special Reward ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateDailySpecialRewardUI()
    {
        int currentCount = Mathf.Min((int)DailySpecialRewardCount, dailySpecialRewardTargetCount);

        // Slider �� ����
        dailySpecialRewardSlider.maxValue = dailySpecialRewardTargetCount;
        dailySpecialRewardSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        dailySpecialRewardCountText.text = $"{currentCount} / {dailySpecialRewardTargetCount}";
        if (isDailySpecialRewardQuestComplete)
        {
            dailySpecialRewardText.text = "Complete";
        }

        bool isComplete = AllDailyQuestsCompleted() && !isDailySpecialRewardQuestComplete;
        dailySpecialRewardButton.interactable = isComplete;
        dailySpecialRewardDisabledBG.SetActive(!isComplete);
    }

    // Daily Special Reward ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveDailySpecialReward()
    {
        if (!dailySpecialRewardButton.interactable || isDailySpecialRewardQuestComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        AddCash(dailySpecialRewardQuestRewardCash);
        isDailySpecialRewardQuestComplete = true;
    }

    // ��� Daily ����Ʈ�� �Ϸ�Ǿ����� Ȯ���ϴ� �Լ�
    private bool AllDailyQuestsCompleted()
    {
        foreach (var quest in dailyQuestDictionary)
        {
            if (!quest.Value.questData.isComplete)
            {
                return false;
            }
        }
        return true;
    }

    // Daily Special Reward ���� �Լ�
    public void AddDailySpecialRewardCount()
    {
        DailySpecialRewardCount++;
    }

    // Daily Special Reward ���� �Լ�
    public void ResetDailySpecialRewardCount()
    {
        DailySpecialRewardCount = 0;
    }



    // [Special Reward Quest - Weekly]

    // Weekly Special Reward Quest �ʱ� ���� �Լ�
    private void InitializeWeeklySpecialReward()
    {
        weeklySpecialRewardTargetCount = weeklyQuestDictionary.Count;

        weeklySpecialRewardButton.onClick.AddListener(ReceiveWeeklySpecialReward);
        isWeeklySpecialRewardQuestComplete = false;
        weeklySpecialRewardButton.interactable = false;
        weeklySpecialRewardDisabledBG.SetActive(true);
        weeklySpecialRewardPlusCashText.text = $"x {weeklySpecialRewardQuestRewardCash}";
        weeklySpecialRewardText.text = "Accept";
    }

    // Weekly Special Reward ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateWeeklySpecialRewardUI()
    {
        int currentCount = Mathf.Min((int)WeeklySpecialRewardCount, weeklySpecialRewardTargetCount);

        // Slider �� ����
        weeklySpecialRewardSlider.maxValue = weeklySpecialRewardTargetCount;
        weeklySpecialRewardSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        weeklySpecialRewardCountText.text = $"{currentCount} / {weeklySpecialRewardTargetCount}";
        if (isWeeklySpecialRewardQuestComplete)
        {
            weeklySpecialRewardText.text = "Complete";
        }

        bool isComplete = AllWeeklyQuestsCompleted() && !isWeeklySpecialRewardQuestComplete;
        weeklySpecialRewardButton.interactable = isComplete;
        weeklySpecialRewardDisabledBG.SetActive(!isComplete);
    }

    // Weekly Special Reward ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveWeeklySpecialReward()
    {
        if (!weeklySpecialRewardButton.interactable || isWeeklySpecialRewardQuestComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        AddCash(weeklySpecialRewardQuestRewardCash);
        isWeeklySpecialRewardQuestComplete = true;
    }

    // ��� Weekly ����Ʈ�� �Ϸ�Ǿ����� Ȯ���ϴ� �Լ�
    private bool AllWeeklyQuestsCompleted()
    {
        foreach (var quest in weeklyQuestDictionary)
        {
            if (!quest.Value.questData.isComplete)
            {
                return false;
            }
        }
        return true;
    }

    // Weekly Special Reward ���� �Լ�
    public void AddWeeklySpecialRewardCount()
    {
        WeeklySpecialRewardCount++;
    }

    // Weekly Special Reward ���� �Լ�
    public void ResetWeeklySpecialRewardCount()
    {
        WeeklySpecialRewardCount = 0;
    }

    // ======================================================================================================================
    // [��ü ���� �ޱ� ����]

    // ��� Ȱ��ȭ�� ������ �����ϴ� �Լ� - Daily
    private void ReceiveAllDailyRewards()
    {
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref dailyQuest.Value.questData.isComplete, dailyQuest.Value.questData.rewardCash,
                    dailyQuest.Value.rewardButton, dailyQuest.Value.rewardDisabledBG, QuestMenuType.Daily);
            }
        }

        // ����� ���� ����
        if (dailySpecialRewardButton.interactable && !isDailySpecialRewardQuestComplete)
        {
            ReceiveDailySpecialReward();
        }
    }
    
    // All Reward ��ư ���¸� ������Ʈ�ϴ� �Լ� - Daily
    private void UpdateAllDailyRewardButtonState()
    {
        // ������ ���� �� �ִ� ��ư�� �ϳ��� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
        bool isAnyRewardAvailable = false;
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
            {
                isAnyRewardAvailable = true;
                break;
            }
        }

        // ����� ���� Ȯ��
        if (dailySpecialRewardButton.interactable && !isDailySpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
        }

        dailyAllRewardButton.interactable = isAnyRewardAvailable;
    }

    // ��� Ȱ��ȭ�� ������ �����ϴ� �Լ� - Weekly
    private void ReceiveAllWeeklyRewards()
    {
        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable && !weeklyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref weeklyQuest.Value.questData.isComplete, weeklyQuest.Value.questData.rewardCash,
                    weeklyQuest.Value.rewardButton, weeklyQuest.Value.rewardDisabledBG, QuestMenuType.Weekly);
            }
        }

        // ����� ���� ����
        if (weeklySpecialRewardButton.interactable && !isWeeklySpecialRewardQuestComplete)
        {
            ReceiveWeeklySpecialReward();
        }
    }

    // All Reward ��ư ���¸� ������Ʈ�ϴ� �Լ� - Weekly
    private void UpdateAllWeeklyRewardButtonState()
    {
        // ������ ���� �� �ִ� ��ư�� �ϳ��� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
        bool isAnyRewardAvailable = false;
        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable && !weeklyQuest.Value.questData.isComplete)
            {
                isAnyRewardAvailable = true;
                break;
            }
        }

        // ����� ���� Ȯ��
        if (weeklySpecialRewardButton.interactable && !isWeeklySpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
        }

        weeklyAllRewardButton.interactable = isAnyRewardAvailable;
    }

    // ���� ����Ʈ ���� ���� ó�� �Լ�
    private void ReceiveQuestReward(ref bool isQuestComplete, int rewardCash, Button rewardButton, GameObject disabledBG, QuestMenuType menuType)
    {
        isQuestComplete = true;
        AddCash(rewardCash);
        rewardButton.interactable = false;
        disabledBG.SetActive(true);

        // QuestType�� �޾ƿͼ� Daily�� Weekly�� �Ǻ��ؾ���
        if (menuType == QuestMenuType.Daily)
        {
            AddDailySpecialRewardCount();
        }
        if (menuType == QuestMenuType.Weekly)
        {
            AddWeeklySpecialRewardCount();
        }
    }

    // ======================================================================================================================
    // [���� �޴�]

    // ���� �޴� ��ư �ʱ�ȭ �� Ŭ�� �̺�Ʈ �߰� �Լ�
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)QuestMenuType.End; i++)
        {
            int index = i;
            subQuestMenuButtons[index].onClick.AddListener(() => ActivateMenu((QuestMenuType)index));
        }

        ActivateMenu(QuestMenuType.Daily);
    }

    // ������ ���� �޴��� Ȱ��ȭ�ϴ� �Լ�
    private void ActivateMenu(QuestMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < mainQuestMenus.Length; i++)
        {
            mainQuestMenus[i].SetActive(i == (int)menuType);
        }

        UpdateSubMenuButtonColors();
    }

    // ���� �޴� ��ư ������ ������Ʈ�ϴ� �Լ�
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < subQuestMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(subQuestMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
        }
    }

    // ���� �޴� ��ư ������ Ȱ�� ���¿� ���� ������Ʈ�ϴ� �Լ�
    private void UpdateSubButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? activeColorCode : inactiveColorCode;
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    // ======================================================================================================================

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ� (Daily)
    public bool HasUnclaimedDailyRewards()
    {
        bool hasActiveQuestReward = false;
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable)
            {
                hasActiveQuestReward = true;
                break;
            }
        }

        hasActiveQuestReward = hasActiveQuestReward || dailySpecialRewardButton.interactable;

        return hasActiveQuestReward;
    }

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ� (Weekly)
    public bool HasUnclaimedWeeklyRewards()
    {
        bool hasActiveQuestReward = false;
        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable)
            {
                hasActiveQuestReward = true;
                break;
            }
        }

        hasActiveQuestReward = hasActiveQuestReward || weeklySpecialRewardButton.interactable;

        return hasActiveQuestReward;
    }

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ� (Repeat)
    public bool HasUnclaimedRepeatRewards()
    {
        bool hasActiveQuestReward = false;
        foreach (var repeatQuest in repeatQuestDictionary)
        {
            if (repeatQuest.Value.rewardButton.interactable)
            {
                hasActiveQuestReward = true;
                break;
            }
        }

        return hasActiveQuestReward;
    }

    // New Image�� ���¸� Update�ϴ� �Լ�
    private void UpdateNewImageStatus()
    {
        bool hasUnclaimedDailyRewards = HasUnclaimedDailyRewards();
        bool hasUnclaimedWeeklyRewards = HasUnclaimedWeeklyRewards();
        bool hasUnclaimedRepeatRewards = HasUnclaimedRepeatRewards();

        bool hasUnclaimedRewards = hasUnclaimedDailyRewards || hasUnclaimedWeeklyRewards || hasUnclaimedRepeatRewards;

        // Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        questButtonNewImage.SetActive(hasUnclaimedRewards);

        // Daily Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        dailyQuestButtonNewImage.SetActive(hasUnclaimedDailyRewards);

        // Weekly Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        weeklyQuestButtonNewImage.SetActive(hasUnclaimedWeeklyRewards);

        // Repeat Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        repeatQuestButtonNewImage.SetActive(hasUnclaimedRepeatRewards);
    }


}
