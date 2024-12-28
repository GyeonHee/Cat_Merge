using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemMenuManager : MonoBehaviour
{
    // Singleton Instance
    public static ItemMenuManager Instance { get; private set; }

    private ActivePanelManager activePanelManager;                  // ActivePanelManager
    [Header("---[ItemMenu]")]
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
    [Header("---[ItemMenues]")]
    [SerializeField] private Button[] itemMenuButtons;                  // itemPanel������ ������ �޴� ��ư��
    [SerializeField] private GameObject[] itemMenues;                   // itemPanel������ �޴� ��ũ��â �ǳ�
    private bool[] isItemMenuButtonsOn = new bool[4];                   // ��ư ���� �����ϱ� ���� boolŸ�� �迭

    // ������ �޴� �ȿ� ������ ���
    [Header("---[ItemMenuList]")]
    [SerializeField] private Button catMaximumIncreaseButton;           // ����� �ִ�ġ ���� ��ư
    [SerializeField] private GameObject disabledBg;                     // ��ư Ŭ�� ���� ���� ���
    private int catMaximumIncreaseFee = 10;                             // ����� �ִ�ġ ���� ���
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseFeeText; // ����� �ִ�ġ ���� ��� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseLvText;  // ����� �ִ�ġ ���� ���� �ؽ�Ʈ
    private int catMaximumIncreaseLv = 1;                               // ����� �ִ�ġ ���� ����
    [SerializeField] private TextMeshProUGUI catHowManyIncreaseText; // ����� ���� �󸶳� �ߴ��� �ؽ�Ʈ
    private int catHowManyIncrease = 1;


    private int step = 0;
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
        }
        InitializeItemMenuManager();

        itemMenues[(int)EitemMenues.itemMenues].SetActive(true);
        if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
        {
            itemMenuButtons[(int)EitemMenuButton.itemMenuButton].GetComponent<Image>().color = parsedColor;
        }

        itemMenuButtons[(int)EitemMenuButton.itemMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.itemMenues]));
        itemMenuButtons[(int)EitemMenuButton.colleagueMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.colleagueMenues]));
        itemMenuButtons[(int)EitemMenuButton.backgroundMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.backgroundMenues]));
        itemMenuButtons[(int)EitemMenuButton.goldenSomethingMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.goldenMenues]));

        for (int i = 1; i < 4; i++)
        {
            isItemMenuButtonsOn[i] = false;
        }
        UpdateIncreaseMaximumFeeText();
        UpdateIncreaseMaximumLvText();

        catMaximumIncreaseButton.onClick.AddListener(IncreaseCatMaximum);
    }

    private void Start()
    {
        // �ǳ� ���
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BottomItemMenu", itemMenuPanel, bottomItemButtonImg);

        catMaximumIncreaseLvText.text = $"Lv.{ ItemFunctionManager.Instance.maxCatsList[0].step}";
        catHowManyIncreaseText.text = $"{ ItemFunctionManager.Instance.maxCatsList[0].value}->{ ItemFunctionManager.Instance.maxCatsList[1].value}"; // 1->2 2->3 3->4
        catMaximumIncreaseFeeText.text = $"{ ItemFunctionManager.Instance.maxCatsList[0].fee}";
    }

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomItemMenuPanel()
    {
        bottomItemButton.onClick.AddListener(() => activePanelManager.TogglePanel("BottomItemMenu"));
        itemBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BottomItemMenu"));
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
                        itemMenuButtons[i].GetComponent<Image>().color = parsedColorT;
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColorF))
                        {
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
        //Debug.Log("����� �ִ�ġ ���� ��ư Ŭ��");
        if (catMaximumIncreaseButton != null)
        {
            //GameManager.Instance.Coin -= catMaximumIncreaseFee;
            //GameManager.Instance.MaxCats++;
            //catMaximumIncreaseFee *= 2;

            //catHowManyIncreaseText.text = $"{++catHowManyIncrease}->{catHowManyIncrease + 1}"; // 1->2 2->3 3->4

            //UpdateIncreaseMaximumFeeText();
            //UpdateIncreaseMaximumLvText();
            if (step < 0 || step > ItemFunctionManager.Instance.maxCatsList.Count)
            {
                Debug.LogError($"Index {step} is out of bounds! List Count: {ItemFunctionManager.Instance.maxCatsList.Count}");
                return;
            }
            step++;
            catMaximumIncreaseLvText.text = $"Lv.{ ItemFunctionManager.Instance.maxCatsList[step].step}";
            catHowManyIncreaseText.text = $"{ ItemFunctionManager.Instance.maxCatsList[step].value}->{ ItemFunctionManager.Instance.maxCatsList[step + 1].value}"; // 1->2 2->3 3->4
            catMaximumIncreaseFeeText.text = $"{ ItemFunctionManager.Instance.maxCatsList[step].fee}";
        }
    }

    // ����� �� �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateIncreaseMaximumFeeText()
    {
        if (catMaximumIncreaseFeeText != null)
        {
            if (GameManager.Instance.Coin < catMaximumIncreaseFee)
            {
                catMaximumIncreaseButton.interactable = false;
                disabledBg.SetActive(true);
            }
            else
            {

                catMaximumIncreaseButton.interactable = true;
                disabledBg.SetActive(false);
            }
            catMaximumIncreaseFeeText.text = $"{catMaximumIncreaseFee}";
        }
    }

    // ����� �ִ�ġ ���� ���� UI ������Ʈ �ϴ� �Լ�
    private void UpdateIncreaseMaximumLvText()
    {
        catMaximumIncreaseLvText.text = $"Lv.{catMaximumIncreaseLv}";
    }

    // ��ư Ŭ�� �̺�Ʈ
    private void OnButtonClicked(GameObject activePanel)
    {
        // ��� UI �г� ��Ȱ��ȭ
        for (int i = 0; i < (int)EitemMenues.end; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        // Ŭ���� ��ư�� UI�� Ȱ��ȭ
        activePanel.SetActive(true);

        for (int i = 0; i < (int)EitemMenues.end; i++)
        {
            if (itemMenues[i].activeSelf)
            {
                isItemMenuButtonsOn[i] = true;
            }
        }
        ChangeSelectedButtonColor(activePanel);
    }

    private void InitializeItemMenuManager()
    {
        OpenCloseBottomItemMenuPanel();
    }
}