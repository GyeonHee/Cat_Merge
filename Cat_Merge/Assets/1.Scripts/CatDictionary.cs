using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CatDictionary : MonoBehaviour
{
    private GameManager gameManager;                                // GameManager

    [SerializeField] private ScrollRect[] dictionaryScrollRects;    // ������ ��ũ�Ѻ� �迭
    [SerializeField] private GameObject[] dictionaryMenus;          // ���� �޴� Panel

    [SerializeField] private Button dictionaryButton;               // ���� ��ư
    [SerializeField] private Image dictionaryButtonImage;           // ���� ��ư �̹���

    [SerializeField] private GameObject dictionaryMenuPanel;        // ���� �޴� Panel
    [SerializeField] private Button dictionaryBackButton;           // ���� �ڷΰ��� ��ư
    private bool isDictionaryMenuOpen;                              // ���� �޴� Panel�� Ȱ��ȭ ����

    [SerializeField] private GameObject slotPrefab;                 // ���� ���� ������

    [SerializeField] private Button[] dictionaryMenuButtons;        // ������ ���� �޴� ��ư �迭

    // Enum���� �޴� Ÿ�� ���� (���� �޴��� �����ϱ� ���� ���)
    private enum DictionaryMenuType
    {
        Normal,         // �Ϲ� ����� �޴�
        Rare,           // ��� ����� �޴�
        Special,        // Ư�� ����� �޴�
        Background,     // ��� �޴�
        End             // Enum�� ���� ǥ��
    }
    private DictionaryMenuType activeMenuType;                      // ���� Ȱ��ȭ�� �޴� Ÿ��

    // �ӽ� (�������� ��� ������� ������ �ʱ�ȭ �ϱ� ����) (�ٸ� ���� �޴����� �߰��Ѵٸ� ��� ���� �ұ� ���)
    [SerializeField] private Transform scrollRectContents;          // �븻 ����� scrollRectContents

    
    // Start()
    private void Start()
    {
        gameManager = GameManager.Instance;
        
        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;

        dictionaryButton.onClick.AddListener(ToggleDictionaryMenuPanel);
        dictionaryBackButton.onClick.AddListener(CloseDictionaryMenuPanel);

        ResetScrollPositions();
        InitializeMenuButtons();
        PopulateDictionary();
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

    // ���� Panel �ݴ� �Լ�
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

        if (gameManager.IsCatUnlocked(cat.CatId))
        {
            button.interactable = true;

            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

            text.text = $"{cat.CatId}. {cat.CatName}";
        }
        else
        {
            button.interactable = false;

            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0f);

            text.text = "???";
        }
    }

    // ���ο� ����̸� �ر��Ҷ����� ������ ������Ʈ�ϴ� �Լ�
    public void UpdateDictionary(int catId)
    {
        // scrollRectContents ���� catId�� ������ ������ ������ ��ȸ�Ͽ� �ش� ������ ������Ʈ
        Transform slot = scrollRectContents.GetChild(catId);

        slot.gameObject.SetActive(true);

        TextMeshProUGUI text = slot.transform.Find("Text Image/Text")?.GetComponent<TextMeshProUGUI>();
        Button button = slot.transform.Find("Button")?.GetComponent<Button>();
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();

        button.interactable = true;

        iconImage.sprite = gameManager.AllCatData[catId].CatImage;
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

        text.text = $"{catId + 1}. {gameManager.AllCatData[catId].CatName}";
    }


}
