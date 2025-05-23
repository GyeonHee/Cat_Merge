using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// ���� ��ũ��Ʈ
[DefaultExecutionOrder(-5)]
public class ShopManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static ShopManager Instance { get; private set; }

    [Header("---[Shop]")]
    [SerializeField] private Button shopButton;                     // ���� ��ư
    [SerializeField] private Image shopButtonImg;                   // ���� ��ư �̹���
    [SerializeField] private GameObject shopPanel;                  // ���� �޴� �г�
    [SerializeField] private Button shopBackButton;                 // ���� �޴� �ڷΰ��� ��ư
    [SerializeField] private GameObject shopNewImage;               // ���� ��ư New �̹��� ������Ʈ

    private ActivePanelManager activePanelManager;                  // ActivePanelManager

    private bool isWaitingForAd = false;                            // ���� ��� ������ Ȯ���ϴ� ���� �߰�


    [Header("---[�⺻���� ��Ÿ�ӵ�]")]
    private const long DEFAULT_CASH_FOR_TIME_COOLTIME = 600;            // ���� ĳ�� ȹ�� ��Ÿ�� (CashForTime) (600��)
    private const long DEFAULT_CASH_FOR_AD_COOLTIME = 600;              // ���� ĳ�� ȹ�� ��Ÿ�� (CashForAd) (600��)
    private const long DEFAULT_DOUBLE_COIN_FOR_AD_COOLTIME = 600;       // ���� 2�� ȹ�� ��Ÿ��  (DoubleCoinForAd) (600��)

    [Header("---[Cash For Time]")]
    [SerializeField] private Button cashForTimeRewardButton;            // ��Ÿ�Ӹ��� Ȱ��ȭ�Ǵ� ���� ĳ�� ȹ�� (���� ĳ�� ȹ�� �Ұ��� �����϶� ��Ȱ��ȭ = interactable ��Ȱ��ȭ)
    [SerializeField] private TextMeshProUGUI cashForTimeNameText;       // Item Name Text
    [SerializeField] private TextMeshProUGUI cashForTimeCoolTimeText;   // ���� ��û ��Ÿ�� Text
    [SerializeField] private TextMeshProUGUI cashForTimeInformationText;// Item Information Text
    [SerializeField] private GameObject cashForTimeNewImage;            // ���� ĳ�� ȹ�� ���� �����϶� Ȱ��ȭ�Ǵ� New �̹��� ������Ʈ
    [SerializeField] private GameObject cashForTimeDisabledBG;          // ���� ĳ�� ȹ�� �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    private long cashForTimeCoolTime = DEFAULT_CASH_FOR_TIME_COOLTIME;  // ���� ĳ�� ȹ�� ��Ÿ��(������ ����ǵ� �ð��� �帣���� ���� �ð��� �������� ���)
    private long lastTimeReward = 0;                                    // ������ cashForTime ���� �ð� ����
    private long passiveCashForTimeCoolTimeReduction = 0;               // �нú�� ���� cashForTime ��Ÿ�� ���ҷ�
    private int cashForTimePrice = 5;                                   // cashForTime ����
    private int passiveCashForTimeAmount = 0;                           // �нú�� ���� cashForTime �߰� ���� ĳ�� ȹ�淮
    public int CashForTimePrice => cashForTimePrice + passiveCashForTimeAmount;


    [Header("---[Cash For Ad]")]
    [SerializeField] private Button cashForAdRewardButton;              // ���� ��û���� ĳ�� ȹ�� (���� ��û �Ұ��� �����϶� ��Ȱ��ȭ = interactable ��Ȱ��ȭ)
    [SerializeField] private TextMeshProUGUI cashForAdNameText;         // Item Name Text
    [SerializeField] private TextMeshProUGUI cashForAdCoolTimeText;     // ���� ��û ��Ÿ�� Text
    [SerializeField] private TextMeshProUGUI cashForAdInformationText;  // Item Information Text
    [SerializeField] private GameObject cashForAdRewardOnImage;         // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject cashForAdRewardOffImage;        // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject cashForAdNewImage;              // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� New �̹��� ������Ʈ
    [SerializeField] private GameObject cashForAdDisabledBG;            // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    private long cashForAdCoolTime = DEFAULT_CASH_FOR_AD_COOLTIME;      // ���� ��û ��Ÿ�� (������ ����ǵ� �ð��� �帣���� ���� �ð��� �������� ���)
    private long lastAdTimeReward = 0;                                  // ������ cashForAd ���� �ð� ����
    private long passiveCashForAdCoolTimeReduction = 0;                 // �нú�� ���� cashForAd ��Ÿ�� ���ҷ�
    private int cashForAdPrice = 30;                                    // cashForAd ����
    private int passiveCashForAdAmount = 0;                             // �нú�� ���� cashForAd �߰� ���� ĳ�� ȹ�淮
    public int CashForAdPrice => cashForAdPrice + passiveCashForAdAmount;


    [Header("---[Double Coin For Ad]")]
    [SerializeField] private Button doubleCoinForAdButton;                      // ���� ��û���� ���� ȹ�淮 ���� ȿ�� ȹ��
    [SerializeField] private TextMeshProUGUI doubleCoinForAdNameText;           // Item Name Text
    [SerializeField] private TextMeshProUGUI doubleCoinForAdCoolTimeText;       // ���� ��û ��Ÿ�� Text
    [SerializeField] private GameObject doubleCoinForAdRewardOnImage;           // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject doubleCoinForAdRewardOffImage;          // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject doubleCoinForAdNewImage;                // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� New �̹��� ������Ʈ
    [SerializeField] private GameObject doubleCoinForAdDisabledBG;              // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    private long doubleCoinForAdCoolTime = DEFAULT_DOUBLE_COIN_FOR_AD_COOLTIME; // ���� ȹ�淮 ��Ÿ�� (������ ����ǵ� �ð��� �帣���� ���� �ð��� �������� ���)
    private long lastDoubleCoinTimeReward = 0;                                  // ������ doubleCoinForAd ���� �ð� ����
    private long passiveDoubleCoinForAdCoolTimeReduction = 0;                   // �нú�� ���� doubleCoinForAd ��Ÿ�� ���ҷ�
    
    private const float doubleCoinDuration = 300f;                          // ���� 2�� ���ӽð� (300��)
    private const float doubleCoinMultiplier = 2f;                          // ���� ȹ�淮 ���
    private float coinMultiplier = 1f;                                      // ���� ���� ���
    private float multiplierEndTime = 0f;                                   // ��� ȿ�� ���� �ð�
    private float passiveDoubleCoinDurationIncrease = 0f;                   // �нú�� ���� ȿ�����ӽð� ������
    private float CurrentDoubleCoinDuration => doubleCoinDuration + passiveDoubleCoinDurationIncrease;


    [Header("---[Diamond Shop]")]
    [SerializeField] private Button buy100CashToCoinButton;                 // 100���̾� ��ǰ ��ư
    [SerializeField] private Button buy500CashToCoinButton;                 // 500���̾� ��ǰ ��ư
    [SerializeField] private Button buy2500CashToCoinButton;                // 2500���̾� ��ǰ ��ư
    private int selectedCashAmount = 0;                                     // ���õ� ���̾� ����

    [SerializeField] private Image cantBuy100CashToCoinImage;               // 100���̾� ��ǰ �ر� �̹���
    [SerializeField] private Image cantBuy500CashToCoinImage;               // 500���̾� ��ǰ �ر� �̹���
    [SerializeField] private Image cantBuy2500CashToCoinImage;              // 2500���̾� ��ǰ �ر� �̹���
    private const int UNLOCK_GRADE_100_CASH = 5;                            // 100���̾� ��ǰ �ر� ���
    private const int UNLOCK_GRADE_500_CASH = 10;                           // 500���̾� ��ǰ �ر� ���
    private const int UNLOCK_GRADE_2500_CASH = 15;                          // 2500���̾� ��ǰ �ر� ���

    [SerializeField] private GameObject buyCashToCoinPanel;                 // ��ǰ ���� Ȯ�� Panel
    [SerializeField] private TextMeshProUGUI buyCashToCoinText;             // ��ǰ ���� Ȯ�� Text
    [SerializeField] private Button closeBuyCashToCoinPanelButton;          // ��ǰ ���� �г� �ݱ� ��ư
    [SerializeField] private Button buyCashToCoinButton;                    // ��ǰ ���� Yes ��ư
    private decimal calculatedCoinAmount = 0;                               // ���� ���� ����


    [Header("---[ETC]")]
    private float battlePauseTime = 0f;                                     // ���� �� ���� �ð�
    private bool isBattlePaused = false;                                    // ���� �� ���� ����

    private readonly WaitForSeconds oneSecondWait = new WaitForSeconds(1f);

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
        InitializeShopManager();
        UpdateAllUI();
        UpdateDiamondShopUnlockState();
        StartCoroutine(CheckRewardStatus());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    #endregion


    #region Initialize

    // �⺻ Shop �ʱ�ȭ �Լ�
    private void InitializeShopManager()
    {
        shopPanel.SetActive(false);

        InitializeActivePanel();
        InitializeButtonListeners();
    }

    // ActivePanel �ʱ�ȭ �Լ�
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("Shop", shopPanel, shopButtonImg);

        shopButton.onClick.AddListener(() => activePanelManager.TogglePanel("Shop"));
        shopBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("Shop"));
    }

    // ��ü UI ������Ʈ �Լ�
    private void UpdateAllUI()
    {
        UpdateCashForAdUI();
        UpdateCashForTimeUI();

        UpdateDoubleCoinForAdUI();

        UpdateShopNewImage();
    }

    // ��ư ������ �ʱ�ȭ �Լ�
    private void InitializeButtonListeners()
    {
        cashForAdRewardButton.onClick.AddListener(OnClickCashForAd);
        cashForTimeRewardButton.onClick.AddListener(OnClickCashForTime);

        doubleCoinForAdButton.onClick.AddListener(OnClickDoubleCoinForAd);

        buy100CashToCoinButton.onClick.AddListener(() => OnClickBuyCashToCoin(100));
        buy500CashToCoinButton.onClick.AddListener(() => OnClickBuyCashToCoin(500));
        buy2500CashToCoinButton.onClick.AddListener(() => OnClickBuyCashToCoin(2500));
        closeBuyCashToCoinPanelButton.onClick.AddListener(CloseBuyCashToCoinPanel);
        buyCashToCoinButton.onClick.AddListener(OnConfirmBuyCashToCoin);
    }

    // New �̹��� ���� ������Ʈ �Լ�
    private void UpdateShopNewImage()
    {
        bool isAnyNewImageActive = cashForAdNewImage.activeSelf || cashForTimeNewImage.activeSelf || doubleCoinForAdNewImage.activeSelf;
        shopNewImage.SetActive(isAnyNewImageActive);
    }

    #endregion


    #region Free Shop System

    // CashForTime UI ������Ʈ �Լ�
    private void UpdateCashForTimeUI()
    {
        bool isTimeRewardAvailable = IsCashForTimeAvailable();

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remainTime = Math.Max(0, cashForTimeCoolTime - (currentTime - lastTimeReward));

        // ��ư ���� ������Ʈ
        cashForTimeRewardButton.interactable = isTimeRewardAvailable;
        cashForTimeNewImage.SetActive(isTimeRewardAvailable);
        cashForTimeDisabledBG.SetActive(!isTimeRewardAvailable);

        // ��Ÿ�� �ؽ�Ʈ ������Ʈ
        if (isTimeRewardAvailable)
        {
            cashForTimeCoolTimeText.text = "�غ� �Ϸ�";
        }
        else
        {
            cashForTimeCoolTimeText.text = $"��Ÿ�� {(int)remainTime}��";
        }

        // ���� ���� �ؽ�Ʈ ������Ʈ
        if (cashForTimeNameText != null)
        {
            cashForTimeNameText.text = $"���� ���̾� {CashForTimePrice}��";
        }
        if (cashForTimeInformationText != null)
        {
            cashForTimeInformationText.text = $"���� ���̾� {CashForTimePrice}�� �ޱ�";
        }
    }

    // CashForTime Ȱ��ȭ ���� �Ǻ� �Լ�
    private bool IsCashForTimeAvailable()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return (currentTime - lastTimeReward) >= cashForTimeCoolTime;
    }

    // CashForTime ��ư Ŭ���� ����Ǵ� �Լ�
    private void OnClickCashForTime()
    {
        if (!IsCashForTimeAvailable()) return;

        // �ð� ���� ����
        GameManager.Instance.Cash += CashForTimePrice;

        // ������ ���� �ð� ����
        lastTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        UpdateCashForTimeUI();
    }

    // �нú�� ���� CashForTime�� ĳ�� ��ȭ �߰� �Լ�
    public void AddPassiveCashForTimeAmount(int amount)
    {
        passiveCashForTimeAmount += amount;

        UpdateCashForTimeUI();
    }

    // �нú�� ���� CashForTime�� ��Ÿ�� ���� �Լ�
    public void AddPassiveCashForTimeCoolTimeReduction(long seconds)
    {
        passiveCashForTimeCoolTimeReduction += seconds;
        cashForTimeCoolTime = DEFAULT_CASH_FOR_TIME_COOLTIME - passiveCashForTimeCoolTimeReduction;

        UpdateCashForTimeUI();
    }

    #endregion


    #region Ad Shop System

    // �ֱ������� ���� ���¸� üũ�ϴ� �ڷ�ƾ
    private IEnumerator CheckRewardStatus()
    {
        while (true)
        {
            UpdateAllUI();

            yield return oneSecondWait;
        }
    }

    // CashForAd UI ������Ʈ �Լ�
    private void UpdateCashForAdUI()
    {
        bool isAdAvailable = IsCashForAdAvailable() && !isWaitingForAd;
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remainTime = Math.Max(0, cashForAdCoolTime - (currentTime - lastAdTimeReward));

        cashForAdRewardButton.interactable = isAdAvailable;
        cashForAdRewardOnImage.SetActive(isAdAvailable);
        cashForAdRewardOffImage.SetActive(!isAdAvailable);
        cashForAdNewImage.SetActive(isAdAvailable);
        cashForAdDisabledBG.SetActive(!isAdAvailable);

        // ��Ÿ�� �ؽ�Ʈ ������Ʈ
        if (isAdAvailable)
        {
            cashForAdCoolTimeText.text = "�غ� �Ϸ�";
        }
        else
        {
            cashForAdCoolTimeText.text = $"��Ÿ�� {(int)remainTime}��";
        }

        // ���� ���� �ؽ�Ʈ ������Ʈ
        if (cashForAdNameText != null)
        {
            cashForAdNameText.text = $"���� ���̾� {CashForAdPrice}��";
        }
        if (cashForAdInformationText != null)
        {
            cashForAdInformationText.text = $"���� ��û �� ���̾� {CashForAdPrice}�� �ޱ�";
        }
    }

    // CashForTime Ȱ��ȭ ���� �Ǻ� �Լ�
    private bool IsCashForAdAvailable()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return (currentTime - lastAdTimeReward) >= cashForAdCoolTime;
    }

    // CashForAd ��ư Ŭ���� ����Ǵ� �Լ�
    private void OnClickCashForAd()
    {
        if (!IsCashForAdAvailable() || isWaitingForAd) return;

        isWaitingForAd = true;
        cashForAdRewardButton.interactable = false;

        GetComponent<GoogleAdsManager>()?.ShowRewardedCashForAd();
    }

    // CashForAd ���� ��û �Ϸ� �� ����Ǵ� �Լ�
    public void OnCashForAdRewardComplete()
    {
        // ���� ���� ����
        GameManager.Instance.Cash += CashForAdPrice;

        // ������ ���� ��û �ð� ����
        lastAdTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        isWaitingForAd = false;

        UpdateCashForAdUI();
    }

    // �нú�� ���� CashForAd�� ĳ�� ��ȭ �߰� �Լ�
    public void AddPassiveCashForAdAmount(int amount)
    {
        passiveCashForAdAmount += amount;
        UpdateCashForAdUI();
    }

    // �нú�� ���� CashForAd�� ��Ÿ�� ���� �Լ�
    public void AddPassiveCashForAdCoolTimeReduction(long seconds)
    {
        passiveCashForAdCoolTimeReduction += seconds;
        cashForAdCoolTime = DEFAULT_CASH_FOR_AD_COOLTIME - passiveCashForAdCoolTimeReduction;

        UpdateCashForTimeUI();
    }


    // DoubleCoinForAd UI ������Ʈ �Լ�
    private void UpdateDoubleCoinForAdUI()
    {
        bool isAdAvailable = IsDoubleCoinForAdAvailable() && !isWaitingForAd;
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remainTime = Math.Max(0, doubleCoinForAdCoolTime - (currentTime - lastDoubleCoinTimeReward));
        float remainEffectTime = GetRemainingEffectTime();

        doubleCoinForAdButton.interactable = isAdAvailable;
        doubleCoinForAdRewardOnImage.SetActive(isAdAvailable);
        doubleCoinForAdRewardOffImage.SetActive(!isAdAvailable);
        doubleCoinForAdNewImage.SetActive(isAdAvailable);
        doubleCoinForAdDisabledBG.SetActive(!isAdAvailable);

        if (doubleCoinForAdNameText != null)
        {
            doubleCoinForAdNameText.text = $"{CurrentDoubleCoinDuration}�� ���� ��ȭ ���޷� {doubleCoinMultiplier}��";
        }

        // ��Ÿ�� �ؽ�Ʈ ������Ʈ
        if (remainEffectTime > 0)
        {
            doubleCoinForAdCoolTimeText.text = $"ȿ�� ���ӽð� {(int)remainEffectTime}��";
        }
        else if (isAdAvailable)
        {
            doubleCoinForAdCoolTimeText.text = "�غ� �Ϸ�";
        }
        else
        {
            doubleCoinForAdCoolTimeText.text = $"��Ÿ�� {(int)remainTime}��";
        }
    }

    // ���� ���� ����� �������� �Լ�
    public float CurrentCoinMultiplier
    {
        get
        {
            if (Time.time < multiplierEndTime)
            {
                return coinMultiplier;
            }
            else
            {
                // ȿ���� ������ �� �ʱ�ȭ
                if (coinMultiplier != 1f)
                {
                    coinMultiplier = 1f;
                    multiplierEndTime = 0f;
                    isBattlePaused = false;
                    battlePauseTime = 0f;
                    UpdateDoubleCoinForAdUI();
                }
                return 1f;
            }
        }
    }

    // ��� ȿ�� ���� �ð��� �������� �Լ�
    public float GetRemainingEffectTime()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleActive)
        {
            if (!isBattlePaused)
            {
                isBattlePaused = true;
                battlePauseTime = Time.time;
            }
            return Mathf.Max(0f, multiplierEndTime - battlePauseTime);
        }
        else
        {
            if (isBattlePaused)
            {
                isBattlePaused = false;
                float pausedDuration = Time.time - battlePauseTime;
                multiplierEndTime += pausedDuration;
            }
            return Mathf.Max(0f, multiplierEndTime - Time.time);
        }
    }

    // DoubleCoinForAd Ȱ��ȭ ���� �Ǻ� �Լ�
    private bool IsDoubleCoinForAdAvailable()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return (currentTime - lastDoubleCoinTimeReward) >= doubleCoinForAdCoolTime;
    }

    // DoubleCoinForAd ��ư Ŭ���� ����Ǵ� �Լ�
    private void OnClickDoubleCoinForAd()
    {
        if (!IsDoubleCoinForAdAvailable() || isWaitingForAd) return;

        isWaitingForAd = true;
        doubleCoinForAdButton.interactable = false;

        GetComponent<GoogleAdsManager>()?.ShowRewardedDoubleCoinForAd();
    }

    // DoubleCoinForAd ���� ��û �Ϸ� �� ����Ǵ� �Լ�
    public void OnDoubleCoinAdRewardComplete()
    {
        if (isWaitingForAd)
        {
            // ���� ȿ�� ���ӽð� ���
            float remainingTime = GetRemainingEffectTime();

            // ���� ���� ���� - ��� ������� ��ȭ ���޷� ������ �ð����� 2��� ����
            coinMultiplier = doubleCoinMultiplier;

            // ���� ȿ�� ���ӽð��� ������ �� �ð��� ���ο� ���ӽð��� ����
            if (remainingTime > 0)
            {
                multiplierEndTime = Time.time + remainingTime + CurrentDoubleCoinDuration;
            }
            else
            {
                multiplierEndTime = Time.time + CurrentDoubleCoinDuration;
            }

            // ���� ���� ���� �ʱ�ȭ
            isBattlePaused = false;
            battlePauseTime = 0f;

            // ������ ���� ��û �ð� ����
            lastDoubleCoinTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            isWaitingForAd = false;

            UpdateDoubleCoinForAdUI();
        }
    }

    // �нú�� ���� DoubleCoinForAd ȿ�� ���ӽð� ���� �Լ�
    public void AddPassiveDoubleCoinDurationIncrease(float seconds)
    {
        passiveDoubleCoinDurationIncrease += seconds;

        UpdateDoubleCoinForAdUI();
    }

    // �нú�� ���� DoubleCoinForAd ��Ÿ�� ���� �Լ�
    public void AddPassiveDoubleCoinForAdCoolTimeReduction(long seconds)
    {
        passiveDoubleCoinForAdCoolTimeReduction += seconds;
        doubleCoinForAdCoolTime = DEFAULT_DOUBLE_COIN_FOR_AD_COOLTIME - passiveDoubleCoinForAdCoolTimeReduction;

        UpdateDoubleCoinForAdUI();
    }

    public void ResetAdState()
    {
        if (isWaitingForAd)
        {
            isWaitingForAd = false;

            // ��ư ���� ����
            cashForAdRewardButton.interactable = IsCashForAdAvailable();
            doubleCoinForAdButton.interactable = IsDoubleCoinForAdAvailable();

            UpdateAllUI();
        }
    }

    #endregion


    #region Diamond Shop System

    // ���̾� ���� �ر� ���� ������Ʈ �Լ�
    public void UpdateDiamondShopUnlockState()
    {
        int maxUnlockedGrade = GetMaxUnlockedCatGrade();

        // 100 ���̾� ��ǰ
        UpdateShopItemUnlockState(buy100CashToCoinButton, cantBuy100CashToCoinImage, maxUnlockedGrade >= UNLOCK_GRADE_100_CASH);

        // 500 ���̾� ��ǰ
        UpdateShopItemUnlockState(buy500CashToCoinButton, cantBuy500CashToCoinImage, maxUnlockedGrade >= UNLOCK_GRADE_500_CASH);

        // 2500 ���̾� ��ǰ
        UpdateShopItemUnlockState(buy2500CashToCoinButton, cantBuy2500CashToCoinImage, maxUnlockedGrade >= UNLOCK_GRADE_2500_CASH);
    }

    // ���� ������ �ر� ���� ������Ʈ �Լ�
    private void UpdateShopItemUnlockState(Button button, Image cantBuyImage, bool isUnlocked)
    {
        button.interactable = isUnlocked;
        cantBuyImage.gameObject.SetActive(!isUnlocked);
    }

    // �ִ� �ر� ����� ��� �������� �Լ�
    private int GetMaxUnlockedCatGrade()
    {
        for (int i = GameManager.Instance.AllCatData.Length - 1; i >= 0; i--)
        {
            if (DictionaryManager.Instance.IsCatUnlocked(i))
            {
                return i + 1;
            }
        }
        return 0;
    }

    // ���� ��ȯ ��ġ ��� �Լ�
    private decimal CalculateJellyAmount(int diamondAmount)
    {
        // ��ȭ ���� �ð� �������� (ItemMenuManager�� reduceCollectingTimeLv ���)
        var collectingTimeList = ItemFunctionManager.Instance.reduceCollectingTimeList;
        float collectingTime = collectingTimeList[ItemMenuManager.Instance.ReduceCollectingTimeLv].value;

        // �ִ� ���� �ر� ����� �⺻ ��ȭ ���޷� ã��
        decimal maxUnlockedCatCoin = 0;
        for (int i = GameManager.Instance.AllCatData.Length - 1; i >= 0; i--)
        {
            if (DictionaryManager.Instance.IsCatUnlocked(i))
            {
                maxUnlockedCatCoin = GameManager.Instance.AllCatData[i].CatGetCoin;
                break;
            }
        }

        // ���� ����
        float multiplier = 1.0f;
        switch (diamondAmount)
        {
            case 100:
                multiplier = 1.2f;
                break;
            case 500:
                multiplier = 1.3f;
                break;
            case 2500:
                multiplier = 1.4f;
                break;
        }

        // ���� ���� ���: ((���̾� / ��ȭ ���� �ð�) * �ִ� ���� �ر� ����� �⺻ ��ȭ ���޷�) * ����
        decimal jellyAmount = decimal.Round((diamondAmount / (decimal)collectingTime) * maxUnlockedCatCoin * (decimal)multiplier);
        return jellyAmount;
    }

    // ���̾� ��ǰ ���� ��ư Ŭ���� ����Ǵ� �Լ�
    private void OnClickBuyCashToCoin(int diamondAmount)
    {
        // �ر� ���� üũ
        int maxUnlockedGrade = GetMaxUnlockedCatGrade();
        bool canBuy = diamondAmount switch
        {
            100 => maxUnlockedGrade >= UNLOCK_GRADE_100_CASH,
            500 => maxUnlockedGrade >= UNLOCK_GRADE_500_CASH,
            2500 => maxUnlockedGrade >= UNLOCK_GRADE_2500_CASH,
            _ => false
        };

        if (!canBuy)
        {
            int requiredGrade = diamondAmount switch
            {
                100 => UNLOCK_GRADE_100_CASH,
                500 => UNLOCK_GRADE_500_CASH,
                2500 => UNLOCK_GRADE_2500_CASH,
                _ => 0
            };
            NotificationManager.Instance.ShowNotification($"{requiredGrade}��� ����� �ر� �� ���� ����!!");
            return;
        }

        selectedCashAmount = diamondAmount;
        calculatedCoinAmount = CalculateJellyAmount(diamondAmount);

        UpdateBuyCashToCoinText();
        buyCashToCoinPanel.SetActive(true);
    }

    // ���� Ȯ�� �ؽ�Ʈ ������Ʈ �Լ�
    private void UpdateBuyCashToCoinText()
    {
        buyCashToCoinText.text = $"[{selectedCashAmount} ���̾�]��\n���� [{GameManager.Instance.FormatNumber(calculatedCoinAmount)}]����\n�����Ͻðڽ��ϱ�?";
    }

    // ���� Ȯ�� ��ư Ŭ���� ����Ǵ� �Լ�
    private void OnConfirmBuyCashToCoin()
    {
        if (GameManager.Instance.Cash < selectedCashAmount)
        {
            NotificationManager.Instance.ShowNotification("���̾ư� �����մϴ�!!");
        }
        else
        {
            GameManager.Instance.Cash -= selectedCashAmount;
            GameManager.Instance.Coin += calculatedCoinAmount;
            NotificationManager.Instance.ShowNotification($"���� [{GameManager.Instance.FormatNumber(calculatedCoinAmount)}]�� ���� �Ϸ�!!");
        }

        CloseBuyCashToCoinPanel();
    }

    // ���� Ȯ�� �г� �ݱ� �Լ�
    private void CloseBuyCashToCoinPanel()
    {
        buyCashToCoinPanel.SetActive(false);
        selectedCashAmount = 0;
        calculatedCoinAmount = 0;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public long lastAdTimeReward;               // ������ CashForAd ���� ���� �ð�
        public long lastTimeReward;                 // ������ CasfFroTime ���� ���� �ð�
        public long lastDoubleCoinTimeReward;       // ������ DoubleCoinForAd ���� ���� �ð�
        public float multiplierEndTimeOffset;       // ��ȭ ȹ�淮 2�� ȿ�� ������� ���� �ð�
    }

    public string GetSaveData()
    {
        float remainingTime = multiplierEndTime - Time.time;

        SaveData data = new SaveData
        {
            lastAdTimeReward = this.lastAdTimeReward,
            lastTimeReward = this.lastTimeReward,
            lastDoubleCoinTimeReward = this.lastDoubleCoinTimeReward,
            multiplierEndTimeOffset = remainingTime > 0 ? remainingTime : 0
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        this.lastAdTimeReward = savedData.lastAdTimeReward;
        this.lastTimeReward = savedData.lastTimeReward;
        this.lastDoubleCoinTimeReward = savedData.lastDoubleCoinTimeReward;

        // ����� ��� ȿ���� �ִٸ� ����
        if (savedData.multiplierEndTimeOffset > 0)
        {
            this.coinMultiplier = doubleCoinMultiplier;
            this.multiplierEndTime = Time.time + savedData.multiplierEndTimeOffset;
        }
        else
        {
            this.coinMultiplier = 1f;
            this.multiplierEndTime = 0f;
        }

        // ���� ���� ���� �ʱ�ȭ
        isBattlePaused = false;
        battlePauseTime = 0f;

        UpdateAllUI();
        UpdateDiamondShopUnlockState();
    }

    #endregion


}
