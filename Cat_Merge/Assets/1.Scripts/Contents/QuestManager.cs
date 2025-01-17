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

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject newImage;                       // ����Ʈ Button�� New Image 

    // ======================================================================================================================

    private float playTimeCount;                                        // �÷���Ÿ�� ī��Ʈ
    public float PlayTimeCount { get => playTimeCount; set => playTimeCount = value; }

    private int mergeCount;                                             // ����� ���� Ƚ��
    public int MergeCount { get => mergeCount; set => mergeCount = value; }

    private int feedCount;                                              // ����� ���� Ƚ��(���� �� Ƚ��)
    public int FeedCount { get => feedCount; set => feedCount = value; }

    private int purchaseCatsCount;                                      // ����� ���� Ƚ��
    public int PurchaseCatsCount { get => purchaseCatsCount; set => purchaseCatsCount = value; }

    // ======================================================================================================================

    [Header("---[PlayTime Quest UI]")]
    [SerializeField] private Slider playTimeQuestSlider;                // �÷���Ÿ�� Slider
    [SerializeField] private TextMeshProUGUI playTimeCountText;         // "?/?" �ؽ�Ʈ
    [SerializeField] private Button playTimeRewardButton;               // �÷���Ÿ�� ���� ��ư
    [SerializeField] private TextMeshProUGUI playTimePlusCashText;      // �÷���Ÿ�� ���� ��ȭ ���� Text
    [SerializeField] private GameObject playTimeRewardDisabledBG;       // �÷���Ÿ�� ���� ��ư ��Ȱ��ȭ BG
    private int playTimeTargetCount = 720;                              // ��ǥ �÷���Ÿ�� (�� ����)
    private int playTimeQuestRewardCash = 5;                            // �÷���Ÿ�� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isPlayTimeQuestComplete = false;                       // �÷���Ÿ�� ����Ʈ �Ϸ� ����

    [Header("---[Merge Cats Quest UI]")]
    [SerializeField] private Slider mergeQuestSlider;                 // ���� Slider
    [SerializeField] private TextMeshProUGUI mergeCountText;          // "?/?" �ؽ�Ʈ
    [SerializeField] private Button mergeRewardButton;                // ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI mergePlusCashText;       // ���� ���� ��ȭ ���� Text
    [SerializeField] private GameObject mergeRewardDisabledBG;        // ���� ���� ��ư ��Ȱ��ȭ BG
    private int mergeTargetCount = 100;                               // ��ǥ ���� Ƚ��
    private int mergeQuestRewardCash = 5;                             // ���� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isMergeQuestComplete = false;                        // ���� ����Ʈ �Ϸ� ����

    [Header("---[Spawn Cats Quest UI]")]
    [SerializeField] private Slider giveFeedQuestSlider;                // ����� ���� Slider
    [SerializeField] private TextMeshProUGUI giveFeedCountText;         // "?/?" �ؽ�Ʈ
    [SerializeField] private Button giveFeedRewardButton;               // ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI giveFeedPlusCashText;      // ���� ���� ��ȭ ���� Text
    [SerializeField] private GameObject giveFeedRewardDisabledBG;       // ���� ���� ��ư ��Ȱ��ȭ BG
    private int giveFeedTargetCount = 1;                                // ��ǥ ���� Ƚ��
    private int increaseGiveFeedTargetCount = 2;                        // ��ǥ ���� Ƚ�� ����ġ
    private int giveFeedQuestRewardCash = 5;                            // ���� ����Ʈ ���� ĳ�� ��ȭ ����
    private bool isGiveFeedQuestComplete = false;                       // ���� ����Ʈ �Ϸ� ����

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
    private int specialRewardQuestRewardCash = 500;                     // Special Reward ����Ʈ ���� ĳ�� ��ȭ ����
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

        InitializePlayTimeQuest();
        //InitializeMergeQuest();
        //InitializeGiveFeedQuest();
        //InitializePurchaseCatsQuest();
        //InitializeSpecialReward();
    }

    // ======================================================================================================================

    // ��� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateQuestUI()
    {
        UpdatePlayTimeQuestUI();
        //UpdateGiveFeedQuestUI();
        //UpdateMergeQuestUI();
        //UpdatePurchaseCatsQuestUI();
        //UpdateSpecialRewardUI();

        UpdateNewImageStatus();
    }

    // New Image ���¸� ������Ʈ�ϴ� �Լ�
    private void UpdateNewImageStatus()
    {
        bool hasActiveReward =
            playTimeRewardButton.interactable ||
            giveFeedRewardButton.interactable ||
            mergeRewardButton.interactable ||
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

    public void AddMergeCount()
    {
        MergeCount++;
    }

    public void ResetMergeCount(int count)
    {
        MergeCount = count;
    }

    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
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
        // �ִ� �ð��� �ʰ����� �ʵ��� ����
        int currentTime = Mathf.Min((int)PlayTimeCount, playTimeTargetCount);

        // Slider �� ����
        playTimeQuestSlider.maxValue = playTimeTargetCount;
        playTimeQuestSlider.value = currentTime;

        // "?/?" �ؽ�Ʈ ������Ʈ
        if (isPlayTimeQuestComplete)
        {
            playTimeCountText.text = "Complete";
        }
        else
        {
            playTimeCountText.text = $"{currentTime}/{playTimeTargetCount}";
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
        isPlayTimeQuestComplete = true;
        AddCash(playTimeQuestRewardCash);
        UpdatePlayTimeQuestUI();
    }
    
    // ======================================================================================================================

    // Merge ����Ʈ �ʱ� ����
    private void InitializeMergeQuest()
    {
        mergeRewardButton.onClick.AddListener(ReceiveMergeReward);
        mergeRewardButton.interactable = false;
        mergePlusCashText.text = $"+{mergeQuestRewardCash}";
    }

    // ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateMergeQuestUI()
    {
        int currentCount = MergeCount;

        // ��ǥ�� 10 �̻��̸� ����Ʈ ���� ���� ó��
        if (mergeTargetCount >= 10)
        {
            mergeQuestSlider.value = mergeQuestSlider.maxValue;
            mergeCountText.text = "Complete";
            mergeRewardButton.interactable = false;
            mergeRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider �� ����
        mergeQuestSlider.maxValue = mergeTargetCount;
        mergeQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        mergeCountText.text = $"{currentCount}/{mergeTargetCount}";

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= mergeTargetCount;
        mergeRewardButton.interactable = isComplete;
        mergeRewardDisabledBG.SetActive(!isComplete);
    }

    // ���� ����Ʈ ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveMergeReward()
    {
        int currentCount = MergeCount;

        // ��ǥ�� 10 �̻��� ��� �� �̻� ���� ���� �Ұ�
        if (mergeTargetCount >= 10)
        {
            isMergeQuestComplete = true;
            return;
        }

        if (currentCount >= mergeTargetCount)
        {
            // �ʰ��� Ƚ�� ���
            int excessCount = currentCount - mergeTargetCount;

            // ����Ʈ �Ϸ� ó��
            AddCash(mergeQuestRewardCash);
            ResetMergeCount(excessCount);
            GameManager.Instance.UpdateCashText();
            UpdateMergeQuestUI();
        }
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
        if (isGiveFeedQuestComplete && isMergeQuestComplete && isPlayTimeQuestComplete && isPurchaseCatsQuestComplete)
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
