using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class QuestUI
{
    public Slider questSlider;                  // Slider
    public TextMeshProUGUI countText;           // "?/?" �ؽ�Ʈ
    public TextMeshProUGUI plusCashText;        // ���� ��ȭ ���� Text
    public Button rewardButton;                 // ���� ��ư
    public TextMeshProUGUI rewardText;          // ���� ȹ�� Text
    public GameObject rewardDisabledBG;         // ���� ��ư ��Ȱ��ȭ BG

    public class QuestData
    {
        public int currentCount;  // ���� ��ġ
        public int targetCount;   // ��ǥ ��ġ
        public int rewardCash;    // ���� ĳ��
        public bool isComplete;   // �Ϸ� ����
    }
    public QuestData questData = new QuestData();
}

// ����Ʈ Script
public class QuestManager : MonoBehaviour
{
    // Singleton Instance
    public static QuestManager Instance { get; private set; }

    [Header("---[QuestManager]")]
    [SerializeField] private Button questButton;                        // ����Ʈ ��ư
    [SerializeField] private Image questButtonImage;                    // ����Ʈ ��ư �̹���
    [SerializeField] private GameObject questMenuPanel;                 // ����Ʈ �޴� Panel
    [SerializeField] private Button questBackButton;                    // ����Ʈ �ڷΰ��� ��ư
    private ActivePanelManager activePanelManager;                      // ActivePanelManager

    [SerializeField] private GameObject[] questMenus;                   // ���� ����Ʈ �޴� Panels
    [SerializeField] private Button[] questMenuButtons;                 // ���� ����Ʈ �޴� ��ư �迭

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject questButtonNewImage;            // Main ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject dailyQuestButtonNewImage;       // Daily ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject weeklyQuestButtonNewImage;      // Weekly ����Ʈ ��ư�� New Image
    [SerializeField] private GameObject repeatQuestButtonNewImage;      // Repeat ����Ʈ ��ư�� New Image

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
    // [Quest]

    [Header("---[Quest UI & Data]")]
    public QuestUI playTimeQuestUI;         // PlayTime Quest
    public QuestUI mergeQuestUI;            // Merge Quest
    public QuestUI spawnQuestUI;            // Spawn Quest
    public QuestUI purchaseCatsQuestUI;     // Purchase Cats Quest
    public QuestUI battleQuestUI;           // Battle Quest
    private Dictionary<string, QuestUI.QuestData> quests = new Dictionary<string, QuestUI.QuestData>();

    [Header("---[All Reward Button]")]
    [SerializeField] private Button dailyAllRewardButton;               // Daily All RewardButton
    [SerializeField] private Button weeklyAllRewardButton;              // Weekly All RewardButton
    [SerializeField] private Button repeatAllRewardButton;              // Repeat All RewardButton

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

    [Header("---[Text UI Color]")]
    private string activeColorCode = "#5f5f5f";                         // Ȱ��ȭ���� Color
    private string inactiveColorCode = "#FFFFFF";                       // ��Ȱ��ȭ���� Color

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
        //InitializeWeeklyQuestManager();
        //InitializeRepeatQuestManager();
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
        InitializePlayTimeQuest();
        InitializeMergeQuest();
        InitializeSpawnQuest();
        InitializePurchaseCatsQuest();
        InitializeBattleQuest();

        InitializeSpecialReward();

