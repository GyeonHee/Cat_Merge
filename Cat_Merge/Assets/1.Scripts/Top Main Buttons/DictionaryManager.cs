using UnityEngine;
using TMPro;
using UnityEngine.UI;

// ����� ���� Script
public class DictionaryManager : MonoBehaviour
{
    private GameManager gameManager;                                // GameManager

    [Header("---[Dictionary Manager]")]
    [SerializeField] private ScrollRect[] dictionaryScrollRects;    // ������ ��ũ�Ѻ� �迭
    [SerializeField] private GameObject[] dictionaryMenus;          // ���� �޴� Panel
    [SerializeField] private Button dictionaryButton;               // ���� ��ư
    [SerializeField] private Image dictionaryButtonImage;           // ���� ��ư �̹���
    [SerializeField] private GameObject dictionaryMenuPanel;        // ���� �޴� Panel
    [SerializeField] private Button dictionaryBackButton;           // ���� �ڷΰ��� ��ư
    private bool isDictionaryMenuOpen;                              // ���� �޴� Panel�� Ȱ��ȭ ����

    [SerializeField] private GameObject slotPrefab;                 // ���� ���� ������

    [SerializeField] private Button[] dictionaryMenuButtons;        // ������ ���� �޴� ��ư �迭

    // ======================================================================================================================

    [Header("---[New Cat Panel UI]")]
    [SerializeField] private GameObject newCatPanel;                // New Cat Panel
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

    // �ӽ� (�ٸ� ���� �޴����� �߰��Ѵٸ� ��� ���� �ұ� ���)
    [SerializeField] private Transform scrollRectContents;          // �븻 ����� scrollRectContents (�������� ��� ������� ������ �ʱ�ȭ �ϱ� ����)
                                                                    // ��� ����� scrollRectContents
                                                                    // Ư�� ����� scrollRectContents
                                                                    // ��� scrollRectContents

    // ======================================================================================================================

