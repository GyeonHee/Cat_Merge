using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// ������ �޴� ��ũ��Ʈ
[DefaultExecutionOrder(-7)]
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
    [SerializeField] private Button foodUpgrade2DisabledButton;         // ���� ���׷��̵�2 ����ư
    [SerializeField] private Button autoCollectingButton;               // �ڵ� �����ֱ� ��ư
    [SerializeField] private Button autoMergeButton;                    // �ڵ� �ռ� ��ư

    [SerializeField] private GameObject[] disabledBg;                   // ��ư Ŭ�� ���� ���� ���

    [Header("---[UI Text]")]
    [SerializeField] private TextMeshProUGUI[] itemMenuesLvText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesValueText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesFeeText;

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";                   // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";                 // ��Ȱ��ȭ���� Color
    private Color activeColor;                                          // Ȱ��ȭ ����
    private Color inactiveColor;                                        // ��Ȱ��ȭ ����

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

    // �ڵ� �ռ� �ð�
    private int autoMergeLv = 0;
    public int AutoMergeLv { get => autoMergeLv; set => autoMergeLv = value; }


    [Header("---[ETC]")]
    private bool isDataLoaded = false;              // ������ �ε� Ȯ��

    [HideInInspector] public int minFoodLv = 1;
    [HideInInspector] public int maxFoodLv = 1;

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
        autoMergeLv = 0;

        // ���� ���׷��̵� �⺻�� (���� ���밪 ����)
        minFoodLv = 1;  // DB���� 2-1
        maxFoodLv = 1;  // DB���� 2-1 
    }

    // �⺻ ItemMenuManager �ʱ�ȭ �Լ�
    private void InitializeItemMenuManager()
    {
        ColorUtility.TryParseHtmlString(activeColorCode, out activeColor);
        ColorUtility.TryParseHtmlString(inactiveColorCode, out inactiveColor);

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

        bottomItemButton.onClick.AddListener(() => {
            activePanelManager.TogglePanel("BottomItemMenu");
            if (activePanelManager.IsPanelActive("BottomItemMenu") && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnOpenItemMenu();
            }
        });
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

        autoMergeButton.onClick.RemoveAllListeners();
        autoMergeButton.onClick.AddListener(AutoMerge);
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

        UpdateItemUI(0, maxCatsLv, increaseCatMaximumButton, ItemFunctionManager.Instance.maxCatsList);
        UpdateItemUI(1, reduceCollectingTimeLv, reduceCollectingTimeButton, ItemFunctionManager.Instance.reduceCollectingTimeList);
        UpdateItemUI(2, maxFoodsLv, increaseFoodMaximumButton, ItemFunctionManager.Instance.maxFoodsList);
        UpdateItemUI(3, reduceProducingFoodTimeLv, reduceProducingFoodTimeButton, ItemFunctionManager.Instance.reduceProducingFoodTimeList);
        UpdateItemUI(4, foodUpgradeLv, foodUpgradeButton, ItemFunctionManager.Instance.foodUpgradeList);
        UpdateItemUI(5, foodUpgrade2Lv, foodUpgrade2Button, ItemFunctionManager.Instance.foodUpgrade2List);
        UpdateItemUI(6, autoCollectingLv, autoCollectingButton, ItemFunctionManager.Instance.autoCollectingList);
        UpdateItemUI(7, autoMergeLv, autoMergeButton, ItemFunctionManager.Instance.autoMergeList);

        UpdateRelatedSystems();

        isDataLoaded = shouldSave;
    }

    // ������ UI ������Ʈ �Լ�
    private void UpdateItemUI(int index, int level, Button button, List<(int step, float value, decimal fee)> itemList)
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

        itemMenuesLvText[index].text = $"Lv.{level}";

        if (index == 4)
        {
            int actualValue = (int)value - 1;
            itemMenuesValueText[index].text = $"{actualValue}";
            maxFoodLv = actualValue;
        }
        else if (index == 5)
        {
            int actualValue = (int)value - 1;
            minFoodLv = actualValue;
            itemMenuesValueText[index].text = $"{actualValue}";
        }
        else
        {
            itemMenuesValueText[index].text = $"{value}";
        }

        itemMenuesFeeText[index].text = "���ſϷ�";
    }
    
    // �Ϲ� ���� UI ���� �Լ�
    private void SetNormalLevelUI(int index, int level, List<(int step, float value, decimal fee)> itemList)
    {
        itemMenuesLvText[index].text = $"Lv.{itemList[level].step - 1}";

        if (index == 4)
        {
            int currentMaxFood = GetActualMaxFoodLevel();
            int nextMaxFood = GetNextActualMaxFoodLevel();
            itemMenuesValueText[index].text = $"{currentMaxFood} �� {nextMaxFood}";
            maxFoodLv = currentMaxFood;

            int currentMinFood = GetActualMinFoodLevel();
            itemMenuesValueText[5].text = $"{currentMinFood}~{maxFoodLv} �� {currentMinFood}~{maxFoodLv}";

            if (maxFoodLv >= 15)
            {
                foodUpgrade2DisabledButton.gameObject.SetActive(false);
                foodUpgrade2Button.gameObject.SetActive(true);
            }
        }
        else if (index == 5)
        {
            int currentMinFood = GetActualMinFoodLevel();
            int nextMinFood = GetNextActualMinFoodLevel();
            minFoodLv = currentMinFood;
            itemMenuesValueText[index].text = $"{currentMinFood}~{maxFoodLv} �� {nextMinFood}~{maxFoodLv}";
        }
        else
        {
            itemMenuesValueText[index].text = $"{itemList[level].value} �� {itemList[level + 1].value}";
        }

        itemMenuesFeeText[index].text = $"{GameManager.Instance.FormatNumber(itemList[level].fee)}";
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
        for (int i = 0; i < itemMenuButtons.Length; i++)
        {
            itemMenuButtons[i].GetComponent<Image>().color = isItemMenuButtonsOn[i] ? activeColor : inactiveColor;
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
            NotificationManager.Instance.ShowNotification("������ �����մϴ�!!");
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
            UpdateItemUI(index, level,  button, itemList);
        }

        onSuccess?.Invoke();
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

                //Debug.Log($"����� �ִ�ġ ���� ���� ��ġ : {ItemFunctionManager.Instance.maxCatsList[maxCatsLv].value}");

                // Ʃ�丮�� ���̶�� ������ ���� �̺�Ʈ �˸�
                if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
                {
                    TutorialManager.Instance.OnMaxCatItemPurchased();
                }
            });
    }

    // ��ȭ ȹ�� �ð� ���� �Լ�
    private void ReduceCollectingTime()
    {
        ProcessUpgrade(
            1, 
            ref reduceCollectingTimeLv,
            ItemFunctionManager.Instance.reduceCollectingTimeList,
            reduceCollectingTimeButton,
            () =>
            {
                //Debug.Log($"��ȭ ȹ�� �ð� ���� ���� ��ġ : {ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].value}");
            });
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
                //Debug.Log($"���� �ִ�ġ ���� ���� ��ġ : {ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].value}");

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
            reduceProducingFoodTimeButton,
            () =>
            {
                //Debug.Log($"���� ���� �ð� ���� ���� ��ġ : {ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].value}");
            });
    }

    // ���� ���׷��̵� 1 �Լ� (��ȯ�Ҷ� �� ���� ����� ����̰� ������ Ȯ���� ����)
    private void FoodUpgrade()
    {
        ProcessUpgrade(
            4,
            ref foodUpgradeLv,
            ItemFunctionManager.Instance.foodUpgradeList,
            foodUpgradeButton,
            () =>
            {
                //Debug.Log($"���� ���׷��̵� 1 ���� ��ġ : {ItemFunctionManager.Instance.foodUpgradeList[foodUpgradeLv].value-1}");
            });
    }

    // ���� ���׷��̵� 2 �Լ� (��ȯ�Ҷ� ���� ����� ����̰� ������ �ʰ� ��)
    private void FoodUpgrade2()
    {
        // ���� ���׷��̵� 2�� ������ ���� ���׷��̵� 1�� ������ ���ų� ���ٸ� ���� �Ұ�
        if (foodUpgrade2Lv >= foodUpgradeLv-1)
        {
            NotificationManager.Instance.ShowNotification("���� ���׷��̵� 1�� ���� ������ �ּ���!!");
            return;
        }

        // ���� ���׷��̵� 2�� ������ ����(24) ���� �� ���� ���׷��̵� 1�� �ִ� ����(38)���� üũ
        if (foodUpgrade2Lv == ItemFunctionManager.Instance.foodUpgrade2List.Count - 2 &&
            foodUpgradeLv < ItemFunctionManager.Instance.foodUpgradeList.Count - 1)
        {
            NotificationManager.Instance.ShowNotification("���� ���׷��̵� 1�� ���� ������ �ּ���!!");
            return;
        }

        ProcessUpgrade(
            5,
            ref foodUpgrade2Lv,
            ItemFunctionManager.Instance.foodUpgrade2List,
            foodUpgrade2Button,
            () =>
            {
                //Debug.Log($"���� ���׷��̵� 2 ���� ��ġ : {ItemFunctionManager.Instance.foodUpgrade2List[foodUpgrade2Lv].value-1}");
            });
    }

    // �ڵ� �����ֱ� �ð� ���� �Լ�
    private void AutoCollecting()
    {
        ProcessUpgrade(
            6, 
            ref autoCollectingLv,
            ItemFunctionManager.Instance.autoCollectingList,
            autoCollectingButton,
            () =>
            {
                //Debug.Log($"�ڵ� �����ֱ� �ð� ���� ���� ��ġ : {ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv].value}");
            });
    }

    // �ڵ� �ռ� �ð� ���� �Լ�
    private void AutoMerge()
    {
        ProcessUpgrade(
            7,
            ref autoMergeLv,
            ItemFunctionManager.Instance.autoMergeList,
            autoMergeButton,
            () =>
            {
                //Debug.Log($"�ڵ� �ռ� �ð� ���� ���� ��ġ : {ItemFunctionManager.Instance.autoMergeList[autoMergeLv].value}");
            });
    }

    #endregion


    #region Food Upgrade Helper Functions

    // ���� ���׷��̵� 1�� ���� ���밪 ��ȯ (DB�� - 1)
    public int GetActualMaxFoodLevel()
    {
        if (foodUpgradeLv >= ItemFunctionManager.Instance.foodUpgradeList.Count)
        {
            return (int)ItemFunctionManager.Instance.foodUpgradeList[ItemFunctionManager.Instance.foodUpgradeList.Count - 1].value - 1;
        }

        return (int)ItemFunctionManager.Instance.foodUpgradeList[foodUpgradeLv].value - 1;
    }

    // ���� ���׷��̵� 2�� ���� ���밪 ��ȯ (DB�� - 1)
    public int GetActualMinFoodLevel()
    {
        if (foodUpgrade2Lv >= ItemFunctionManager.Instance.foodUpgrade2List.Count)
        {
            return (int)ItemFunctionManager.Instance.foodUpgrade2List[ItemFunctionManager.Instance.foodUpgrade2List.Count - 1].value - 1;
        }

        return (int)ItemFunctionManager.Instance.foodUpgrade2List[foodUpgrade2Lv].value - 1;
    }

    // ���� ���׷��̵� 1�� ���� ���� ���� ���밪 ��ȯ
    private int GetNextActualMaxFoodLevel()
    {
        if (foodUpgradeLv + 1 >= ItemFunctionManager.Instance.foodUpgradeList.Count)
        {
            return GetActualMaxFoodLevel();
        }

        return (int)ItemFunctionManager.Instance.foodUpgradeList[foodUpgradeLv + 1].value - 1;
    }

    // ���� ���׷��̵� 2�� ���� ���� ���� ���밪 ��ȯ
    private int GetNextActualMinFoodLevel()
    {
        if (foodUpgrade2Lv + 1 >= ItemFunctionManager.Instance.foodUpgrade2List.Count)
        {
            return GetActualMinFoodLevel();
        }

        return (int)ItemFunctionManager.Instance.foodUpgrade2List[foodUpgrade2Lv + 1].value - 1;
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
        public int foodUpgradeLv;                   // ���� ���׷��̵�1 ����
        public int foodUpgrade2Lv;                  // ���� ���׷��̵�2 ����
        public int autoCollectingLv;                // �ڵ� �����ֱ� ����
        public int autoMergeLv;                     // �ڵ� �ռ� ����

        public int minFoodLv;                       // ���� �ּ� ����
        public int maxFoodLv;                       // ���� �ִ� ����
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            maxCatsLv = this.maxCatsLv,
            reduceCollectingTimeLv = this.reduceCollectingTimeLv,
            maxFoodsLv = this.maxFoodsLv,
            reduceProducingFoodTimeLv = this.reduceProducingFoodTimeLv,
            foodUpgradeLv = this.foodUpgradeLv,
            foodUpgrade2Lv = this.foodUpgrade2Lv,
            autoCollectingLv = this.autoCollectingLv,
            autoMergeLv = this.autoMergeLv,
            minFoodLv = this.minFoodLv,
            maxFoodLv = this.maxFoodLv
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
        this.foodUpgradeLv = savedData.foodUpgradeLv;
        this.foodUpgrade2Lv = savedData.foodUpgrade2Lv;
        this.autoCollectingLv = savedData.autoCollectingLv;
        this.autoMergeLv = savedData.autoMergeLv;
        this.minFoodLv = GetActualMinFoodLevel();
        this.maxFoodLv = GetActualMaxFoodLevel();

        UpdateAllUI();

        isDataLoaded = true;
    }

    #endregion


}