        InitializeAllRewardButton();
    }

    // AllRewardButton ���� �Լ�
    private void InitializeAllRewardButton()
    {
        dailyAllRewardButton.onClick.AddListener(ReceiveAllRewards);
    }

    // ======================================================================================================================

    // ��� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateQuestUI()
    {
        UpdateNewImageStatus();

        UpdateDailyQuestUI();
        //UpdateWeeklyQuestManager();
        //UpdateRepeatQuestManager();
    }

    // Daily Quest UI ������Ʈ �Լ�
    private void UpdateDailyQuestUI()
    {
        UpdatePlayTimeQuestUI();
        UpdateMergeQuestUI();
        UpdateSpawnQuestUI();
        UpdatePurchaseCatsQuestUI();
        UpdateBattleQuestUI();

        UpdateSpecialRewardUI();
    }

    // ĳ�� �߰� �Լ�
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    // ======================================================================================================================
    // [PlayTime Quest]

    // �÷���Ÿ�� ����Ʈ �ʱ� ���� �Լ�
    private void InitializePlayTimeQuest()
    {
        // Dictionary�� �ʱⰪ ����
        quests["PlayTime"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 10,
            rewardCash = 5,
            isComplete = false
        };

        // �� ����
        playTimeQuestUI.questData = quests["PlayTime"];

        // ���� ��ư ����
        playTimeQuestUI.rewardButton.onClick.AddListener(ReceivePlayTimeReward);
        playTimeQuestUI.rewardButton.interactable = false;
        playTimeQuestUI.rewardDisabledBG.SetActive(false);
        playTimeQuestUI.plusCashText.text = $"x {playTimeQuestUI.questData.rewardCash}";
        playTimeQuestUI.rewardText.text = "Accept";
    }

    // �÷���Ÿ�� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdatePlayTimeQuestUI()
    {
        playTimeQuestUI.questData.currentCount = Mathf.Min((int)PlayTimeCount, playTimeQuestUI.questData.targetCount);

        // Slider �� ����
        playTimeQuestUI.questSlider.maxValue = playTimeQuestUI.questData.targetCount;
        playTimeQuestUI.questSlider.value = playTimeQuestUI.questData.currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        playTimeQuestUI.countText.text = $"{playTimeQuestUI.questData.currentCount} / {playTimeQuestUI.questData.targetCount}";
        if (playTimeQuestUI.questData.isComplete)
        {
            playTimeQuestUI.rewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = playTimeQuestUI.questData.currentCount >= playTimeQuestUI.questData.targetCount && !playTimeQuestUI.questData.isComplete;
        playTimeQuestUI.rewardButton.interactable = isComplete;
        playTimeQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // �÷���Ÿ�� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceivePlayTimeReward()
    {
        if (!playTimeQuestUI.rewardButton.interactable || playTimeQuestUI.questData.isComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref playTimeQuestUI.questData.isComplete, playTimeQuestUI.questData.rewardCash, 
            playTimeQuestUI.rewardButton, playTimeQuestUI.rewardDisabledBG);
    }

    // �÷���Ÿ�� ���� �Լ�
    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
    }

    // �÷���Ÿ�� ���� �Լ�
    public void ResetPlayTimeCount()
    {
        PlayTimeCount = 0;
    }

    // ======================================================================================================================
    // [Merge Quest]

    // ����� ���� ����Ʈ �ʱ� ���� �Լ�
    private void InitializeMergeQuest()
    {
        // Dictionary�� �ʱⰪ ����
        quests["Merge"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 1,
            rewardCash = 5,
            isComplete = false
        };

        // �� ����
        mergeQuestUI.questData = quests["Merge"];

        // ���� ��ư ����
        mergeQuestUI.rewardButton.onClick.AddListener(ReceiveMergeReward);
        mergeQuestUI.rewardButton.interactable = false;
        mergeQuestUI.rewardDisabledBG.SetActive(false);
        mergeQuestUI.plusCashText.text = $"x {mergeQuestUI.questData.rewardCash}";
        mergeQuestUI.rewardText.text = "Accept";
    }

    // ����� ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateMergeQuestUI()
    {
        mergeQuestUI.questData.currentCount = Mathf.Min((int)MergeCount, mergeQuestUI.questData.targetCount);

        // Slider �� ����
        mergeQuestUI.questSlider.maxValue = mergeQuestUI.questData.targetCount;
        mergeQuestUI.questSlider.value = mergeQuestUI.questData.currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        mergeQuestUI.countText.text = $"{mergeQuestUI.questData.currentCount} / {mergeQuestUI.questData.targetCount}";
        if (mergeQuestUI.questData.isComplete)
        {
            mergeQuestUI.rewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = mergeQuestUI.questData.currentCount >= mergeQuestUI.questData.targetCount && !mergeQuestUI.questData.isComplete;
        mergeQuestUI.rewardButton.interactable = isComplete;
        mergeQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // ����� ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveMergeReward()
    {
        if (!mergeQuestUI.rewardButton.interactable || mergeQuestUI.questData.isComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref mergeQuestUI.questData.isComplete, mergeQuestUI.questData.rewardCash,
            mergeQuestUI.rewardButton, mergeQuestUI.rewardDisabledBG);
    }

    // ����� ���� Ƚ�� ���� �Լ�
    public void AddMergeCount()
    {
        MergeCount++;
    }

    // ����� ���� Ƚ�� ���� �Լ�
    public void ResetMergeCount()
    {
        MergeCount = 0;
    }

    // ======================================================================================================================
    // [Spawn Quest]

    // ����� ���� ����Ʈ �ʱ� ���� �Լ�
    private void InitializeSpawnQuest()
    {
        // Dictionary�� �ʱⰪ ����
        quests["Spawn"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 1,
            rewardCash = 5,
            isComplete = false
        };

        // �� ����
        spawnQuestUI.questData = quests["Spawn"];

        // ���� ��ư ����
        spawnQuestUI.rewardButton.onClick.AddListener(ReceiveSpawnReward);
        spawnQuestUI.rewardButton.interactable = false;
        spawnQuestUI.rewardDisabledBG.SetActive(false);
        spawnQuestUI.plusCashText.text = $"x {spawnQuestUI.questData.rewardCash}";
        spawnQuestUI.rewardText.text = "Accept";
    }

    // ����� ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateSpawnQuestUI()
    {
        spawnQuestUI.questData.currentCount = Mathf.Min((int)SpawnCount, spawnQuestUI.questData.targetCount);

        // Slider �� ����
        spawnQuestUI.questSlider.maxValue = spawnQuestUI.questData.targetCount;
        spawnQuestUI.questSlider.value = spawnQuestUI.questData.currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        spawnQuestUI.countText.text = $"{spawnQuestUI.questData.currentCount} / {spawnQuestUI.questData.targetCount}";
        if (spawnQuestUI.questData.isComplete)
        {
            spawnQuestUI.rewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = spawnQuestUI.questData.currentCount >= spawnQuestUI.questData.targetCount && !spawnQuestUI.questData.isComplete;
        spawnQuestUI.rewardButton.interactable = isComplete;
        spawnQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // ����� ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveSpawnReward()
    {
        if (!spawnQuestUI.rewardButton.interactable || spawnQuestUI.questData.isComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref spawnQuestUI.questData.isComplete, spawnQuestUI.questData.rewardCash,
            spawnQuestUI.rewardButton, spawnQuestUI.rewardDisabledBG);
    }

    // ����� ���� Ƚ�� ���� �Լ�
    public void AddSpawnCount()
    {
        SpawnCount++;
    }

    // ����� ���� Ƚ�� ���� �Լ�
    public void ResetSpawnCount()
    {
        SpawnCount = 0;
    }

    // ======================================================================================================================
    // [Purchase Cats Quest]

    // ����� ���� ����Ʈ �ʱ� ���� �Լ�
    private void InitializePurchaseCatsQuest()
    {
        // Dictionary�� �ʱⰪ ����
        quests["PurchaseCats"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 1,
            rewardCash = 5,
            isComplete = false
        };

        // �� ����
        purchaseCatsQuestUI.questData = quests["PurchaseCats"];

        // ���� ��ư ����
        purchaseCatsQuestUI.rewardButton.onClick.AddListener(ReceivePurchaseCatsReward);
        purchaseCatsQuestUI.rewardButton.interactable = false;
        purchaseCatsQuestUI.rewardDisabledBG.SetActive(false);
        purchaseCatsQuestUI.plusCashText.text = $"x {purchaseCatsQuestUI.questData.rewardCash}";
        purchaseCatsQuestUI.rewardText.text = "Accept";
    }

    // ����� ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdatePurchaseCatsQuestUI()
    {
        purchaseCatsQuestUI.questData.currentCount = Mathf.Min((int)PurchaseCatsCount, purchaseCatsQuestUI.questData.targetCount);

        // Slider �� ����
        purchaseCatsQuestUI.questSlider.maxValue = purchaseCatsQuestUI.questData.targetCount;
        purchaseCatsQuestUI.questSlider.value = purchaseCatsQuestUI.questData.currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        purchaseCatsQuestUI.countText.text = $"{purchaseCatsQuestUI.questData.currentCount} / {purchaseCatsQuestUI.questData.targetCount}";
        if (purchaseCatsQuestUI.questData.isComplete)
        {
            purchaseCatsQuestUI.rewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = purchaseCatsQuestUI.questData.currentCount >= purchaseCatsQuestUI.questData.targetCount && !purchaseCatsQuestUI.questData.isComplete;
        purchaseCatsQuestUI.rewardButton.interactable = isComplete;
        purchaseCatsQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // ����� ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceivePurchaseCatsReward()
    {
        if (!purchaseCatsQuestUI.rewardButton.interactable || purchaseCatsQuestUI.questData.isComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref purchaseCatsQuestUI.questData.isComplete, purchaseCatsQuestUI.questData.rewardCash,
            purchaseCatsQuestUI.rewardButton, purchaseCatsQuestUI.rewardDisabledBG);
    }

    // ����� ���� Ƚ�� ���� �Լ�
    public void AddPurchaseCatsCount()
    {
        PurchaseCatsCount++;
    }

    // ����� ���� Ƚ�� ���� �Լ�
    public void ResetPurchaseCatsCount()
    {
        PurchaseCatsCount = 0;
    }

    // ======================================================================================================================
    // [Battle Count Quest]

    // ��Ʋ ����Ʈ �ʱ� ���� �Լ�
    private void InitializeBattleQuest()
    {
        // Dictionary�� �ʱⰪ ����
        quests["Battle"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 1,
            rewardCash = 5,
            isComplete = false
        };

        // �� ����
        battleQuestUI.questData = quests["Battle"];

        // ���� ��ư ����
        battleQuestUI.rewardButton.onClick.AddListener(ReceiveBattleReward);
        battleQuestUI.rewardButton.interactable = false;
        battleQuestUI.rewardDisabledBG.SetActive(false);
        battleQuestUI.plusCashText.text = $"x {battleQuestUI.questData.rewardCash}";
        battleQuestUI.rewardText.text = "Accept";
    }

    // ��Ʋ ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateBattleQuestUI()
    {
        battleQuestUI.questData.currentCount = Mathf.Min((int)BattleCount, battleQuestUI.questData.targetCount);

        // Slider �� ����
        battleQuestUI.questSlider.maxValue = battleQuestUI.questData.targetCount;
        battleQuestUI.questSlider.value = battleQuestUI.questData.currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        battleQuestUI.countText.text = $"{battleQuestUI.questData.currentCount} / {battleQuestUI.questData.targetCount}";
        if (battleQuestUI.questData.isComplete)
        {
            battleQuestUI.rewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = battleQuestUI.questData.currentCount >= battleQuestUI.questData.targetCount && !battleQuestUI.questData.isComplete;
        battleQuestUI.rewardButton.interactable = isComplete;
        battleQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // ��Ʋ ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveBattleReward()
    {
        if (!battleQuestUI.rewardButton.interactable || battleQuestUI.questData.isComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref battleQuestUI.questData.isComplete, battleQuestUI.questData.rewardCash,
            battleQuestUI.rewardButton, battleQuestUI.rewardDisabledBG);
    }

    // ��Ʋ Ƚ�� ���� �Լ�
    public void AddBattleCount()
    {
        BattleCount++;
    }

    // ��Ʋ Ƚ�� ���� �Լ�
    public void ResetBattleCount()
    {
        BattleCount = 0;
    }

    // ======================================================================================================================
    // [Special Reward Quest]

    // Special Reward Quest �ʱ� ���� �Լ�
    private void InitializeSpecialReward()
    {
        specialRewardTargetCount = quests.Count;

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
        foreach (var quest in quests)
        {
            if (quest.Value.isComplete)
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
        //foreach (var quest in quests)
        //{
        //    if (quest.Value.rewardButton.interactable && !quest.Value.questData.isComplete)
        //    {
        //        ReceiveQuestReward(ref quest.Value.questData.isComplete, quest.Value.questData.rewardCash,
        //            quest.Value.rewardButton, quest.Value.rewardDisabledBG);
        //    }
        //}

        // �÷���Ÿ�� ����Ʈ ���� ó��
        if (playTimeQuestUI.rewardButton.interactable && !playTimeQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref playTimeQuestUI.questData.isComplete, playTimeQuestUI.questData.rewardCash,
                playTimeQuestUI.rewardButton, playTimeQuestUI.rewardDisabledBG);
        }

        // ����� ���� ����Ʈ ���� ó��
        if (mergeQuestUI.rewardButton.interactable && !mergeQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref mergeQuestUI.questData.isComplete, mergeQuestUI.questData.rewardCash,
                mergeQuestUI.rewardButton, mergeQuestUI.rewardDisabledBG);
        }

        // ����� ���� ����Ʈ ���� ó��
        if (spawnQuestUI.rewardButton.interactable && !spawnQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref spawnQuestUI.questData.isComplete, spawnQuestUI.questData.rewardCash,
                spawnQuestUI.rewardButton, spawnQuestUI.rewardDisabledBG);
        }

        // ����� ���� ����Ʈ ���� ó��
        if (purchaseCatsQuestUI.rewardButton.interactable && !purchaseCatsQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref purchaseCatsQuestUI.questData.isComplete, purchaseCatsQuestUI.questData.rewardCash,
                purchaseCatsQuestUI.rewardButton, purchaseCatsQuestUI.rewardDisabledBG);
        }

        // ���� ����Ʈ ���� ó��
        if (battleQuestUI.rewardButton.interactable && !battleQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref battleQuestUI.questData.isComplete, battleQuestUI.questData.rewardCash,
                battleQuestUI.rewardButton, battleQuestUI.rewardDisabledBG);
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
        bool isAnyRewardAvailable =
            (playTimeQuestUI.rewardButton.interactable && !playTimeQuestUI.questData.isComplete) ||
            (mergeQuestUI.rewardButton.interactable && !mergeQuestUI.questData.isComplete) ||
            (spawnQuestUI.rewardButton.interactable && !spawnQuestUI.questData.isComplete) ||
            (purchaseCatsQuestUI.rewardButton.interactable && !purchaseCatsQuestUI.questData.isComplete) ||
            (battleQuestUI.rewardButton.interactable && !battleQuestUI.questData.isComplete);

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
            questMenuButtons[index].onClick.AddListener(() => ActivateMenu((QuestMenuType)index));
        }

        ActivateMenu(QuestMenuType.Daily);
    }

    // ������ ���� �޴��� Ȱ��ȭ�ϴ� �Լ�
    private void ActivateMenu(QuestMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < questMenus.Length; i++)
        {
            questMenus[i].SetActive(i == (int)menuType);
        }

        UpdateSubMenuButtonColors();
    }

    // ���� �޴� ��ư ������ ������Ʈ�ϴ� �Լ�
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < questMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(questMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
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

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ�
    public bool HasUnclaimedRewards()
    {
        bool hasActiveReward =
            playTimeQuestUI.rewardButton.interactable ||
            mergeQuestUI.rewardButton.interactable ||
            spawnQuestUI.rewardButton.interactable ||
            purchaseCatsQuestUI.rewardButton.interactable ||
            battleQuestUI.rewardButton.interactable ||
            specialRewardButton.interactable;

        return hasActiveReward;
    }

    // New Image�� ���¸� Update�ϴ� �Լ�
    private void UpdateNewImageStatus()
    {
        bool hasUnclaimedRewards = HasUnclaimedRewards();

        // Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        questButtonNewImage.SetActive(hasUnclaimedRewards);

        // Daily Quest Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        dailyQuestButtonNewImage.SetActive(hasUnclaimedRewards);
    }


}
