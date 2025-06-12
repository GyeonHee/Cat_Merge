using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;

// ����Ʈ ��ũ��Ʈ
[DefaultExecutionOrder(-1)]
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
            public int baseTargetCount;             // �⺻ ��ǥ ��ġ (�ʱⰪ)
            public int rewardCount;                 // ���� ���� Ƚ��
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
    private int weeklySpecialRewardQuestRewardCash = 2500;                      // Weekly Special Reward ����Ʈ ���� ĳ�� ��ȭ ����
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

    // ���� ����Ʈ ī����
    private float dailyPlayTimeCount;
    private int dailyMergeCount;
    private int dailySpawnCount;
    private int dailyBattleCount;

    // �ְ� ����Ʈ ī����
    private float weeklyPlayTimeCount;
    private int weeklyMergeCount;
    private int weeklySpawnCount;
    private int weeklyBattleCount;

    // �ݺ� ����Ʈ ī���� (����)
    private int totalMergeCount;
    private int totalSpawnCount;
    private int totalBattleCount;
    //private int totalPurchaseCount;

    public int StageCount { get => BattleManager.Instance.BossStage; }  // �������� �ܰ�

    private int dailySpecialRewardCount;                                // Daily ���� ����Ʈ ���� Ƚ��
    public int DailySpecialRewardCount { get => dailySpecialRewardCount; set => dailySpecialRewardCount = value; }

    private int weeklySpecialRewardCount;                               // Weekly ���� ����Ʈ ���� Ƚ��
    public int WeeklySpecialRewardCount { get => weeklySpecialRewardCount; set => weeklySpecialRewardCount = value; }


    private long lastDailyReset;                // ���� ����Ʈ ������ ���� �ð� ����
    private long lastWeeklyReset;               // �ְ� ����Ʈ ������ ���� �ð� ����

    private bool isDataLoaded = false;          // ������ �ε� Ȯ��

    private readonly WaitForSeconds oneSecondWait = new WaitForSeconds(1f);
    private Coroutine questCheckCoroutine;

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
#if UNITY_EDITOR
        InitializeDebugControls();
