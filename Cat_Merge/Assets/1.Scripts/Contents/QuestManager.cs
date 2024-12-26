using UnityEngine;
using TMPro;
using UnityEngine.UI;

// ����Ʈ Script
public class QuestManager : MonoBehaviour
{
    // Singleton Instance
    public static QuestManager Instance { get; private set; }

    [Header("---[QuestManager]")]
    [SerializeField] private ScrollRect questScrollRect;                // ����Ʈ�� ��ũ�Ѻ�
    [SerializeField] private Button questButton;                        // ����Ʈ ��ư
    [SerializeField] private Image questButtonImage;                    // ����Ʈ ��ư �̹���
    [SerializeField] private GameObject questMenuPanel;                 // ����Ʈ �޴� Panel
    [SerializeField] private Button questBackButton;                    // ����Ʈ �ڷΰ��� ��ư
    private ActivePanelManager activePanelManager;                      // ActivePanelManager

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject newImage;                       // ����Ʈ Button�� New Image 

    // ======================================================================================================================

    private int feedCount;                                              // ����� ���� Ƚ��(���� �� Ƚ��)
    public int FeedCount { get => feedCount; set => feedCount = value; }

    private int combineCount;                                           // ����� ���� Ƚ��
    public int CombineCount { get => combineCount; set => combineCount = value; }

    private int getCoinCount;                                           // ȹ������ ����
    public int GetCoinCount { get => getCoinCount; set => getCoinCount = value; }

    private float playTimeCount;                                        // �÷���Ÿ�� ī��Ʈ
    public float PlayTimeCount { get => playTimeCount; set => playTimeCount = value; }

    private int purchaseCatsCount;                                      // ����� ���� Ƚ��
    public int PurchaseCatsCount { get => purchaseCatsCount; set => purchaseCatsCount = value; }

    // ======================================================================================================================

    [Header("---[Give Feed Quest UI]")]
    [SerializeField] private Slider giveFeedQuestSlider;                // ����� ���� Slider
    [SerializeField] private TextMeshProUGUI giveFeedCountText;         // "?/?" �ؽ�Ʈ
    [SerializeField] private Button giveFeedRewardButton;               // ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI giveFeedPlusCashText;      // ���� ���� ��ȭ ���� Text
    [SerializeField] private GameObject giveFeedRewardDisabledBG;       // ���� ���� ��ư ��Ȱ��ȭ BG
    private int giveFeedTargetCount = 1;                                // ��ǥ ���� Ƚ��
    private int increaseGiveFeedTargetCount = 2;                        // ��ǥ ���� Ƚ�� ����ġ
    private int giveFeedQuestRewardCash = 5;                            // ���� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isGiveFeedQuestComplete = false;                       // ���� ����Ʈ �Ϸ� ����

    [Header("---[Combine Quest UI]")]
    [SerializeField] private Slider combineQuestSlider;                 // ���� Slider
    [SerializeField] private TextMeshProUGUI combineCountText;          // "?/?" �ؽ�Ʈ
    [SerializeField] private Button combineRewardButton;                // ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI combinePlusCashText;       // ���� ���� ��ȭ ���� Text
    [SerializeField] private GameObject combineRewardDisabledBG;        // ���� ���� ��ư ��Ȱ��ȭ BG
    private int combineTargetCount = 1;                                 // ��ǥ ���� Ƚ��
    private int increaseCombineTargetCount = 2;                         // ��ǥ ���� Ƚ�� ����ġ
    private int combineQuestRewardCash = 5;                             // ���� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isCombineQuestComplete = false;                        // ���� ����Ʈ �Ϸ� ����

    [Header("---[Cat GetCoin UI]")]
    [SerializeField] private Slider getCoinQuestSlider;                 // ȹ������ Slider
    [SerializeField] private TextMeshProUGUI getCoinCountText;          // "?/?" �ؽ�Ʈ
    [SerializeField] private Button getCoinRewardButton;                // ȹ������ ���� ��ư
    [SerializeField] private TextMeshProUGUI getCoinPlusCashText;       // ȹ������ ���� ��ȭ ���� Text
    [SerializeField] private GameObject getCoinRewardDisabledBG;        // ȹ������ ���� ��ư ��Ȱ��ȭ BG
    private int getCoinTargetCount = 1;                                 // ��ǥ ȹ������ Ƚ��
    private int increaseGetCoinTargetCount = 2;                         // ��ǥ ȹ������ ���� ����ġ
    private int getCoinQuestRewardCash = 5;                             // ȹ������ ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isGetCoinQuestComplete = false;                        // ȹ������ ����Ʈ �Ϸ� ����

