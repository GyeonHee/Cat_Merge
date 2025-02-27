using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

// Quest Script
public class QuestManager : MonoBehaviour
{
    // Singleton Instance
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
        mainQuestButtonNewImage.SetActive(false);
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
    }

    // ======================================================================================================================
    // [Initialize]

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
        InitializeQuest("�÷��� �ð�", 10, 5, QuestMenuType.Daily);
        InitializeQuest("����� ���� Ƚ��", 1, 5, QuestMenuType.Daily);
        InitializeQuest("����� ��ȯ Ƚ��", 1, 5, QuestMenuType.Daily);
        InitializeQuest("����� ���� Ƚ��", 1, 5, QuestMenuType.Daily);
        InitializeQuest("���� Ƚ��", 1, 5, QuestMenuType.Daily);

        InitializeDailySpecialReward();

        allRewardButtons[(int)QuestMenuType.Daily].onClick.AddListener(ReceiveAllDailyRewards);
        UpdateAllDailyRewardButtonState();
    }

    // Weekly Quest ���� �Լ�
    private void InitializeWeeklyQuestManager()
    {
        InitializeQuest("�÷��� �ð�", 20, 50, QuestMenuType.Weekly);
        InitializeQuest("����� ���� Ƚ��", 10, 50, QuestMenuType.Weekly);
        InitializeQuest("����� ��ȯ Ƚ��", 10, 50, QuestMenuType.Weekly);
        InitializeQuest("����� ���� Ƚ��", 10, 50, QuestMenuType.Weekly);

        InitializeWeeklySpecialReward();

        allRewardButtons[(int)QuestMenuType.Weekly].onClick.AddListener(ReceiveAllWeeklyRewards);
        UpdateAllWeeklyRewardButtonState();
    }

    // Repeat Quest ���� �Լ�
    private void InitializeRepeatQuestManager()
    {
        InitializeQuest("����� ���� Ƚ��", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("����� ��ȯ Ƚ��", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("����� ���� Ƚ��", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("���� ��������", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("����1", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("����2", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("����3", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("����4", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("����5", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("����6", 1, 5, QuestMenuType.Repeat);

        // �ʱ� ��ũ�� ��ġ �ʱ�ȭ
        InitializeScrollPosition();

        allRewardButtons[(int)QuestMenuType.Repeat].onClick.AddListener(ReceiveAllRepeatRewards);
        UpdateAllRepeatRewardButtonState();
    }

    // ��ũ�� �ʱ� ��ġ�� �����ϴ� �Լ�
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

    // ======================================================================================================================
    // [���� �޴�]

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

    // ======================================================================================================================
    // [New Image ����]

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
        mainQuestButtonNewImage.SetActive(hasUnclaimedRewards);

        // Daily Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        subQuestButtonNewImages[(int)QuestMenuType.Daily].SetActive(hasUnclaimedDailyRewards);

        // Weekly Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        subQuestButtonNewImages[(int)QuestMenuType.Weekly].SetActive(hasUnclaimedWeeklyRewards);

        // Repeat Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        subQuestButtonNewImages[(int)QuestMenuType.Repeat].SetActive(hasUnclaimedRepeatRewards);
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
            slotTransform = newQuestSlot.transform,

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
        else if (menuType == QuestMenuType.Repeat)
        {
            repeatQuestDictionary[questName] = questUI;
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
        questUI.questSlider.value = questData.currentCount;

        // �ؽ�Ʈ ������Ʈ
        questUI.countText.text = $"{questData.currentCount} / {questData.targetCount}";

        // �Ϸ� ���� Ȯ��
        if (menuType == QuestMenuType.Repeat)
        {
            bool isComplete = questData.currentCount >= questData.targetCount;
            questUI.rewardButton.interactable = isComplete;
            questUI.rewardDisabledBG.SetActive(!isComplete);
        }
        else
        {
            bool isComplete = questData.currentCount >= questData.targetCount && !questData.isComplete;
            questUI.rewardButton.interactable = isComplete;
            questUI.rewardDisabledBG.SetActive(!isComplete);

            if (questData.isComplete)
            {
                questUI.rewardText.text = "���� �Ϸ�";
            }
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

    // ======================================================================================================================
    // [PlayTime Quest]

    // �÷���Ÿ�� ���� �Լ�
    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;

        dailyQuestDictionary["�÷��� �ð�"].questData.currentCount = (int)PlayTimeCount;
        weeklyQuestDictionary["�÷��� �ð�"].questData.currentCount = (int)PlayTimeCount;

        UpdateQuestProgress("�÷��� �ð�");
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

    // ======================================================================================================================
    // [Spawn Quest]

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

    // ======================================================================================================================
    // [Purchase Cats Quest]

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

    // ======================================================================================================================
    // [Battle Count Quest]

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

    // ======================================================================================================================
    // [Stage Count Quest]

    // �������� ���� �Լ�
    public void AddStageCount()
    {
        repeatQuestDictionary["���� ��������"].questData.currentCount = StageCount - 1;

        UpdateQuestProgress("���� ��������");
    }

    // ======================================================================================================================
    // [Special Reward Quest]

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

    // Weekly Special Reward ���� �Լ�          - ���߿� ������ �ð��� �ʱ�ȭ�ǰ� �ϱ� ����
    public void ResetWeeklySpecialRewardCount()
    {
        WeeklySpecialRewardCount = 0;
    }

    // ======================================================================================================================
    // [��ü ���� �ޱ� ���� - AllReward]

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
            UpdateAllDailyRewardButtonState();      // ���� �̰ŷθ� ȣ���� �Ǿ��ϴµ� PlayTime�� Update�� �־ �̰� ��� ��� ȣ������� ������.
        }
        else if (menuType == QuestMenuType.Weekly)
        {
            AddWeeklySpecialRewardCount();
            UpdateWeeklySpecialRewardUI();
            UpdateAllWeeklyRewardButtonState();     // ���� �̰ŷθ� ȣ���� �Ǿ��ϴµ� PlayTime�� Update�� �־ �̰� ��� ��� ȣ������� ������.
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

    // ======================================================================================================================
    // [�߰� ��ɵ�]

    // ĳ�� �߰� �Լ�
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    // ======================================================================================================================
    // [�ݺ�����Ʈ ���� ����]

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

    // ======================================================================================================================



}