#endif

        // GoogleManager���� �����͸� �ε����� ���� ��쿡�� �ʱ�ȭ
        if (!isDataLoaded)
        {
            lastDailyReset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            lastWeeklyReset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        InitializeQuestManager();

        CheckAndResetQuestsOnStart();

        UpdateAllUI();

        questCheckCoroutine = StartCoroutine(QuestCheckRoutine());
    }

    private void OnDisable()
    {
        if (questCheckCoroutine != null)
        {
            StopCoroutine(questCheckCoroutine);
            questCheckCoroutine = null;
        }
    }

    #endregion


    #region Daily & Weekly Quest Initialize Check

    // ����Ʈ üũ �ڷ�ƾ
    private IEnumerator QuestCheckRoutine()
    {
        while (true)
        {
            // �÷���Ÿ�� ����Ʈ �ִ�ġ üũ
            bool canAddDailyPlayTime = dailyQuestDictionary["�÷��� �ð�"].questData.currentCount < dailyQuestDictionary["�÷��� �ð�"].questData.targetCount;
            bool canAddWeeklyPlayTime = weeklyQuestDictionary["�÷��� �ð�"].questData.currentCount < weeklyQuestDictionary["�÷��� �ð�"].questData.targetCount;

            // �� �� �ϳ��� �ִ�ġ�� �������� �ʾҴٸ� �÷���Ÿ�� �߰�
            if (canAddDailyPlayTime || canAddWeeklyPlayTime)
            {
                AddPlayTimeCount();
            }

            CheckAndResetQuests();

            yield return oneSecondWait;
        }
    }

    #endregion


    #region Initialize

    // �� ���� �� �ð� üũ �� �ʱ�ȭ
    private void CheckAndResetQuestsOnStart()
    {
        DateTimeOffset currentUtc = DateTimeOffset.UtcNow;
        DateTimeOffset lastDailyResetTime = DateTimeOffset.FromUnixTimeSeconds(lastDailyReset);
        DateTimeOffset lastWeeklyResetTime = DateTimeOffset.FromUnixTimeSeconds(lastWeeklyReset);

        // ������ ���� ���� ���� ���� ��¥ �� ���
        int daysSinceLastDaily = (currentUtc.Date - lastDailyResetTime.Date).Days;

        // �Ϸ� �̻� �����ٸ� ���� ����Ʈ �ʱ�ȭ
        if (daysSinceLastDaily > 0)
        {
            //Debug.Log($"[Quest] ���� ����Ʈ �ʱ�ȭ - ������ ����: {lastDailyResetTime}, ����: {currentUtc}, �����: {daysSinceLastDaily}");
            ResetDailyQuests();
            lastDailyReset = currentUtc.ToUnixTimeSeconds();
        }

        // �ְ� ����Ʈ üũ
        DateTimeOffset nextThursday = GetNextThursday(lastWeeklyResetTime);
        if (currentUtc >= nextThursday)
        {
            //Debug.Log($"[Quest] �ְ� ����Ʈ �ʱ�ȭ - ������ ����: {lastWeeklyResetTime}, ����: {currentUtc}, ���� �����: {nextThursday}");
            ResetWeeklyQuests();
            lastWeeklyReset = currentUtc.ToUnixTimeSeconds();
        }
    }

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
        InitializeQuest("�÷��� �ð�", 600, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("����� �ռ� Ƚ��", 60, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ��ȯ Ƚ��", 60, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("���� Ƚ��", 1, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");

        InitializeDailySpecialReward();

        allRewardButtons[(int)QuestMenuType.Daily].onClick.AddListener(ReceiveAllDailyRewards);
        UpdateAllDailyRewardButtonState();
    }

    // Weekly Quest ���� �Լ�
    private void InitializeWeeklyQuestManager()
    {
        InitializeQuest("�÷��� �ð�", 3000, 50, QuestMenuType.Weekly, "I_UI_Mission_Weekly.9");
        InitializeQuest("����� �ռ� Ƚ��", 600, 50, QuestMenuType.Weekly, "I_UI_Mission_Weekly.9");
        InitializeQuest("����� ��ȯ Ƚ��", 600, 50, QuestMenuType.Weekly, "I_UI_Mission_Weekly.9");
        InitializeQuest("���� Ƚ��", 5, 50, QuestMenuType.Weekly, "I_UI_Mission_Weekly.9");

        InitializeWeeklySpecialReward();

        allRewardButtons[(int)QuestMenuType.Weekly].onClick.AddListener(ReceiveAllWeeklyRewards);
        UpdateAllWeeklyRewardButtonState();
    }

    // Repeat Quest ���� �Լ�
    private void InitializeRepeatQuestManager()
    {
        InitializeQuest("����� �ռ� Ƚ��", 160, 4, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("����� ��ȯ Ƚ��", 160, 4, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        //InitializeQuest("����� ���� Ƚ��", 20, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("���� Ƚ��", 3, 4, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");

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
                baseTargetCount = targetCount,
                rewardCount = 1,
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

        SortRepeatQuests();
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

        if (menuType == QuestMenuType.Daily)
        {
            UpdateAllDailyRewardButtonState();
        }
        else if (menuType == QuestMenuType.Weekly)
        {
            UpdateAllWeeklyRewardButtonState();
        }
        else
        {
            UpdateAllRepeatRewardButtonState();
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
        }
        // �ְ� ����Ʈ ������Ʈ
        if (weeklyQuestDictionary.ContainsKey(questName))
        {
            QuestUI questUI = weeklyQuestDictionary[questName];
            QuestUI.QuestData questData = questUI.questData;

            questData.currentCount = Mathf.Min(questData.currentCount, questData.targetCount);

            UpdateQuestUI(questName, QuestMenuType.Weekly);
        }
        // �ݺ� ����Ʈ ������Ʈ
        if (repeatQuestDictionary.ContainsKey(questName))
        {
            UpdateQuestUI(questName, QuestMenuType.Repeat);
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
        float addTime = 1f;

        // ���� ����Ʈ �÷���Ÿ��
        if (dailyQuestDictionary["�÷��� �ð�"].questData.currentCount < dailyQuestDictionary["�÷��� �ð�"].questData.targetCount)
        {
            dailyPlayTimeCount += addTime;
            dailyQuestDictionary["�÷��� �ð�"].questData.currentCount = (int)dailyPlayTimeCount;
        }

        // �ְ� ����Ʈ �÷���Ÿ��
        if (weeklyQuestDictionary["�÷��� �ð�"].questData.currentCount < weeklyQuestDictionary["�÷��� �ð�"].questData.targetCount)
        {
            weeklyPlayTimeCount += addTime;
            weeklyQuestDictionary["�÷��� �ð�"].questData.currentCount = (int)weeklyPlayTimeCount;
        }

        UpdateQuestProgress("�÷��� �ð�");
    }

    // �÷���Ÿ�� ���� �Լ�
    public void ResetPlayTimeCount()
    {
        dailyPlayTimeCount = 0;
        weeklyPlayTimeCount = 0;
    }

    #endregion


    #region Merge Quest

    // ����� ���� ���� �Լ�
    public void AddMergeCount()
    {
        dailyMergeCount++;
        weeklyMergeCount++;
        totalMergeCount++;

        dailyQuestDictionary["����� �ռ� Ƚ��"].questData.currentCount = dailyMergeCount;
        weeklyQuestDictionary["����� �ռ� Ƚ��"].questData.currentCount = weeklyMergeCount;
        repeatQuestDictionary["����� �ռ� Ƚ��"].questData.currentCount = totalMergeCount;

        UpdateQuestProgress("����� �ռ� Ƚ��");
    }

    // ����� ���� ���� �Լ�
    public void ResetMergeCount()
    {
        dailyMergeCount = 0;
        weeklyMergeCount = 0;
        totalMergeCount = 0;
    }

    #endregion


    #region Spawn Quest

    // ����� ���� ���� �Լ�
    public void AddSpawnCount()
    {
        dailySpawnCount++;
        weeklySpawnCount++;
        totalSpawnCount++;

        dailyQuestDictionary["����� ��ȯ Ƚ��"].questData.currentCount = dailySpawnCount;
        weeklyQuestDictionary["����� ��ȯ Ƚ��"].questData.currentCount = weeklySpawnCount;
        repeatQuestDictionary["����� ��ȯ Ƚ��"].questData.currentCount = totalSpawnCount;

        UpdateQuestProgress("����� ��ȯ Ƚ��");
    }

    // ����� ���� ���� �Լ�
    public void ResetSpawnCount()
    {
        dailySpawnCount = 0;
        weeklySpawnCount = 0;
        totalSpawnCount = 0;
    }

    #endregion


    #region Purchase Cats Quest

    //// ����� ���� ���� �Լ�
    //public void AddPurchaseCatsCount()
    //{
    //    totalPurchaseCount++;

    //    repeatQuestDictionary["����� ���� Ƚ��"].questData.currentCount = totalPurchaseCount;

    //    UpdateQuestProgress("����� ���� Ƚ��");
    //}

    //// ����� ���� ���� �Լ�
    //public void ResetPurchaseCatsCount()
    //{
    //    totalPurchaseCount = 0;
    //}

    #endregion


    #region Battle Count Quest

    // ��Ʋ ���� �Լ�
    public void AddBattleCount()
    {
        dailyBattleCount++;
        weeklyBattleCount++;
        totalBattleCount++;

        dailyQuestDictionary["���� Ƚ��"].questData.currentCount = dailyBattleCount;
        weeklyQuestDictionary["���� Ƚ��"].questData.currentCount = weeklyBattleCount;
        repeatQuestDictionary["���� Ƚ��"].questData.currentCount = totalBattleCount;

        UpdateQuestProgress("���� Ƚ��");
    }

    // ��Ʋ ���� �Լ�
    public void ResetBattleCount()
    {
        dailyBattleCount = 0;
        weeklyBattleCount = 0;
        totalBattleCount = 0;
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
        UpdateAllDailyRewardButtonState();
        UpdateNewImageStatus();
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
        UpdateAllWeeklyRewardButtonState();
        UpdateNewImageStatus();
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

        UpdateNewImageStatus();
    }

    // All Reward ��ư ���¸� ������Ʈ�ϴ� �Լ� - Daily
    private void UpdateAllDailyRewardButtonState()
    {
        // ������ ���� �� �ִ� ��ư�� �ϳ��� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
        bool isAnyRewardAvailable = false;

        // ����� ���� Ȯ��
        if (dailySpecialRewardButton.interactable && !isDailySpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
        }

        // �Ϲ� ����Ʈ Ȯ��
        if (!isAnyRewardAvailable)
        {
            foreach (var dailyQuest in dailyQuestDictionary)
            {
                if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
                {
                    isAnyRewardAvailable = true;
                    break;
                }
            }
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

        UpdateNewImageStatus();
    }

    // All Reward ��ư ���¸� ������Ʈ�ϴ� �Լ� - Weekly
    private void UpdateAllWeeklyRewardButtonState()
    {
        // ������ ���� �� �ִ� ��ư�� �ϳ��� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
        bool isAnyRewardAvailable = false;

        // ����� ���� Ȯ��
        if (weeklySpecialRewardButton.interactable && !isWeeklySpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
        }

        // �Ϲ� ����Ʈ Ȯ��
        if (!isAnyRewardAvailable)
        {
            foreach (var weeklyQuest in weeklyQuestDictionary)
            {
                if (weeklyQuest.Value.rewardButton.interactable && !weeklyQuest.Value.questData.isComplete)
                {
                    isAnyRewardAvailable = true;
                    break;
                }
            }
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
        UpdateNewImageStatus();
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
        }
        else if (menuType == QuestMenuType.Weekly)
        {
            AddWeeklySpecialRewardCount();
            UpdateWeeklySpecialRewardUI();
        }

        // ����Ʈ UI ������Ʈ ȣ��
        UpdateQuestUI(questName, menuType);
    }

    // ���� ����Ʈ ���� ���� ó�� �Լ� - Repeat
    private void ReceiveRepeatQuestReward(string questName, int rewardCash)
    {
        var questData = repeatQuestDictionary[questName].questData;

        questData.rewardCount++;

        int additionalMultiplier = (questData.rewardCount - 1) / 10;                    // 0���� �����Ͽ� 10ȸ���� 1�� ����
        int currentIncrease = questData.baseTargetCount * (additionalMultiplier + 1);   // �⺻�� + �߰� ������
        questData.targetCount += currentIncrease;

        AddCash(rewardCash);

        UpdateQuestUI(questName, QuestMenuType.Repeat);
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


    #region Time Reset System

    private void CheckAndResetQuests()
    {
#if UNITY_EDITOR
        long currentTime = useDebugTime ? debugCurrentTime.ToUnixTimeSeconds() : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        DateTimeOffset currentUtc = DateTimeOffset.FromUnixTimeSeconds(currentTime);
#else
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        DateTimeOffset currentUtc = DateTimeOffset.FromUnixTimeSeconds(currentTime);
#endif
        DateTimeOffset lastDailyResetTime = DateTimeOffset.FromUnixTimeSeconds(lastDailyReset);
        DateTimeOffset lastWeeklyResetTime = DateTimeOffset.FromUnixTimeSeconds(lastWeeklyReset);

        // ���� ����Ʈ �ʱ�ȭ üũ
        if (currentUtc.Date > lastDailyResetTime.Date)
        {
            //Debug.Log($"[Quest] ���� ����Ʈ �ʱ�ȭ - ����: {currentUtc}, ������ ����: {lastDailyResetTime}");
            ResetDailyQuests();
            lastDailyReset = currentTime;
        }

        // �ְ� ����Ʈ �ʱ�ȭ üũ (UTC ���� �����)
        DateTimeOffset nextThursday = GetNextThursday(lastWeeklyResetTime);
        if (currentUtc.Date >= nextThursday.Date && lastWeeklyResetTime.Date < nextThursday.Date)
        {
            //Debug.Log($"[Quest] �ְ� ����Ʈ �ʱ�ȭ - ����: {currentUtc}, ������ ����: {lastWeeklyResetTime}, ���� �����: {nextThursday}");
            ResetWeeklyQuests();
            lastWeeklyReset = currentTime;
        }
    }

    // ���� ����� ���� �ð��� ����ϴ� �Լ�
    private DateTimeOffset GetNextThursday(DateTimeOffset fromTime)
    {
        // ����� ������ �������� ���
        DateTimeOffset thursdayMidnight = fromTime.Date;

        if (fromTime.DayOfWeek == DayOfWeek.Thursday)
        {
            // ������̸鼭 ������ ���� ���
            if (fromTime.TimeOfDay > TimeSpan.Zero)
            {
                return thursdayMidnight.AddDays(7);
            }
            // ����������� ������ ���
            return thursdayMidnight;
        }

        // ���� ����ϱ��� ���� �ϼ� ���
        int daysUntilThursday = ((int)DayOfWeek.Thursday - (int)fromTime.DayOfWeek + 7) % 7;

        return thursdayMidnight.AddDays(daysUntilThursday);
    }

    // ���� ����Ʈ �ʱ�ȭ �Լ�
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

        // ���� ī���͸� �ʱ�ȭ
        dailyPlayTimeCount = 0;
        dailyMergeCount = 0;
        dailySpawnCount = 0;
        dailyBattleCount = 0;

        // UI ������Ʈ
        UpdateAllDailyRewardButtonState();
        UpdateNewImageStatus();
    }

    // �ְ�����Ʈ �ʱ�ȭ �Լ�
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

        // �ְ� ī���͸� �ʱ�ȭ
        weeklyPlayTimeCount = 0;
        weeklyMergeCount = 0;
        weeklySpawnCount = 0;
        weeklyBattleCount = 0;

        // UI ������Ʈ
        UpdateAllWeeklyRewardButtonState();
        UpdateNewImageStatus();
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        // ���� �ð� (Unix timestamp)
        public long lastDailyReset;     // ������ ���� ����Ʈ ���� �ð�
        public long lastWeeklyReset;    // ������ �ְ� ����Ʈ ���� �ð�

        // ���� ����Ʈ ī����
        public float dailyPlayTimeCount;
        public int dailyMergeCount;
        public int dailySpawnCount;
        public int dailyBattleCount;

        // �ְ� ����Ʈ ī����
        public float weeklyPlayTimeCount;
        public int weeklyMergeCount;
        public int weeklySpawnCount;
        public int weeklyBattleCount;

        // �ݺ� ����Ʈ ī����
        public int totalMergeCount;
        public int totalSpawnCount;
        //public int totalPurchaseCount;
        public int totalBattleCount;

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
            public int baseTargetCount;
            public int rewardCount;
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
            lastDailyReset = this.lastDailyReset,
            lastWeeklyReset = this.lastWeeklyReset,

            // ���� ����Ʈ ī����
            dailyPlayTimeCount = this.dailyPlayTimeCount,
            dailyMergeCount = this.dailyMergeCount,
            dailySpawnCount = this.dailySpawnCount,
            dailyBattleCount = this.dailyBattleCount,

            // �ְ� ����Ʈ ī����
            weeklyPlayTimeCount = this.weeklyPlayTimeCount,
            weeklyMergeCount = this.weeklyMergeCount,
            weeklySpawnCount = this.weeklySpawnCount,
            weeklyBattleCount = this.weeklyBattleCount,

            // �ݺ� ����Ʈ ī����
            totalMergeCount = this.totalMergeCount,
            totalSpawnCount = this.totalSpawnCount,
            //totalPurchaseCount = this.totalPurchaseCount,
            totalBattleCount = this.totalBattleCount,

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
                baseTargetCount = quest.Value.questData.baseTargetCount,
                rewardCount = quest.Value.questData.rewardCount,
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
            lastDailyReset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            lastWeeklyReset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return;
        }

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        // ���� �ð� ����
        lastDailyReset = savedData.lastDailyReset;
        lastWeeklyReset = savedData.lastWeeklyReset;

        // ����Ʈ ī���� ����
        dailyPlayTimeCount = savedData.dailyPlayTimeCount;
        dailyMergeCount = savedData.dailyMergeCount;
        dailySpawnCount = savedData.dailySpawnCount;
        dailyBattleCount = savedData.dailyBattleCount;

        weeklyPlayTimeCount = savedData.weeklyPlayTimeCount;
        weeklyMergeCount = savedData.weeklyMergeCount;
        weeklySpawnCount = savedData.weeklySpawnCount;
        weeklyBattleCount = savedData.weeklyBattleCount;

        totalMergeCount = savedData.totalMergeCount;
        totalSpawnCount = savedData.totalSpawnCount;
        //totalPurchaseCount = savedData.totalPurchaseCount;
        totalBattleCount = savedData.totalBattleCount;

        // ����� ���� ������ ����
        isDailySpecialRewardQuestComplete = savedData.isDailySpecialRewardQuestComplete;
        isWeeklySpecialRewardQuestComplete = savedData.isWeeklySpecialRewardQuestComplete;
        dailySpecialRewardCount = savedData.dailySpecialRewardCount;
        weeklySpecialRewardCount = savedData.weeklySpecialRewardCount;

        // ����Ʈ ������ ����
        LoadQuestData(savedData);

        CheckAndResetQuestsOnStart();

        UpdateAllUI();

        isDataLoaded = true;
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

                    // ���� ī��Ʈ ������Ʈ (����Ʈ Ÿ�Կ� ���� �ٸ� �� ���)
                    switch (menuType)
                    {
                        case QuestMenuType.Daily:
                            questData.currentCount = questKey switch
                            {
                                "�÷��� �ð�" => (int)dailyPlayTimeCount,
                                "����� �ռ� Ƚ��" => dailyMergeCount,
                                "����� ��ȯ Ƚ��" => dailySpawnCount,
                                "���� Ƚ��" => dailyBattleCount,
                                _ => savedQuestData.currentCount
                            };
                            break;
                        case QuestMenuType.Weekly:
                            questData.currentCount = questKey switch
                            {
                                "�÷��� �ð�" => (int)weeklyPlayTimeCount,
                                "����� �ռ� Ƚ��" => weeklyMergeCount,
                                "����� ��ȯ Ƚ��" => weeklySpawnCount,
                                "���� Ƚ��" => weeklyBattleCount,
                                _ => savedQuestData.currentCount
                            };
                            break;
                        case QuestMenuType.Repeat:
                            questData.currentCount = questKey switch
                            {
                                "����� �ռ� Ƚ��" => totalMergeCount,
                                "����� ��ȯ Ƚ��" => totalSpawnCount,
                                "���� Ƚ��" => totalBattleCount,
                                _ => savedQuestData.currentCount
                            };
                            break;
                    }

                    // �ݺ� ����Ʈ�� ��� ��ǥ���� ����
                    if (menuType == QuestMenuType.Repeat)
                    {
                        questData.targetCount = savedQuestData.targetCount;
                        questData.baseTargetCount = savedQuestData.baseTargetCount;
                        questData.rewardCount = savedQuestData.rewardCount;
                    }
                    else
                    {
                        questData.isComplete = savedQuestData.isComplete;
                        if (questData.isComplete)
                        {
                            questDict[questKey].rewardText.text = "���� �Ϸ�";
                            questDict[questKey].rewardButton.interactable = false;
                            questDict[questKey].rewardDisabledBG.SetActive(true);
                        }
                    }

                    // UI �ʱ�ȭ
                    //questDict[questKey].questSlider.maxValue = questData.targetCount;
                    //questDict[questKey].questSlider.value = Mathf.Min(questData.currentCount, questData.targetCount);
                    //questDict[questKey].countText.text = $"{Mathf.Min(questData.currentCount, questData.targetCount)} / {questData.targetCount}";
                    UpdateQuestUI(questKey, menuType);
                }
            }

            //// UI ������Ʈ
            //foreach (var quest in questDict)
            //{
            //    UpdateQuestUI(quest.Key, menuType);
            //}
        }

        //// ���� ī��Ʈ ���� �������� �Լ� (����)
        //int GetCurrentCount(string questKey, int savedCount)
        //{
        //    //switch (questKey)
        //    //{
        //    //    case "�÷��� �ð�":
        //    //        return (int)dailyPlayTimeCount;
        //    //    case "����� �ռ� Ƚ��":
        //    //        return menuType == QuestMenuType.Daily ? dailyMergeCount :
        //    //               menuType == QuestMenuType.Weekly ? weeklyMergeCount : totalMergeCount;
        //    //    case "����� ��ȯ Ƚ��":
        //    //        return menuType == QuestMenuType.Daily ? dailySpawnCount :
        //    //               menuType == QuestMenuType.Weekly ? weeklySpawnCount : totalSpawnCount;
        //    //    case "���� Ƚ��":
        //    //        return menuType == QuestMenuType.Daily ? dailyBattleCount :
        //    //               menuType == QuestMenuType.Weekly ? weeklyBattleCount : totalBattleCount;
        //    //    default:
        //    //        return savedCount;
        //    //}
        //    return questKey switch
        //    {
        //        "�÷��� �ð�" => (int)dailyPlayTimeCount,
        //        "����� �ռ� Ƚ��" => dailyMergeCount,
        //        "����� ��ȯ Ƚ��" => dailySpawnCount,
        //        "���� Ƚ��" => dailyBattleCount,
        //        _ => savedCount
        //    };
        //}

        // �� ����Ʈ Ÿ�Ժ� ������ ����
        RestoreQuestData(dailyQuestDictionary, savedData.dailyQuestKeys, savedData.dailyQuestValues, QuestMenuType.Daily);
        RestoreQuestData(weeklyQuestDictionary, savedData.weeklyQuestKeys, savedData.weeklyQuestValues, QuestMenuType.Weekly);
        RestoreQuestData(repeatQuestDictionary, savedData.repeatQuestKeys, savedData.repeatQuestValues, QuestMenuType.Repeat);

        // ����� ���� UI �ʱ�ȭ
        UpdateDailySpecialRewardUI();
        UpdateWeeklySpecialRewardUI();

        //// �ݺ� ����Ʈ ���� �� UI ������Ʈ
        //SortRepeatQuests();
        //InitializeRepeatQuestUIFromSortedList();
    }

    #endregion


    #region Unity Editor Test

