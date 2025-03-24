using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;

// Quest Script
public class QuestManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static QuestManager Instance { get; private set; }

    // QuestUI Class
    public class QuestUI
    {
        public TextMeshProUGUI questName;           // ����Ʈ �̸�
        public Slider questSlider;                  // Slider
        public TextMeshProUGUI countText;           // "?/?" Text
        public TextMeshProUGUI plusCashText;        // ���� ��ȭ ���� Text
        public Button rewardButton;                 // ���� ��ư
        public TextMeshProUGUI rewardText;          // ���� ȹ�� Text
        public GameObject rewardDisabledBG;         // ���� ��ư ��Ȱ��ȭ BG
        public Transform slotTransform;             // �ش� ������ Transform ����
        public Image questImage;                    // ����Ʈ �̹���

        public class QuestData
        {
            public int currentCount;                // ���� ��ġ
            public int targetCount;                 // ��ǥ ��ġ
            public int plusTargetCount;             // ��ǥ ��ġ ���� ��ġ
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
    [SerializeField] private GameObject questSlotPrefab;                        // Quest Slot Prefab

    [SerializeField] private GameObject[] mainQuestMenus;                       // ���� ����Ʈ �޴� Panels
    [SerializeField] private Button[] subQuestMenuButtons;                      // ���� ����Ʈ �޴� ��ư �迭
    [SerializeField] private Transform[] questSlotParents;                      // ���Ե��� ��ġ�� �θ� ��ü�� (����, �ְ�, �ݺ�)


    private Dictionary<string, QuestUI> dailyQuestDictionary = new Dictionary<string, QuestUI>();       // ��������Ʈ Dictionary
    private Dictionary<string, QuestUI> weeklyQuestDictionary = new Dictionary<string, QuestUI>();      // �ְ�����Ʈ Dictionary

    private Dictionary<string, QuestUI> repeatQuestDictionary = new Dictionary<string, QuestUI>();      // �ݺ�����Ʈ Dictionary
    private List<QuestUI> sortedRepeatQuestList = new List<QuestUI>();                                  // ���ĵ� �ݺ�����Ʈ ������ ���� List
    private Dictionary<Transform, Coroutine> activeAnimations = new Dictionary<Transform, Coroutine>(); // �ݺ�����Ʈ ���� �ڷ�ƾ Dictionary

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
    [SerializeField] private GameObject mainQuestButtonNewImage;                // Main ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject[] subQuestButtonNewImages;              // Sub ����Ʈ ��ư���� New Image

    [Header("---[All Reward Button]")]
    [SerializeField] private Button[] allRewardButtons;                         // All RewardButtons

    [Header("---[Text UI Color]")]
    private string activeColorCode = "#FFCC74";                                 // Ȱ��ȭ���� Color
    private string inactiveColorCode = "#FFFFFF";                               // ��Ȱ��ȭ���� Color

    private string resourcePath = "Sprites/UI/I_UI_Quest/";                     // ����Ʈ���� �̹��� �⺻ ���

    // ======================================================================================================================

    // Enum���� �޴� Ÿ�� ���� (���� �޴��� �����ϱ� ���� ���)
    private enum QuestMenuType
    {
        Daily,                              // ���� ����Ʈ �޴�
        Weekly,                             // �ְ� ����Ʈ �޴�
        Repeat,                             // �ݺ� ����Ʈ �޴�
        End                                 // Enum�� ��
    }
    private QuestMenuType activeMenuType;   // ���� Ȱ��ȭ�� �޴� Ÿ��

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

    private int stageCount;                                             // �������� �ܰ�
    public int StageCount { get => BattleManager.Instance.BossStage; }


    private int dailySpecialRewardCount;                                // Daily ���� ����Ʈ ���� Ƚ��
    public int DailySpecialRewardCount { get => dailySpecialRewardCount; set => dailySpecialRewardCount = value; }

    private int weeklySpecialRewardCount;                               // Weekly ���� ����Ʈ ���� Ƚ��
    public int WeeklySpecialRewardCount { get => weeklySpecialRewardCount; set => weeklySpecialRewardCount = value; }


    private DateTime lastDailyReset;
    private DateTime lastWeeklyReset;

    private bool isDataLoaded = false;          // ������ �ε� Ȯ��

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
            lastDailyReset = DateTime.Now;
            lastWeeklyReset = DateTime.Now;
        }

        InitializeQuestManager();
        CheckAndResetQuests();
    }

    private void Update()
    {
        AddPlayTimeCount();

        if (Time.frameCount % 60 == 0)
        {
            CheckAndResetQuests();
        }
    }

    #endregion


    #region Initialize

    // ��� QuestManager ���� �Լ��� ����
    private void InitializeQuestManager()
    {
        questMenuPanel.SetActive(false);
        mainQuestButtonNewImage.SetActive(false);
        activeMenuType = QuestMenuType.Daily;

        InitializeActivePanel();
        InitializeSubMenuButtons();

        InitializeQuests();
    }

    // ��� Quest �ʱ�ȭ
    private void InitializeQuests()
    {
        InitializeDailyQuestManager();
        InitializeWeeklyQuestManager();
        InitializeRepeatQuestManager();
    }

    // ActivePanel �ʱ�ȭ �Լ�
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("QuestMenu", questMenuPanel, questButtonImage);

        questButton.onClick.AddListener(() =>
        {
            activePanelManager.TogglePanel("QuestMenu");
            InitializeRepeatQuestUIFromSortedList();
        });
        questBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("QuestMenu"));
    }

    // Daily Quest ���� �Լ�
    private void InitializeDailyQuestManager()
    {
        InitializeQuest("�÷��� �ð�", 10, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ���� Ƚ��", 1, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ��ȯ Ƚ��", 1, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ���� Ƚ��", 1, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("���� Ƚ��", 1, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");

        InitializeDailySpecialReward();

        allRewardButtons[(int)QuestMenuType.Daily].onClick.AddListener(ReceiveAllDailyRewards);
        UpdateAllDailyRewardButtonState();
    }

    // Weekly Quest ���� �Լ�
    private void InitializeWeeklyQuestManager()
    {
        InitializeQuest("�÷��� �ð�", 20, 50, QuestMenuType.Weekly, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ���� Ƚ��", 10, 50, QuestMenuType.Weekly, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ��ȯ Ƚ��", 10, 50, QuestMenuType.Weekly, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ���� Ƚ��", 10, 50, QuestMenuType.Weekly, "I_UI_Mission_Daily.9");

        InitializeWeeklySpecialReward();

        allRewardButtons[(int)QuestMenuType.Weekly].onClick.AddListener(ReceiveAllWeeklyRewards);
        UpdateAllWeeklyRewardButtonState();
    }

    // Repeat Quest ���� �Լ�
    private void InitializeRepeatQuestManager()
    {
        InitializeQuest("����� ���� Ƚ��", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ��ȯ Ƚ��", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ���� Ƚ��", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("���� ��������", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("����1", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("����2", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("����3", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("����4", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("����5", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("����6", 1, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");

        // �ʱ� ��ũ�� ��ġ �ʱ�ȭ
        InitializeScrollPosition();

        allRewardButtons[(int)QuestMenuType.Repeat].onClick.AddListener(ReceiveAllRepeatRewards);
        UpdateAllRepeatRewardButtonState();
    }

    // ���� ��ũ�� �г� �ʱ� ��ġ�� �����ϴ� �Լ�
    private void InitializeScrollPosition()
    {
        ScrollRect scrollRect = mainQuestMenus[(int)QuestMenuType.Repeat].GetComponentInChildren<ScrollRect>();
        RectTransform content = scrollRect.content;
        GridLayoutGroup gridLayout = content.GetComponent<GridLayoutGroup>();

        // ���� ������ ������ ũ�� ���
        int slotCount = content.childCount;
        float cellHeight = gridLayout.cellSize.y;
        float spacingHeight = gridLayout.spacing.y;

        // ������ ��ü ���� ��� �� ����
        float contentHeight = (cellHeight + spacingHeight) * slotCount - spacingHeight;

        // ������ ũ��� ScrollRect ũ�⸦ ��
        if (contentHeight > scrollRect.GetComponent<RectTransform>().rect.height)
        {
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            scrollRect.verticalNormalizedPosition = 1f;
        }
        else
        {
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollRect.GetComponent<RectTransform>().rect.height);
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    #endregion


    #region Sub Menus

    // ���� �޴� ��ư �ʱ�ȭ �� Ŭ�� �̺�Ʈ �߰� �Լ�
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)QuestMenuType.End; i++)
        {
            int index = i;
            subQuestMenuButtons[index].onClick.AddListener(() =>
            {
                ActivateMenu((QuestMenuType)index);

                if (index == (int)QuestMenuType.Repeat)
                {
                    InitializeRepeatQuestUIFromSortedList();
                }
            });
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

    #endregion


    #region New Image

    // New Image�� ���¸� Update�ϴ� �Լ�
    private void UpdateNewImageStatus()
    {
        bool hasUnclaimedDailyRewards = HasUnclaimedDailyRewards();
        bool hasUnclaimedWeeklyRewards = HasUnclaimedWeeklyRewards();
        bool hasUnclaimedRepeatRewards = HasUnclaimedRepeatRewards();
        bool hasUnclaimedRewards = hasUnclaimedDailyRewards || hasUnclaimedWeeklyRewards || hasUnclaimedRepeatRewards;

        // Quest Button / Daily / Weekly / Repeat
        mainQuestButtonNewImage.SetActive(hasUnclaimedRewards);
        subQuestButtonNewImages[(int)QuestMenuType.Daily].SetActive(hasUnclaimedDailyRewards);
        subQuestButtonNewImages[(int)QuestMenuType.Weekly].SetActive(hasUnclaimedWeeklyRewards);
        subQuestButtonNewImages[(int)QuestMenuType.Repeat].SetActive(hasUnclaimedRepeatRewards);
    }

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ� (Daily)
    public bool HasUnclaimedDailyRewards()
    {
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable)
            {
                return true;
            }
        }
        return dailySpecialRewardButton.interactable;
    }

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ� (Weekly)
    public bool HasUnclaimedWeeklyRewards()
    {
        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable)
            {
                return true;
            }
        }
        return weeklySpecialRewardButton.interactable;
    }

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ� (Repeat)
    public bool HasUnclaimedRepeatRewards()
    {
        foreach (var repeatQuest in repeatQuestDictionary)
        {
            if (repeatQuest.Value.rewardButton.interactable)
            {
                return true;
            }
        }
        return false;
    }

    #endregion


    #region Initialize and Update Quests

    // ����Ʈ �ʱ�ȭ
    private void InitializeQuest(string questName, int targetCount, int rewardCash, QuestMenuType menuType, string imageName)
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
            slotTransform = newQuestSlot.transform,
            questImage = newQuestSlot.transform.Find("Icon").GetComponent<Image>(),

            questData = new QuestUI.QuestData
            {
                currentCount = 0,
                targetCount = targetCount,
                plusTargetCount = targetCount,
                rewardCash = rewardCash,
                isComplete = false
            }
        };

        // UI �ؽ�Ʈ �ʱ�ȭ
        questUI.questName.text = questName;
        questUI.plusCashText.text = $"x {rewardCash}";
        questUI.rewardText.text = "�ޱ�";

        // �̹��� ����
        questUI.questImage.sprite = Resources.Load<Sprite>($"{resourcePath}{imageName}");

        // ���� ��ư ������ ���
        questUI.rewardButton.onClick.AddListener(() => ReceiveQuestReward(questName, menuType));

        // ����Ʈ �����͸� Dictionary�� �߰� (�޴� Ÿ�Կ� ���� �ٸ�)
        switch (menuType)
        {
            case QuestMenuType.Daily:
                dailyQuestDictionary[questName] = questUI;
                break;
            case QuestMenuType.Weekly:
                weeklyQuestDictionary[questName] = questUI;
                break;
            case QuestMenuType.Repeat:
                repeatQuestDictionary[questName] = questUI;
                break;
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
        else if (menuType == QuestMenuType.Repeat && repeatQuestDictionary.ContainsKey(questName))
        {
            questUI = repeatQuestDictionary[questName];
        }

        // ����Ʈ UI�� ���ٸ� ����
        if (questUI == null) return;

        QuestUI.QuestData questData = questUI.questData;

        // Slider �� ����
        questUI.questSlider.maxValue = questData.targetCount;
        questUI.questSlider.value = Mathf.Min(questData.currentCount, questData.targetCount);

        // �ؽ�Ʈ ������Ʈ (����/�ְ� ����Ʈ�� targetCount�� ���� �ʵ���)
        int displayCount = questData.currentCount;
        if (menuType == QuestMenuType.Daily || menuType == QuestMenuType.Weekly)
        {
            displayCount = Mathf.Min(questData.currentCount, questData.targetCount);
        }
        questUI.countText.text = $"{displayCount} / {questData.targetCount}";

        // �Ϸ� ���� Ȯ��
        bool isComplete = questData.currentCount >= questData.targetCount && !questData.isComplete;
        questUI.rewardButton.interactable = isComplete;
        questUI.rewardDisabledBG.SetActive(!isComplete);

        if (questData.isComplete)
        {
            questUI.rewardText.text = "���� �Ϸ�";
        }

        UpdateNewImageStatus();
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
            UpdateAllDailyRewardButtonState();
        }
        // �ְ� ����Ʈ ������Ʈ
        if (weeklyQuestDictionary.ContainsKey(questName))
        {
            QuestUI questUI = weeklyQuestDictionary[questName];
            QuestUI.QuestData questData = questUI.questData;

            questData.currentCount = Mathf.Min(questData.currentCount, questData.targetCount);

            UpdateQuestUI(questName, QuestMenuType.Weekly);
            UpdateAllWeeklyRewardButtonState();
        }
        // �ݺ� ����Ʈ ������Ʈ
        if (repeatQuestDictionary.ContainsKey(questName))
        {
            UpdateQuestUI(questName, QuestMenuType.Repeat);
            UpdateAllRepeatRewardButtonState();
            SortRepeatQuests();
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
        else if (menuType == QuestMenuType.Repeat && repeatQuestDictionary.ContainsKey(questName))
        {
            questUI = repeatQuestDictionary[questName];
        }

        // ����Ʈ UI�� ���ٸ� ����
        if (questUI == null) return;

        QuestUI.QuestData questData = questUI.questData;

        // ���� ��ư�� ��Ȱ��ȭ�Ǿ� �ְų� ����Ʈ�� �̹� �Ϸ�Ǿ����� ����
        if (!questUI.rewardButton.interactable || questData.isComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        if (menuType == QuestMenuType.Repeat)
        {
            ReceiveRepeatQuestReward(questName, questData.rewardCash);
        }
        else
        {
            ReceiveQuestReward(ref questData.isComplete, questData.rewardCash, questUI.rewardButton, questUI.rewardDisabledBG, menuType, questName);
        }
    }

    #endregion


    #region Quests

    #region PlayTime Quest

    // �÷���Ÿ�� ���� �Լ�
    public void AddPlayTimeCount()
    {
        //isUpdatingFromPlayTime = true;
        PlayTimeCount += Time.deltaTime;

        dailyQuestDictionary["�÷��� �ð�"].questData.currentCount = (int)PlayTimeCount;
        weeklyQuestDictionary["�÷��� �ð�"].questData.currentCount = (int)PlayTimeCount;

        UpdateQuestProgress("�÷��� �ð�");
        //isUpdatingFromPlayTime = false;
    }

    // �÷���Ÿ�� ���� �Լ�
    public void ResetPlayTimeCount()
    {
        PlayTimeCount = 0;
    }

    #endregion


    #region Merge Quest

    // ����� ���� ���� �Լ�
    public void AddMergeCount()
    {
        MergeCount++;

        dailyQuestDictionary["����� ���� Ƚ��"].questData.currentCount = MergeCount;
        weeklyQuestDictionary["����� ���� Ƚ��"].questData.currentCount = MergeCount;
        repeatQuestDictionary["����� ���� Ƚ��"].questData.currentCount = MergeCount;

        UpdateQuestProgress("����� ���� Ƚ��");
    }

    // ����� ���� ���� �Լ�
    public void ResetMergeCount()
    {
        MergeCount = 0;
    }

    #endregion


    #region Spawn Quest

    // ����� ���� ���� �Լ�
    public void AddSpawnCount()
    {
        SpawnCount++;

        dailyQuestDictionary["����� ��ȯ Ƚ��"].questData.currentCount = SpawnCount;
        weeklyQuestDictionary["����� ��ȯ Ƚ��"].questData.currentCount = SpawnCount;
        repeatQuestDictionary["����� ��ȯ Ƚ��"].questData.currentCount = SpawnCount;

        UpdateQuestProgress("����� ��ȯ Ƚ��");
    }

    // ����� ���� ���� �Լ�
    public void ResetSpawnCount()
    {
        SpawnCount = 0;
    }

    #endregion


    #region Purchase Cats Quest

    // ����� ���� ���� �Լ�
    public void AddPurchaseCatsCount()
    {
        PurchaseCatsCount++;

        dailyQuestDictionary["����� ���� Ƚ��"].questData.currentCount = PurchaseCatsCount;
        weeklyQuestDictionary["����� ���� Ƚ��"].questData.currentCount = PurchaseCatsCount;
        repeatQuestDictionary["����� ���� Ƚ��"].questData.currentCount = PurchaseCatsCount;

        UpdateQuestProgress("����� ���� Ƚ��");
    }

    // ����� ���� ���� �Լ�
    public void ResetPurchaseCatsCount()
    {
        PurchaseCatsCount = 0;
    }

    #endregion


    #region Battle Count Quest

    // ��Ʋ ���� �Լ�
    public void AddBattleCount()
    {
        BattleCount++;

        dailyQuestDictionary["���� Ƚ��"].questData.currentCount = BattleCount;

        UpdateQuestProgress("���� Ƚ��");
    }

    // ��Ʋ ���� �Լ�
    public void ResetBattleCount()
    {
        BattleCount = 0;
    }

    #endregion


    #region Stage Count Quest

    // �������� ���� �Լ�
    public void AddStageCount()
    {
        repeatQuestDictionary["���� ��������"].questData.currentCount = StageCount - 1;

        UpdateQuestProgress("���� ��������");
    }

    #endregion

    #endregion


    #region Special Reward Quest - Daily, Weekly

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
        dailySpecialRewardText.text = "�ޱ�";

        UpdateDailySpecialRewardUI();
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
            dailySpecialRewardText.text = "���� �Ϸ�";
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

        UpdateDailySpecialRewardUI();
    }

    // ��� Daily ����Ʈ�� �Ϸ�Ǿ����� Ȯ���ϴ� �Լ�
    private bool AllDailyQuestsCompleted()
    {
        return dailyQuestDictionary.Values.All(quest => quest.questData.isComplete);
    }

    // Daily Special Reward ���� �Լ�
    public void AddDailySpecialRewardCount()
    {
        DailySpecialRewardCount++;
    }

    // Daily Special Reward ���� �Լ�           - ���߿� ������ �ð��� �ʱ�ȭ�ǰ� �ϱ� ����
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
        weeklySpecialRewardText.text = "�ޱ�";
        UpdateWeeklySpecialRewardUI();
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
            weeklySpecialRewardText.text = "���� �Ϸ�";
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

        UpdateWeeklySpecialRewardUI();
    }

    // ��� Weekly ����Ʈ�� �Ϸ�Ǿ����� Ȯ���ϴ� �Լ�
    private bool AllWeeklyQuestsCompleted()
    {
        return weeklyQuestDictionary.Values.All(quest => quest.questData.isComplete);
    }

    // Weekly Special Reward ���� �Լ�
    public void AddWeeklySpecialRewardCount()
    {
        WeeklySpecialRewardCount++;
    }

    // Weekly Special Reward ���� �Լ�          - ���߿� ������ �ð��� �ʱ�ȭ�ǰ� �ϱ� ����
    public void ResetWeeklySpecialRewardCount()
    {
        WeeklySpecialRewardCount = 0;
    }

    #endregion


    #region ��ü ���� �ޱ� - AllReward

    // ��� Ȱ��ȭ�� ������ �����ϴ� �Լ� - Daily
    private void ReceiveAllDailyRewards()
    {
        // ����� ���� Ȱ��ȭ �����Ͻ� ����
        if (dailySpecialRewardButton.interactable && !isDailySpecialRewardQuestComplete)
        {
            ReceiveDailySpecialReward();
        }

        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref dailyQuest.Value.questData.isComplete, dailyQuest.Value.questData.rewardCash,
                    dailyQuest.Value.rewardButton, dailyQuest.Value.rewardDisabledBG, QuestMenuType.Daily, dailyQuest.Key);
            }
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

        allRewardButtons[(int)QuestMenuType.Daily].interactable = isAnyRewardAvailable;
    }

    // ��� Ȱ��ȭ�� ������ �����ϴ� �Լ� - Weekly
    private void ReceiveAllWeeklyRewards()
    {
        // ����� ���� Ȱ��ȭ �����Ͻ� ����
        if (weeklySpecialRewardButton.interactable && !isWeeklySpecialRewardQuestComplete)
        {
            ReceiveWeeklySpecialReward();
        }

        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable && !weeklyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref weeklyQuest.Value.questData.isComplete, weeklyQuest.Value.questData.rewardCash,
                    weeklyQuest.Value.rewardButton, weeklyQuest.Value.rewardDisabledBG, QuestMenuType.Weekly, weeklyQuest.Key);
            }
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

        allRewardButtons[(int)QuestMenuType.Weekly].interactable = isAnyRewardAvailable;
    }

    // ��� Ȱ��ȭ�� ������ �����ϴ� �Լ� - Repeat
    private void ReceiveAllRepeatRewards()
    {
        foreach (var repeatQuest in repeatQuestDictionary)
        {
            if (repeatQuest.Value.rewardButton.interactable)
            {
                ReceiveRepeatQuestReward(repeatQuest.Key, repeatQuest.Value.questData.rewardCash);
            }
        }
    }

    // All Reward ��ư ���¸� ������Ʈ�ϴ� �Լ� - Repeat
    private void UpdateAllRepeatRewardButtonState()
    {
        // ������ ���� �� �ִ� ��ư�� �ϳ��� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
        bool isAnyRewardAvailable = false;
        foreach (var repeatQuest in repeatQuestDictionary)
        {
            if (repeatQuest.Value.rewardButton.interactable)
            {
                isAnyRewardAvailable = true;
                break;
            }
        }

        allRewardButtons[(int)QuestMenuType.Repeat].interactable = isAnyRewardAvailable;
    }


    // ���� ����Ʈ ���� ���� ó�� �Լ� - Daily, Weekly
    private void ReceiveQuestReward(ref bool isQuestComplete, int rewardCash, Button rewardButton, GameObject disabledBG, QuestMenuType menuType, string questName)
    {
        isQuestComplete = true;
        AddCash(rewardCash);
        rewardButton.interactable = false;
        disabledBG.SetActive(true);

        // QuestType�� ���� Ư�� ���� ī��Ʈ �߰�
        if (menuType == QuestMenuType.Daily)
        {
            AddDailySpecialRewardCount();
            UpdateDailySpecialRewardUI();
            //UpdateAllDailyRewardButtonState();      // ���� ���⼭ ȣ���� �Ǿ��ϴµ� PlayTime�� Update���� �þ�� ��� ȣ������� ���� ��� ������ �ǹ���. (����ȭ ���� �ʿ�)
        }
        else if (menuType == QuestMenuType.Weekly)
        {
            AddWeeklySpecialRewardCount();
            UpdateWeeklySpecialRewardUI();
            //UpdateAllWeeklyRewardButtonState();     // ���� ����
        }

        // ����Ʈ UI ������Ʈ ȣ��
        UpdateQuestUI(questName, menuType);
    }

    // ���� ����Ʈ ���� ���� ó�� �Լ� - Repeat
    private void ReceiveRepeatQuestReward(string questName, int rewardCash)
    {
        repeatQuestDictionary[questName].questData.targetCount += repeatQuestDictionary[questName].questData.plusTargetCount;
        AddCash(rewardCash);

        UpdateQuestUI(questName, QuestMenuType.Repeat);
        UpdateAllRepeatRewardButtonState();
        SortRepeatQuests();
    }

    #endregion


    #region �߰� ���

    // ĳ�� �߰� �Լ�
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    #endregion


    #region Repeat Quest Sort

    // �ݺ� ����Ʈ ���� �Լ�
    private void SortRepeatQuests()
    {
        // Dictionary ���� List�� ��ȯ
        var sortedQuests = repeatQuestDictionary.Values.ToList();

        // ����
        sortedQuests.Sort((a, b) =>
        {
            // ���� ��ư�� Ȱ��ȭ�� ����Ʈ�� ��ܿ� ������ ����
            if (a.rewardButton.interactable && !b.rewardButton.interactable) return -1;
            if (!a.rewardButton.interactable && b.rewardButton.interactable) return 1;

            // ���� ��ư�� �����ϰ� Ȱ��ȭ�� ���, Ȱ��ȭ�� ������ ����
            if (a.rewardButton.interactable && b.rewardButton.interactable)
            {
                return a.slotTransform.GetSiblingIndex() - b.slotTransform.GetSiblingIndex();
            }

            return 0;
        });

        // ���ĵ� ������ List ����
        sortedRepeatQuestList = new List<QuestUI>(sortedQuests);

        // ���ĵ� ������ ���� UI�� �θ� �� ��ġ ����
        Transform parentTransform = questSlotParents[(int)QuestMenuType.Repeat];

        // ���Ե��� ������: ���� ��ư�� Ȱ��ȭ�� ����Ʈ�� �׷��� ���� ����Ʈ��
        List<QuestUI> rewardAvailableQuests = new List<QuestUI>();
        List<QuestUI> rewardUnavailableQuests = new List<QuestUI>();

        // Ȱ��ȭ�� ���� ��ư�� �ִ� ����Ʈ���� ���� ��ܿ� ��ġ
        foreach (var quest in sortedQuests)
        {
            if (quest.rewardButton.interactable)
            {
                rewardAvailableQuests.Add(quest);
            }
            else
            {
                rewardUnavailableQuests.Add(quest);
            }
        }

        // Ȱ��ȭ�� ���� ��ư�� �ִ� ����Ʈ�� ��ܿ� ��ġ
        int siblingIndex = 0;

        // ���� ��ư Ȱ��ȭ�� ����Ʈ�� ���� ��ġ
        foreach (var quest in rewardAvailableQuests)
        {
            Transform slotTransform = quest.slotTransform;

            if (slotTransform.parent != parentTransform)
            {
                slotTransform.SetParent(parentTransform);
            }

            // �ִϸ��̼� ����
            AnimateSlotPosition(slotTransform, siblingIndex++);
        }

        // ���� ��ư�� Ȱ��ȭ���� ���� ����Ʈ�� �� �Ʒ��� ��ġ
        foreach (var quest in rewardUnavailableQuests)
        {
            Transform slotTransform = quest.slotTransform;

            if (slotTransform.parent != parentTransform)
            {
                slotTransform.SetParent(parentTransform);
            }

            // �ִϸ��̼� ����
            AnimateSlotPosition(slotTransform, siblingIndex++);
        }
    }

    // ���� �̵� �ִϸ��̼� ó�� �Լ�
    private void AnimateSlotPosition(Transform slotTransform, int targetSiblingIndex)
    {
        // ��ǥ ��ġ ����
        Vector3 targetPosition = GetSlotPosition(targetSiblingIndex);

        // ���� ��ġ���� �ִϸ��̼� ����
        Coroutine animation = StartCoroutine(SlotMoveAnimation(slotTransform, targetPosition));
        activeAnimations[slotTransform] = animation;
    }

    // ������ ��ǥ ��ġ ��� �Լ�
    private Vector3 GetSlotPosition(int siblingIndex)
    {
        ScrollRect scrollRect = mainQuestMenus[(int)QuestMenuType.Repeat].GetComponentInChildren<ScrollRect>();
        RectTransform content = scrollRect.content;
        GridLayoutGroup gridLayout = content.GetComponent<GridLayoutGroup>();

        // ������ ������ �� ������ �� ũ��� �����̽� ����
        int slotCount = content.childCount;
        float cellHeight = gridLayout.cellSize.y;
        float spacingHeight = gridLayout.spacing.y;

        // ��ǥ ��ġ ���: siblingIndex�� �ش��ϴ� ������ y ��ǥ
        int halfSlotCount = slotCount / 2;
        float targetYPosition;
        if (slotCount % 2 == 0)
        {
            targetYPosition = (halfSlotCount - siblingIndex - 1) * (cellHeight + spacingHeight) + (cellHeight / 2 + spacingHeight / 2);
        }
        else
        {
            targetYPosition = (halfSlotCount - siblingIndex) * (cellHeight + spacingHeight);
        }

        return new Vector3(0, targetYPosition, 0);
    }

    // ���� �̵� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator SlotMoveAnimation(Transform slotTransform, Vector3 targetPosition)
    {
        const float animationDuration = 0.2f; // �ִϸ��̼� ���� �ð�
        float elapsedTime = 0f;

        // ���� ��ġ ����
        Vector3 startPosition = slotTransform.localPosition;

        // �ִϸ��̼� ����
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float time = Mathf.Clamp01(elapsedTime / animationDuration);

            slotTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, time);

            yield return null;
        }

        // �ִϸ��̼� �Ϸ� �� ��Ȯ�� ��ġ�� ����
        slotTransform.localPosition = targetPosition;

        // ���� �ִϸ��̼� ���� ����
        activeAnimations.Remove(slotTransform);
    }

    // ���ĵ� ����Ʈ ����Ʈ ���� �Լ�
    private void InitializeRepeatQuestUIFromSortedList()
    {
        Transform parentTransform = questSlotParents[(int)QuestMenuType.Repeat];

        foreach (var quest in sortedRepeatQuestList)
        {
            Transform slotTransform = quest.slotTransform;
            slotTransform.SetParent(parentTransform);

            int index = sortedRepeatQuestList.IndexOf(quest);
            slotTransform.SetSiblingIndex(index);
        }
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        // ���� �ð�
        public string lastDailyReset;    // ������ ���� ����Ʈ ���� �ð�
        public string lastWeeklyReset;   // ������ �ְ� ����Ʈ ���� �ð�

        // ����Ʈ ī����
        public float playTimeCount;
        public int mergeCount;
        public int spawnCount;
        public int purchaseCatsCount;
        public int battleCount;

        // ����� ���� ������
        public bool isDailySpecialRewardQuestComplete;
        public bool isWeeklySpecialRewardQuestComplete;
        public int dailySpecialRewardCount;
        public int weeklySpecialRewardCount;

        // ����Ʈ �����͸� ������ ����ü
        [Serializable]
        public struct QuestSaveData
        {
            public int currentCount;
            public int targetCount;
            public bool isComplete;
        }

        // Dictionary�� ����ȭ ������ ���·� ����
        public List<string> dailyQuestKeys = new List<string>();
        public List<QuestSaveData> dailyQuestValues = new List<QuestSaveData>();

        public List<string> weeklyQuestKeys = new List<string>();
        public List<QuestSaveData> weeklyQuestValues = new List<QuestSaveData>();

        public List<string> repeatQuestKeys = new List<string>();
        public List<QuestSaveData> repeatQuestValues = new List<QuestSaveData>();
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            // ���� �ð�
            lastDailyReset = lastDailyReset.ToString(),
            lastWeeklyReset = lastWeeklyReset.ToString(),

            // ����Ʈ ī����
            playTimeCount = this.playTimeCount,
            mergeCount = this.mergeCount,
            spawnCount = this.spawnCount,
            purchaseCatsCount = this.purchaseCatsCount,
            battleCount = this.battleCount,

            // ����� ���� ������
            isDailySpecialRewardQuestComplete = this.isDailySpecialRewardQuestComplete,
            isWeeklySpecialRewardQuestComplete = this.isWeeklySpecialRewardQuestComplete,
            dailySpecialRewardCount = this.dailySpecialRewardCount,
            weeklySpecialRewardCount = this.weeklySpecialRewardCount
        };

        // ���� ����Ʈ ������ ����
        foreach (var quest in dailyQuestDictionary)
        {
            data.dailyQuestKeys.Add(quest.Key);
            data.dailyQuestValues.Add(new SaveData.QuestSaveData
            {
                currentCount = quest.Value.questData.currentCount,
                targetCount = quest.Value.questData.targetCount,
                isComplete = quest.Value.questData.isComplete
            });
        }

        // �ְ� ����Ʈ ������ ����
        foreach (var quest in weeklyQuestDictionary)
        {
            data.weeklyQuestKeys.Add(quest.Key);
            data.weeklyQuestValues.Add(new SaveData.QuestSaveData
            {
                currentCount = quest.Value.questData.currentCount,
                targetCount = quest.Value.questData.targetCount,
                isComplete = quest.Value.questData.isComplete
            });
        }

        // �ݺ� ����Ʈ ������ ����
        foreach (var quest in repeatQuestDictionary)
        {
            data.repeatQuestKeys.Add(quest.Key);
            data.repeatQuestValues.Add(new SaveData.QuestSaveData
            {
                currentCount = quest.Value.questData.currentCount,
                targetCount = quest.Value.questData.targetCount,
                isComplete = quest.Value.questData.isComplete
            });
        }

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            // �ʱ� ������ ����
            lastDailyReset = DateTime.Now;
            lastWeeklyReset = DateTime.Now;
            return;
        }

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        // ���� �ð� ����
        if (DateTime.TryParse(savedData.lastDailyReset, out DateTime dailyReset))
        {
            lastDailyReset = dailyReset;
        }
        if (DateTime.TryParse(savedData.lastWeeklyReset, out DateTime weeklyReset))
        {
            lastWeeklyReset = weeklyReset;
        }

        // ����Ʈ ī���� ����
        playTimeCount = savedData.playTimeCount;
        mergeCount = savedData.mergeCount;
        spawnCount = savedData.spawnCount;
        purchaseCatsCount = savedData.purchaseCatsCount;
        battleCount = savedData.battleCount;

        // ����� ���� ������ ����
        isDailySpecialRewardQuestComplete = savedData.isDailySpecialRewardQuestComplete;
        isWeeklySpecialRewardQuestComplete = savedData.isWeeklySpecialRewardQuestComplete;
        dailySpecialRewardCount = savedData.dailySpecialRewardCount;
        weeklySpecialRewardCount = savedData.weeklySpecialRewardCount;

        // ����Ʈ ������ ����
        LoadQuestData(savedData);

        // UI ������Ʈ
        UpdateAllUI();

        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();
        }
    }

    private void LoadQuestData(SaveData savedData)
    {
        // ����Ʈ ������ ������ ���� ���� �Լ�
        void RestoreQuestData(Dictionary<string, QuestUI> questDict, List<string> keys, List<SaveData.QuestSaveData> values, QuestMenuType menuType)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                string questKey = keys[i];
                if (questDict.ContainsKey(questKey))
                {
                    var questData = questDict[questKey].questData;
                    var savedQuestData = values[i];

                    // ���� ī��Ʈ ������Ʈ
                    questData.currentCount = GetCurrentCount(questKey, savedQuestData.currentCount);

                    // �ݺ� ����Ʈ�� ��� ��ǥ���� ����
                    if (menuType == QuestMenuType.Repeat)
                    {
                        questData.targetCount = savedQuestData.targetCount;
                    }
                    else
                    {
                        questData.isComplete = savedQuestData.isComplete;
                        if (questData.isComplete)
                        {
                            questDict[questKey].rewardText.text = "���� �Ϸ�";
                        }
                    }
                }
            }

            // UI ������Ʈ
            foreach (var quest in questDict)
            {
                UpdateQuestUI(quest.Key, menuType);
            }
        }

        // ���� ī��Ʈ ���� �������� �Լ�
        int GetCurrentCount(string questKey, int savedCount)
        {
            return questKey switch
            {
                "�÷��� �ð�" => (int)playTimeCount,
                "����� ���� Ƚ��" => mergeCount,
                "����� ��ȯ Ƚ��" => spawnCount,
                "����� ���� Ƚ��" => purchaseCatsCount,
                "���� Ƚ��" => battleCount,
                "���� ��������" => StageCount - 1,
                _ => savedCount
            };
        }

        // �� ����Ʈ Ÿ�Ժ� ������ ����
        RestoreQuestData(dailyQuestDictionary, savedData.dailyQuestKeys, savedData.dailyQuestValues, QuestMenuType.Daily);
        RestoreQuestData(weeklyQuestDictionary, savedData.weeklyQuestKeys, savedData.weeklyQuestValues, QuestMenuType.Weekly);
        RestoreQuestData(repeatQuestDictionary, savedData.repeatQuestKeys, savedData.repeatQuestValues, QuestMenuType.Repeat);
    }

    private void UpdateAllUI()
    {
        // ����� ���� UI ������Ʈ
        UpdateDailySpecialRewardUI();
        UpdateWeeklySpecialRewardUI();

        // ���� ��ư ���� ������Ʈ
        UpdateAllDailyRewardButtonState();
        UpdateAllWeeklyRewardButtonState();
        UpdateAllRepeatRewardButtonState();

        // New �̹��� ���� ������Ʈ
        UpdateNewImageStatus();
    }

    #endregion


    #region Time Reset System

    private void CheckAndResetQuests()
    {
        CheckDailyReset();
        CheckWeeklyReset();
    }

    private void CheckDailyReset()
    {
        DateTime now = DateTime.Now;

        if (now.Date > lastDailyReset.Date)
        {
            ResetDailyQuests();
            lastDailyReset = now;
            GoogleSave();
        }
    }

    private void CheckWeeklyReset()
    {
        DateTime now = DateTime.Now;

        // ���� ������ �����(4)�̰�, ������ ���� �ð��� ���ú��� ������ ���
        if (now.DayOfWeek == DayOfWeek.Thursday && now.Date > lastWeeklyReset.Date)
        {
            ResetWeeklyQuests();
            lastWeeklyReset = now;
            GoogleSave();
        }
    }

    private void ResetDailyQuests()
    {
        // ���� ����Ʈ �ʱ�ȭ
        foreach (var quest in dailyQuestDictionary)
        {
            quest.Value.questData.currentCount = 0;
            quest.Value.questData.isComplete = false;
            quest.Value.rewardText.text = "�ޱ�";
            UpdateQuestUI(quest.Key, QuestMenuType.Daily);
        }

        // ���� Ư�� ���� �ʱ�ȭ
        isDailySpecialRewardQuestComplete = false;
        dailySpecialRewardCount = 0;
        UpdateDailySpecialRewardUI();

        // ī���� �ʱ�ȭ
        ResetPlayTimeCount();
        ResetMergeCount();
        ResetSpawnCount();
        ResetPurchaseCatsCount();
        ResetBattleCount();

        // UI ������Ʈ
        UpdateAllDailyRewardButtonState();
        UpdateNewImageStatus();

        GoogleSave();
    }

    private void ResetWeeklyQuests()
    {
        // �ְ� ����Ʈ �ʱ�ȭ
        foreach (var quest in weeklyQuestDictionary)
        {
            quest.Value.questData.currentCount = 0;
            quest.Value.questData.isComplete = false;
            quest.Value.rewardText.text = "�ޱ�";
            UpdateQuestUI(quest.Key, QuestMenuType.Weekly);
        }

        // �ְ� Ư�� ���� �ʱ�ȭ
        isWeeklySpecialRewardQuestComplete = false;
        weeklySpecialRewardCount = 0;
        UpdateWeeklySpecialRewardUI();

        // UI ������Ʈ
        UpdateAllWeeklyRewardButtonState();
        UpdateNewImageStatus();

        GoogleSave();
    }

    #endregion


}
