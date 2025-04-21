using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

// ����� ���� Script
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


    [Header("---[New Cat Panel UI]")]
    [SerializeField] private GameObject newCatPanel;                // New Cat Panel
    [SerializeField] private Image newCatHighlightImage;            // New Cat Highlight Image
    [SerializeField] private Image newCatIcon;                      // New Cat Icon
    [SerializeField] private TextMeshProUGUI newCatName;            // New Cat Name Text
    [SerializeField] private TextMeshProUGUI newCatExplain;         // New Cat Explanation Text
    [SerializeField] private TextMeshProUGUI newCatGetCoin;         // New Cat Get Coin Text
    [SerializeField] private Button submitButton;                   // New Cat Panel Submit Button

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
    private Sprite informationCatDefaultImage;                      // Information Cat Default Image
    [SerializeField] private TextMeshProUGUI informationCatDetails; // informationCatDetails Text
    [SerializeField] private GameObject catInformationPanel;        // catInformation Panel (������ ĭ Panel)
    [SerializeField] private RectTransform fullInformationPanel;    // fullInformation Panel (������ ��ũ�� Panel)

    // �ӽ� (�ٸ� ���� �޴����� �߰��Ѵٸ� ��� ���� �ұ� ���)
    [Header("---[Sub Contents]")]
    [SerializeField] private Transform scrollRectContents;          // �븻 ����� scrollRectContents (�������� �븻 ������� ������ �ʱ�ȭ �ϱ� ����)
                                                                    // ��� ����� scrollRectContents
                                                                    // Ư�� ����� scrollRectContents

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";               // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";             // ��Ȱ��ȭ���� Color


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

        dictionaryButton.onClick.AddListener(() =>
        {
            activePanelManager.TogglePanel("DictionaryMenu");

            // DictionaryMenu�� Ȱ��ȭ�� �� InformationPanel ������Ʈ
            if (activePanelManager.ActivePanelName == "DictionaryMenu")
            {
                UpdateInformationPanel();
            }
        });
        dictionaryBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("DictionaryMenu"));
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

        SaveToLocal();
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
        QuestManager.Instance.AddCash(GameManager.Instance.AllCatData[catGrade].CatGetCoin);

        // �̺�Ʈ �߻�
        OnCatDataChanged?.Invoke();

        SaveToLocal();
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

        RectTransform textRect = text.GetComponent<RectTransform>();

        // CatGrade�� 1���� �����ϹǷ� �迭 �ε����� ��ȯ
        int catIndex = cat.CatGrade - 1;

        if (IsCatUnlocked(catIndex))
        {
            button.interactable = true;
            text.text = $"{cat.CatGrade}. {cat.CatName}";
            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

            // ù �ر� ���� Ȯ�� �ÿ��� �ε��� ���
            if (IsGetFirstUnlockedReward(catIndex))
            {
                firstOpenBG.gameObject.SetActive(false);
            }
            else
            {
                firstOpenBG.gameObject.SetActive(true);
                firstOpenCashtext.text = $"+ {cat.CatGetCoin}";
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
            textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, 0);
            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0f);
            firstOpenBG.gameObject.SetActive(false);
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

        RectTransform textRect = text.GetComponent<RectTransform>();

        button.interactable = true;

        Cat catData = GameManager.Instance.AllCatData[catGrade];
        iconImage.sprite = catData.CatImage;
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

        // ǥ�õǴ� ����� 1-based
        text.text = $"{catGrade + 1}. {catData.CatName}";
        textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, 100);

        if (!IsGetFirstUnlockedReward(catGrade))
        {
            firstOpenBG.gameObject.SetActive(true);
            firstOpenCashtext.text = $"+ {catData.CatGetCoin}";
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

        // �ؽ�Ʈ ����
        informationCatDetails.text = $"����̸� �����ϼ���\n";

        // ��ũ�� ��Ȱ��ȭ
        catInformationPanel.GetComponent<ScrollRect>().enabled = false;

        // �ʱ� catInformation Mask �ʱ�ȭ
        catInformationPanel.GetComponent<Mask>().enabled = true;

        // fullInformationPanel�� Y��ǥ�� -312.5f�� ����
        fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);

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

        string catInfo = $"�̸�: {catData.CatName}\n" +
                         $"���: {catData.CatGrade}\n" +
                         $"���ݷ�: {catData.BaseDamage}%\n" +
                         $"ü��: {catData.CatHp}\n" +
                         $"��ȭ���޷�: {catData.CatGetCoin}";
        informationCatDetails.text = catInfo;

        // ��ũ�� ����
        catInformationPanel.GetComponent<ScrollRect>().enabled = true;
        catInformationPanel.GetComponent<ScrollRect>().velocity = Vector2.zero;
        fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);

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
        Cat newCat = GameManager.Instance.AllCatData[catGrade];

        newCatPanel.SetActive(true);

        newCatIcon.sprite = newCat.CatImage;
        newCatName.text = newCat.CatGrade.ToString() + ". " + newCat.CatName;
        newCatExplain.text = newCat.CatExplain;
        newCatGetCoin.text = "��ȭ ȹ�淮: " + newCat.CatGetCoin.ToString();

        // Highlight Image ȸ�� �ִϸ��̼� ����
        StartCoroutine(RotateHighlightImage());

        // Ȯ�� ��ư�� ������ �г��� ��Ȱ��ȭ
        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // Highlight Image ȸ�� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator RotateHighlightImage()
    {
        while (newCatPanel.activeSelf)
        {
            newCatHighlightImage.transform.Rotate(new Vector3(0, 0, 1), 90 * Time.deltaTime);
            yield return null;
        }
    }

    // New Cat Panel�� �ݴ� �Լ�
    private void CloseNewCatPanel()
    {
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

    // New Image UI�� �����ϴ� �Լ�
    private void UpdateNewImageStatus()
    {
        bool hasUnclaimedRewards = HasUnclaimedRewards();

        // ���� Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        if (dictionaryButtonNewImage != null)
        {
            dictionaryButtonNewImage.SetActive(hasUnclaimedRewards);
        }

        // Normal Cat Button�� New Image Ȱ��ȭ/��Ȱ��ȭ
        if (normalCatButtonNewImage != null)
        {
            normalCatButtonNewImage.SetActive(hasUnclaimedRewards);
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

    private void SaveToLocal()
    {
        string data = GetSaveData();
        string key = this.GetType().FullName;
        GoogleManager.Instance?.SaveToPlayerPrefs(key, data);
    }

    #endregion


}