#if UNITY_EDITOR
    [Header("---[Debug Time Control]")]
    [SerializeField] private bool useDebugTime = false;
    [SerializeField] private Button skipDayButton;         // ������ ��ŵ ��ư
    [SerializeField] private Button skipWeekButton;        // ���� �� ����� ��ŵ ��ư
    private DateTimeOffset debugCurrentTime;

    // ����� �ð� ���� �Լ�
    public void SetDebugTime(int addDays = 0, int addHours = 0)
    {
        if (!useDebugTime) return;

        debugCurrentTime = DateTimeOffset.UtcNow.AddDays(addDays).AddHours(addHours);
        CheckAndResetQuests();
    }

    private void InitializeDebugControls()
    {
        if (skipDayButton != null)
        {
            skipDayButton.onClick.AddListener(SkipToNextDay);
        }

        if (skipWeekButton != null)
        {
            skipWeekButton.onClick.AddListener(SkipToNextThursday);
        }
    }

    // ����׿� �������� �̵�
    public void SkipToNextDay()
    {
        if (!useDebugTime) return;
        //Debug.Log("������ ��ư");

        SetDebugTime(1);
    }

    // ����׿� ���� �� ����Ϸ� �̵�
    public void SkipToNextThursday()
    {
        if (!useDebugTime) return;

        //Debug.Log("������ ��ư");

        DateTimeOffset current = useDebugTime ? debugCurrentTime : DateTimeOffset.UtcNow;
        int daysUntilThursday = ((int)DayOfWeek.Thursday - (int)current.DayOfWeek + 7) % 7;
        if (daysUntilThursday == 0) daysUntilThursday = 7;

        // ���� ����Ϸ� �ð� ����
        debugCurrentTime = current.AddDays(daysUntilThursday);

        // ������ �ְ� ���� �ð��� ���� �ð����� �������� �����Ͽ� ������ �ʱ�ȭ Ʈ����
        lastWeeklyReset = debugCurrentTime.AddDays(-7).ToUnixTimeSeconds();

        // ����Ʈ üũ �� �ʱ�ȭ ����
        CheckAndResetQuests();
    }
#endif

    #endregion


}
