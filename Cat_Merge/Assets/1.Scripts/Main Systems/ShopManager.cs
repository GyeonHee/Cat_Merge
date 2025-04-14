using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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

    [Header("---[Free]")]
    [SerializeField] private Button cashForAdRewardButton;              // ���� ��û���� ĳ�� ȹ�� (���� ��û �Ұ��� �����϶� ��Ȱ��ȭ = interactable ��Ȱ��ȭ)
    [SerializeField] private TextMeshProUGUI cashForAdCoolTimeText;     // ���� ��û ��Ÿ�� Text
    [SerializeField] private GameObject cashForAdRewardOnImage;         // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject cashForAdRewardOffImage;        // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject cashForAdNewImage;              // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� New �̹��� ������Ʈ
    [SerializeField] private GameObject cashForAdDisabledBG;            // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    private int cashForAdCoolTime = 20;                                // ���� ��û ��Ÿ�� (600��)(������ ����ǵ� �ð��� �帣���� ���� �ð��� �������� ���)
    private long lastAdTimeReward = 0;                                  // ������ cashForAd ���� �ð� ����
    private long remainingCashAdCoolTimeBeforeAd = 0;                   // ���� ��û �� ���� ��Ÿ���� ����
    private int cashForAdPrice = 30;                                    // cashForAd ����

    [SerializeField] private Button cashForTimeRewardButton;            // ��Ÿ�Ӹ��� Ȱ��ȭ�Ǵ� ���� ĳ�� ȹ�� (���� ĳ�� ȹ�� �Ұ��� �����϶� ��Ȱ��ȭ = interactable ��Ȱ��ȭ)
    [SerializeField] private TextMeshProUGUI cashForTimeCoolTimeText;   // ���� ��û ��Ÿ�� Text
    [SerializeField] private GameObject cashForTimeNewImage;            // ���� ĳ�� ȹ�� ���� �����϶� Ȱ��ȭ�Ǵ� New �̹��� ������Ʈ
    [SerializeField] private GameObject cashForTimeDisabledBG;          // ���� ĳ�� ȹ�� �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    private int cashForTimeCoolTime = 20;                              // ���� ĳ�� ȹ�� ��Ÿ�� (600��)(������ ����ǵ� �ð��� �帣���� ���� �ð��� �������� ���)
    private long lastTimeReward = 0;                                    // ������ cashForTime ���� �ð� ����
    private long remainingCoolTimeBeforeAd = 0;                         // ���� ��û �� ���� ��Ÿ���� ����
    private int cashForTimePrice = 5;                                   // cashForTime ����

    [Header("---[Ad]")]
    [SerializeField] private Button doubleCoinForAdButton;                  // ���� ��û���� ���� ȹ�淮 ���� ȿ�� ȹ��
    [SerializeField] private TextMeshProUGUI doubleCoinForAdCoolTimeText;   // ���� ��û ��Ÿ�� Text
    [SerializeField] private GameObject doubleCoinForAdRewardOnImage;       // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject doubleCoinForAdRewardOffImage;      // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject doubleCoinForAdNewImage;            // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� New �̹��� ������Ʈ
    [SerializeField] private GameObject doubleCoinForAdDisabledBG;          // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    private int doubleCoinForAdCoolTime = 20;                              // ���� ȹ�淮 ��Ÿ�� (600��)(������ ����ǵ� �ð��� �帣���� ���� �ð��� �������� ���)
    private long lastDoubleCoinTimeReward = 0;                              // ������ doubleCoinForAd ���� �ð� ����
    private long remainingDoubleCoinCoolTimeBeforeAd = 0;                   // ���� ��û �� ���� ��Ÿ���� ����
    
    private const float doubleCoinDuration = 10f;                          // ���� 2�� ���ӽð� (300��)
    private const float doubleCoinMultiplier = 2f;                          // ���� ȹ�淮 ���
    private float coinMultiplier = 1f;                                      // ���� ���� ���
    private float multiplierEndTime = 0f;                                   // ��� ȿ�� ���� �ð�

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
        StartCoroutine(CheckRewardStatus());
    }

    private void OnDisable()
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

    }

    // New �̹��� ���� ������Ʈ �Լ�
    private void UpdateShopNewImage()
    {
        bool isAnyNewImageActive = cashForAdNewImage.activeSelf || cashForTimeNewImage.activeSelf || doubleCoinForAdNewImage.activeSelf;
        shopNewImage.SetActive(isAnyNewImageActive);
    }

    #endregion


    #region Free Shop System

    // �ֱ������� ���� ���¸� üũ�ϴ� �ڷ�ƾ
    private IEnumerator CheckRewardStatus()
    {
        WaitForSeconds waitTime = new WaitForSeconds(1f);

        while (true)
        {
            UpdateCashForAdUI();
            UpdateCashForTimeUI();

            UpdateDoubleCoinForAdUI();

            UpdateShopNewImage();
            yield return waitTime;
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

        // CashForTime�� DoubleCoinAd�� ���� ��Ÿ�� ����
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        remainingCoolTimeBeforeAd = Math.Max(0, cashForTimeCoolTime - (currentTime - lastTimeReward));
        remainingDoubleCoinCoolTimeBeforeAd = Math.Max(0, doubleCoinForAdCoolTime - (currentTime - lastDoubleCoinTimeReward));

        // TODO: ���� ��� ���� �߰�
        isWaitingForAd = true;
        cashForAdRewardButton.interactable = false;

        GetComponent<GoogleAdsManager>().ShowRewardedCashForAd();
    }

    // ���� ��û �Ϸ� �� ����Ǵ� �Լ�
    public void OnAdRewardComplete()
    {
        // ���� ���� ����
        GameManager.Instance.Cash += cashForAdPrice;

        // ������ ���� ��û �ð� ����
        lastAdTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // CashForTime�� ������ ���� �ð��� ���� ��û �ð���ŭ ����
        if (remainingCoolTimeBeforeAd > 0)
        {
            lastTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (cashForTimeCoolTime - remainingCoolTimeBeforeAd);
        }

        // DoubleCoinAd�� ������ ���� �ð��� ���� ��û �ð���ŭ ����
        if (remainingDoubleCoinCoolTimeBeforeAd > 0)
        {
            lastDoubleCoinTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (doubleCoinForAdCoolTime - remainingDoubleCoinCoolTimeBeforeAd);
        }

        GoogleSave();

        isWaitingForAd = false;
        UpdateCashForAdUI();
    }

    // CashForTime UI ������Ʈ �Լ�
    private void UpdateCashForTimeUI()
    {
        bool isTimeRewardAvailable = IsCashForTimeAvailable();

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remainTime = Math.Max(0, cashForTimeCoolTime - (currentTime - lastTimeReward));

        cashForTimeRewardButton.interactable = isTimeRewardAvailable;
        cashForTimeNewImage.SetActive(isTimeRewardAvailable);
        cashForTimeDisabledBG.SetActive(!isTimeRewardAvailable);

        if (isTimeRewardAvailable)
        {
            cashForTimeCoolTimeText.text = "�غ� �Ϸ�";
        }
        else
        {
            cashForTimeCoolTimeText.text = $"��Ÿ�� {(int)remainTime}��";
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
        GameManager.Instance.Cash += cashForTimePrice;

        // ������ ���� �ð� ����
        lastTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        GoogleSave();

        UpdateCashForTimeUI();
    }

    #endregion


    #region Ad Shop System

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
        get => Time.time < multiplierEndTime ? coinMultiplier : 1f;
    }

    // ��� ȿ�� ���� �ð��� �������� �Լ�
    private float GetRemainingEffectTime()
    {
        return Mathf.Max(0f, multiplierEndTime - Time.time);
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

        // CashForTime�� CashForAd�� ���� ��Ÿ�� ����
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        remainingCoolTimeBeforeAd = Math.Max(0, cashForTimeCoolTime - (currentTime - lastTimeReward));
        remainingCashAdCoolTimeBeforeAd = Math.Max(0, cashForAdCoolTime - (currentTime - lastAdTimeReward));

        isWaitingForAd = true;
        doubleCoinForAdButton.interactable = false;

        GetComponent<GoogleAdsManager>().ShowRewardedDoubleCoinForAd();
    }

    // DoubleCoinForAd ���� ��û �Ϸ� �� ����Ǵ� �Լ�
    public void OnDoubleCoinAdRewardComplete()
    {
        // ���� ���� ���� - ��� ������� ��ȭ ���޷� 300�ʵ��� 2��� ����
        coinMultiplier = doubleCoinMultiplier;
        multiplierEndTime = Time.time + doubleCoinDuration;

        // ������ ���� ��û �ð� ����
        lastDoubleCoinTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // CashForTime�� ������ ���� �ð��� ���� ��û �ð���ŭ ����
        if (remainingCoolTimeBeforeAd > 0)
        {
            lastTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (cashForTimeCoolTime - remainingCoolTimeBeforeAd);
        }

        // CashForAd�� ������ ���� �ð��� ���� ��û �ð���ŭ ����
        if (remainingCashAdCoolTimeBeforeAd > 0)
        {
            lastAdTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (cashForAdCoolTime - remainingCashAdCoolTimeBeforeAd);
        }

        GoogleSave();

        isWaitingForAd = false;
        UpdateDoubleCoinForAdUI();
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public long lastAdTimeReward;           // 
        public long lastTimeReward;             // 
        public long lastDoubleCoinTimeReward;   // 
        public float multiplierEndTimeOffset;   // ȿ�� ������� ���� �ð�
        public float coinMultiplier;            // ���� ���� ���� ���
    }

    public string GetSaveData()
    {
        float remainingTime = multiplierEndTime - Time.time;                    // ���� �ð� ���

        SaveData data = new SaveData
        {
            lastAdTimeReward = this.lastAdTimeReward,
            lastTimeReward = this.lastTimeReward,
            lastDoubleCoinTimeReward = this.lastDoubleCoinTimeReward,
            multiplierEndTimeOffset = remainingTime > 0 ? remainingTime : 0,    // ���� �ð��� ���� ���� ����
            coinMultiplier = this.coinMultiplier
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
            this.coinMultiplier = savedData.coinMultiplier;
            this.multiplierEndTime = Time.time + savedData.multiplierEndTimeOffset;
        }
        else
        {
            this.coinMultiplier = 1f;
            this.multiplierEndTime = 0f;
        }

        UpdateAllUI();
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}