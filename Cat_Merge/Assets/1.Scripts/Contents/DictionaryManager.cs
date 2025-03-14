using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


// ����� ���� Script
public class DictionaryManager : MonoBehaviour
{
    // Singleton Instance
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

    // ======================================================================================================================

    // ���� �ر� ���� ����
    private bool[] isCatUnlocked;                                   // ����� �ر� ���� �迭
    private bool[] isGetFirstUnlockedReward;                        // ����� ù �ر� ���� ȹ�� ���� �迭

    // ======================================================================================================================

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

    // ======================================================================================================================

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";               // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";             // ��Ȱ��ȭ���� Color

    // ======================================================================================================================
    // [������]

    [Header("---[Test]")]
    [SerializeField] private Transform buttonParent; // ��ư�� ��ġ�� �θ� ������Ʈ
    [SerializeField] private Button[] buttonPrefabs; // ���� 1~5 ��ư ������ �迭 (5��)

    [Header("---[Friendship System]")]
    [SerializeField] private Transform friendshipButtonParent;  // ������ ��ư���� �θ� Transform
    [SerializeField] private Button[] friendshipButtonPrefabs;  // ���� 1~5 ��ư ������ �迭
    private Button[] activeButtons; // ���� ���Ǵ� 5���� ��ư

    // ���� ���õ� ����� ��� ����
    public int currentSelectedCatGrade = -1;

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
            return;
        }

        newCatPanel.SetActive(false);
        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;

        // ������ ��ư �ʱ�ȭ�� �� ���� ����
        InitializeFriendshipButtons();
        InitializeDictionaryManager();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("DictionaryMenu", dictionaryMenuPanel, dictionaryButtonImage);
    }

    // ======================================================================================================================
    // [�ʱ� ����]

    // ��� ���� �Լ���
    private void InitializeDictionaryManager()
    {
        LoadUnlockedCats();
        ResetScrollPositions();
        InitializeDictionaryButton();
        InitializeSubMenuButtons();
        PopulateDictionary();
        InitializeInformationCatDefaultImage();

        // �̺�Ʈ ��� (���°� ����� �� UpdateNewImageStatus ȣ��)
        OnCatDataChanged += UpdateNewImageStatus;

        // �ʱ�ȭ �� New Image UI ����
        UpdateNewImageStatus();
    }

    // ======================================================================================================================
    // [�ر� ����]

    // ��� �ر� ���¸� �ҷ����� �Լ�
    public void LoadUnlockedCats()
    {
        InitializeCatUnlockData();
    }

    // ����� �ر� ���� �ʱ�ȭ �Լ�
    public void InitializeCatUnlockData()
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

    // Ư�� ����̸� �ر��ϴ� �Լ�
    public void UnlockCat(int CatGrade)
    {
        if (CatGrade < 0 || CatGrade >= isCatUnlocked.Length || isCatUnlocked[CatGrade])
        {
            return;
        }

        isCatUnlocked[CatGrade] = true;
        SaveUnlockedCats(CatGrade);

        // �̺�Ʈ �߻�
        OnCatDataChanged?.Invoke();
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

    // ======================================================================================================================
    // [���� ����]

    // �ʱ� ��ũ�� ��ġ �ʱ�ȭ �Լ�
    private void ResetScrollPositions()
    {
        foreach (var scrollRect in dictionaryScrollRects)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    // DictionaryButton �����ϴ� �Լ�
    private void InitializeDictionaryButton()
    {
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

        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // ���� �����͸� ä��� �Լ�
    private void PopulateDictionary()
    {
        if (GameManager.Instance.AllCatData == null || GameManager.Instance.AllCatData.Length == 0)
        {
            Debug.LogError("No cat data found in GameManager.");
            return;
        }

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

        if (IsCatUnlocked(cat.CatGrade - 1))
        {
            button.interactable = true;
            text.text = $"{cat.CatGrade}. {cat.CatName}";
            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

            // ù �ر� ������ �޾Ҵٸ� �ش� ������ firstOpenBG ��Ȱ��ȭ / ���� �ʾҴٸ� firstOpenBG Ȱ��ȭ
            if (IsGetFirstUnlockedReward(cat.CatGrade))
            {
                firstOpenBG.gameObject.SetActive(false);
            }
            else
            {
                firstOpenBG.gameObject.SetActive(true);
                firstOpenCashtext.text = $"+ {GameManager.Instance.AllCatData[cat.CatGrade].CatGetCoin}";
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

        //// ��ư�� �ν��Ͻ�ȭ�� �� SFX ���
        //if (button != null && OptionManager.Instance != null)
        //{
        //    button.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
        //}
    }

    // ���ο� ����̸� �ر��ϸ� ������ ������Ʈ�ϴ� �Լ�
    public void UpdateDictionary(int catGrade)
    {
        // scrollRectContents ���� CatGrade�� ������ ������ ������ ã�Ƽ� �ش� ������ ������Ʈ
        Transform slot = scrollRectContents.GetChild(catGrade);

        slot.gameObject.SetActive(true);

        Button button = slot.transform.Find("Button")?.GetComponent<Button>();
        TextMeshProUGUI text = slot.transform.Find("Button/Name Text")?.GetComponent<TextMeshProUGUI>();
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();

        RectTransform textRect = text.GetComponent<RectTransform>();

        button.interactable = true;

        iconImage.sprite = GameManager.Instance.AllCatData[catGrade].CatImage;
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

        text.text = $"{catGrade + 1}. {GameManager.Instance.AllCatData[catGrade].CatName}";
        textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, 100);

        if (!IsGetFirstUnlockedReward(catGrade))
        {
            firstOpenBG.gameObject.SetActive(true);
            firstOpenCashtext.text = $"+ {GameManager.Instance.AllCatData[catGrade].CatGetCoin}";
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

        // ��ư ������Ʈ �� SFX ���
        if (OptionManager.Instance != null)
        {
            button.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
        }
    }

    private void InitializeInformationCatDefaultImage()
    {
        //informationCatDefaultImage = Resources.Load<Sprite>("Sprites/UI/I_UI_Book_mission/I_UI_Book_CatBG.9");
    }

    // ������ Ȱ��ȭ �ǰų� ���� �޴��� ������ InformationPanel�� �⺻ ������ ������Ʈ ���ִ� �Լ�
    private void UpdateInformationPanel()
    {
        currentSelectedCatGrade = -1;

        // �̹��� ����
        //informationCatIcon.sprite = informationCatDefaultImage;
        informationCatIcon.gameObject.SetActive(false);

        // �ؽ�Ʈ ����
        informationCatDetails.text = $"����̸� �����ϼ���\n";

        // ��ũ�� ��Ȱ��ȭ
        catInformationPanel.GetComponent<ScrollRect>().enabled = false;

        // �ʱ� catInformation Mask �ʱ�ȭ
        catInformationPanel.GetComponent<Mask>().enabled = true;

        // fullInformationPanel�� Y��ǥ�� -312.5f�� ����
        fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);

        // ������ ��ư ��Ȱ��ȭ
        if (activeButtons != null)
        {
            foreach (var button in activeButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(false);
            }
        }
    }

    // ����� ������ Information Panel�� ǥ���ϴ� �Լ�
    private void ShowInformationPanel(int catGrade)
    {
        currentSelectedCatGrade = catGrade + 1;
        var catData = GameManager.Instance.AllCatData[catGrade];

        // ���� ���� ǥ�� �ڵ�...
        informationCatIcon.gameObject.SetActive(true);
        informationCatIcon.sprite = catData.CatImage;

        string catInfo = $"�̸�: {catData.CatName}\n" +
                         $"���: {catData.CatGrade}\n" +
                         $"���ݷ� ������: {catData.CatDamage}%\n" +
                         $"ü��: {catData.CatHp}\n" +
                         $"��ȭ���޷�: {catData.CatGetCoin}";
        informationCatDetails.text = catInfo;

        // ��ũ�� ����
        catInformationPanel.GetComponent<ScrollRect>().enabled = true;
        catInformationPanel.GetComponent<ScrollRect>().velocity = Vector2.zero;
        fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);

        // ������ UI ������Ʈ
        UpdateFriendshipButtonsForCat(currentSelectedCatGrade);
        FriendshipManager.Instance.UpdateFriendshipUI(currentSelectedCatGrade);
    }

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
        //submitButton.onClick.RemoveAllListeners();            // �̰� ������ sfx�Ҹ��� �ش� ��ư�� �� ��
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

    // ======================================================================================================================
    // [���� �޴�]

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

    // ======================================================================================================================

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

    // 
    private void OnDestroy()
    {
        // �̺�Ʈ �ڵ鷯 ����
        OnCatDataChanged -= UpdateNewImageStatus;

        // ������ ��ư ����
        if (activeButtons != null)
        {
            foreach (var button in activeButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
        }
    }

    // ======================================================================================================================

    // ���� ���õ� ����� ��� ��ȯ �Լ� �߰�
    public int GetCurrentSelectedCatGrade()
    {
        return currentSelectedCatGrade;
    }

    // ======================================================================================================================

    // ������ ��ư �ʱ�ȭ
    private void InitializeFriendshipButtons()
    {
        if (activeButtons != null) return; // �̹� �ʱ�ȭ�Ǿ� �ִٸ� ��ŵ

        activeButtons = new Button[5];

        // 5���� ��ư�� ����
        for (int i = 0; i < friendshipButtonPrefabs.Length; i++)
        {
            activeButtons[i] = Instantiate(friendshipButtonPrefabs[i], friendshipButtonParent);
            activeButtons[i].gameObject.SetActive(false); // �ʱ⿡�� ��Ȱ��ȭ
        }
    }

    // ������ ��ư ���� ������Ʈ
    public void UpdateFriendshipButtonStates(int catGrade)
    {
        if (activeButtons == null) return;

        var friendshipInfo = FriendshipManager.Instance.GetFriendshipInfo(catGrade);

        for (int i = 0; i < activeButtons.Length; i++)
        {
            var button = activeButtons[i];
            if (button != null)
            {
                // LockBG ���� ������Ʈ
                Transform lockBG = button.transform.Find("LockBG");
                if (lockBG != null)
                {
                    lockBG.gameObject.SetActive(!friendshipInfo.isUnlocked[i]);
                }

                // FirstOpenBG ���� ������Ʈ
                Transform firstOpenBG = button.transform.Find("FirstOpenBG");
                if (firstOpenBG != null)
                {
                    firstOpenBG.gameObject.SetActive(friendshipInfo.isUnlocked[i] && !friendshipInfo.isClaimed[i]);
                }

                // Star ���� ������Ʈ
                Transform star = button.transform.FindDeepChild("Star");
                if (star != null)
                {
                    star.gameObject.SetActive(friendshipInfo.isUnlocked[i]);
                    if(friendshipInfo.isUnlocked[i])
                    {
                        GameObject fullStarObj = GameObject.Find("fullStar");
                        if(fullStarObj != null)
                        {
                            RectTransform fullStar = fullStarObj.GetComponent<RectTransform>(); // RectTransform ��������
                            if (fullStar != null)
                            {
                                Vector2 newOffsetMax = fullStar.offsetMax;
                                switch (i)
                                {
                                    case 0:
                                        newOffsetMax.x = -168; // right ���� 168�� ����
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                    case 1:
                                        newOffsetMax.x = -126; // right ���� 168�� ����
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                    case 2:
                                        newOffsetMax.x = -84; // right ���� 168�� ����
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                    case 3:
                                        newOffsetMax.x = -42; // right ���� 168�� ����
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                    case 4:
                                        newOffsetMax.x = 0; // right ���� 168�� ����
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                }
                            }
                        }                   
                    }
                }

                // ��ư ��ȣ�ۿ� ���� ����
                button.interactable = friendshipInfo.isUnlocked[i] && !friendshipInfo.isClaimed[i];
            }
        }
    }

    // ������ ��ư ���� ������Ʈ (����� ���� ��)
    private void UpdateFriendshipButtonsForCat(int catGrade)
    {
        if (activeButtons == null) return;

        var friendshipInfo = FriendshipManager.Instance.GetFriendshipInfo(catGrade);

        for (int i = 0; i < activeButtons.Length; i++)
        {
            var button = activeButtons[i];
            if (button != null)
            {
                button.gameObject.SetActive(true);

                // ���� �ݾ� �ؽ�Ʈ ����
                Transform firstOpenBG = button.transform.Find("FirstOpenBG");
                if (firstOpenBG != null)
                {
                    TextMeshProUGUI cashText = firstOpenBG.Find("Cash Text")?.GetComponent<TextMeshProUGUI>();
                    if (cashText != null)
                    {
                        int rewardAmount = FriendshipManager.Instance.GetRewardAmount(i);
                        cashText.text = $"+ {rewardAmount}";
                    }
                }

                // Ŭ�� �̺�Ʈ �缳��
                int level = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnFriendshipButtonClick(catGrade, level));
            }
        }

        UpdateFriendshipButtonStates(catGrade);
    }


    // ������ ��ư Ŭ�� ó��
    private void OnFriendshipButtonClick(int catGrade, int level)
    {
        if (FriendshipManager.Instance.CanClaimLevelReward(catGrade, level))
        {
            FriendshipManager.Instance.ClaimReward(catGrade, level);
            UpdateFriendshipButtonStates(catGrade);
        }
    }

}

public static class TransformExtensions
{
    public static Transform FindDeepChild(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = child.FindDeepChild(name);
            if (found != null)
                return found;
        }
        return null;
    }
}