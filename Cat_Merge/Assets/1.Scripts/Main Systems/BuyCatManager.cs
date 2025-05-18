using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// ����� ���� ��ũ��Ʈ
[DefaultExecutionOrder(-3)]
public class BuyCatManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static BuyCatManager Instance { get; private set; }

    [Header("---[BuyCat]")]
    [SerializeField] private ScrollRect buyCatScrollRect;       // ����� ���� ��ũ�Ѻ�
    [SerializeField] private Button bottomBuyCatButton;         // ����� ���� ��ư
    [SerializeField] private Image bottomBuyCatButtonImg;       // ����� ���� ��ư �̹���
    [SerializeField] private GameObject buyCatMenuPanel;        // ������ �޴� �ǳ�
    [SerializeField] private Button buyCatBackButton;           // ����� ���� �޴� �ڷΰ��� ��ư
    [SerializeField] private GameObject buyCatSlotPrefab;       // ����� ���� ���� ������
    [SerializeField] private Transform scrollRectContents;      // ��ũ�Ѻ� ������ Transform
    [SerializeField] private TextMeshProUGUI emptyCatText;      // ���� ������ ����̰� ������ Ȱ��ȭ�Ǵ� �ؽ�Ʈ

    private Button[] buyCatCoinButtons;                         // ����� ��ȭ�� ���� ��ư
    private Button[] buyCatCashButtons;                         // ũ����Ż ��ȭ�� ���� ��ư

    private ActivePanelManager activePanelManager;              // ActivePanelManager

    // ���� ����� ��޿� ���� ���� ���� Ŭ����
    [Serializable]
    public class CatPurchaseInfo
    {
        public int catGrade;
        public int coinPurchaseCount;
        public int cashPurchaseCount;
        public long coinPrice;
        public long cashPrice;

        // ���� ���� ��� ��� �Լ�
        public long CalculateCoinPrice()
        {
            Cat catData = GameManager.Instance.AllCatData[catGrade];
            int effectivePurchaseCount = Mathf.Min(coinPurchaseCount, 100); // �ִ� 100ȸ������ ���
            double basePrice = catData.CatGetCoin * 3600.0;
            double multiplier = Math.Pow(1.128, effectivePurchaseCount);
            double exactPrice = basePrice * multiplier;

            return (long)Math.Floor(exactPrice);
        }

        // ĳ�� ���� ��� ��� �Լ�
        public long CalculateCashPrice()
        {
            Cat catData = GameManager.Instance.AllCatData[catGrade];
            return (catData.CatGrade * 10) + (5 * cashPurchaseCount);
        }
    }

    // UI ��ҵ��� ���� Ŭ����
    private class BuyCatSlotUI
    {
        public int catGrade;
        public GameObject coinDisabledBg;
        public GameObject cashDisabledBg;
        public Button coinButton;
        public Button cashButton;
        public TextMeshProUGUI countExplainText;
        public TextMeshProUGUI coinFeeText;
        public TextMeshProUGUI cashFeeText;
        public Image catImage;
        public TextMeshProUGUI titleText;
    }

    // ��� ����� ���� ������ �����ϴ� ��ųʸ�
    private Dictionary<int, CatPurchaseInfo> catPurchaseInfos = new Dictionary<int, CatPurchaseInfo>();
    private Dictionary<int, BuyCatSlotUI> catSlotUIs = new Dictionary<int, BuyCatSlotUI>();

    private bool isDataLoaded = false;              // ������ �ε� Ȯ��

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
        // GoogleManager���� �����͸� �ε����� ���� ��쿡�� �ʱ�ȭ
        if (!isDataLoaded)
        {
            InitializeCatPurchaseInfos();
        }

        InitializeBuyCatManager();
    }

    #endregion


    #region Initialize

    // �⺻ BuyCat �ʱ�ȭ �Լ�
    private void InitializeBuyCatManager()
    {
        buyCatMenuPanel.SetActive(false);
        emptyCatText.gameObject.SetActive(true);

        InitializeActivePanel();
        CreateBuyCatSlots();
        InitializeScrollPositions();
        UpdateAllUI();
    }

    // ActivePanel �ʱ�ȭ �Լ�
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BuyCatMenu", buyCatMenuPanel, bottomBuyCatButtonImg);

        bottomBuyCatButton.onClick.AddListener(() => activePanelManager.TogglePanel("BuyCatMenu"));
        buyCatBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BuyCatMenu"));
    }

    // ����� ���� ���� �ʱ�ȭ �Լ�
    private void CreateBuyCatSlots()
    {
        // ���� ���Ե� ����
        foreach (Transform child in scrollRectContents)
        {
            Destroy(child.gameObject);
        }
        catSlotUIs.Clear();

        // ��� ����� �����Ϳ� ���� ���� ����
        int catCount = GameManager.Instance.AllCatData.Length;
        buyCatCoinButtons = new Button[catCount];
        buyCatCashButtons = new Button[catCount];

        for (int i = 0; i < catCount; i++)
        {
            CreateBuyCatSlot(i);
        }
    }

    // ����� ���� ���� �߰� �Լ�
    private void CreateBuyCatSlot(int catGrade)
    {
        GameObject slotObject = Instantiate(buyCatSlotPrefab, scrollRectContents);
        Cat catData = GameManager.Instance.AllCatData[catGrade];

        BuyCatSlotUI slotUI = new BuyCatSlotUI
        {
            catGrade = catGrade,
            coinDisabledBg = slotObject.transform.Find("BackGround/BuyWithCoin Button/Coin DisabledBG").gameObject,
            cashDisabledBg = slotObject.transform.Find("BackGround/BuyWithCash Button/Cash DisabledBG").gameObject,
            coinButton = slotObject.transform.Find("BackGround/BuyWithCoin Button").GetComponent<Button>(),
            cashButton = slotObject.transform.Find("BackGround/BuyWithCash Button").GetComponent<Button>(),
            countExplainText = slotObject.transform.Find("BackGround/BuyCount Text").GetComponent<TextMeshProUGUI>(),
            coinFeeText = slotObject.transform.Find("BackGround/BuyWithCoin Button/CoinFee Text").GetComponent<TextMeshProUGUI>(),
            cashFeeText = slotObject.transform.Find("BackGround/BuyWithCash Button/CashFee Text").GetComponent<TextMeshProUGUI>(),
            catImage = slotObject.transform.Find("BackGround/Cat Image BackGround").GetComponent<Image>(),
            titleText = slotObject.transform.Find("BackGround/Title Text").GetComponent<TextMeshProUGUI>()
        };

        // ��ư �迭�� ����
        buyCatCoinButtons[catGrade] = slotUI.coinButton;
        buyCatCashButtons[catGrade] = slotUI.cashButton;

        // ��ư �̺�Ʈ ���� - catGrade�� Ŭ������ ĸó
        int capturedGrade = catGrade;
        slotUI.coinButton.onClick.AddListener(() => BuyCatCoin(slotUI.coinButton, capturedGrade));
        slotUI.cashButton.onClick.AddListener(() => BuyCatCash(slotUI.cashButton, capturedGrade));

        // �⺻ ���� ����
        slotUI.catImage.sprite = catData.CatImage;
        slotUI.titleText.text = $"{catData.CatGrade}. {catData.CatName}";

        // �ʱ⿡�� ��� ������ ��Ȱ��ȭ
        slotObject.SetActive(false);

        catSlotUIs[catGrade] = slotUI;
    }

    // ���ο� ����̰� �رݵǾ��� �� ȣ��Ǵ� �Լ�
    public void UnlockBuySlot(int unlockedCatGrade)
    {
        Cat unlockedCat = GameManager.Instance.AllCatData[unlockedCatGrade];
        int canOpenerValue = unlockedCat.CanOpener - 1;

        if (canOpenerValue < 0)
        {
            return;
        }

        // CanOpener ���� �ش��ϴ� ��ޱ����� ���� ���� Ȱ��ȭ
        for (int i = 0; i <= canOpenerValue; i++)
        {
            if (catSlotUIs.TryGetValue(i, out BuyCatSlotUI slotUI))
            {
                Transform slotTransform = slotUI.coinButton.transform.parent.parent;
                slotTransform.gameObject.SetActive(true);
            }
        }

        CheckAndUpdateEmptyText();
    }

    // Ȱ��ȭ�� ������ �ִ��� Ȯ���ϰ� �ؽ�Ʈ ǥ�ø� ������Ʈ�ϴ� �Լ�
    private void CheckAndUpdateEmptyText()
    {
        bool hasActiveSlot = false;
        foreach (var slotUI in catSlotUIs.Values)
        {
            Transform slotTransform = slotUI.coinButton.transform.parent.parent;
            if (slotTransform.gameObject.activeSelf)
            {
                hasActiveSlot = true;
                break;
            }
        }

        // Ȱ��ȭ�� ������ ������ �ؽ�Ʈ�� �����, ������ �ؽ�Ʈ�� ǥ��
        emptyCatText.gameObject.SetActive(!hasActiveSlot);
    }

    // �ʱ� ��ũ�� ��ġ �ʱ�ȭ �Լ�
    private void InitializeScrollPositions()
    {
        if (buyCatScrollRect != null && buyCatScrollRect.content != null)
        {
            // ��ü ���� ���� ��� (�� �ٿ� 1����)
            int totalSlots = GameManager.Instance.AllCatData.Length;

            // ���� ������ ���� ������ ���� (�� �ٿ� 1����)
            int rowCount = totalSlots;

            // Grid Layout Group ������
            const int SPACING_Y = 0;            // y spacing
            const int CELL_SIZE_Y = 150;        // y cell size
            const int VIEWPORT_HEIGHT = 1180;   // viewport height

            // ��ü ������ ���� ���: (�� ���� * (�� ����-1)) + (�� ���� * �� ����)
            float contentHeight = (SPACING_Y * (rowCount - 1)) + (CELL_SIZE_Y * rowCount);

            // ��ũ�� ���� ��ġ ���: -(��ü ���� - ����Ʈ ����) * 0.5f
            float targetY = -(contentHeight - VIEWPORT_HEIGHT) * 0.5f;

            // ��ũ�� �������� ��ġ�� ������� ����
            buyCatScrollRect.content.anchoredPosition = new Vector2(
                buyCatScrollRect.content.anchoredPosition.x,
                targetY
            );

            // ��ũ�� �ӵ� �ʱ�ȭ
            buyCatScrollRect.velocity = Vector2.zero;
        }
    }

    // ����� ���� ���� �ʱ�ȭ �Լ�
    private void InitializeCatPurchaseInfos()
    {
        if (!GameManager.Instance) return;

        catPurchaseInfos.Clear();
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            var info = new CatPurchaseInfo
            {
                catGrade = i,
                coinPurchaseCount = 0,
                cashPurchaseCount = 0
            };
            info.coinPrice = info.CalculateCoinPrice();
            info.cashPrice = info.CalculateCashPrice();
            catPurchaseInfos[i] = info;
        }

        UpdateAllUI();
    }

    // ��ü UI ������Ʈ �Լ�
    private void UpdateAllUI()
    {
        // �ʱ�ȭ �߿��� �������� �ʵ��� ����
        bool shouldSave = isDataLoaded;
        isDataLoaded = false;

        foreach (var pair in catPurchaseInfos)
        {
            int catGrade = pair.Key;
            var info = pair.Value;
            if (catSlotUIs.TryGetValue(catGrade, out BuyCatSlotUI slotUI))
            {
                slotUI.countExplainText.text = $"���� Ƚ�� : {info.coinPurchaseCount}ȸ + {info.cashPurchaseCount}ȸ";
                slotUI.coinFeeText.text = $"{GameManager.Instance.FormatNumber(info.CalculateCoinPrice())}";
                slotUI.cashFeeText.text = $"{GameManager.Instance.FormatNumber(info.CalculateCashPrice())}";
            }
        }

        isDataLoaded = shouldSave;
    }

    #endregion


    #region Button System

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCoin(Button button, int catGrade)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("����� �������� �ִ��Դϴ�!!");
            return;
        }

        var info = GetCatPurchaseInfo(catGrade);
        long currentPrice = info.CalculateCoinPrice();

        if (GameManager.Instance.Coin < currentPrice)
        {
            NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!!");
            return;
        }

        GameManager.Instance.Coin -= currentPrice;
        info.coinPurchaseCount++;
        info.coinPrice = info.CalculateCoinPrice(); // ���� ���� ���� ������Ʈ

        ProcessCatPurchase(catGrade);
    }

    // ����� ���� ��ư �Լ�(ũ����Ż�� ����)
    private void BuyCatCash(Button button, int catGrade)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("����� �������� �ִ��Դϴ�!!");
            return;
        }

        var info = GetCatPurchaseInfo(catGrade);
        long currentPrice = info.CalculateCashPrice();

        if (GameManager.Instance.Cash < currentPrice)
        {
            NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!!");
            return;
        }

        GameManager.Instance.Cash -= currentPrice;
        info.cashPurchaseCount++;
        info.cashPrice = info.CalculateCashPrice(); // ���� ���� ���� ������Ʈ

        ProcessCatPurchase(catGrade);
    }

    // ����� ���� ó�� �Լ�
    private void ProcessCatPurchase(int catGrade)
    {
        //QuestManager.Instance.AddPurchaseCatsCount();
        DictionaryManager.Instance.UnlockCat(catGrade);
        GetComponent<SpawnManager>().SpawnGradeCat(catGrade);

        UpdateAllUI();
    }

    // ����� ���� ���� ��������
    private CatPurchaseInfo GetCatPurchaseInfo(int catGrade)
    {
        return catPurchaseInfos[catGrade];
    }

    #endregion


    #region Battle System

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleBuyCatState()
    {
        bottomBuyCatButton.interactable = false;

        if (buyCatMenuPanel.activeSelf == true)
        {
            activePanelManager.TogglePanel("BuyCatMenu");
        }
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleBuyCatState()
    {
        bottomBuyCatButton.interactable = true;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public List<CatPurchaseInfo> purchaseInfos = new List<CatPurchaseInfo>();
        public List<int> unlockedSlots = new List<int>();
    }

    public string GetSaveData()
    {
        SaveData saveData = new SaveData();
        foreach (var infos in catPurchaseInfos)
        {
            saveData.purchaseInfos.Add(new CatPurchaseInfo
            {
                catGrade = infos.Value.catGrade,
                coinPurchaseCount = infos.Value.coinPurchaseCount,
                cashPurchaseCount = infos.Value.cashPurchaseCount,
                coinPrice = infos.Value.coinPrice,
                cashPrice = infos.Value.cashPrice
            });
        }

        // Ȱ��ȭ�� ���Ե��� ��� ����
        foreach (var pair in catSlotUIs)
        {
            Transform slotTransform = pair.Value.coinButton.transform.parent.parent;
            if (slotTransform.gameObject.activeSelf)
            {
                saveData.unlockedSlots.Add(pair.Key);
            }
        }
        return JsonUtility.ToJson(saveData);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        catPurchaseInfos.Clear();
        foreach (var info in savedData.purchaseInfos)
        {
            catPurchaseInfos[info.catGrade] = new CatPurchaseInfo
            {
                catGrade = info.catGrade,
                coinPurchaseCount = info.coinPurchaseCount,
                cashPurchaseCount = info.cashPurchaseCount,
                coinPrice = info.coinPrice,
                cashPrice = info.cashPrice
            };
        }

        CreateBuyCatSlots();

        // ���Ե��� Ȱ��ȭ ���� ����
        foreach (var pair in catSlotUIs)
        {
            Transform slotTransform = pair.Value.coinButton.transform.parent.parent;
            slotTransform.gameObject.SetActive(savedData.unlockedSlots.Contains(pair.Key));
        }
        CheckAndUpdateEmptyText();
        UpdateAllUI();

        isDataLoaded = true;
    }

    #endregion


}