    // Start
    private void Start()
    {
        gameManager = GameManager.Instance;

        newCatPanel.SetActive(false);
        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;

        dictionaryButton.onClick.AddListener(ToggleDictionaryMenuPanel);
        dictionaryBackButton.onClick.AddListener(CloseDictionaryMenuPanel);

        ResetScrollPositions();
        InitializeMenuButtons();
        PopulateDictionary();

        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // �ʱ� ��ũ�� ��ġ �ʱ�ȭ �Լ�
    private void ResetScrollPositions()
    {
        foreach (var scrollRect in dictionaryScrollRects)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    // ���� �޴� ��ư �ʱ�ȭ �� Ŭ�� �̺�Ʈ �߰� �Լ�
    private void InitializeMenuButtons()
    {
        for (int i = 0; i < (int)DictionaryMenuType.End; i++)
        {
            int index = i;
            dictionaryMenuButtons[index].onClick.AddListener(() => ActivateMenu((DictionaryMenuType)index));
        }

        ActivateMenu(DictionaryMenuType.Normal);
    }

    // ���� Panel ���� �ݴ� �Լ�
    private void ToggleDictionaryMenuPanel()
    {
        isDictionaryMenuOpen = !isDictionaryMenuOpen;
        dictionaryMenuPanel.SetActive(isDictionaryMenuOpen);
        UpdateButtonColor(dictionaryButtonImage, isDictionaryMenuOpen);
    }

    // ���� Panel �ݴ� �Լ� (dictionaryBackButton)
    private void CloseDictionaryMenuPanel()
    {
        isDictionaryMenuOpen = false;
        dictionaryMenuPanel.SetActive(false);
        UpdateButtonColor(dictionaryButtonImage, false);
    }

    // ������ ���� �޴��� Ȱ��ȭ�ϴ� �Լ�
    private void ActivateMenu(DictionaryMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < dictionaryMenus.Length; i++)
        {
            dictionaryMenus[i].SetActive(i == (int)menuType);
        }

        UpdateMenuButtonColors();
    }

    // ���� �޴� ��ư ������ ������Ʈ�ϴ� �Լ�
    private void UpdateMenuButtonColors()
    {
        for (int i = 0; i < dictionaryMenuButtons.Length; i++)
        {
            UpdateButtonColor(dictionaryMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
        }
    }

    // ��ư ������ Ȱ�� ���¿� ���� ������Ʈ�ϴ� �Լ�
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? "#5f5f5f" : "#E2E2E2";
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    // ���� �����͸� ä��� �Լ�
    private void PopulateDictionary()
    {
        if (gameManager.AllCatData == null || gameManager.AllCatData.Length == 0)
        {
            Debug.LogError("No cat data found in GameManager.");
            return;
        }

        foreach (Transform child in scrollRectContents)
        {
            Destroy(child.gameObject);
        }

        foreach (Cat cat in gameManager.AllCatData)
        {
            InitializeSlot(cat);
        }
    }

    // ����� �����͸� �������� �ʱ� ������ �����ϴ� �Լ�
    private void InitializeSlot(Cat cat)
    {
        GameObject slot = Instantiate(slotPrefab, scrollRectContents);

        Button button = slot.transform.Find("Button")?.GetComponent<Button>();
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        TextMeshProUGUI text = slot.transform.Find("Text Image/Text")?.GetComponent<TextMeshProUGUI>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();

        // ���߿� ������ �ҷ����� ����� �ִٸ� ������ �־���ϱ� ������ �ۼ� (����� ������ else������ �Ѿ)
        if (gameManager.IsCatUnlocked(cat.CatGrade - 1))
        {
            button.interactable = true;
            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);
            text.text = $"{cat.CatGrade}. {cat.CatName}";

            // ù �ر� ������ �޾Ҵٸ� �ش� ������ firstOpenBG ��Ȱ��ȭ / ���� �ʾҴٸ� firstOpenBG Ȱ��ȭ
            firstOpenBG.gameObject.SetActive(false);
        }
        else
        {
            button.interactable = false;
            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0f);
            text.text = "???";

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
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        TextMeshProUGUI text = slot.transform.Find("Text Image/Text")?.GetComponent<TextMeshProUGUI>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();

        button.interactable = true;

        iconImage.sprite = gameManager.AllCatData[catGrade].CatImage;
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

        text.text = $"{catGrade + 1}. {gameManager.AllCatData[catGrade].CatName}";
        
        if (!gameManager.IsGetFirstUnlockedReward(catGrade))
        {
            firstOpenBG.gameObject.SetActive(true);
            firstOpenCashtext.text = $"+ {gameManager.AllCatData[catGrade].CatGetCoin}";
        }
        else
        {
            firstOpenBG.gameObject.SetActive(false);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (!gameManager.IsGetFirstUnlockedReward(catGrade))
            {
                gameManager.GetFirstUnlockedReward(catGrade);
                firstOpenBG.gameObject.SetActive(false);
            }
            else
            {
                ShowNewCatPanel(catGrade);
            }
        });
    }

    // ���ο� ����� �ر� ȿ�� & �������� �ش� ����� ��ư�� ������ ������ New Cat Panel �Լ�
    public void ShowNewCatPanel(int catGrade)
    {
        Cat newCat = gameManager.AllCatData[catGrade];

        newCatPanel.SetActive(true);

        newCatIcon.sprite = newCat.CatImage;
        newCatName.text = newCat.CatName;
        newCatExplain.text = newCat.CatExplain;
        newCatGetCoin.text = "Get Coin : " + newCat.CatGetCoin.ToString();

        // Ȯ�� ��ư�� ������ �г��� ��Ȱ��ȭ
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // New Cat Panel�� �ݴ� �Լ�
    private void CloseNewCatPanel()
    {
        newCatPanel.SetActive(false);
    }


}