    [Header("---[PlayTime Quest UI]")]
    [SerializeField] private Slider playTimeQuestSlider;                // �÷���Ÿ�� Slider
    [SerializeField] private TextMeshProUGUI playTimeCountText;         // "?/?" �ؽ�Ʈ
    [SerializeField] private Button playTimeRewardButton;               // �÷���Ÿ�� ���� ��ư
    [SerializeField] private TextMeshProUGUI playTimePlusCashText;      // �÷���Ÿ�� ���� ��ȭ ���� Text
    [SerializeField] private GameObject playTimeRewardDisabledBG;       // �÷���Ÿ�� ���� ��ư ��Ȱ��ȭ BG
    private int playTimeTargetCount = 10;                               // ��ǥ �÷���Ÿ�� (�� ����)
    private int increasePlayTimeTargetCount = 20;                       // ��ǥ �÷���Ÿ�� ����ġ
    private int playTimeQuestRewardCash = 5;                            // �÷���Ÿ�� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isPlayTimeQuestComplete = false;                       // �÷���Ÿ�� ����Ʈ �Ϸ� ����

    [Header("---[Purchase Cats Quest UI]")]
    [SerializeField] private Slider purchaseCatsQuestSlider;            // ����� ���� Slider
    [SerializeField] private TextMeshProUGUI purchaseCatsCountText;     // "?/?" �ؽ�Ʈ
    [SerializeField] private Button purchaseCatsRewardButton;           // ����� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI purchaseCatsPlusCashText;  // ����� ���� ���� ��ȭ ���� Text
    [SerializeField] private GameObject purchaseCatsRewardDisabledBG;   // ����� ���� ���� ��ư ��Ȱ��ȭ BG
    private int purchaseCatsTargetCount = 1;                            // ��ǥ ����� ���� Ƚ��
    private int increasePurchaseCatsTargetCount = 2;                    // ��ǥ ����� ���� Ƚ�� ����ġ
    private int purchaseCatsQuestRewardCash = 5;                        // ����� ���� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isPurchaseCatsQuestComplete = false;                   // ����� ���� ����Ʈ �Ϸ� ����

