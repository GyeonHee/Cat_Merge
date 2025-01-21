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
    [SerializeField] private Button questButton;                        // ����Ʈ ��ư
    [SerializeField] private Image questButtonImage;                    // ����Ʈ ��ư �̹���
    [SerializeField] private GameObject questMenuPanel;                 // ����Ʈ �޴� Panel
    [SerializeField] private Button questBackButton;                    // ����Ʈ �ڷΰ��� ��ư
    private ActivePanelManager activePanelManager;                      // ActivePanelManager

    [SerializeField] private GameObject[] mainQuestMenus;               // ���� ����Ʈ �޴� Panels
    [SerializeField] private Button[] subQuestMenuButtons;              // ���� ����Ʈ �޴� ��ư �迭



    [Header("---[Quest]")]
    [SerializeField] private GameObject questSlotPrefab;                // Quest Slot Prefab
    [SerializeField] private Transform questSlotParent;                 // ���Ե��� ��ġ�� �θ� ��ü
    private Dictionary<string, QuestUI> dailyQuestDictionary = new Dictionary<string, QuestUI>(); // Daily Quest Dictionary



    [SerializeField] private ScrollRect repeatQuestScrollRects;         // �ݺ�����Ʈ�� ��ũ�Ѻ�

    // ======================================================================================================================

    [Header("---[Special Reward UI]")]
    [SerializeField] private Slider specialRewardQuestSlider;           // Special Reward Slider
    [SerializeField] private TextMeshProUGUI specialRewardCountText;    // "?/?" �ؽ�Ʈ
    [SerializeField] private Button specialRewardButton;                // Special Reward ��ư
    [SerializeField] private TextMeshProUGUI specialRewardPlusCashText; // Special Reward ���� ��ȭ ���� Text
    [SerializeField] private TextMeshProUGUI specialRewardText;         // Special Reward ���� ȹ�� Text
    [SerializeField] private GameObject specialRewardDisabledBG;        // Special Reward ���� ��ư ��Ȱ��ȭ BG
    private int specialRewardTargetCount;                               // ��ǥ Ƚ��
    private int specialRewardQuestRewardCash = 500;                     // Special Reward ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isSpecialRewardQuestComplete;                          // Special Reward ����Ʈ �Ϸ� ���� ����

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject questButtonNewImage;            // Main ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject dailyQuestButtonNewImage;       // Daily ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject weeklyQuestButtonNewImage;      // Weekly ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject repeatQuestButtonNewImage;      // Repeat ����Ʈ ��ư�� New Image

    [Header("---[All Reward Button]")]
    [SerializeField] private Button dailyAllRewardButton;               // Daily All RewardButton
    [SerializeField] private Button weeklyAllRewardButton;              // Weekly All RewardButton
    [SerializeField] private Button repeatAllRewardButton;              // Repeat All RewardButton

    [Header("---[Text UI Color]")]
    private string activeColorCode = "#5f5f5f";                         // Ȱ��ȭ���� Color
    private string inactiveColorCode = "#FFFFFF";                       // ��Ȱ��ȭ���� Color

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

    private int specialRewardCount;                                     // ���� ����Ʈ ���� Ƚ��
    public int SpecialRewardCount { get => specialRewardCount; set => specialRewardCount = value; }

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
        UpdateAllRewardButtonState();
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
        InitializeQuest("PlayTime", 10, 5);
        InitializeQuest("Merge Cats", 1, 5);
        InitializeQuest("Spawn Cats", 1, 5);
        InitializeQuest("Purchase Cats", 1, 5);
        InitializeQuest("Battle", 1, 5);

        InitializeSpecialReward();

        // AllReward ��ư ���
        dailyAllRewardButton.onClick.AddListener(ReceiveAllRewards);
    }

    // Weekly Quest ���� �Լ�
    private void InitializeWeeklyQuestManager()
    {

    }

    // Repeat Quest ���� �Լ�
    private void InitializeRepeatQuestManager()
    {
        // �ʱ� ��ũ�� ��ġ �ʱ�ȭ
        repeatQuestScrollRects.verticalNormalizedPosition = 1f;
    }

    // ======================================================================================================================
    // [����Ʈ ����]

    // ����Ʈ �ʱ�ȭ
    private void InitializeQuest(string questName, int targetCount, int rewardCash)
    {
        // Quest Slot ����
        GameObject newQuestSlot = Instantiate(questSlotPrefab, questSlotParent);

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
        questUI.rewardButton.onClick.AddListener(() => ReceiveQuestReward(questName));

        // Dictionary�� ���
        dailyQuestDictionary[questName] = questUI;

        // UI ������Ʈ
        UpdateQuestUI(questName);
    }

    // ����Ʈ UI ������Ʈ
    private void UpdateQuestUI(string questName)
    {
        if (!dailyQuestDictionary.ContainsKey(questName)) return;

        QuestUI questUI = dailyQuestDictionary[questName];
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
        if (!dailyQuestDictionary.ContainsKey(questName)) return;

        QuestUI questUI = dailyQuestDictionary[questName];
        QuestUI.QuestData questData = questUI.questData;

        // ���൵ ������Ʈ
        questData.currentCount = Mathf.Min(questData.currentCount, questData.targetCount);

        // UI ������Ʈ
        UpdateQuestUI(questName);
    }

    // ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveQuestReward(string questName)
    {
        if (!dailyQuestDictionary.ContainsKey(questName)) return;

        QuestUI questUI = dailyQuestDictionary[questName];
        QuestUI.QuestData questData = questUI.questData;

        if (!questUI.rewardButton.interactable || questData.isComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref questData.isComplete, questData.rewardCash, questUI.rewardButton, questUI.rewardDisabledBG);

        // UI ������Ʈ
        UpdateQuestUI(questName);
    }

    // ======================================================================================================================

    // ��� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateQuestUI()
    {
        UpdateNewImageStatus();

        UpdateDailyQuestUI();
        UpdateWeeklyQuestUI();
        UpdateRepeatQuestUI();
    }

    // Daily Quest UI ������Ʈ �Լ�
    private void UpdateDailyQuestUI()
    {
        UpdateQuestProgress("PlayTime");
        UpdateQuestProgress("Merge Cats");
        UpdateQuestProgress("Spawn Cats");
        UpdateQuestProgress("Purchase Cats");
        UpdateQuestProgress("Battle");

        UpdateSpecialRewardUI();
    }

    // Weekly Quest UI ������Ʈ �Լ�
    private void UpdateWeeklyQuestUI()
    {

    }

    // Repeat Quest UI ������Ʈ �Լ�
    private void UpdateRepeatQuestUI()
    {

    }

    // ĳ�� �߰� �Լ�
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    // ======================================================================================================================
    // [PlayTime Quest]

    // �÷���Ÿ�� ���� �Լ�
    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
        dailyQuestDictionary["PlayTime"].questData.currentCount = (int)PlayTimeCount;
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
    // [Special Reward Quest]

    // Special Reward Quest �ʱ� ���� �Լ�
    private void InitializeSpecialReward()
    {
        specialRewardTargetCount = dailyQuestDictionary.Count;

        specialRewardButton.onClick.AddListener(ReceiveSpecialReward);
        isSpecialRewardQuestComplete = false;
        specialRewardButton.interactable = false;
        specialRewardDisabledBG.SetActive(true);
        specialRewardPlusCashText.text = $"x {specialRewardQuestRewardCash}";
        specialRewardText.text = "Accept";
    }

    // Special Reward ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateSpecialRewardUI()
    {
        int currentCount = Mathf.Min((int)SpecialRewardCount, specialRewardTargetCount);

        // Slider �� ����
        specialRewardQuestSlider.maxValue = specialRewardTargetCount;
        specialRewardQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        specialRewardCountText.text = $"{currentCount} / {specialRewardTargetCount}";
        if (isSpecialRewardQuestComplete)
        {
            specialRewardText.text = "Complete";
        }

        bool isComplete = AllQuestsCompleted() && !isSpecialRewardQuestComplete;
        specialRewardButton.interactable = isComplete;
        specialRewardDisabledBG.SetActive(!isComplete);
    }

    // Special Reward ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveSpecialReward()
    {
        if (!specialRewardButton.interactable || isSpecialRewardQuestComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        AddCash(specialRewardQuestRewardCash);
        isSpecialRewardQuestComplete = true;
    }

    // Special Reward ���� �Լ�
    public void AddSpecialRewardCount()
    {
        SpecialRewardCount++;
    }

    // Special Reward ���� �Լ�
    public void ResetSpecialRewardCount()
    {
        SpecialRewardCount = 0;
    }

    // ��� ����Ʈ�� �Ϸ�Ǿ����� Ȯ���ϴ� �Լ�
    private bool AllQuestsCompleted()
    {
        foreach (var quest in dailyQuestDictionary)
        {
            if (quest.Value.questData.isComplete)
            {
                continue;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    // ======================================================================================================================
    // [��ü ���� �ޱ� ����]

    // ��� Ȱ��ȭ�� ������ �����ϴ� �Լ�
    private void ReceiveAllRewards()
    {
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref dailyQuest.Value.questData.isComplete, dailyQuest.Value.questData.rewardCash,
                    dailyQuest.Value.rewardButton, dailyQuest.Value.rewardDisabledBG);
            }
        }

        // ����� ���� ����
        if (specialRewardButton.interactable && !isSpecialRewardQuestComplete)
        {
            ReceiveSpecialReward();
        }
    }

    // ���� ����Ʈ ���� ���� ó�� �Լ�
    private void ReceiveQuestReward(ref bool isQuestComplete, int rewardCash, Button rewardButton, GameObject disabledBG)
    {
        isQuestComplete = true;
        AddCash(rewardCash);
        rewardButton.interactable = false;
        disabledBG.SetActive(true);

        AddSpecialRewardCount();
    }

    // All Reward ��ư ���¸� ������Ʈ�ϴ� �Լ�
    private void UpdateAllRewardButtonState()
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
        if (specialRewardButton.interactable && !isSpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
        }

        dailyAllRewardButton.interactable = isAnyRewardAvailable;
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

        hasActiveQuestReward = hasActiveQuestReward || specialRewardButton.interactable;

        return hasActiveQuestReward;
    }

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ� (Weekly)
    public bool HasUnclaimedWeeklyRewards()
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

        hasActiveQuestReward = hasActiveQuestReward || specialRewardButton.interactable;

        return hasActiveQuestReward;
    }

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ� (Repeat)
    public bool HasUnclaimedRepeatRewards()
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

        hasActiveQuestReward = hasActiveQuestReward || specialRewardButton.interactable;

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
        //weeklyQuestButtonNewImage.SetActive(hasUnclaimedWeeklyRewards);

        // Repeat Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        //repeatQuestButtonNewImage.SetActive(hasUnclaimedRepeatRewards);
    }


}
