using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// ������ �޴� ��ũ��Ʈ
[DefaultExecutionOrder(-4)]
public class ItemMenuManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static ItemMenuManager Instance { get; private set; }

    [Header("---[ItemMenu]")]
    [SerializeField] private Button bottomItemButton;               // ������ ��ư
    [SerializeField] private Image bottomItemButtonImg;             // ������ ��ư �̹���
    [SerializeField] private GameObject itemMenuPanel;              // ������ �޴� �ǳ�
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
    [SerializeField] private Button foodUpgradeButton;                  // ���� ���׷��̵� ��ư
    [SerializeField] private Button foodUpgrade2Button;                 // ���� ���׷��̵�2 ��ư
    [SerializeField] private Button autoCollectingButton;               // �ڵ� �����ֱ� ��ư

    [SerializeField] private GameObject[] disabledBg;                   // ��ư Ŭ�� ���� ���� ���

    [Header("---[UI Text]")]
    [SerializeField] private TextMeshProUGUI[] itemMenuesLvText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesValueText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesFeeText;

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";                   // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";                 // ��Ȱ��ȭ���� Color

    private ActivePanelManager activePanelManager;                      // ActivePanelManager

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

    // ���� ���׷��̵�
    private int foodUpgradeLv = 0;
    public int FoodUpgradeLv { get => foodUpgradeLv; set => foodUpgradeLv = value; }

    // ���� ���׷��̵�2
    private int foodUpgrade2Lv = 0;
    public int FoodUpgrade2Lv { get => foodUpgrade2Lv; set => foodUpgrade2Lv = value; }

    // �ڵ� �����ֱ� �ð�
    private int autoCollectingLv = 0;
    public int AutoCollectingLv { get => autoCollectingLv; set => autoCollectingLv = value; }


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
            InitializeDefaultValues();
        }

        InitializeItemMenuManager();
        UpdateAllUI();
    }

    #endregion


    #region Initialize

    // �⺻�� �ʱ�ȭ �Լ�
    private void InitializeDefaultValues()
    {
        maxCatsLv = 0;
        reduceCollectingTimeLv = 0;
        maxFoodsLv = 0;
        reduceProducingFoodTimeLv = 0;
        foodUpgradeLv = 0;
        foodUpgrade2Lv = 0;
        autoCollectingLv = 0;
    }

    // �⺻ ItemMenuManager �ʱ�ȭ �Լ�
    private void InitializeItemMenuManager()
    {
        InitializeActivePanel();
        InitializeMenuButtons();
        InitializeItemButtons();
        SetupInitialMenuState();
    }

    // ActivePanel �ʱ�ȭ �Լ�
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BottomItemMenu", itemMenuPanel, bottomItemButtonImg);

        bottomItemButton.onClick.AddListener(() => activePanelManager.TogglePanel("BottomItemMenu"));
        itemBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BottomItemMenu"));
    }

    // �޴� ��ư �ʱ�ȭ �Լ�
    private void InitializeMenuButtons()
    {
        for (int i = 0; i < itemMenuButtons.Length; i++)
        {
            int index = i;
            itemMenuButtons[i].onClick.RemoveAllListeners();
            itemMenuButtons[i].onClick.AddListener(() => OnButtonClicked(itemMenues[index]));
        }
    }

    // ������ ��ư �ʱ�ȭ �Լ�
    private void InitializeItemButtons()
    {
        increaseCatMaximumButton.onClick.RemoveAllListeners();
        increaseCatMaximumButton.onClick.AddListener(IncreaseCatMaximum);

        reduceCollectingTimeButton.onClick.RemoveAllListeners();
        reduceCollectingTimeButton.onClick.AddListener(ReduceCollectingTime);

        increaseFoodMaximumButton.onClick.RemoveAllListeners();
        increaseFoodMaximumButton.onClick.AddListener(IncreaseFoodMaximum);

        reduceProducingFoodTimeButton.onClick.RemoveAllListeners();
        reduceProducingFoodTimeButton.onClick.AddListener(ReduceProducingFoodTime);

        foodUpgradeButton.onClick.RemoveAllListeners();
        foodUpgradeButton.onClick.AddListener(FoodUpgrade);

        foodUpgrade2Button.onClick.RemoveAllListeners();
        foodUpgrade2Button.onClick.AddListener(FoodUpgrade2);

        autoCollectingButton.onClick.RemoveAllListeners();
        autoCollectingButton.onClick.AddListener(AutoCollecting);
    }

    // �ʱ� �޴� ���� ���� �Լ�
    private void SetupInitialMenuState()
    {
        itemMenues[0].SetActive(true);
        for (int i = 1; i < itemMenues.Length; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        if (ColorUtility.TryParseHtmlString(activeColorCode, out Color parsedColor))
        {
            itemMenuButtons[0].GetComponent<Image>().color = parsedColor;
        }
        isItemMenuButtonsOn[0] = true;
    }

    #endregion


    #region UI Updates

    // ��� UI ������Ʈ �Լ�
    private void UpdateAllUI()
    {
        bool shouldSave = isDataLoaded;
        isDataLoaded = false;

        UpdateItemUI(maxCatsLv, 0, increaseCatMaximumButton, ItemFunctionManager.Instance.maxCatsList);
        UpdateItemUI(reduceCollectingTimeLv, 1, reduceCollectingTimeButton, ItemFunctionManager.Instance.reduceCollectingTimeList);
        UpdateItemUI(maxFoodsLv, 2, increaseFoodMaximumButton, ItemFunctionManager.Instance.maxFoodsList);
        UpdateItemUI(reduceProducingFoodTimeLv, 3, reduceProducingFoodTimeButton, ItemFunctionManager.Instance.reduceProducingFoodTimeList);
        UpdateItemUI(foodUpgradeLv, 4, foodUpgradeButton, ItemFunctionManager.Instance.foodUpgradeList);
        UpdateItemUI(foodUpgrade2Lv, 5, foodUpgrade2Button, ItemFunctionManager.Instance.foodUpgrade2List);
        UpdateItemUI(autoCollectingLv, 6, autoCollectingButton, ItemFunctionManager.Instance.autoCollectingList);

        UpdateRelatedSystems();

        isDataLoaded = shouldSave;
    }

    // ������ UI ������Ʈ �Լ�
    private void UpdateItemUI(int level, int index, Button button, List<(int step, float value, decimal fee)> itemList)
    {
        if (itemMenuesFeeText[index] == null) return;

        bool isMaxLevel = level >= itemList.Count - 1;
        if (isMaxLevel)
        {
            SetMaxLevelUI(index, level, button, itemList[level].value);
        }
        else
        {
            SetNormalLevelUI(index, level, itemList);
        }
    }

    // �ִ� ���� UI ���� �Լ�
    private void SetMaxLevelUI(int index, int level, Button button, float value)
    {
        button.interactable = false;
        disabledBg[index].SetActive(true);

        itemMenuesLvText[index].text = $"Lv.{level + 1}";
        itemMenuesValueText[index].text = $"{value}";
        itemMenuesFeeText[index].text = "���ſϷ�";
    }


    private int nowFoodLv = 0;  // Ŭ���� ������ �����Ͽ� �� ����
    // �Ϲ� ���� UI ���� �Լ�
    private void SetNormalLevelUI(int index, int level, List<(int step, float value, decimal fee)> itemList)
    {
        itemMenuesLvText[index].text = $"Lv.{itemList[level].step}";
        if(index == 4)
        {
            itemMenuesValueText[index].text = $"{itemList[level].value - 1} �� {itemList[level + 1].value - 1}";
            nowFoodLv = (int)itemList[level].value - 1;
        }
        else if(index == 5)
        {
            itemMenuesValueText[index].text = $"{itemList[level].value}~{nowFoodLv}";
        }
        else
        {
            itemMenuesValueText[index].text = $"{itemList[level].value} �� {itemList[level + 1].value}";
        }
       
        itemMenuesFeeText[index].text = $"{GameManager.Instance.FormatPriceNumber(itemList[level].fee)}";
    }

    // �ý��� ������Ʈ �Լ�
    private void UpdateRelatedSystems()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MaxCats = (int)ItemFunctionManager.Instance.maxCatsList[maxCatsLv].value;
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.UpdateFoodText();
        }
    }

    #endregion


    #region Button System

    // ��ư Ŭ�� ó�� �Լ�
    private void OnButtonClicked(GameObject activePanel)
    {
        // ��� UI �г� ��Ȱ��ȭ
        for (int i = 0; i < itemMenues.Length; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        // Ŭ���� ��ư�� UI�� Ȱ��ȭ
        activePanel.SetActive(true);

        for (int i = 0; i < itemMenues.Length; i++)
        {
            isItemMenuButtonsOn[i] = itemMenues[i].activeSelf;
        }
        UpdateMenuButtonColors();
    }

    // �޴� ��ư ���� ������Ʈ �Լ�
    private void UpdateMenuButtonColors()
    {
        if (ColorUtility.TryParseHtmlString(activeColorCode, out Color activeColor) &&
            ColorUtility.TryParseHtmlString(inactiveColorCode, out Color inactiveColor))
        {
            for (int i = 0; i < itemMenuButtons.Length; i++)
            {
                itemMenuButtons[i].GetComponent<Image>().color = isItemMenuButtonsOn[i] ? activeColor : inactiveColor;
            }
        }
    }

    #endregion


    #region ItemMenu System

    // ������ ���׷��̵� ó�� �Լ�
    private void ProcessUpgrade(int index, ref int level, List<(int step, float value, decimal fee)> itemList, Button button, Action onSuccess = null)
    {
        if (itemMenuesFeeText[index] == null) return;

        if (level >= itemList.Count - 1)
        {
            SetMaxLevelUI(index, level, button, itemList[level].value);
            return;
        }

        decimal fee = itemList[level].fee;
        if (GameManager.Instance.Coin < fee)
        {
            NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!!");
            return;
        }

        GameManager.Instance.Coin -= fee;
        level++;

        if (level >= itemList.Count - 1)
        {
            SetMaxLevelUI(index, level, button, itemList[level].value);
        }
        else
        {
            UpdateItemUI(level, index, button, itemList);
        }

        onSuccess?.Invoke();
        GoogleSave();
    }

    // ����� �ִ�ġ ���� �Լ�
    private void IncreaseCatMaximum()
    {
        ProcessUpgrade(
            0, 
            ref maxCatsLv, 
            ItemFunctionManager.Instance.maxCatsList,
            increaseCatMaximumButton,
            () =>
            {
                GameManager.Instance.MaxCats = (int)ItemFunctionManager.Instance.maxCatsList[maxCatsLv].value;
            });

    }

    // ��ȭ ȹ�� �ð� ���� �Լ�
    private void ReduceCollectingTime()
    {
        ProcessUpgrade(
            1, 
            ref reduceCollectingTimeLv,
            ItemFunctionManager.Instance.reduceCollectingTimeList,
            reduceCollectingTimeButton
            );
    }

    // ���� �ִ�ġ ���� �Լ�
    private void IncreaseFoodMaximum()
    {
        ProcessUpgrade(
            2, 
            ref maxFoodsLv, 
            ItemFunctionManager.Instance.maxFoodsList,
            increaseFoodMaximumButton,
            () => 
            {
                SpawnManager.Instance.UpdateFoodText();
                SpawnManager.Instance.OnMaxFoodIncreased();
            });
    }

    // ���� ���� �ð� ���� �Լ�
    private void ReduceProducingFoodTime()
    {
        ProcessUpgrade(
            3, 
            ref reduceProducingFoodTimeLv,
            ItemFunctionManager.Instance.reduceProducingFoodTimeList,
            reduceProducingFoodTimeButton
            );
    }

    // �ڵ� �����ֱ� �ð� ���� �Լ�
    private void FoodUpgrade()
    {
        ProcessUpgrade(
            4,
            ref foodUpgradeLv,
            ItemFunctionManager.Instance.foodUpgradeList,
            foodUpgradeButton
            );
    }

    // �ڵ� �����ֱ� �ð� ���� �Լ�
    private void FoodUpgrade2()
    {
        ProcessUpgrade(
            5,
            ref foodUpgrade2Lv,
            ItemFunctionManager.Instance.foodUpgrade2List,
            foodUpgrade2Button
            );
    }

    // �ڵ� �����ֱ� �ð� ���� �Լ�
    private void AutoCollecting()
    {
        ProcessUpgrade(
            6, 
            ref autoCollectingLv,
            ItemFunctionManager.Instance.autoCollectingList,
            autoCollectingButton
            );
    }

    #endregion


    #region Battle System

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

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public int maxCatsLv;                       // ����� �ִ� ������ ����
        public int reduceCollectingTimeLv;          // ��ȭ ȹ�� �ð� ����
        public int maxFoodsLv;                      // ���� �ִ�ġ ����
        public int reduceProducingFoodTimeLv;       // ���� ���� �ð� ����
        public int autoCollectingLv;                // �ڵ� �����ֱ� ����
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            maxCatsLv = this.maxCatsLv,
            reduceCollectingTimeLv = this.reduceCollectingTimeLv,
            maxFoodsLv = this.maxFoodsLv,
            reduceProducingFoodTimeLv = this.reduceProducingFoodTimeLv,
            autoCollectingLv = this.autoCollectingLv
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        this.maxCatsLv = savedData.maxCatsLv;
        this.reduceCollectingTimeLv = savedData.reduceCollectingTimeLv;
        this.maxFoodsLv = savedData.maxFoodsLv;
        this.reduceProducingFoodTimeLv = savedData.reduceProducingFoodTimeLv;
        this.autoCollectingLv = savedData.autoCollectingLv;

        UpdateAllUI();

        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}