    [Header("---[Special Reward UI]")]
    [SerializeField] private Button specialRewardButton;                // Special Reward ��ư
    [SerializeField] private TextMeshProUGUI specialRewardPlusCashText; // Special Reward ���� ��ȭ ���� Text
    [SerializeField] private GameObject specialRewardRewardDisabledBG;  // Special Reward ���� ��ư ��Ȱ��ȭ BG
    [SerializeField] private GameObject specialRewardDisabledBG;        // Special Reward ��Ȱ��ȭ BG
    private int specialRewardQuestRewardCash = 50;                      // Special Reward ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isSpecialRewardActive = false;                         // Special Reward Ȱ��ȭ ����

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
    }

    // ======================================================================================================================

    // ��� QuestManager ���� �Լ��� ����
    private void InitializeQuestManager()
    {
        InitializeQuestButton();
        ResetScrollPositions();

        InitializeGiveFeedQuest();
        InitializeCombineQuest();
        InitializeGetCoinQuest();
        InitializePlayTimeQuest();
        InitializePurchaseCatsQuest();
        InitializeSpecialReward();
    }

    // ======================================================================================================================

    // ��� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateQuestUI()
    {
        UpdateGiveFeedQuestUI();
        UpdateCombineQuestUI();
        UpdateGetCoinQuestUI();
        UpdatePlayTimeQuestUI();
        UpdatePurchaseCatsQuestUI();
        UpdateSpecialRewardUI();

        UpdateNewImageStatus();
    }

    // New Image ���¸� ������Ʈ�ϴ� �Լ�
    private void UpdateNewImageStatus()
    {
        bool hasActiveReward =
            giveFeedRewardButton.interactable ||
            combineRewardButton.interactable ||
            getCoinRewardButton.interactable ||
            playTimeRewardButton.interactable ||
            purchaseCatsRewardButton.interactable ||
            specialRewardButton.interactable;

        newImage.SetActive(hasActiveReward);
    }

    // QuestButton ����
    private void InitializeQuestButton()
    {
        questButton.onClick.AddListener(() => activePanelManager.TogglePanel("QuestMenu"));
        questBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("QuestMenu"));
    }

    // �ʱ� ��ũ�� ��ġ �ʱ�ȭ �Լ�
    private void ResetScrollPositions()
    {
        questScrollRect.verticalNormalizedPosition = 1f;
    }

    // ======================================================================================================================

    // ����Ʈ ���� ������

    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    public void AddFeedCount()
    {
        FeedCount++;
    }

    public void ResetFeedCount(int count)
    {
        FeedCount = count;
    }

    public void AddCombineCount()
    {
        CombineCount++;
    }

    public void ResetCombineCount(int count)
    {
        CombineCount = count;
    }

    public void AddGetCoinCount(int count)
    {
        GetCoinCount += count;
    }

    public void ResetGetCoinCount(int count)
    {
        GetCoinCount = count;
    }

    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
    }

    public void ResetPlayTime(int count)
    {
        PlayTimeCount = count;
    }

    public void AddPurchaseCatsCount()
    {
        PurchaseCatsCount++;
    }

    public void ResetPurchaseCatsCount(int count)
    {
        PurchaseCatsCount = count;
    }

    // ======================================================================================================================

    // GiveFeed ����Ʈ �ʱ� ����
    private void InitializeGiveFeedQuest()
    {
        giveFeedRewardButton.onClick.AddListener(ReceiveGiveFeedReward);
        giveFeedRewardButton.interactable = false;
        giveFeedPlusCashText.text = $"+{giveFeedQuestRewardCash}";
    }

    // ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateGiveFeedQuestUI()
    {
        int currentCount = FeedCount;

        // ��ǥ�� 10 �̻��̸� ����Ʈ ���� ���� ó��
        if (giveFeedTargetCount >= 10)
        {
            giveFeedQuestSlider.value = giveFeedQuestSlider.maxValue;
            giveFeedCountText.text = "Complete";
            giveFeedRewardButton.interactable = false;
            giveFeedRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider �� ����
        giveFeedQuestSlider.maxValue = giveFeedTargetCount;
        giveFeedQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        giveFeedCountText.text = $"{currentCount}/{giveFeedTargetCount}";

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= giveFeedTargetCount;
        giveFeedRewardButton.interactable = isComplete;
        giveFeedRewardDisabledBG.SetActive(!isComplete);
    }

    // ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveGiveFeedReward()
    {
        int currentCount = FeedCount;

        // ��ǥ�� 10 �̻��� ��� �� �̻� ���� ���� �Ұ�
        if (giveFeedTargetCount >= 10)
        {
            isGiveFeedQuestComplete = true;
            return;
        }

        if (currentCount >= giveFeedTargetCount)
        {
            // �ʰ��� Ƚ�� ���
            int excessCount = currentCount - giveFeedTargetCount;

            // ��ǥ ���� Ƚ�� ���� (����Ʈ ���̵� ���)
            giveFeedTargetCount += increaseGiveFeedTargetCount;

            // ����Ʈ �Ϸ� ó��
            AddCash(giveFeedQuestRewardCash);
            ResetFeedCount(excessCount);
            GameManager.Instance.UpdateCashText();
            UpdateGiveFeedQuestUI();
        }
    }

    // ======================================================================================================================

    // Combine ����Ʈ �ʱ� ����
    private void InitializeCombineQuest()
    {
        combineRewardButton.onClick.AddListener(ReceiveCombineReward);
        combineRewardButton.interactable = false;
        combinePlusCashText.text = $"+{combineQuestRewardCash}";
    }

    // ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateCombineQuestUI()
    {
        int currentCount = CombineCount;

        // ��ǥ�� 10 �̻��̸� ����Ʈ ���� ���� ó��
        if (combineTargetCount >= 10)
        {
            combineQuestSlider.value = combineQuestSlider.maxValue;
            combineCountText.text = "Complete";
            combineRewardButton.interactable = false;
            combineRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider �� ����
        combineQuestSlider.maxValue = combineTargetCount;
        combineQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        combineCountText.text = $"{currentCount}/{combineTargetCount}";

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= combineTargetCount;
        combineRewardButton.interactable = isComplete;
        combineRewardDisabledBG.SetActive(!isComplete);
    }

    // ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveCombineReward()
    {
        int currentCount = CombineCount;

        // ��ǥ�� 10 �̻��� ��� �� �̻� ���� ���� �Ұ�
        if (combineTargetCount >= 10)
        {
            isCombineQuestComplete = true;
            return;
        }

        if (currentCount >= combineTargetCount)
        {
            // �ʰ��� Ƚ�� ���
            int excessCount = currentCount - combineTargetCount;

            // ��ǥ ���� Ƚ�� ���� (����Ʈ ���̵� ���)
            combineTargetCount += increaseCombineTargetCount;

            // ����Ʈ �Ϸ� ó��
            AddCash(combineQuestRewardCash);
            ResetCombineCount(excessCount);
            GameManager.Instance.UpdateCashText();
            UpdateCombineQuestUI();
        }
    }

    // ======================================================================================================================

    // GetCoin ����Ʈ �ʱ� ����
    private void InitializeGetCoinQuest()
    {
        getCoinRewardButton.onClick.AddListener(ReceiveGetCoinReward);
        getCoinRewardButton.interactable = false;
        getCoinPlusCashText.text = $"+{getCoinQuestRewardCash}";
    }

    // ȹ������ ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateGetCoinQuestUI()
    {
        int currentCount = GetCoinCount;

        // ��ǥ�� 10 �̻��̸� ����Ʈ ���� ���� ó��
        if (getCoinTargetCount >= 10)
        {
            getCoinQuestSlider.value = getCoinQuestSlider.maxValue;
            getCoinCountText.text = "Complete";
            getCoinRewardButton.interactable = false;
            getCoinRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider �� ����
        getCoinQuestSlider.maxValue = getCoinTargetCount;
        getCoinQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        getCoinCountText.text = $"{currentCount}/{getCoinTargetCount}";

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= getCoinTargetCount;
        getCoinRewardButton.interactable = isComplete;
        getCoinRewardDisabledBG.SetActive(!isComplete);
    }

    // ȹ������ ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveGetCoinReward()
    {
        int currentCount = GetCoinCount;

        // ��ǥ�� 10 �̻��� ��� �� �̻� ���� ���� �Ұ�
        if (getCoinTargetCount >= 10)
        {
            isGetCoinQuestComplete = true;
            return;
        }

        if (currentCount >= getCoinTargetCount)
        {
            // �ʰ��� Ƚ�� ���
            int excessCount = currentCount - getCoinTargetCount;

            // ��ǥ ȹ������ Ƚ�� ���� (����Ʈ ���̵� ���)
            getCoinTargetCount += increaseGetCoinTargetCount;

            // ����Ʈ �Ϸ� ó��
            AddCash(getCoinQuestRewardCash);
            ResetGetCoinCount(excessCount);
            GameManager.Instance.UpdateCashText();
            UpdateGetCoinQuestUI();
        }
    }

    // ======================================================================================================================

    // PlayTime ����Ʈ �ʱ� ����
    private void InitializePlayTimeQuest()
    {
        playTimeRewardButton.onClick.AddListener(ReceivePlayTimeReward);
        playTimeRewardButton.interactable = false;
        playTimePlusCashText.text = $"+{playTimeQuestRewardCash}";
    }

    // �÷���Ÿ�� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdatePlayTimeQuestUI()
    {
        int currentTime = (int)PlayTimeCount;

        // ��ǥ�� 200 �̻��̸� ����Ʈ ���� ���� ó��
        if (playTimeTargetCount >= 200)
        {
            playTimeQuestSlider.value = playTimeQuestSlider.maxValue;
            playTimeCountText.text = "Complete";
            playTimeRewardButton.interactable = false;
            playTimeRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider �� ����
        playTimeQuestSlider.maxValue = playTimeTargetCount;
        playTimeQuestSlider.value = currentTime;

        // "?/?" �ؽ�Ʈ ������Ʈ
        playTimeCountText.text = $"{currentTime}/{playTimeTargetCount}";

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentTime >= playTimeTargetCount;
        playTimeRewardButton.interactable = isComplete;
        playTimeRewardDisabledBG.SetActive(!isComplete);
    }

    // �÷���Ÿ�� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceivePlayTimeReward()
    {
        int currentTime = (int)PlayTimeCount;

        // ��ǥ�� 200 �̻��� ��� �� �̻� ���� ���� �Ұ�
        if (playTimeTargetCount >= 200)
        {
            isPlayTimeQuestComplete = true;
            return;
        }

        if (currentTime >= playTimeTargetCount)
        {
            int excessTime = currentTime - playTimeTargetCount;

            // ��ǥ �÷���Ÿ�� ����
            playTimeTargetCount += increasePlayTimeTargetCount;

            // ����Ʈ �Ϸ� ó��
            AddCash(playTimeQuestRewardCash);
            ResetPlayTime(excessTime);
            GameManager.Instance.UpdateCashText();
            UpdatePlayTimeQuestUI();
        }
    }

    // ======================================================================================================================

    // Purchase Cats ����Ʈ �ʱ� ����
    private void InitializePurchaseCatsQuest()
    {
        purchaseCatsRewardButton.onClick.AddListener(ReceivePurchaseCatsReward);
        purchaseCatsRewardButton.interactable = false;
        purchaseCatsPlusCashText.text = $"+{purchaseCatsQuestRewardCash}";
    }

    // ����� ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdatePurchaseCatsQuestUI()
    {
        int currentCount = PurchaseCatsCount;

        // ��ǥ�� 10 �̻��̸� ����Ʈ ���� ���� ó��
        if (purchaseCatsTargetCount >= 10)
        {
            purchaseCatsQuestSlider.value = purchaseCatsQuestSlider.maxValue;
            purchaseCatsCountText.text = "Complete";
            purchaseCatsRewardButton.interactable = false;
            purchaseCatsRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider �� ����
        purchaseCatsQuestSlider.maxValue = purchaseCatsTargetCount;
        purchaseCatsQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        purchaseCatsCountText.text = $"{currentCount}/{purchaseCatsTargetCount}";

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= purchaseCatsTargetCount;
        purchaseCatsRewardButton.interactable = isComplete;
        purchaseCatsRewardDisabledBG.SetActive(!isComplete);
    }

    // ����� ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceivePurchaseCatsReward()
    {
        int currentCount = PurchaseCatsCount;

        // ��ǥ�� 10 �̻��� ��� �� �̻� ���� ���� �Ұ�
        if (purchaseCatsTargetCount >= 10)
        {
            isPurchaseCatsQuestComplete = true;
            return;
        }

        if (currentCount >= purchaseCatsTargetCount)
        {
            int excessCount = currentCount - purchaseCatsTargetCount;

            // ��ǥ ����� ���� Ƚ�� ����
            purchaseCatsTargetCount += increasePurchaseCatsTargetCount;

            // ����Ʈ �Ϸ� ó��
            AddCash(purchaseCatsQuestRewardCash);
            ResetPurchaseCatsCount(excessCount);
            GameManager.Instance.UpdateCashText();
            UpdatePurchaseCatsQuestUI();
        }
    }

    // ======================================================================================================================

    // ����Ʈ �ʱ� �������� Special Reward ��ư �ʱ�ȭ
    private void InitializeSpecialReward()
    {
        specialRewardButton.onClick.AddListener(ReceiveSpecialReward);
        specialRewardButton.interactable = false;
        specialRewardDisabledBG.SetActive(true);
        specialRewardPlusCashText.text = $"+{specialRewardQuestRewardCash}";
    }

    // ��� ����Ʈ�� �Ϸ�Ǿ����� Ȯ���ϴ� �Լ�
    private bool AllQuestsCompleted()
    {
        if (isGiveFeedQuestComplete && isCombineQuestComplete && isGetCoinQuestComplete && isPlayTimeQuestComplete && isPurchaseCatsQuestComplete)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Special Reward Ȱ��ȭ ���¸� ������Ʈ�ϴ� �Լ�
    private void UpdateSpecialRewardUI()
    {
        if (AllQuestsCompleted())
        {
            isSpecialRewardActive = true;
            specialRewardDisabledBG.SetActive(false);
            specialRewardButton.interactable = true;
        }
    }

    // Special Reward ���� ���� �Լ�
    private void ReceiveSpecialReward()
    {
        if (isSpecialRewardActive)
        {
            // ���� ó�� ����
            AddCash(specialRewardQuestRewardCash);

            // ���� ���� �� ��ư�� ��Ȱ��ȭ ���·� ���ư�
            specialRewardRewardDisabledBG.SetActive(true);
            specialRewardButton.interactable = false;
            isSpecialRewardActive = false;

            // ����Ʈ UI ������Ʈ
            UpdateSpecialRewardUI();
            GameManager.Instance.UpdateCashText();
        }
    }

}
