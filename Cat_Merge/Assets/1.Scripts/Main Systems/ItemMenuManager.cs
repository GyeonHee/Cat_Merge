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
    [SerializeField] private Button increaseCatMaximumButton;           // ����� �ִ�ġ ���� ��ư
    [SerializeField] private Button reduceCollectingTimeButton;         // ��ȭ ȹ�� �ð� ���� ��ư
    [SerializeField] private Button increaseFoodMaximumButton;          // ���� �ִ�ġ ���� ��ư
    [SerializeField] private Button reduceProducingFoodTimeButton;      // ���� ���� �ð� ���� ��ư
    [SerializeField] private Button autoCollectingButton;               // �ڵ� �����ֱ� ��ư

    [SerializeField] private GameObject[] disabledBg;                     // ��ư Ŭ�� ���� ���� ���

    [SerializeField] private TextMeshProUGUI[] itemMenuesLvText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesValueText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesFeeText;

    // ����� �ִ�ġ
    private int maxCatsLv = 0;
    public int MaxCatsLv { get => maxCatsLv; set => maxCatsLv = value; }

    // ��ȭ ȹ�� �ð�
    private int reduceCollectingTimeLv = 0;
    public int ReduceCollectingTimeLv { get => reduceCollectingTimeLv; set => reduceCollectingTimeLv = value; }

    // ���� �ִ�ġ
    private int maxFoodsLv = 0;
    public int MaxFoodsLv { get => maxFoodsLv; set => maxFoodsLv = value; }

    // ���� ���� �ð�
    private int reduceProducingFoodTimeLv = 0;
    public int ReduceProducingFoodTimeLv { get => reduceProducingFoodTimeLv; set => reduceProducingFoodTimeLv = value; }

    // �ڵ� �����ֱ� �ð�
    private int autoCollectingLv = 0;
    public int AutoCollectingLv { get => autoCollectingLv; set => autoCollectingLv = value; }

    // ======================================================================================================================

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";               // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";             // ��Ȱ��ȭ���� Color

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

        itemMenues[(int)EitemMenues.itemMenues].SetActive(true);
        if (ColorUtility.TryParseHtmlString(activeColorCode, out Color parsedColor))
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

        increaseCatMaximumButton.onClick.AddListener(IncreaseCatMaximum);
        reduceCollectingTimeButton.onClick.AddListener(ReduceCollectingTime);
        increaseFoodMaximumButton.onClick.AddListener(IncreaseFoodMaximum);
        reduceProducingFoodTimeButton.onClick.AddListener(ReduceProducingFoodTime);
        autoCollectingButton.onClick.AddListener(AutoCollecting);
        OpenCloseBottomItemMenuPanel();
    }

    private void Start()
    {
        // �ǳ� ���
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BottomItemMenu", itemMenuPanel, bottomItemButtonImg);

        InitItemMenuText();
    }

    private void InitItemMenuText()
    {
        for(int i = 0; i <= 3; i++)
        {
            switch(i)
            {
                case 0:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.maxCatsList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.maxCatsList[0].value}->{ ItemFunctionManager.Instance.maxCatsList[1].value}"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.maxCatsList[0].fee}";
                    break;
                case 1:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.reduceCollectingTimeList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.reduceCollectingTimeList[0].value}->{ ItemFunctionManager.Instance.reduceCollectingTimeList[1].value}"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.reduceCollectingTimeList[0].fee}";
                    break;
                case 2:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.maxFoodsList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.maxFoodsList[0].value}->{ ItemFunctionManager.Instance.maxFoodsList[1].value}"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.maxFoodsList[0].fee}";
                    break;
                case 3:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[0].value}->{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[1].value}"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[0].fee}";
                    break;
                case 4:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.autoCollectingList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.autoCollectingList[0].value}sec"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.autoCollectingList[0].fee}";
                    break;
                default:
                    break;
            }
        }
    }

    private void Update()
    {
        UpdateItemMenuUI();
    }

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleItemMenuState()
    {
        bottomItemButton.interactable = false;

        if (itemMenuPanel.activeSelf == true)
        {
            activePanelManager.TogglePanel("BottomItemMenu");
        }
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleItemMenuState()
    {
        bottomItemButton.interactable = true;
    }

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomItemMenuPanel()
    {
        bottomItemButton.onClick.AddListener(() => activePanelManager.TogglePanel("BottomItemMenu"));
        itemBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BottomItemMenu"));
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

    // ������ �޴��� ������, ����, ���, Ȳ�ݱ��� ��ư ���� ����
    private void ChangeSelectedButtonColor(GameObject menues)
    {
        if (menues.gameObject.activeSelf)
        {
            if (ColorUtility.TryParseHtmlString(activeColorCode, out Color parsedColorT))
            {
                for (int i = 0; i < (int)EitemMenues.end; i++)
                {
                    if (isItemMenuButtonsOn[i])
                    {
                        itemMenuButtons[i].GetComponent<Image>().color = parsedColorT;
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString(inactiveColorCode, out Color parsedColorF))
                        {
                            itemMenuButtons[i].GetComponent<Image>().color = parsedColorF;
                        }
                    }
                }
            }
        }
    }

    private void UpdateItemMenuUI()
    {
        // ����� �ִ�ġ ����
        if (itemMenuesFeeText[0] != null)
        {
            if (GameManager.Instance.Coin < (int)ItemFunctionManager.Instance.maxCatsList[maxCatsLv].fee)
            {
                increaseCatMaximumButton.interactable = false;
                disabledBg[0].SetActive(true);
            }
            else
            {
                increaseCatMaximumButton.interactable = true;
                disabledBg[0].SetActive(false);
            }
            itemMenuesFeeText[0].text = $"{(int)ItemFunctionManager.Instance.maxCatsList[maxCatsLv].fee}";
        }

        // ��ȭ ȹ�� �ð� ����
        if (itemMenuesFeeText[1] != null)
        {
            if (GameManager.Instance.Coin < (int)ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].fee)
            {
                reduceCollectingTimeButton.interactable = false;
                disabledBg[1].SetActive(true);
            }
            else
            {
                reduceCollectingTimeButton.interactable = true;
                disabledBg[1].SetActive(false);
            }
            itemMenuesFeeText[1].text = $"{(int)ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].fee}";
        }

        // ���� �ִ�ġ ����
        if (itemMenuesFeeText[2] != null)
        {
            if (GameManager.Instance.Coin < (int)ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].fee)
            {
                increaseFoodMaximumButton.interactable = false;
                disabledBg[2].SetActive(true);
            }
            else
            {
                increaseFoodMaximumButton.interactable = true;
                disabledBg[2].SetActive(false);
            }
            itemMenuesFeeText[2].text = $"{(int)ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].fee}";
        }

        // ���� ���� �ð� ����
        if (itemMenuesFeeText[3] != null)
        {
            if (GameManager.Instance.Coin < (int)ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].fee)
            {
                reduceProducingFoodTimeButton.interactable = false;
                disabledBg[3].SetActive(true);
            }
            else
            {
                reduceProducingFoodTimeButton.interactable = true;
                disabledBg[3].SetActive(false);
            }
            itemMenuesFeeText[3].text = $"{(int)ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].fee}";
        }
    }

    // ����� �ִ�ġ ����
    private void IncreaseCatMaximum()
    {
        //Debug.Log("����� �ִ�ġ ���� ��ư Ŭ��");
        if (increaseCatMaximumButton != null)
        {
            if (maxCatsLv < 0 || maxCatsLv > ItemFunctionManager.Instance.maxCatsList.Count)
            {
                Debug.LogError($"Index {maxCatsLv} is out of bounds! List Count: {ItemFunctionManager.Instance.maxCatsList.Count}");
                return;
            }
            GameManager.Instance.Coin -= (int)ItemFunctionManager.Instance.maxCatsList[maxCatsLv].fee;
            GameManager.Instance.MaxCats++;
            maxCatsLv++;
            itemMenuesLvText[0].text = $"Lv.{ ItemFunctionManager.Instance.maxCatsList[maxCatsLv].step}";
            itemMenuesValueText[0].text = $"{ ItemFunctionManager.Instance.maxCatsList[maxCatsLv].value}->{ ItemFunctionManager.Instance.maxCatsList[maxCatsLv + 1].value}"; // 1->2 2->3 3->4
            itemMenuesFeeText[0].text = $"{ ItemFunctionManager.Instance.maxCatsList[maxCatsLv].fee}";
        }
    }

    // ��ȭ ȹ�� �ð� ����
    private void ReduceCollectingTime()
    {
        //Debug.Log("��ȭ ��� �ð� ���� ��ư Ŭ��");
        if (reduceCollectingTimeButton != null)
        {
            if (reduceCollectingTimeLv < 0 || reduceCollectingTimeLv > ItemFunctionManager.Instance.reduceCollectingTimeList.Count)
            {
                Debug.LogError($"Index {reduceCollectingTimeLv} is out of bounds! List Count: {ItemFunctionManager.Instance.reduceCollectingTimeList.Count}");
                return;
            }
            GameManager.Instance.Coin -= (int)ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].fee;

            reduceCollectingTimeLv++;
            itemMenuesLvText[1].text = $"Lv.{ ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].step}";
            itemMenuesValueText[1].text = $"{ ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].value}->{ ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv + 1].value}"; // 3.0->2.9
            itemMenuesFeeText[1].text = $"{ ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].fee}";
        }
    }

    // ���� �ִ�ġ ����
    private void IncreaseFoodMaximum()
    {
        //Debug.Log("���� �ִ�ġ ���� ��ư Ŭ��");
        if (increaseFoodMaximumButton != null)
        {
            if (maxFoodsLv < 0 || maxFoodsLv > ItemFunctionManager.Instance.maxFoodsList.Count)
            {
                Debug.LogError($"Index {maxFoodsLv} is out of bounds! List Count: {ItemFunctionManager.Instance.maxFoodsList.Count}");
                return;
            }
            GameManager.Instance.Coin -= (int)ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].fee;
            maxFoodsLv++;
            itemMenuesLvText[2].text = $"Lv.{ ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].step}";
            itemMenuesValueText[2].text = $"{ ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].value}->{ ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv + 1].value}"; // 3.0->2.9
            itemMenuesFeeText[2].text = $"{ ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].fee}";
            SpawnManager.Instance.OnMaxFoodIncreased();
        }
    }

    // ���� ���� �ð� ����
    private void ReduceProducingFoodTime()
    {
        //Debug.Log("���� ���� �ð� ���� ��ư Ŭ��");
        if (reduceProducingFoodTimeButton != null)
        {
            if (reduceProducingFoodTimeLv < 0 || reduceProducingFoodTimeLv > ItemFunctionManager.Instance.reduceProducingFoodTimeList.Count)
            {
                Debug.LogError($"Index {reduceProducingFoodTimeLv} is out of bounds! List Count: {ItemFunctionManager.Instance.reduceProducingFoodTimeList.Count}");
                return;
            }
            GameManager.Instance.Coin -= (int)ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].fee;
            reduceProducingFoodTimeLv++;
            itemMenuesLvText[3].text = $"Lv.{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].step}";
            itemMenuesValueText[3].text = $"{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].value}->{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv + 1].value}"; // 7->6.8
            itemMenuesFeeText[3].text = $"{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].fee}";
        }
    }

    private void AutoCollecting()
    {
        //Debug.Log("�ڵ� �����ֱ� �ð� ���� ��ư Ŭ��");
        if(autoCollectingButton != null)
        {
            if(autoCollectingLv < 0 || autoCollectingLv > ItemFunctionManager.Instance.autoCollectingList.Count)
            {
                Debug.LogError($"Index {autoCollectingLv} is out of bounds! List Count: {ItemFunctionManager.Instance.autoCollectingList.Count}");
                return;
            }
            GameManager.Instance.Coin -= (int)ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv].fee;
            autoCollectingLv++;
            itemMenuesLvText[4].text = $"Lv.{ ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv].step}";
            itemMenuesValueText[4].text = $"{ ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv].value}sec"; // 30�ʸ��� ... 15�ʸ���
            itemMenuesFeeText[4].text = $"{ ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv].fee}";
        }
    }
}