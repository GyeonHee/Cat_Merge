using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CatFriendshipUI
{
    public List<Button> buttons = new List<Button>(); // ����̺� ��ư (5��)
    public List<Image> images = new List<Image>(); // �� ��ư�� �ش��ϴ� �̹���
}

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

    [Header("---[Friendship UnLock Buttons]")]
    //[SerializeField] public List<CatFriendshipUI> catFriendshipUIs = new List<CatFriendshipUI>();
    //[SerializeField] public Button[] friendshipUnlockButtons;      // 
    //[SerializeField] public GameObject[] friendshipGetCrystalImg;   // 
    //[SerializeField] public GameObject[] friendshipLockImg;         // 
    //[SerializeField] public GameObject[] friendshipStarImg;         // ��� scrollRectContents

    [Header("---[Test]")]
    [SerializeField] private Transform buttonParent; // ��ư�� ��ġ�� �θ� ������Ʈ
    [SerializeField] private Button[] buttonPrefabs; // ���� 1~5 ��ư ������ �迭 (5��)

    //[SerializeField] public List<Button> friendshipUnlockButtonss = new List<Button>(); // ���� ��ư ����Ʈ
    //[SerializeField] public Image[][] img;

    public Dictionary<int, List<Button>> characterButtons = new Dictionary<int, List<Button>>();
    private Dictionary<int, Image[][]> characterImages = new Dictionary<int, Image[][]>(); // ĳ���ͺ� ��ư�� �̹��� �迭
    public void Initialize(int characterId)
    {
        //img = new Image[buttonPrefabs.Length][];

        //// ���� ��ư ���� (���� ĳ���� ��ư ����)
        //foreach (var btn in friendshipUnlockButtonss)
        //{
        //    Destroy(btn.gameObject);
        //}
        //friendshipUnlockButtonss.Clear();

        //// 5���� ��ư�� �� ĳ���͸��� ���������� ����
        //for (int i = 0; i < buttonPrefabs.Length; i++)
        //{
        //    Button newButton = Instantiate(buttonPrefabs[i], buttonParent); // �ش� ���� ��ư ����
        //    friendshipUnlockButtonss.Add(newButton);
        //    int level = i + 1; // ��ư ���� (1~5)
        //    newButton.onClick.AddListener(() => UnlockFriendship(level));

        //    img[i] = newButton.GetComponentsInChildren<Image>(true);
        //}

        ////  �θ� ������Ʈ�� ���� �ִ� ��ư�� ���� ����
        //foreach (Transform child in buttonParent)
        //{
        //    Destroy(child.gameObject);
        //}

        //  ���� ��ư ����
        if (characterButtons.ContainsKey(characterId))
        {
            characterButtons[characterId].Clear();
        }
        else
        {
            characterButtons[characterId] = new List<Button>();
            characterImages[characterId] = new Image[buttonPrefabs.Length][];
        }

        //  ���ο� ��ư ����
        for (int i = 0; i < buttonPrefabs.Length; i++)
        {
            Button newButton = Instantiate(buttonPrefabs[i], buttonParent);
            characterButtons[characterId].Add(newButton); //  ĳ���� ID�� ��ư ����

            int level = i + 1; // ���� 1~5
            newButton.onClick.AddListener(() => UnlockFriendship( level));

            // ��ư ������ �̹��� ����
            characterImages[characterId][i] = newButton.GetComponentsInChildren<Image>(true);
        }
    }

    private void UnlockFriendship(int level)
    {
        Debug.Log($"{currentSelectedCatGrade}����� ���� {level} ���� �ر�!");

        // �ش� ĳ������ ���� �ر� ���� �߰�

        //if(level == 1)
        //{
        //    // Debug�� img �迭 Ȯ��
        //    for (int i = 0; i < img.Length; i++)
        //    {
        //        for (int j = 0; j < img[i].Length; j++)
        //        {
        //            if (img[level - 1][j].name == "FirstOpenBG")
        //            {
        //                img[level - 1][j].gameObject.SetActive(false);
        //            }
        //        }
        //    }
        //}    

        if (characterImages.ContainsKey(currentSelectedCatGrade))
        {
            foreach (var img in characterImages[currentSelectedCatGrade][level - 1])
            {
                if (img.name == "FirstOpenBG")
                {
                    img.gameObject.SetActive(false);
                }
            }
        }
    }

    public List<Button> GetCharacterButtons(int characterId)
    {
        return characterButtons.ContainsKey(characterId) ? characterButtons[characterId] : null;
    }

    public Image[][] GetCharacterImages(int characterId)
    {
        return characterImages.ContainsKey(characterId) ? characterImages[characterId] : null;
    }
    // ======================================================================================================================

    // ���� ���õ� ����� ��� ����
    public int currentSelectedCatGrade = -1;

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
        newCatPanel.SetActive(false);
        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;

        InitializeDictionaryManager();
        //InitializeFriendshipButton();
     
        for(int i = 0; i < 3; i++)
        {
            Initialize(i);
        }

        int totalButtonCount = characterButtons.Sum(pair => pair.Value.Count);
        Debug.Log($"�� ��ư ����: {totalButtonCount}");
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
    }

    private void InitializeInformationCatDefaultImage()
    {
        informationCatDefaultImage = Resources.Load<Sprite>("Sprites/UI/I_UI_Book_mission/I_UI_Book_CatBG.9");
    }

    // ������ Ȱ��ȭ �ǰų� ���� �޴��� ������ InformationPanel�� �⺻ ������ ������Ʈ ���ִ� �Լ�
    private void UpdateInformationPanel()
    {
        currentSelectedCatGrade = -1;

        // �̹��� ����
        informationCatIcon.sprite = informationCatDefaultImage;

        // �ؽ�Ʈ ����
        informationCatDetails.text = $"����̸� �����ϼ���\n";

        // ��ũ�� ��Ȱ��ȭ
        catInformationPanel.GetComponent<ScrollRect>().enabled = false;

        // �ʱ� catInformation Mask �ʱ�ȭ
        catInformationPanel.GetComponent<Mask>().enabled = true;

        // fullInformationPanel�� Y��ǥ�� -312.5f�� ����
        fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);
    }

    // ����� ������ Information Panel�� ǥ���ϴ� �Լ�
    private void ShowInformationPanel(int catGrade)
    {
        currentSelectedCatGrade = catGrade + 1; // ���� ������� ���� (0-based to 1-based)

        // ����� ���� �ҷ�����
        var catData = GameManager.Instance.AllCatData[catGrade];

        // �̹��� ����
        informationCatIcon.sprite = catData.CatImage;

        // ���� ���� ���� ���� Ȯ��
        bool canClaimReward = FriendshipManager.Instance.CanClaimReward(currentSelectedCatGrade);


        // �ؽ�Ʈ ����
        string catInfo = $"�̸�: {catData.CatName}\n" +
                         $"���: {catData.CatGrade}\n" +
                         $"���ݷ� ������: {catData.CatDamage}%\n" +
                         $"ü��: {catData.CatHp}\n" +
                         $"��ȭ���޷�: {catData.CatGetCoin}";
        informationCatDetails.text = catInfo;

        // ��ũ�� Ȱ��ȭ, ��ũ�� ���̾��ٸ� ����
        catInformationPanel.GetComponent<ScrollRect>().enabled = true;
        catInformationPanel.GetComponent<ScrollRect>().velocity = Vector2.zero;

        // fullInformationPanel�� Y��ǥ�� -312.5f�� ����
        fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);

        // ==================================================================================
        // ȣ���� ����

       

        // ȣ���� ������ ������Ʈ
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
        submitButton.onClick.RemoveAllListeners();
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
    }


    // ============================================================================================================
    // ��ư���� ���� �Լ���

    //private void InitializeFriendshipButton()
    //{
    //    friendshipUnlockButtons[0].onClick.AddListener(UnLockLevel1Friendship);
    //    friendshipUnlockButtons[1].onClick.AddListener(UnLockLevel2Friendship);
    //    friendshipUnlockButtons[2].onClick.AddListener(UnLockLevel3Friendship);
    //    friendshipUnlockButtons[3].onClick.AddListener(UnLockLevel4Friendship);
    //    friendshipUnlockButtons[4].onClick.AddListener(UnLockLevel5Friendship);

    //    for (int i = 0; i < friendshipUnlockButtons.Length; i++)
    //    {
    //        friendshipUnlockButtons[i].interactable = false;
    //    }
    //}
    //private void UnLockLevel1Friendship()
    //{
    //    friendshipGetCrystalImg[0].SetActive(false);    
    //    GameManager.Instance.Cash += FriendshipDataLoader.Instance.GetDataByGrade(1)[0].reward;
    //    friendshipUnlockButtons[0].interactable = false;

    //    var info = FriendshipManager.Instance.GetFriendshipInfo(1);
    //    info.currentExp -= info.nextLevelExp;  
    //    info.nextLevelExp = FriendshipDataLoader.Instance.GetDataByGrade(1)[1].exp;
    //    // ���� ���� ó��
    //    FriendshipManager.Instance.ClaimReward(1);

    //    // UI ������Ʈ
    //    FriendshipManager.Instance.UpdateFriendshipUI(1);

    //}
    //private void UnLockLevel2Friendship()
    //{
    //    friendshipGetCrystalImg[1].SetActive(false);
    //    GameManager.Instance.Cash += FriendshipDataLoader.Instance.GetDataByGrade(1)[1].reward;
    //    friendshipUnlockButtons[1].interactable = false;

    //    // ���� ���� ó��
    //    FriendshipManager.Instance.ClaimReward(1);

    //    // UI ������Ʈ
    //    FriendshipManager.Instance.UpdateFriendshipUI(1);
    //}
    //private void UnLockLevel3Friendship()
    //{
    //    friendshipGetCrystalImg[2].SetActive(false);
    //    GameManager.Instance.Cash += FriendshipDataLoader.Instance.GetDataByGrade(1)[2].reward;
    //    friendshipUnlockButtons[2].interactable = false;

    //    // ���� ���� ó��
    //    FriendshipManager.Instance.ClaimReward(1);

    //    // UI ������Ʈ
    //    FriendshipManager.Instance.UpdateFriendshipUI(1);
    //}
    //private void UnLockLevel4Friendship()
    //{
    //    friendshipGetCrystalImg[3].SetActive(false);
    //    GameManager.Instance.Cash += FriendshipDataLoader.Instance.GetDataByGrade(1)[3].reward;
    //    friendshipUnlockButtons[3].interactable = false;

    //    // ���� ���� ó��
    //    FriendshipManager.Instance.ClaimReward(1);

    //    // UI ������Ʈ
    //    FriendshipManager.Instance.UpdateFriendshipUI(1);
    //}
    //private void UnLockLevel5Friendship()
    //{
    //    friendshipGetCrystalImg[4].SetActive(false);
    //    GameManager.Instance.Cash += FriendshipDataLoader.Instance.GetDataByGrade(1)[4].reward;
    //    friendshipUnlockButtons[4].interactable = false;
    //}

    // ======================================================================================================================

    // ���� ���õ� ����� ��� ��ȯ �Լ� �߰�
    public int GetCurrentSelectedCatGrade()
    {
        return currentSelectedCatGrade;
    }


}
