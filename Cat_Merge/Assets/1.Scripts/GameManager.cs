using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// GameManager Script
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }        // SingleTon

    // Data
    private Cat[] allCatData;                                       // ��� ����� ������ ����
    public Cat[] AllCatData => allCatData;
    private bool[] isCatUnlocked;                                   // ����� �ر� ���� �迭

    // ======================================================================================================================

    // ����Ʈ�� ���� ������
    private int feedCount;                                          // ����� ���� Ƚ��(���� �� Ƚ��)
    public int FeedCount { get => feedCount; set => feedCount = value; }

    private int combineCount;                                       // ����� ���� Ƚ��
    public int CombineCount { get => combineCount; set => combineCount = value; }

    private int getCoinCount;                                       // ȹ������ ����
    public int GetCoinCount { get => getCoinCount; set => getCoinCount = value; }

    private float playTimeCount;                                    // �÷���Ÿ�� ī��Ʈ
    public float PlayTimeCount { get => playTimeCount; set => playTimeCount = value; }

    private int purchaseCatsCount;                                  // ����� ���� Ƚ��
    public int PurchaseCatsCount { get => purchaseCatsCount; set => purchaseCatsCount = value; }

    // ======================================================================================================================

    // Main UI Text
    [Header("---[Main UI Text]")]
    [SerializeField] private TextMeshProUGUI catCountText;          // ����� �� �ؽ�Ʈ
    private int currentCatCount = 0;                                // ȭ�� �� ����� ��
    private int maxCats = 8;                                        // �ִ� ����� ��

    // �⺻ ��ȭ
    [SerializeField] private TextMeshProUGUI coinText;              // �⺻��ȭ �ؽ�Ʈ
    private int coin = 1000;                                        // �⺻��ȭ

    // ĳ�� ��ȭ
    [SerializeField] private TextMeshProUGUI cashText;              // ĳ����ȭ �ؽ�Ʈ
    private int cash = 1000;                                        // ĳ����ȭ

    // ======================================================================================================================

    // Merge On/Off
    [Header("---[Merge On/Off]")]
    [SerializeField] private Button openMergePanelButton;           // ���� �г� ���� ��ư
    [SerializeField] private GameObject mergePanel;                 // ���� On/Off �г�
    [SerializeField] private Button closeMergePanelButton;          // ���� �г� �ݱ� ��ư
    [SerializeField] private Button mergeStateButton;               // ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI mergeStateText;        // ���� ���� ���� �ؽ�Ʈ
    private bool isMergeEnabled = true;

    // ======================================================================================================================

    // AutoMove On/Off
    [Header("----[AutoMove On/Off]")]
    [SerializeField] private Button openAutoMovePanelButton;        // �ڵ� �̵� �г� ���� ��ư
    [SerializeField] private GameObject autoMovePanel;              // �ڵ� �̵� On/Off �г�
    [SerializeField] private Button closeAutoMovePanelButton;       // �ڵ� �̵� �г� �ݱ� ��ư
    [SerializeField] private Button autoMoveStateButton;            // �ڵ� �̵� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMoveStateText;     // �ڵ� �̵� ���� ���� �ؽ�Ʈ
    private bool isAutoMoveEnabled = true;                          // �ڵ� �̵� Ȱ��ȭ ����

    // ======================================================================================================================

    // AutoMerge
    [Header("---[AutoMerge]")]
    [SerializeField] private Button openAutoMergePanelButton;       // �ڵ� ���� �г� ���� ��ư
    [SerializeField] private GameObject autoMergePanel;             // �ڵ� ���� �г�
    [SerializeField] private Button closeAutoMergePanelButton;      // �ڵ� ���� �г� �ݱ� ��ư
    [SerializeField] private Button autoMergeStateButton;           // �ڵ� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMergeCostText;     // �ڵ� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMergeTimerText;    // �ڵ� ���� Ÿ�̸� �ؽ�Ʈ
    private int autoMergeCost = 30;                                 // �ڵ� ���� ���

    // Sort System
    [Header("---[Sort]")]
    [SerializeField] private Button sortButton;                     // ���� ��ư
    [SerializeField] private Transform gamePanel;                   // GamePanel

    // ======================================================================================================================

    // ItemMenu Select Button
    [Header("ItemMenu")]
    [SerializeField] private Button bottomItemButton;               // ������ ��ư
    [SerializeField] private Image bottomItemButtonImg;             // ������ ��ư �̹���

    [SerializeField] private GameObject itemMenuPanel;              // ������ �޴� �ǳ�
    private bool isOnToggleItemMenu;                                // ������ �޴� �ǳ� ���
    [SerializeField] private Button itemBackButton;                 // ������ �ڷΰ��� ��ư

    // ������ �޴� ��ư �׷�(�迭�� ������)
    enum EitemMenuButton
    {
        itemMenuButton = 0,
        colleagueMenuButton,
        backgroundMenuButton,
        goldenSomethingMenuButton,
        end,
    }
    // ������ �޴� �ǳڵ� �׷�(�迭�� ������)
    enum EitemMenues
    {
        itemMenues = 0,
        colleagueMenues,
        backgroundMenues,
        goldenMenues,
        end,
    }

    // ������ �޴� �ȿ� ������, ����, ���, Ȳ�ݱ����� ��ư��
    [Header("ItemMenues")]
    [SerializeField] private Button[] itemMenuButtons;              // itemPanel������ ������ �޴� ��ư��
    [SerializeField] private GameObject[] itemMenues;               // itemPanel������ �޴� ��ũ��â �ǳ�
    private bool[] isItemMenuButtonsOn = new bool[4];               // ��ư ���� �����ϱ� ���� boolŸ�� �迭


    // ������ �޴� �ȿ� ������ ���
    [Header("ItemMenuList")]
    [SerializeField] private Button catMaximumIncreaseButton;           // ����� �ִ�ġ ���� ��ư
    [SerializeField] private GameObject disabledBg;                     // ��ư Ŭ�� ���� ���� ���
    private int catMaximumIncreaseFee = 10;                             // ����� �ִ�ġ ���� ���
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseText;    // ����� �ִ�ġ ���� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseLvText;  // ����� �ִ�ġ ���� ���� �ؽ�Ʈ
    private int catMaximumIncreaseLv = 1;                               // ����� �ִ�ġ ���� ����

    // ======================================================================================================================
    [Header("BuyCat")]
    [SerializeField] private Button bottomBuyCatButton;               // ����� ���� ��ư
    [SerializeField] private Image bottomBuyCatButtonImg;             // ����� ���� ��ư �̹���
    [SerializeField] private GameObject buyCatCoinDisabledBg;         // ��ư Ŭ�� ���� ���� ���
    [SerializeField] private GameObject buyCatCashDisabledBg;         // ��ư Ŭ�� ���� ���� ���
    [SerializeField] private GameObject buyCatMenuPanel;              // ������ �޴� �ǳ�
    private bool isOnToggleBuyCat;                                    // ������ �޴� �ǳ� ���
    [SerializeField] private Button buyCatBackButton;                 // ����� ���� �޴� �ڷΰ��� ��ư

    [SerializeField] private Button buyCatCoinButton;                // ����� ��ȭ�� ���� ��ư
    [SerializeField] private Button buyCatCashButton;                // ũ����Ż ��ȭ�� ���� ��ư

    [SerializeField] private TextMeshProUGUI buyCatCountExplainText;  // ����� ���� Ƚ�� ����â
    [SerializeField] private TextMeshProUGUI buyCatCoinFeeText;       // ����� ���� ��� (���)
    [SerializeField] private TextMeshProUGUI buyCatCashFeeText;       // ����� ���� ��� (ũ����Ż)
    private int buyCatCoinCount = 0;                                  // ����� ���� Ƚ��(����)
    private int buyCatCashCount = 0;                                  // ����� ���� Ƚ��(ũ����Ż)
    private int buyCatCoinFee = 5;                                    // ����� ���� ��� (����)
    private int buyCatCashFee = 5;                                    // ����� ���� ��� (ũ����Ż)

    // ======================================================================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // �⺻ ����
        LoadAllCats();
        UpdateCatCountText();
        UpdateCoinText();
        UpdateCashText();

        // �ڵ��̵� On/Off ����
        UpdateAutoMoveStateText();
        UpdateOpenButtonColor();
        openAutoMovePanelButton.onClick.AddListener(OpenAutoMovePanel);
        closeAutoMovePanelButton.onClick.AddListener(CloseAutoMovePanel);
        autoMoveStateButton.onClick.AddListener(ToggleAutoMove);

        // ���� On/Off ����
        UpdateMergeStateText();
        UpdateMergeButtonColor();
        openMergePanelButton.onClick.AddListener(OpenMergePanel);
        closeMergePanelButton.onClick.AddListener(CloseMergePanel);
        mergeStateButton.onClick.AddListener(ToggleMergeState);

        // ���� ����
        sortButton.onClick.AddListener(SortCats);

        // �ڵ����� ����
        UpdateAutoMergeCostText();
        openAutoMergePanelButton.onClick.AddListener(OpenAutoMergePanel);
        closeAutoMergePanelButton.onClick.AddListener(CloseAutoMergePanel);
        autoMergeStateButton.onClick.AddListener(StartAutoMerge);
        UpdateAutoMergeTimerVisibility(false);

        // ������ �޴� ����
        bottomItemButton.onClick.AddListener(OpenCloseBottomItemMenuPanel);
        catMaximumIncreaseButton.onClick.AddListener(IncreaseCatMaximum);
        itemBackButton.onClick.AddListener(CloseBottomItemMenuPanel);

        itemMenues[(int)EitemMenues.itemMenues].SetActive(true);
        if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
        {
            itemMenuButtons[(int)EitemMenuButton.itemMenuButton].GetComponent<Image>().color = parsedColor;
        }

        itemMenuButtons[(int)EitemMenuButton.itemMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.itemMenues]));
        itemMenuButtons[(int)EitemMenuButton.colleagueMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.colleagueMenues]));
        itemMenuButtons[(int)EitemMenuButton.backgroundMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.backgroundMenues]));
        itemMenuButtons[(int)EitemMenuButton.goldenSomethingMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.goldenMenues]));

        for(int i = 1; i < 4; i++)
        {
            isItemMenuButtonsOn[i] = false;
        }
        UpdateIncreaseMaximumFeeText();
        UpdateIncreaseMaximumLvText();

        // ����� ���� �޴� ����
        bottomBuyCatButton.onClick.AddListener(OpenCloseBottomBuyCatMenuPanel);
        buyCatBackButton.onClick.AddListener(CloseBottomBuyCatMenuPanel);

        buyCatCoinButton.onClick.AddListener(BuyCatCoin);
        buyCatCashButton.onClick.AddListener(BuyCatCash);
    }

    // ======================================================================================================================

    // ���� �ر� ����

    private void Start()
    {
        LoadUnlockedCats();
    }

    // ��� �ر� ���� �ҷ�����
    public void LoadUnlockedCats()
    {
        InitializeCatUnlockData();
    }

    // ����� �ر� ���� �ʱ�ȭ
    public void InitializeCatUnlockData()
    {
        isCatUnlocked = new bool[AllCatData.Length];

        // ��� ����̸� ��� ���·� �ʱ�ȭ
        for (int i = 0; i < isCatUnlocked.Length; i++)
        {
            isCatUnlocked[i] = false;
        }
    }

    // Ư�� ����̸� �ر�
    public void UnlockCat(int CatGrade)
    {
        if (CatGrade < 0 || CatGrade >= isCatUnlocked.Length || isCatUnlocked[CatGrade])
        {
            return;
        }

        isCatUnlocked[CatGrade] = true;
        SaveUnlockedCats(CatGrade);
    }

    // Ư�� ������� �ر� ���� Ȯ��
    public bool IsCatUnlocked(int CatGrade)
    {
        if (CatGrade < 0 || CatGrade >= isCatUnlocked.Length)
        {
            return false;
        }

        return isCatUnlocked[CatGrade];
    }

    // ��� �ر� ���� ����
    public void SaveUnlockedCats(int CatGrade)
    {
        GetComponent<DictionaryManager>().UpdateDictionary(CatGrade);
        GetComponent<DictionaryManager>().ShowNewCatPanel(CatGrade);
    }

    // ======================================================================================================================

    // ����Ʈ ����

    private void Update()
    {
        AddPlayTimeCount();
    }

    public void AddCash(int amount)
    {
        cash += amount;
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
    {   // ����̰� ��ȭ�� �ڵ����� ���������� ȣ��
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
    {   // ����̸� �����Ҷ� ȣ��
        PurchaseCatsCount++;
    }

    public void ResetPurchaseCatsCount(int count)
    {
        PurchaseCatsCount = count;
    }

    // ======================================================================================================================

    // ����� ���� Load �Լ�
    private void LoadAllCats()
    {
        // CatDataLoader���� catDictionary ��������
        CatDataLoader catDataLoader = FindObjectOfType<CatDataLoader>();
        if (catDataLoader == null || catDataLoader.catDictionary == null)
        {
            Debug.LogError("CatDataLoader�� ���ų� ����� �����Ͱ� �ε���� �ʾҽ��ϴ�.");
            return;
        }

        // Dictionary�� ��� ���� �迭�� ��ȯ
        allCatData = new Cat[catDataLoader.catDictionary.Count];
        catDataLoader.catDictionary.Values.CopyTo(allCatData, 0);

        Debug.Log($"����� ������ {allCatData.Length}���� �ε�Ǿ����ϴ�.");
    }

    // ======================================================================================================================

    // ����� �� �Ǻ� �Լ�
    public bool CanSpawnCat()
    {
        return currentCatCount < maxCats;
    }

    // ���� ����� �� ������Ű�� �Լ�
    public void AddCatCount()
    {
        if (currentCatCount < maxCats)
        {
            currentCatCount++;
            UpdateCatCountText();
        }
    }

    // ���� ����� �� ���ҽ�Ű�� �Լ�
    public void DeleteCatCount()
    {
        if (currentCatCount > 0)
        {
            currentCatCount--;
            UpdateCatCountText();
        }
    }

    // ����� �� �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }

    // �⺻��ȭ �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    public void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"{coin}";
        }
    }

    // ĳ����ȭ �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    public void UpdateCashText()
    {
        if (cashText != null)
        {
            // ���ڸ� 3�ڸ����� �޸��� �߰��Ͽ� ǥ��
            cashText.text = cash.ToString("N0");
        }
    }

    // ======================================================================================================================

    // ���� On/Off �г� ���� �Լ�
    private void OpenMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(true);
        }
    }

    // ���� ���� ��ȯ �Լ�
    private void ToggleMergeState()
    {
        isMergeEnabled = !isMergeEnabled;
        UpdateMergeStateText();
        UpdateMergeButtonColor();
        CloseMergePanel();
    }

    // ���� ���� Text ������Ʈ �Լ�
    private void UpdateMergeStateText()
    {
        if (mergeStateText != null)
        {
            mergeStateText.text = isMergeEnabled ? "OFF" : "ON";
        }
    }

    // ���� ��ư ���� ������Ʈ �Լ�
    private void UpdateMergeButtonColor()
    {
        if (openMergePanelButton != null)
        {
            Color normalColor = isMergeEnabled ? new Color(1f, 1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorBlock colors = openMergePanelButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;
            colors.selectedColor = normalColor;
            openMergePanelButton.colors = colors;
        }
    }

    // ���� �г� �ݴ� �Լ�
    public void CloseMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
    }

    // ���� ���� ��ȯ �Լ�
    public bool IsMergeEnabled()
    {
        return isMergeEnabled;
    }

    // ======================================================================================================================

    // �ڵ��̵� �г� ���� �Լ�
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // �ڵ��̵� ���� ��ȯ �Լ�
    public void ToggleAutoMove()
    {
        isAutoMoveEnabled = !isAutoMoveEnabled;

        // ��� ����̿� ���� ����
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        // ���� ������Ʈ �� ��ư ���� ����
        UpdateAutoMoveStateText();
        UpdateOpenButtonColor();

        // �г� �ݱ�
        CloseAutoMovePanel();
    }

    // �ڵ��̵� ���� Text ������Ʈ �Լ�
    private void UpdateAutoMoveStateText()
    {
        if (autoMoveStateText != null)
        {
            autoMoveStateText.text = isAutoMoveEnabled ? "OFF" : "ON";
        }
    }

    // �ڵ��̵� ��ư ���� ������Ʈ �Լ�
    private void UpdateOpenButtonColor()
    {
        if (openAutoMovePanelButton != null)
        {
            Color normalColor = isAutoMoveEnabled ? new Color(1f, 1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorBlock colors = openAutoMovePanelButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;
            colors.selectedColor = normalColor;
            openAutoMovePanelButton.colors = colors;
        }
    }

    // �ڵ��̵� �г� �ݴ� �Լ�
    public void CloseAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // �ڵ��̵� ���� ���� ��ȯ�ϴ� �Լ�
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

    // ======================================================================================================================

    // ����� ���� �Լ�
    private void SortCats()
    {
        StartCoroutine(SortCatsCoroutine());
    }

    // ����̵��� ���ĵ� ��ġ�� ��ġ�ϴ� �ڷ�ƾ
    private IEnumerator SortCatsCoroutine()
    {
        // ���� �� �ڵ� �̵� ��� ���� (���� �ڵ��̵��� �������� ����̵� ���� = CatData�� AutoMove�� �������̾ ����)
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(false); // �ڵ� �̵� ��Ȱ��ȭ
            }
        }

        // ����� ��ü���� ����� �������� ���� (���� ����� ���� ������)
        List<GameObject> sortedCats = new List<GameObject>();
        foreach (Transform child in gamePanel)
        {
            sortedCats.Add(child.gameObject);
        }

        sortedCats.Sort((cat1, cat2) =>
        {
            int grade1 = GetCatGrade(cat1);
            int grade2 = GetCatGrade(cat2);

            if (grade1 == grade2) return 0;
            return grade1 > grade2 ? -1 : 1;
        });

        // ����� �̵� �ڷ�ƾ ����
        List<Coroutine> moveCoroutines = new List<Coroutine>();
        for (int i = 0; i < sortedCats.Count; i++)
        {
            GameObject cat = sortedCats[i];
            Coroutine moveCoroutine = StartCoroutine(MoveCatToPosition(cat, i));
            moveCoroutines.Add(moveCoroutine);
        }

        // ��� ����̰� �̵��� ��ĥ ������ ��ٸ���
        foreach (Coroutine coroutine in moveCoroutines)
        {
            yield return coroutine;
        }

        // ���� �� �ڵ� �̵� ���� ���� (������ �Ϸ�ǰ� ����̵��� �ٽ� �ֱ⸶�� �ڵ��̵��� �����ϰ� �������·� ����)
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(isAutoMoveEnabled);
            }
        }
    }

    // ������� ����� ��ȯ�ϴ� �Լ�
    private int GetCatGrade(GameObject catObject)
    {
        int grade = catObject.GetComponent<CatData>().catData.CatGrade;
        return grade;
    }

    // ����̵��� �ε巴�� ���ĵ� ��ġ�� �̵���Ű�� �Լ�
    private IEnumerator MoveCatToPosition(GameObject catObject, int index)
    {
        RectTransform rectTransform = catObject.GetComponent<RectTransform>();

        // ��ǥ ��ġ ��� (index�� ���ĵ� ����)
        float targetX = (index % 7 - 3) * (rectTransform.rect.width + 10);
        float targetY = (index / 7) * (rectTransform.rect.height + 10);
        Vector2 targetPosition = new Vector2(targetX, -targetY);

        // ���� ��ġ�� ��ǥ ��ġ�� ���̸� ����Ͽ� �ε巴�� �̵�
        float elapsedTime = 0f;
        float duration = 0.1f;          // �̵� �ð� (��)

        Vector2 initialPosition = rectTransform.anchoredPosition;

        // ��ǥ ��ġ�� �ε巴�� �̵�
        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �̵��� ������ �� ��Ȯ�� ��ǥ ��ġ�� ����
        rectTransform.anchoredPosition = targetPosition;
    }

    // ======================================================================================================================

    // �ڵ����� ��� Text ������Ʈ �Լ�
    private void UpdateAutoMergeCostText()
    {
        if (autoMergeCostText != null)
        {
            autoMergeCostText.text = $"{autoMergeCost}";
        }
    }

    // �ڵ����� �г� ���� �Լ�
    private void OpenAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(true);
        }
    }

    // �ڵ����� �г� �ݴ� �Լ�
    private void CloseAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(false);
        }
    }

    // �ڵ����� ���� �Լ�
    private void StartAutoMerge()
    {
        if (cash >= autoMergeCost)
        {
            cash -= autoMergeCost;
            UpdateCashText();

            AutoMerge autoMergeScript = FindObjectOfType<AutoMerge>();
            if (autoMergeScript != null)
            {
                autoMergeScript.OnClickedAutoMerge();
            }
        }
        else
        {
            Debug.Log("Not enough coins to start AutoMerge!");
        }
    }

    // �ڵ����� ���¿� ���� Ÿ�̸� �ؽ�Ʈ ���ü� ������Ʈ �Լ�
    public void UpdateAutoMergeTimerVisibility(bool isVisible)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.gameObject.SetActive(isVisible);
        }
    }

    // �ڵ����� Ÿ�̸� ������Ʈ �Լ�
    public void UpdateAutoMergeTimerText(int remainingTime)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.text = $"{remainingTime}";
        }
    }

    // ======================================================================================================================

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomItemMenuPanel()
    {
        Debug.Log("Item Toggle");
        if (itemMenuPanel != null)
        {
            isOnToggleItemMenu = !isOnToggleItemMenu;
            if (isOnToggleItemMenu)
            {
                Debug.Log("Item Toggle On");
                itemMenuPanel.SetActive(true);
                //if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
                //{
                //    bottomItemButtonImg.color = parsedColor;
                //}
                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
            else
            {
                Debug.Log("Item Toggle Off");
                itemMenuPanel.SetActive(false);
                //if (ColorUtility.TryParseHtmlString("#E2E2E2", out Color parsedColor))
                //{
                //    bottomItemButtonImg.color = parsedColor;
                //}
                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
        }
    }

    // ������ �޴� �ǳ� �ݴ� �Լ�(�ڷΰ��� ��ư)
    private void CloseBottomItemMenuPanel()
    {
        if(itemMenuPanel != null)
        {
            isOnToggleItemMenu = false;
            itemMenuPanel.SetActive(false);
            //if (ColorUtility.TryParseHtmlString("#E2E2E2", out Color parsedColor))
            //{
            //    bottomItemButtonImg.color = parsedColor;
            //}
            ChangeSelectedButtonColor(isOnToggleItemMenu);
        }
    }

    // ���� �޴� ��ư ���� ����
    private void ChangeSelectedButtonColor(bool isOnToggle)
    {
        if(isOnToggle)
        {
            if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
            {
                if(itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if(buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }
                
            }
        }
        else
        {
            if (ColorUtility.TryParseHtmlString("#E2E2E2", out Color parsedColor))
            {
                if(!itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if(!buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }

            }
        }
    }

    // ������ �޴��� ������, ����, ���, Ȳ�ݱ��� ��ư ���� ����
    private void ChangeSelectedButtonColor(GameObject menues)
    {
        if (menues.gameObject.activeSelf)
        {
            if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColorT))
            {
                for (int i = 0; i < (int)EitemMenues.end; i++)
                {
                    if (isItemMenuButtonsOn[i])
                    {
                        // ���󺯰�
                        itemMenuButtons[i].GetComponent<Image>().color = parsedColorT;
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString("#E2E2E2", out Color parsedColorF))
                        {
                             // ���󺯰�
                             itemMenuButtons[i].GetComponent<Image>().color = parsedColorF;
                        }
                    }
                }
            }
        }
    }

    // ����� �ִ�ġ ����
    private void IncreaseCatMaximum()
    {
        Debug.Log("����� �ִ�ġ ���� ��ư Ŭ��");
        if (catMaximumIncreaseButton != null)
        {
            maxCats++;
            catMaximumIncreaseFee *= 2;
            catMaximumIncreaseLv++;
            UpdateCatCountText();
            UpdateIncreaseMaximumFeeText();
            UpdateIncreaseMaximumLvText();
        }
    }

    // ����� �� �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateIncreaseMaximumFeeText()
    {
        if (catMaximumIncreaseText != null)
        {
            if (coin < catMaximumIncreaseFee)
            {
                catMaximumIncreaseButton.interactable = false;
                disabledBg.SetActive(true);
            }
            else
            {
                catMaximumIncreaseButton.interactable = true;
                disabledBg.SetActive(false);
            }
            catMaximumIncreaseText.text = $"{catMaximumIncreaseFee}";
            UpdateCoinText();
            coin -= catMaximumIncreaseFee;

        }
    }

    // ����� �ִ�ġ ���� ���� UI ������Ʈ �ϴ� �Լ�
    private void UpdateIncreaseMaximumLvText()
    {
        catMaximumIncreaseLvText.text = $"Lv.{catMaximumIncreaseLv}";

    }

    // ��ư Ŭ�� �̺�Ʈ
    void OnButtonClicked(GameObject activePanel)
    {
        // ��� UI �г� ��Ȱ��ȭ
        for(int i = 0; i < (int)EitemMenues.end; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        // Ŭ���� ��ư�� UI�� Ȱ��ȭ
        activePanel.SetActive(true);
       
        for(int i = 0; i < (int)EitemMenues.end; i++)
        {
            if(itemMenues[i].activeSelf)
            {
                isItemMenuButtonsOn[i] = true;
            }
        }
        ChangeSelectedButtonColor(activePanel);
    }
    // ======================================================================================================================

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomBuyCatMenuPanel()
    {
        Debug.Log("Buy Cat Toggle");
        if (buyCatMenuPanel != null)
        {
            isOnToggleBuyCat = !isOnToggleBuyCat;
            if (isOnToggleBuyCat)
            {
                Debug.Log("Buy Cat Toggle On");
                buyCatMenuPanel.SetActive(true);
                ChangeSelectedButtonColor(isOnToggleBuyCat);
            }
            else
            {
                Debug.Log("Buy Cat Toggle Off");
                buyCatMenuPanel.SetActive(false);
                ChangeSelectedButtonColor(isOnToggleBuyCat);
            }
        }
    }

    // ������ �޴� �ǳ� �ݴ� �Լ�(�ڷΰ��� ��ư)
    private void CloseBottomBuyCatMenuPanel()
    {
        if (buyCatMenuPanel != null)
        {
            isOnToggleBuyCat = false;
            buyCatMenuPanel.SetActive(false);
            ChangeSelectedButtonColor(isOnToggleBuyCat);
        }
    }

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCoin()
    {
        if (CanSpawnCat())
        {
            Debug.Log("����� ���� ��ư Ŭ��(��������)");
            coin -= buyCatCoinFee;
            buyCatCoinCount++;
            buyCatCoinFee *= 2;

            CatSpawn catSpawn = GetComponent<CatSpawn>();
            catSpawn.OnClickedSpawn();

            UpdateCoinText();
            UpdateBuyCatUI();
        }
    }

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCash()
    {
        // ���� ����� ���� ����� �ִ� �� �̸��� ��
        if (CanSpawnCat()) 
        {
            Debug.Log("����� ���� ��ư Ŭ��(ĳ����)");
            cash -= buyCatCashFee;
            buyCatCashCount++;


            CatSpawn catSpawn = GetComponent<CatSpawn>();
            catSpawn.OnClickedSpawn();

            UpdateCashText();
            UpdateBuyCatUI();
        }
    }

    // ����� ���� ���� UI ������Ʈ �Լ�
    private void UpdateBuyCatUI()
    {
        buyCatCountExplainText.text = $"BuyCount :{buyCatCoinCount}cnt + {buyCatCashCount}cnt";
        buyCatCoinFeeText.text = $"{buyCatCoinFee}";
        buyCatCashFeeText.text = $"{buyCatCashFee}";
        if (CanSpawnCat())
        {
            if (coin < buyCatCoinFee)
            {
                buyCatCoinButton.interactable = false;
                buyCatCoinDisabledBg.SetActive(true);
            }
            else
            {
                buyCatCoinButton.interactable = true;
                buyCatCoinDisabledBg.SetActive(false);
            }

            if (cash < buyCatCashFee)
            {
                buyCatCashButton.interactable = false;
                buyCatCashDisabledBg.SetActive(true);
            }
            else
            {
                buyCatCashButton.interactable = true;
                buyCatCashDisabledBg.SetActive(false);
            }      
        }     
    }

    // ======================================================================================================================
}
