using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

// ����� ���� ��ũ��Ʈ
[DefaultExecutionOrder(-6)]
public class DictionaryManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static DictionaryManager Instance { get; private set; }

    [Header("---[Dictionary Manager]")]
    [SerializeField] private ScrollRect[] dictionaryScrollRects;    // ������ ��ũ�Ѻ� �迭
    [SerializeField] private Button dictionaryButton;               // ���� ��ư
    [SerializeField] private Image dictionaryButtonImage;           // ���� ��ư �̹���
    [SerializeField] private GameObject dictionaryMenuPanel;        // ���� �޴� Panel
    [SerializeField] private Button dictionaryBackButton;           // ���� �ڷΰ��� ��ư
    private ActivePanelManager activePanelManager;                  // ActivePanelManager

    [SerializeField] private GameObject slotPrefab;                 // ���� ���� ������
    [SerializeField] private GameObject[] dictionaryMenus;          // ���� �޴� Panels
    [SerializeField] private Button[] dictionaryMenuButtons;        // ������ ���� �޴� ��ư �迭

    [SerializeField] private GameObject dictionaryButtonNewImage;   // ���� Button�� New Image
    [SerializeField] private GameObject normalCatButtonNewImage;    // Normal Cat Button�� New Image
    private event Action OnCatDataChanged;                          // �̺�Ʈ ����

    // ���� �ر� ���� ����
    private bool[] isCatUnlocked;                                   // ����� �ر� ���� �迭
    private bool[] isGetFirstUnlockedReward;                        // ����� ù �ر� ���� ȹ�� ���� �迭

    private int currentSelectedCatGrade;                            // ���� ���õ� ����� ��� ����

    private static readonly Vector2 defaultTextPosition = new Vector2(0, 0);
    private static readonly Vector2 unlockedTextPosition = new Vector2(0, 100);
    private static readonly Color transparentIconColor = new Color(1f, 1f, 1f, 0f);
    private static readonly Color visibleIconColor = new Color(1f, 1f, 1f, 1f);


    [Header("---[New Cat Panel UI]")]
    [SerializeField] private GameObject newCatPanel;                // New Cat Panel
    [SerializeField] private Image newCatHighlightImage;            // New Cat Highlight Image
    [SerializeField] private Image newCatIcon;                      // New Cat Icon
    [SerializeField] private TextMeshProUGUI newCatName;            // New Cat Name Text
    [SerializeField] private TextMeshProUGUI newCatExplain;         // New Cat Explanation Text
    [SerializeField] private TextMeshProUGUI newCatGetCoin;         // New Cat Get Coin Text
    [SerializeField] private Button submitButton;                   // New Cat Panel Submit Button
    private Coroutine highlightRotationCoroutine;                   // Highlight ȸ�� �ڷ�ƾ ������ ����

    // Highlight Image ȸ���� ����� Vector3 ĳ��
    private static readonly Vector3 rotationVector = new Vector3(0, 0, 1);
    private static readonly float rotationSpeed = 90f;

    // Enum���� �޴� Ÿ�� ���� (���� �޴��� �����ϱ� ���� ���)
    private enum DictionaryMenuType
    {
        Normal,                 // �Ϲ� ����� �޴�
        Rare,                   // ��� ����� �޴�
        Special,                // Ư�� ����� �޴�
        Background,             // ��� �޴�
        End                     // Enum�� ��
    }
    private DictionaryMenuType activeMenuType;                      // ���� Ȱ��ȭ�� �޴� Ÿ��


    [Header("---[Information Panel UI]")]
    [SerializeField] private Image informationCatIcon;              // Information Cat Icon
    [SerializeField] private TextMeshProUGUI informationCatDetails; // informationCatDetails Text
    [SerializeField] private GameObject catInformationPanel;        // catInformation Panel (������ ĭ Panel)
    [SerializeField] private RectTransform fullInformationPanel;    // fullInformation Panel (������ ��ũ�� Panel)
    private static readonly Vector2 defaultInformationPanelPosition = new Vector2(0, -312.5f);  // Information Panel ��ġ ���


    [Header("---[ScrollRect Transform]")]
    private const int SPACING_Y = 30;                               // y spacing
    private const int CELL_SIZE_Y = 280;                            // y cell size
    private const int VIEWPORT_HEIGHT = 680;                        // viewport height


    // �ӽ� (�ٸ� ���� �޴����� �߰��Ѵٸ� ��� ���� �ұ� ���)
    [Header("---[Sub Contents]")]
    [SerializeField] private Transform scrollRectContents;          // �븻 ����� scrollRectContents (�������� �븻 ������� ������ �ʱ�ȭ �ϱ� ����)
                                                                    // ��� ����� scrollRectContents
                                                                    // Ư�� ����� scrollRectContents


    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";               // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";             // ��Ȱ��ȭ���� Color


    [Header("---[ETC]")]
    private bool isDataLoaded = false;                              // ������ �ε� Ȯ��

    #endregion


    #region Unity Menthods

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
        InitializeDictionaryManager();

        // GoogleManager���� �����͸� �ε����� ���� ��쿡�� �ʱ�ȭ
        if (!isDataLoaded)
        {
            InitializeCatUnlockData();
            PopulateDictionary();
        }

        InitializeNewImage();
        InitializeScrollPositions();
    }

    private void OnDestroy()
    {
        // �̺�Ʈ �ڵ鷯 ����
        OnCatDataChanged -= UpdateNewImageStatus;
    }

    #endregion


    #region Initialize

    // �⺻ ���� �ʱ�ȭ
    private void InitializeDefaultValues()
    {
        newCatPanel.SetActive(false);
        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;
    }

    // ��� ������� �ʱ�ȭ
    private void InitializeDictionaryManager()
    {
        InitializeDefaultValues();
        InitializeActivePanel();

        InitializeDictionaryButton();

        InitializeSubMenuButtons();
    }

    // New �̹��� ���� �̺�Ʈ �� UI �ʱ�ȭ
    private void InitializeNewImage()
    {
        // �̺�Ʈ ��� (���°� ����� �� UpdateNewImageStatus ȣ��)
        OnCatDataChanged += UpdateNewImageStatus;

        // �ʱ�ȭ �� New Image UI ����
        UpdateNewImageStatus();
    }

    // �ʱ� ��ũ�� ��ġ �ʱ�ȭ �Լ�
    private void InitializeScrollPositions()
    {
        foreach (var scrollRect in dictionaryScrollRects)
        {
            if (scrollRect != null && scrollRect.content != null)
            {
                // ��ü ���� ������ 3�� ����� �ø� ���
                int totalSlots = GameManager.Instance.AllCatData.Length;
                int adjustedTotalSlots = Mathf.CeilToInt(totalSlots / 3f) * 3;

                // ���� ���� ��� (3���� �� ��)
                int rowCount = adjustedTotalSlots / 3;

                // Grid Layout Group ������
                const int SPACING_Y = 30;           // y spacing
                const int CELL_SIZE_Y = 280;        // y cell size
                const int VIEWPORT_HEIGHT = 680;    // viewport height

                // ��ü ������ ���� ���: (�� ���� * (�� ����-1)) + (�� ���� * �� ����)
                float contentHeight = (SPACING_Y * (rowCount - 1)) + (CELL_SIZE_Y * rowCount);

                // ��ũ�� ���� ��ġ ���: -(��ü ���� - ����Ʈ ����) * 0.5f
                float targetY = -(contentHeight - VIEWPORT_HEIGHT) * 0.5f;

                // ��ũ�� �������� ��ġ�� ������� ����
                scrollRect.content.anchoredPosition = new Vector2(
                    scrollRect.content.anchoredPosition.x,
                    targetY
                );

                // ��ũ�� �ӵ� �ʱ�ȭ
                scrollRect.velocity = Vector2.zero;
            }
        }
    }

    // ActivePanel �ʱ�ȭ �Լ�
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("DictionaryMenu", dictionaryMenuPanel, dictionaryButtonImage);

        // ���� Ʃ�丮�� �Ϸ� ���ο� ���� ��ư ��ȣ�ۿ� ����
        if (TutorialManager.Instance != null)
        {
            dictionaryButton.interactable = TutorialManager.Instance.IsMainTutorialEnd;
        }

        dictionaryButton.onClick.AddListener(() =>
        {
            activePanelManager.TogglePanel("DictionaryMenu");

            // DictionaryMenu�� Ȱ��ȭ�� ���� ����
            if (activePanelManager.ActivePanelName == "DictionaryMenu")
            {
                // ���� �г��� ó�� ������ �� Ʃ�丮�� ����
                if (TutorialManager.Instance != null && !TutorialManager.Instance.IsDictionaryTutorialEnd)
                {
                    TutorialManager.Instance.StartDictionaryTutorial();
                }
                UpdateInformationPanel();
            }
        });
        dictionaryBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("DictionaryMenu"));
    }

    // ���� ��ư ��ȣ�ۿ� ������Ʈ �Լ� �߰�
    public void UpdateDictionaryButtonInteractable()
    {
        if (TutorialManager.Instance != null && dictionaryButton != null)
        {
            dictionaryButton.interactable = TutorialManager.Instance.IsMainTutorialEnd;
        }
    }

    // DictionaryButton �ʱ�ȭ �Լ�
    private void InitializeDictionaryButton()
    {
        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    #endregion


    #region Cat Unlock System

    // ��� �ر� ���� �ʱ�ȭ �Լ�
    private void InitializeCatUnlockData()
    {
        int allCatDataLength = GameManager.Instance.AllCatData.Length;

        isCatUnlocked = new bool[allCatDataLength];
        isGetFirstUnlockedReward = new bool[allCatDataLength];

        // ��� ����̸� ��� ���·� �ʱ�ȭ & ù ���� ȹ�� false�� �ʱ�ȭ
        for (int i = 0; i < isCatUnlocked.Length; i++)
        {
            isCatUnlocked[i] = false;
            isGetFirstUnlockedReward[i] = false;
        }
    }

    // Ư�� ����� �ر� �Լ�
    public void UnlockCat(int CatGrade)
    {
        if (CatGrade < 0 || CatGrade >= isCatUnlocked.Length || isCatUnlocked[CatGrade]) return;

        isCatUnlocked[CatGrade] = true;
        SaveUnlockedCats(CatGrade);

        // �̺�Ʈ �߻�
        OnCatDataChanged?.Invoke();

        // BuyCatManager�� ���� ���� ���� ������Ʈ
        BuyCatManager.Instance?.UnlockBuySlot(CatGrade);
    }

    // Ư�� ������� �ر� ���� Ȯ�� �Լ�
    public bool IsCatUnlocked(int catGrade)
    {
        return isCatUnlocked[catGrade];
    }

    // Ư�� ������� ù �ر� ���� ȹ�� �Լ�
    public void GetFirstUnlockedReward(int catGrade)
    {
        if (catGrade < 0 || catGrade >= isGetFirstUnlockedReward.Length || isGetFirstUnlockedReward[catGrade])
        {
            return;
        }

        isGetFirstUnlockedReward[catGrade] = true;
        QuestManager.Instance.AddCash(GameManager.Instance.AllCatData[catGrade].CatFirstOpenCash);

        // �̺�Ʈ �߻�
        OnCatDataChanged?.Invoke();
    }

    // Ư�� ������� ù �ر� ���� ȹ�� ���� Ȯ�� �Լ�
    public bool IsGetFirstUnlockedReward(int catGrade)
    {
        return isGetFirstUnlockedReward[catGrade];
    }

    // ��� �ر� ���� ���� �Լ�
    public void SaveUnlockedCats(int CatGrade)
    {
        GetComponent<DictionaryManager>().UpdateDictionary(CatGrade);
        GetComponent<DictionaryManager>().ShowNewCatPanel(CatGrade);
    }

    #endregion


    #region ���� �� UI

    // ���� �����͸� ä��� �Լ�
    private void PopulateDictionary()
    {
        if (GameManager.Instance.AllCatData == null || GameManager.Instance.AllCatData.Length == 0) return;

        foreach (Transform child in scrollRectContents)
        {
            Destroy(child.gameObject);
        }

        foreach (Cat cat in GameManager.Instance.AllCatData)
        {
            InitializeSlot(cat);
        }
    }

    // ����� �����͸� �������� �ʱ� ������ �����ϴ� �Լ�
    private void InitializeSlot(Cat cat)
    {
        GameObject slot = Instantiate(slotPrefab, scrollRectContents);

        Button button = slot.transform.Find("Button")?.GetComponent<Button>();
        TextMeshProUGUI text = slot.transform.Find("Button/Name Text")?.GetComponent<TextMeshProUGUI>();
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();
        GameObject friendshipNewImage = slot.transform.Find("Button/New Image")?.gameObject;

        RectTransform textRect = text.GetComponent<RectTransform>();

        // CatGrade�� 1���� �����ϹǷ� �迭 �ε����� ��ȯ
        int catIndex = cat.CatGrade - 1;

        if (IsCatUnlocked(catIndex))
        {
            button.interactable = true;
            text.text = $"{cat.CatGrade}. {cat.CatName}";
            iconImage.sprite = cat.CatImage;
            iconImage.color = visibleIconColor;

            friendshipNewImage.SetActive(FriendshipManager.Instance.HasUnclaimedFriendshipRewards(cat.CatGrade));

            // ù �ر� ���� Ȯ�� �ÿ��� �ε��� ���
            if (IsGetFirstUnlockedReward(catIndex))
            {
                firstOpenBG.gameObject.SetActive(false);
            }
            else
            {
                firstOpenBG.gameObject.SetActive(true);
                firstOpenCashtext.text = $"+{GameManager.Instance.FormatNumber(cat.CatFirstOpenCash)}";
            }

            // ��ư Ŭ�� �̺�Ʈ ����
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (!IsGetFirstUnlockedReward(catIndex))
                {
                    GetFirstUnlockedReward(catIndex);
                    firstOpenBG.gameObject.SetActive(false);
                }
                else
                {
                    ShowInformationPanel(catIndex);
                }
            });

            // ��ư SFX ���
            if (OptionManager.Instance != null)
            {
                button.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
            }
        }
        else
        {
            button.interactable = false;
            text.text = "???";
            textRect.anchoredPosition = defaultTextPosition;
            iconImage.sprite = cat.CatImage;
            iconImage.color = transparentIconColor;
            firstOpenBG.gameObject.SetActive(false);
            friendshipNewImage.SetActive(false);
        }
    }

    // ���ο� ����̸� �ر��ϸ� ������ ������Ʈ�ϴ� �Լ�
    public void UpdateDictionary(int catGrade)
    {
        // catGrade�� �̹� 0-based index�̹Ƿ� �״�� ���
        Transform slot = scrollRectContents.GetChild(catGrade);

        slot.gameObject.SetActive(true);

        Button button = slot.transform.Find("Button")?.GetComponent<Button>();
        TextMeshProUGUI text = slot.transform.Find("Button/Name Text")?.GetComponent<TextMeshProUGUI>();
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();
        GameObject friendshipNewImage = slot.transform.Find("Button/New Image")?.gameObject;

        RectTransform textRect = text.GetComponent<RectTransform>();

        button.interactable = true;

        Cat catData = GameManager.Instance.AllCatData[catGrade];
        iconImage.sprite = catData.CatImage;
        iconImage.color = visibleIconColor;

        friendshipNewImage.SetActive(FriendshipManager.Instance.HasUnclaimedFriendshipRewards(catGrade + 1));

        // ǥ�õǴ� ����� 1-based
        text.text = $"{catGrade + 1}. {catData.CatName}";
        textRect.anchoredPosition = unlockedTextPosition;

        if (!IsGetFirstUnlockedReward(catGrade))
        {
            firstOpenBG.gameObject.SetActive(true);
            firstOpenCashtext.text = $"+ {GameManager.Instance.FormatNumber(catData.CatFirstOpenCash)}";
        }
        else
        {
            firstOpenBG.gameObject.SetActive(false);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (!IsGetFirstUnlockedReward(catGrade))
            {
                GetFirstUnlockedReward(catGrade);
                firstOpenBG.gameObject.SetActive(false);
            }
            else
            {
                ShowInformationPanel(catGrade);
            }
        });

        // ��ư SFX ���
        if (OptionManager.Instance != null)
        {
            button.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
        }
    }

    // ������ Ȱ��ȭ �ǰų� ���� �޴��� ������ InformationPanel�� �⺻ ������ ������Ʈ ���ִ� �Լ�
    private void UpdateInformationPanel()
    {
        currentSelectedCatGrade = -1;

        // �̹��� ����
        informationCatIcon.gameObject.SetActive(false);

        // ��ư �̺�Ʈ �ʱ�ȭ
        Button iconButton = informationCatIcon.GetComponent<Button>();
        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
        }

        // �ؽ�Ʈ ����
        informationCatDetails.text = $"����̸� �����ϼ���\n";

        // ��ũ�� ��Ȱ��ȭ
        catInformationPanel.GetComponent<ScrollRect>().enabled = false;

        // �ʱ� catInformation Mask �ʱ�ȭ
        catInformationPanel.GetComponent<Mask>().enabled = true;

        // fullInformationPanel�� Y��ǥ�� -312.5f�� ����
        fullInformationPanel.anchoredPosition = defaultInformationPanelPosition;

        // ������ UI �ʱ�ȭ
        FriendshipManager.Instance.ResetUI();
    }

    // ����� ������ Information Panel�� ǥ���ϴ� �Լ�
    private void ShowInformationPanel(int catGrade)
    {
        // catGrade�� 0-based index�̹Ƿ�, UI�� ǥ���� �� 1�� ����
        currentSelectedCatGrade = catGrade + 1;
        var catData = GameManager.Instance.AllCatData[catGrade];

        // ���� ���� ǥ�� �ڵ�
        informationCatIcon.gameObject.SetActive(true);
        informationCatIcon.sprite = catData.CatImage;

        // ������ ��ư Ŭ�� �̺�Ʈ ����
        Button iconButton = informationCatIcon.GetComponent<Button>();
        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(() => ShowNewCatPanel(catGrade));

            // ��ư SFX ���
            if (OptionManager.Instance != null)
            {
                iconButton.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
            }
        }

        string catInfo = $"�̸�: {catData.CatName}\n" +
                         $"���: {catData.CatGrade}\n" +
                         $"���ݷ�: {catData.BaseDamage}%\n" +
                         $"ü��: {catData.CatHp}\n" +
                         $"��ȭ���޷�: {catData.CatGetCoin}";
        informationCatDetails.text = catInfo;

        // ��ũ�� ����
        catInformationPanel.GetComponent<ScrollRect>().enabled = true;
        catInformationPanel.GetComponent<ScrollRect>().velocity = Vector2.zero;
        fullInformationPanel.anchoredPosition = defaultInformationPanelPosition;

        // ������ �ý��� ������Ʈ ȣ��
        FriendshipManager.Instance.OnCatSelected(currentSelectedCatGrade);
    }

    // ���� ���õ� ����� ��� ��ȯ �Լ� �߰�
    public int GetCurrentSelectedCatGrade()
    {
        return currentSelectedCatGrade;
    }

    #endregion


    #region New Cat Panel

    // ���ο� ����� �ر� ȿ�� �Լ�
    public void ShowNewCatPanel(int catGrade)
    {
        // ���� ȸ�� �ڷ�ƾ�� �ִٸ� ����
        if (highlightRotationCoroutine != null)
        {
            StopCoroutine(highlightRotationCoroutine);
            highlightRotationCoroutine = null;
        }

        Cat newCat = GameManager.Instance.AllCatData[catGrade];

        newCatPanel.SetActive(true);

        newCatIcon.sprite = newCat.CatImage;
        newCatName.text = newCat.CatGrade.ToString() + ". " + newCat.CatName;
        newCatExplain.text = newCat.CatExplain;
        newCatGetCoin.text = "��ȭ ȹ�淮: " + newCat.CatGetCoin.ToString();

        // Highlight Image ȸ�� �ִϸ��̼� ����
        highlightRotationCoroutine = StartCoroutine(RotateHighlightImage());

        // Ȯ�� ��ư�� ������ �г��� ��Ȱ��ȭ
        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // Highlight Image ȸ�� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator RotateHighlightImage()
    {
        // ȸ�� ���� �ʱ�ȭ
        if (newCatHighlightImage != null)
        {
            newCatHighlightImage.transform.rotation = Quaternion.identity;
        }

        while (newCatPanel.activeSelf)
        {
            if (newCatHighlightImage != null)
            {
                newCatHighlightImage.transform.Rotate(rotationVector, rotationSpeed * Time.deltaTime);
            }
            yield return null;
        }

        highlightRotationCoroutine = null;
    }

    // New Cat Panel�� �ݴ� �Լ�
    private void CloseNewCatPanel()
    {
        if (highlightRotationCoroutine != null)
        {
            StopCoroutine(highlightRotationCoroutine);
            highlightRotationCoroutine = null;
        }

        newCatPanel.SetActive(false);
    }

    #endregion


    #region Sub Menus

    // ���� �޴� ��ư �ʱ�ȭ �� Ŭ�� �̺�Ʈ �߰� �Լ�
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)DictionaryMenuType.End; i++)
        {
            int index = i;
            dictionaryMenuButtons[index].onClick.AddListener(() => ActivateMenu((DictionaryMenuType)index));
        }

        ActivateMenu(DictionaryMenuType.Normal);
    }

    // ������ ���� �޴��� Ȱ��ȭ�ϴ� �Լ�
    private void ActivateMenu(DictionaryMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < dictionaryMenus.Length; i++)
        {
            dictionaryMenus[i].SetActive(i == (int)menuType);
        }

        UpdateInformationPanel();
        UpdateSubMenuButtonColors();
    }

    // ���� �޴� ��ư ������ ������Ʈ�ϴ� �Լ�
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < dictionaryMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(dictionaryMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
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

    #endregion


    #region Notification System

    // ������ ���� �� �ִ� ���¸� Ȯ���ϴ� �Լ�
    public bool HasUnclaimedRewards()
    {
        for (int i = 0; i < isCatUnlocked.Length; i++)
        {
            if (isCatUnlocked[i] && !isGetFirstUnlockedReward[i])
            {
                return true;
            }
        }
        return false;
    }

    // ������ ������ ���� �� �ִ� ����̰� �ִ��� Ȯ���ϴ� �Լ� (friendship New Image)
    private bool HasUnclaimedFriendshipRewards()
    {
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            if (IsCatUnlocked(i - 1) && FriendshipManager.Instance.HasUnclaimedFriendshipRewards(i))
            {
                return true;
            }
        }
        return false;
    }

    // New Image UI�� �����ϴ� �Լ�
    public void UpdateNewImageStatus()
    {
        bool hasUnclaimedRewards = HasUnclaimedRewards();
        bool hasUnclaimedFriendshipRewards = HasUnclaimedFriendshipRewards();

        // ���� Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        if (dictionaryButtonNewImage != null)
        {
            dictionaryButtonNewImage.SetActive(hasUnclaimedRewards || hasUnclaimedFriendshipRewards);
        }

        // Normal Cat Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        if (normalCatButtonNewImage != null)
        {
            normalCatButtonNewImage.SetActive(hasUnclaimedRewards || hasUnclaimedFriendshipRewards);
        }
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool[] isCatUnlocked;                   // ����� �ر� ����
        public bool[] isGetFirstUnlockedReward;        // ù �ر� ���� ���� ����
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            isCatUnlocked = this.isCatUnlocked,
            isGetFirstUnlockedReward = this.isGetFirstUnlockedReward
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        LoadCatUnlockData(savedData);
        PopulateDictionary();

        UpdateNewImageStatus();

        isDataLoaded = true;
    }

    private void LoadCatUnlockData(SaveData savedData)
    {
        // ������ ����
        int length = GameManager.Instance.AllCatData.Length;
        isCatUnlocked = new bool[length];
        isGetFirstUnlockedReward = new bool[length];

        for (int i = 0; i < length; i++)
        {
            isCatUnlocked[i] = savedData.isCatUnlocked[i];
            isGetFirstUnlockedReward[i] = savedData.isGetFirstUnlockedReward[i];
        }
    }

    #endregion


}
