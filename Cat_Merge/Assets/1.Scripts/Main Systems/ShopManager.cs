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

    [Header("---[Free]")]
    [SerializeField] private Button cashForAdRewardButton;              // ���� ��û���� ĳ�� ȹ�� (���� ��û �Ұ��� �����϶� ��Ȱ��ȭ = interactable ��Ȱ��ȭ)
    [SerializeField] private TextMeshProUGUI cashForAdCoolTimeText;     // ���� ��û ��Ÿ�� Text
    [SerializeField] private GameObject cashForAdRewardOnImage;         // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject cashForAdRewardOffImage;        // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    [SerializeField] private GameObject cashForAdNewImage;              // ���� ��û ���� �����϶� Ȱ��ȭ�Ǵ� New �̹��� ������Ʈ
    [SerializeField] private GameObject cashForAdDisabledBG;            // ���� ��û �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    private int cashForAdCoolTime = 10;                                // ���� ��û ��Ÿ�� (������ ����ǵ� �ð��� �帣���� ���� �ð��� �������� ���)
    private long lastAdTimeReward = 0;
    private int cashForAdPrice = 30;
    
    [SerializeField] private Button cashForTimeRewardButton;            // ��Ÿ�Ӹ��� Ȱ��ȭ�Ǵ� ���� ĳ�� ȹ�� (���� ĳ�� ȹ�� �Ұ��� �����϶� ��Ȱ��ȭ = interactable ��Ȱ��ȭ)
    [SerializeField] private TextMeshProUGUI cashForTimeCoolTimeText;   // ���� ��û ��Ÿ�� Text
    [SerializeField] private GameObject cashForTimeNewImage;            // ���� ĳ�� ȹ�� ���� �����϶� Ȱ��ȭ�Ǵ� New �̹��� ������Ʈ
    [SerializeField] private GameObject cashForTimeDisabledBG;          // ���� ĳ�� ȹ�� �Ұ��� �����϶� Ȱ��ȭ�Ǵ� �̹��� ������Ʈ
    private int cashForTimeCoolTime = 10;                              // ���� ĳ�� ȹ�� ��Ÿ�� (������ ����ǵ� �ð��� �帣���� ���� �ð��� �������� ���)
    private long lastTimeReward = 0;
    private int cashForTimePrice = 5;


    private bool isWaitingForAd = false;  // ���� ��� ������ Ȯ���ϴ� ���� �߰�

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
        StopAllCoroutines();  // �ڷ�ƾ ����
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



    }

    // ��ư ������ �ʱ�ȭ �Լ�
    private void InitializeButtonListeners()
    {
        cashForAdRewardButton.onClick.AddListener(OnClickCashForAd);
        cashForTimeRewardButton.onClick.AddListener(OnClickCashForTime);


    }

    // New �̹��� ���� ������Ʈ �Լ�
    private void UpdateShopNewImage()
    {
        bool isAnyNewImageActive = cashForAdNewImage.activeSelf || cashForTimeNewImage.activeSelf;
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
            cashForAdCoolTimeText.text = "�غ� �Ϸ�"; //$"��Ÿ�� {cashForAdCoolTime}��";
        }
        else
        {
            cashForAdCoolTimeText.text = $"��Ÿ�� {remainTime}��";
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

        // TODO: ���� ��� ���� �߰�
        // �׽�Ʈ�� 3�� ��� �� ���� ���� �ڷ�ƾ
        StartCoroutine(TestAdCoroutine());
    }

    // �׽�Ʈ�� ���� �ڷ�ƾ
    private IEnumerator TestAdCoroutine()
    {
        // 3�� ���
        float waitTime = 3f;
        float elapsed = 0f;

        while (elapsed < waitTime)
        {
            elapsed += Time.deltaTime;

            // ���⿡ ���� ���¸� ǥ���ϴ� UI�� �߰� ����
            yield return null;
        }

        // ���� ��û �Ϸ� ó��
        isWaitingForAd = true;
        cashForAdRewardButton.interactable = false;
        OnAdRewardComplete();
        isWaitingForAd = false;
    }

    // ���� ��û �Ϸ� �� ����Ǵ� �Լ�
    private void OnAdRewardComplete()
    {
        // ���� ���� ����
        GameManager.Instance.Cash += cashForAdPrice;

        // ������ ���� ��û �ð� ����
        lastAdTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        GoogleSave();

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
            cashForTimeCoolTimeText.text = "�غ� �Ϸ�"; //$"��Ÿ�� {cashForTimeCoolTime}��";
        }
        else
        {
            cashForTimeCoolTimeText.text = $"��Ÿ�� {remainTime}��";
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


    #region Save System

    [Serializable]
    private class SaveData
    {
        public long lastAdTimeReward;
        public long lastTimeReward;
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            lastAdTimeReward = this.lastAdTimeReward,
            lastTimeReward = this.lastTimeReward
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        this.lastAdTimeReward = savedData.lastAdTimeReward;
        this.lastTimeReward = savedData.lastTimeReward;

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
