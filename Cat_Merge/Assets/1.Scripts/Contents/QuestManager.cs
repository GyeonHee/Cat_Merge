using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    [SerializeField] private GameObject[] questMenus;                   // ����Ʈ �޴� Panels
    [SerializeField] private Button[] questMenuButtons;                 // ����Ʈ ���� �޴� ��ư �迭

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject newImage;                       // ����Ʈ ��ư�� New Image 

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

    private int specialRewardCount;                                    // ���� ����Ʈ ���� Ƚ��
    public int SpecialRewardCount { get => specialRewardCount; set => specialRewardCount = value; }

    // ======================================================================================================================

    [Header("---[PlayTime Quest UI]")]
    [SerializeField] private Slider playTimeQuestSlider;                // �÷���Ÿ�� Slider
    [SerializeField] private TextMeshProUGUI playTimeCountText;         // "?/?" �ؽ�Ʈ
    [SerializeField] private Button playTimeRewardButton;               // �÷���Ÿ�� ���� ��ư
    [SerializeField] private TextMeshProUGUI playTimePlusCashText;      // �÷���Ÿ�� ���� ��ȭ ���� Text
    [SerializeField] private TextMeshProUGUI playTimeRewardText;        // �÷���Ÿ�� ���� ȹ�� Text
    [SerializeField] private GameObject playTimeRewardDisabledBG;       // �÷���Ÿ�� ���� ��ư ��Ȱ��ȭ BG
    private int playTimeTargetCount = 10;                               // ��ǥ �÷���Ÿ�� (�� ����)                 == 720
    private int playTimeQuestRewardCash = 5;                            // �÷���Ÿ�� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isPlayTimeQuestComplete;                               // �÷���Ÿ�� ����Ʈ �Ϸ� ����

    [Header("---[Merge Cats Quest UI]")]
    [SerializeField] private Slider mergeQuestSlider;                   // ����� ���� Slider
    [SerializeField] private TextMeshProUGUI mergeCountText;            // "?/?" �ؽ�Ʈ
    [SerializeField] private Button mergeRewardButton;                  // ����� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI mergePlusCashText;         // ����� ���� ���� ��ȭ ���� Text
    [SerializeField] private TextMeshProUGUI mergeRewardText;           // ����� ���� ���� ȹ�� Text
    [SerializeField] private GameObject mergeRewardDisabledBG;          // ����� ���� ���� ��ư ��Ȱ��ȭ BG
    private int mergeTargetCount = 1;                                   // ��ǥ ����� ���� Ƚ��                     == 100
    private int mergeQuestRewardCash = 5;                               // ����� ���� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isMergeQuestComplete;                                  // ����� ���� ����Ʈ �Ϸ� ����

    [Header("---[Spawn Cats Quest UI]")]
    [SerializeField] private Slider spawnQuestSlider;                   // ����� ���� Slider
    [SerializeField] private TextMeshProUGUI spawnCountText;            // "?/?" �ؽ�Ʈ
    [SerializeField] private Button spawnRewardButton;                  // ����� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI spawnPlusCashText;         // ����� ���� ���� ��ȭ ���� Text
    [SerializeField] private TextMeshProUGUI spawnRewardText;           // ����� ���� ���� ȹ�� Text
    [SerializeField] private GameObject spawnRewardDisabledBG;          // ����� ���� ���� ��ư ��Ȱ��ȭ BG
    private int spawnTargetCount = 1;                                   // ��ǥ ����� ���� Ƚ��                     == 100
    private int spawnQuestRewardCash = 5;                               // ����� ���� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isSpawnQuestComplete;                                  // ����� ���� ����Ʈ �Ϸ� ����

    [Header("---[Purchase Cats Quest UI]")]
    [SerializeField] private Slider purchaseCatsQuestSlider;            // ����� ���� Slider
    [SerializeField] private TextMeshProUGUI purchaseCatsCountText;     // "?/?" �ؽ�Ʈ
    [SerializeField] private Button purchaseCatsRewardButton;           // ����� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI purchaseCatsPlusCashText;  // ����� ���� ���� ��ȭ ���� Text
    [SerializeField] private TextMeshProUGUI purchaseCatsRewardText;    // ����� ���� ���� ȹ�� Text
    [SerializeField] private GameObject purchaseCatsRewardDisabledBG;   // ����� ���� ���� ��ư ��Ȱ��ȭ BG
    private int purchaseCatsTargetCount = 1;                            // ��ǥ ����� ���� Ƚ��                     == 10
    private int purchaseCatsQuestRewardCash = 5;                        // ����� ���� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isPurchaseCatsQuestComplete;                           // ����� ���� ����Ʈ �Ϸ� ����

    [Header("---[Battle Quest UI]")]
    [SerializeField] private Slider battleQuestSlider;                  // ���� Slider
    [SerializeField] private TextMeshProUGUI battleCountText;           // "?/?" �ؽ�Ʈ
    [SerializeField] private Button battleRewardButton;                 // ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI battlePlusCashText;        // ���� ���� ��ȭ ���� Text
    [SerializeField] private TextMeshProUGUI battleRewardText;          // ���� ���� ȹ�� Text
    [SerializeField] private GameObject battleRewardDisabledBG;         // ���� ���� ��ư ��Ȱ��ȭ BG
    private int battleTargetCount = 1;                                  // ��ǥ ���� Ƚ��                           == 1
    private int battleQuestRewardCash = 5;                              // ���� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isBattleQuestComplete;                                 // ���� ����Ʈ �Ϸ� ����



    [Header("---[All Reward Button]")]
    [SerializeField] private Button dailyAllRewardButton;               // Daily All RewardButton

    [Header("---[Special Reward UI]")]
    [SerializeField] private Slider specialRewardQuestSlider;           // Special Reward Slider
    [SerializeField] private TextMeshProUGUI specialRewardCountText;    // "?/?" �ؽ�Ʈ
    [SerializeField] private Button specialRewardButton;                // Special Reward ��ư
    [SerializeField] private TextMeshProUGUI specialRewardPlusCashText; // Special Reward ���� ��ȭ ���� Text
    [SerializeField] private TextMeshProUGUI specialRewardText;         // Special Reward ���� ȹ�� Text
    [SerializeField] private GameObject specialRewardDisabledBG;        // Special Reward ���� ��ư ��Ȱ��ȭ BG
    private int specialRewardTargetCount = 5;                           // ��ǥ Ƚ��
    private int specialRewardQuestRewardCash = 500;                     // Special Reward ����Ʈ ���� ĳ�� ��ȭ ���� == 500
    private bool isSpecialRewardQuestComplete;                          // Special Reward ����Ʈ �Ϸ� ���� ����

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
        newImage.SetActive(false);
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

        InitializePlayTimeQuest();
        InitializeMergeQuest();
        InitializeSpawnQuest();
        InitializePurchaseCatsQuest();
        InitializeBattleQuest();

        InitializeSpecialReward();

        InitializeAllRewardButton();
    }

    // ======================================================================================================================

    // ��� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateQuestUI()
    {
        UpdatePlayTimeQuestUI();
        UpdateMergeQuestUI();
        UpdateSpawnQuestUI();
        UpdatePurchaseCatsQuestUI();
        UpdateBattleQuestUI();

        UpdateSpecialRewardUI();

        UpdateNewImageStatus();
    }

    // New Image ���¸� ������Ʈ�ϴ� �Լ�
    private void UpdateNewImageStatus()
    {
        bool hasActiveReward =
            playTimeRewardButton.interactable ||
            mergeRewardButton.interactable ||
            spawnRewardButton.interactable ||
            purchaseCatsRewardButton.interactable ||
            battleRewardButton.interactable ||
            specialRewardButton.interactable;

        newImage.SetActive(hasActiveReward);
    }

    // QuestButton ���� �Լ�
    private void InitializeQuestButton()
    {
        questButton.onClick.AddListener(() => activePanelManager.TogglePanel("QuestMenu"));
        questBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("QuestMenu"));
    }

    // AllRewardButton ���� �Լ�
    private void InitializeAllRewardButton()
    {
        dailyAllRewardButton.onClick.AddListener(ReceiveAllRewards);
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
        playTimeRewardButton.onClick.AddListener(ReceivePlayTimeReward);
        isPlayTimeQuestComplete = false;
        playTimeRewardButton.interactable = false;
        playTimeRewardDisabledBG.SetActive(false);
        playTimePlusCashText.text = $"x {playTimeQuestRewardCash}";
        playTimeRewardText.text = "Accept";
    }

    // �÷���Ÿ�� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdatePlayTimeQuestUI()
    {
        // �ִ� �ð��� �ʰ����� �ʵ��� ����
        int currentTime = Mathf.Min((int)PlayTimeCount, playTimeTargetCount);

        // Slider �� ����
        playTimeQuestSlider.maxValue = playTimeTargetCount;
        playTimeQuestSlider.value = currentTime;

        // "?/?" �ؽ�Ʈ ������Ʈ
        playTimeCountText.text = $"{currentTime} / {playTimeTargetCount}";
        if (isPlayTimeQuestComplete)
        {
            playTimeRewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentTime >= playTimeTargetCount && !isPlayTimeQuestComplete;
        playTimeRewardButton.interactable = isComplete;
        playTimeRewardDisabledBG.SetActive(!isComplete);
    }

    // �÷���Ÿ�� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceivePlayTimeReward()
    {
        if (!playTimeRewardButton.interactable || isPlayTimeQuestComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref isPlayTimeQuestComplete, playTimeQuestRewardCash, playTimeRewardButton, playTimeRewardDisabledBG);
        //isPlayTimeQuestComplete = true;
        //AddCash(playTimeQuestRewardCash);
        //AddSpecialRewardCount();
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
        mergeRewardButton.onClick.AddListener(ReceiveMergeReward);
        isMergeQuestComplete = false;
        mergeRewardButton.interactable = false;
        mergeRewardDisabledBG.SetActive(false);
        mergePlusCashText.text = $"x {mergeQuestRewardCash}";
        mergeRewardText.text = "Accept";
    }

    // ����� ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateMergeQuestUI()
    {
        int currentCount = Mathf.Min((int)MergeCount, mergeTargetCount);

        // Slider �� ����
        mergeQuestSlider.maxValue = mergeTargetCount;
        mergeQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        mergeCountText.text = $"{currentCount} / {mergeTargetCount}";
        if (isMergeQuestComplete)
        {
            mergeRewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= mergeTargetCount && !isMergeQuestComplete;
        mergeRewardButton.interactable = isComplete;
        mergeRewardDisabledBG.SetActive(!isComplete);
    }

    // ����� ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveMergeReward()
    {
        if (!mergeRewardButton.interactable || isMergeQuestComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref isMergeQuestComplete, mergeQuestRewardCash, mergeRewardButton, mergeRewardDisabledBG);
        //isMergeQuestComplete = true;
        //AddCash(mergeQuestRewardCash);
        //AddSpecialRewardCount();
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
        spawnRewardButton.onClick.AddListener(ReceiveSpawnReward);
        isSpawnQuestComplete = false;
        spawnRewardButton.interactable = false;
        spawnRewardDisabledBG.SetActive(false);
        spawnPlusCashText.text = $"x {spawnQuestRewardCash}";
        spawnRewardText.text = "Accept";
    }

    // ����� ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateSpawnQuestUI()
    {
        int currentCount = Mathf.Min((int)SpawnCount, spawnTargetCount);

        // Slider �� ����
        spawnQuestSlider.maxValue = spawnTargetCount;
        spawnQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        spawnCountText.text = $"{currentCount} / {spawnTargetCount}";
        if (isSpawnQuestComplete)
        {
            spawnRewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= spawnTargetCount && !isSpawnQuestComplete;
        spawnRewardButton.interactable = isComplete;
        spawnRewardDisabledBG.SetActive(!isComplete);
    }

    // ����� ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveSpawnReward()
    {
        if (!spawnRewardButton.interactable || isSpawnQuestComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref isSpawnQuestComplete, spawnQuestRewardCash, spawnRewardButton, spawnRewardDisabledBG);
        //isSpawnQuestComplete = true;
        //AddCash(spawnQuestRewardCash);
        //AddSpecialRewardCount();
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
        purchaseCatsRewardButton.onClick.AddListener(ReceivePurchaseCatsReward);
        isPurchaseCatsQuestComplete = false;
        purchaseCatsRewardButton.interactable = false;
        purchaseCatsRewardDisabledBG.SetActive(false);
        purchaseCatsPlusCashText.text = $"x {purchaseCatsQuestRewardCash}";
        purchaseCatsRewardText.text = "Accept";
    }

    // ����� ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdatePurchaseCatsQuestUI()
    {
        int currentCount = Mathf.Min((int)PurchaseCatsCount, purchaseCatsTargetCount);

        // Slider �� ����
        purchaseCatsQuestSlider.maxValue = purchaseCatsTargetCount;
        purchaseCatsQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        purchaseCatsCountText.text = $"{currentCount} / {purchaseCatsTargetCount}";
        if (isPurchaseCatsQuestComplete)
        {
            purchaseCatsRewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= purchaseCatsTargetCount && !isPurchaseCatsQuestComplete;
        purchaseCatsRewardButton.interactable = isComplete;
        purchaseCatsRewardDisabledBG.SetActive(!isComplete);
    }

    // ����� ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceivePurchaseCatsReward()
    {
        if (!purchaseCatsRewardButton.interactable || isPurchaseCatsQuestComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref isPurchaseCatsQuestComplete, purchaseCatsQuestRewardCash, purchaseCatsRewardButton, purchaseCatsRewardDisabledBG);
        //AddCash(purchaseCatsQuestRewardCash);
        //isPurchaseCatsQuestComplete = true;
        //AddSpecialRewardCount();
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
        battleRewardButton.onClick.AddListener(ReceiveBattleReward);
        isBattleQuestComplete = false;
        battleRewardButton.interactable = false;
        battleRewardDisabledBG.SetActive(false);
        battlePlusCashText.text = $"x {battleQuestRewardCash}";
        battleRewardText.text = "Accept";
    }

    // ��Ʋ ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateBattleQuestUI()
    {
        int currentCount = Mathf.Min((int)BattleCount, battleTargetCount);

        // Slider �� ����
        battleQuestSlider.maxValue = battleTargetCount;
        battleQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        battleCountText.text = $"{currentCount} / {battleTargetCount}";
        if (isBattleQuestComplete)
        {
            battleRewardText.text = "Complete";
        }

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= battleTargetCount && !isBattleQuestComplete;
        battleRewardButton.interactable = isComplete;
        battleRewardDisabledBG.SetActive(!isComplete);
    }

    // ��Ʋ ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveBattleReward()
    {
        if (!battleRewardButton.interactable || isBattleQuestComplete) return;

        // ���� ���� ó�� & ����Ʈ �Ϸ� ó��
        ReceiveQuestReward(ref isBattleQuestComplete, battleQuestRewardCash, battleRewardButton, battleRewardDisabledBG);
        //isBattleQuestComplete = true;
        //AddCash(battleQuestRewardCash);
        //AddSpecialRewardCount();
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

        //// "?/?" �ؽ�Ʈ ������Ʈ
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
        if (isPlayTimeQuestComplete && isMergeQuestComplete && isSpawnQuestComplete && isPurchaseCatsQuestComplete && isBattleQuestComplete)
        {
            return true;
        }
        else
        {
            return false;
        }
    }




    // ======================================================================================================================
    // [��ü ���� �ޱ� ����]

    // ��� Ȱ��ȭ�� ������ �����ϴ� �Լ�
    private void ReceiveAllRewards()
    {
        //bool isAnyRewardGiven = false;

        // �÷���Ÿ�� ����Ʈ ���� ó��
        if (playTimeRewardButton.interactable && !isPlayTimeQuestComplete)
        {
            ReceiveQuestReward(ref isPlayTimeQuestComplete, playTimeQuestRewardCash, playTimeRewardButton, playTimeRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        // ����� ���� ����Ʈ ���� ó��
        if (mergeRewardButton.interactable && !isMergeQuestComplete)
        {
            ReceiveQuestReward(ref isMergeQuestComplete, mergeQuestRewardCash, mergeRewardButton, mergeRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        // ����� ���� ����Ʈ ���� ó��
        if (spawnRewardButton.interactable && !isSpawnQuestComplete)
        {
            ReceiveQuestReward(ref isSpawnQuestComplete, spawnQuestRewardCash, spawnRewardButton, spawnRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        // ����� ���� ����Ʈ ���� ó��
        if (purchaseCatsRewardButton.interactable && !isPurchaseCatsQuestComplete)
        {
            ReceiveQuestReward(ref isPurchaseCatsQuestComplete, purchaseCatsQuestRewardCash, purchaseCatsRewardButton, purchaseCatsRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        // ���� ����Ʈ ���� ó��
        if (battleRewardButton.interactable && !isBattleQuestComplete)
        {
            ReceiveQuestReward(ref isBattleQuestComplete, battleQuestRewardCash, battleRewardButton, battleRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        //// ������ ������ ��� UI ������Ʈ (Update���� �ٲ�� ����)
        //if (isAnyRewardGiven)
        //{
        //    UpdateQuestsUI();
        //}
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
            (playTimeRewardButton.interactable && !isPlayTimeQuestComplete) ||
            (mergeRewardButton.interactable && !isMergeQuestComplete) ||
            (spawnRewardButton.interactable && !isSpawnQuestComplete) ||
            (purchaseCatsRewardButton.interactable && !isPurchaseCatsQuestComplete) ||
            (battleRewardButton.interactable && !isBattleQuestComplete);

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
        string colorCode = isActive ? "#5f5f5f" : "#FFFFFF";
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }


}
