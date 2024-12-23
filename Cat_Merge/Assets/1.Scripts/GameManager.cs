using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// GameManager Script
[DefaultExecutionOrder(-1)]     // ��ũ��Ʈ ���� ���� ����
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }        // SingleTon

    // ======================================================================================================================

    // Data
    private Cat[] allCatData;                                       // ��� ����� ������ ����
    public Cat[] AllCatData => allCatData;

    // ======================================================================================================================

    // Main UI Text
    [Header("---[Main UI Text]")]
    [SerializeField] private TextMeshProUGUI catCountText;          // ����� �� �ؽ�Ʈ
    private int currentCatCount = 0;                                // ȭ�� �� ����� ��
    private int maxCats = 8;                                        // �ִ� ����� ��

    // �⺻ ��ȭ
    [SerializeField] private TextMeshProUGUI coinText;              // �⺻��ȭ �ؽ�Ʈ
    private int coin = 1000;                                        // �⺻��ȭ
    public int Coin
    {
        get => coin;
        set
        {
            coin = value;
            UpdateCoinText();
        }
    }

    // ĳ�� ��ȭ
    [SerializeField] private TextMeshProUGUI cashText;              // ĳ����ȭ �ؽ�Ʈ
    private int cash = 1000;                                        // ĳ����ȭ
    public int Cash 
    { 
        get => cash;
        set
        {
            cash = value;
            UpdateCashText();
        }
    }

    // ======================================================================================================================

    // ItemMenu Select Button
    [Header("ItemMenu")]
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
    [Header("ItemMenues")]
    [SerializeField] private Button[] itemMenuButtons;                  // itemPanel������ ������ �޴� ��ư��
    [SerializeField] private GameObject[] itemMenues;                   // itemPanel������ �޴� ��ũ��â �ǳ�
    private bool[] isItemMenuButtonsOn = new bool[4];                   // ��ư ���� �����ϱ� ���� boolŸ�� �迭


    // ������ �޴� �ȿ� ������ ���
    [Header("ItemMenuList")]
    [SerializeField] private Button catMaximumIncreaseButton;           // ����� �ִ�ġ ���� ��ư
    [SerializeField] private GameObject disabledBg;                     // ��ư Ŭ�� ���� ���� ���
    private int catMaximumIncreaseFee = 10;                             // ����� �ִ�ġ ���� ���
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseText;    // ����� �ִ�ġ ���� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseLvText;  // ����� �ִ�ġ ���� ���� �ؽ�Ʈ
    private int catMaximumIncreaseLv = 1;                               // ����� �ִ�ġ ���� ����

    // ======================================================================================================================

    [Header("BuyCat")]
    [SerializeField] private Button bottomBuyCatButton;                 // ����� ���� ��ư
    [SerializeField] private Image bottomBuyCatButtonImg;               // ����� ���� ��ư �̹���
    [SerializeField] private GameObject buyCatCoinDisabledBg;           // ��ư Ŭ�� ���� ���� ���
    [SerializeField] private GameObject buyCatCashDisabledBg;           // ��ư Ŭ�� ���� ���� ���
    [SerializeField] private GameObject buyCatMenuPanel;                // ������ �޴� �ǳ�
    private bool isOnToggleBuyCat;                                      // ������ �޴� �ǳ� ���
    [SerializeField] private Button buyCatBackButton;                   // ����� ���� �޴� �ڷΰ��� ��ư

    [SerializeField] private Button buyCatCoinButton;                   // ����� ��ȭ�� ���� ��ư
    [SerializeField] private Button buyCatCashButton;                   // ũ����Ż ��ȭ�� ���� ��ư

    [SerializeField] private TextMeshProUGUI buyCatCountExplainText;    // ����� ���� Ƚ�� ����â
    [SerializeField] private TextMeshProUGUI buyCatCoinFeeText;         // ����� ���� ��� (���)
    [SerializeField] private TextMeshProUGUI buyCatCashFeeText;         // ����� ���� ��� (ũ����Ż)
    private int buyCatCoinCount = 0;                                    // ����� ���� Ƚ��(����)
    private int buyCatCashCount = 0;                                    // ����� ���� Ƚ��(ũ����Ż)
    private int buyCatCoinFee = 5;                                      // ����� ���� ��� (����)
    private int buyCatCashFee = 5;                                      // ����� ���� ��� (ũ����Ż)

    // ======================================================================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // �⺻ ����
        LoadAllCats();
        UpdateCatCountText();
        UpdateCoinText();
        UpdateCashText();



        // ������ �޴� ����
        bottomItemButton.onClick.AddListener(OpenCloseBottomItemMenuPanel);
        catMaximumIncreaseButton.onClick.AddListener(IncreaseCatMaximum);
        itemBackButton.onClick.AddListener(CloseBottomItemMenuPanel);

        itemMenues[(int)EitemMenues.itemMenues].SetActive(true);
        if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
        {
            itemMenuButtons[(int)EitemMenuButton.itemMenuButton].GetComponent<Image>().color = parsedColor;
        }

        itemMenuButtons[(int)EitemMenuButton.itemMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.itemMenues]));
        itemMenuButtons[(int)EitemMenuButton.colleagueMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.colleagueMenues]));
        itemMenuButtons[(int)EitemMenuButton.backgroundMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.backgroundMenues]));
        itemMenuButtons[(int)EitemMenuButton.goldenSomethingMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.goldenMenues]));

        for(int i = 1; i < 4; i++)
        {
            isItemMenuButtonsOn[i] = false;
        }
        UpdateIncreaseMaximumFeeText();
        UpdateIncreaseMaximumLvText();

        // ����� ���� �޴� ����
        bottomBuyCatButton.onClick.AddListener(OpenCloseBottomBuyCatMenuPanel);
        buyCatBackButton.onClick.AddListener(CloseBottomBuyCatMenuPanel);

        buyCatCoinButton.onClick.AddListener(BuyCatCoin);
        buyCatCashButton.onClick.AddListener(BuyCatCash);
    }

    // ======================================================================================================================

    // ����� ���� Load �Լ�
    private void LoadAllCats()
    {
        // CatDataLoader���� catDictionary ��������
        CatDataLoader catDataLoader = FindObjectOfType<CatDataLoader>();
        if (catDataLoader == null || catDataLoader.catDictionary == null)
        {
            Debug.LogError("CatDataLoader�� ���ų� ����� �����Ͱ� �ε���� �ʾҽ��ϴ�.");
            return;
        }

        // Dictionary�� ��� ���� �迭�� ��ȯ
        allCatData = new Cat[catDataLoader.catDictionary.Count];
        catDataLoader.catDictionary.Values.CopyTo(allCatData, 0);

        Debug.Log($"����� ������ {allCatData.Length}���� �ε�Ǿ����ϴ�.");
    }

    // ======================================================================================================================

    // ����� �� �Ǻ� �Լ�
    public bool CanSpawnCat()
    {
        return currentCatCount < maxCats;
    }

    // ���� ����� �� ������Ű�� �Լ�
    public void AddCatCount()
    {
        if (currentCatCount < maxCats)
        {
            currentCatCount++;
            UpdateCatCountText();
        }
    }

    // ���� ����� �� ���ҽ�Ű�� �Լ�
    public void DeleteCatCount()
    {
        if (currentCatCount > 0)
        {
            currentCatCount--;
            UpdateCatCountText();
        }
    }

    // ����� �� �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }

    // �⺻��ȭ �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    public void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"{coin}";
        }
    }

    // ĳ����ȭ �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    public void UpdateCashText()
    {
        if (cashText != null)
        {
            // ���ڸ� 3�ڸ����� �޸��� �߰��Ͽ� ǥ��
            cashText.text = cash.ToString("N0");
        }
    }

    // ======================================================================================================================

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomItemMenuPanel()
    {
        Debug.Log("Item Toggle");
        if (itemMenuPanel != null)
        {
            isOnToggleItemMenu = !isOnToggleItemMenu;
            if (isOnToggleItemMenu)
            {
                Debug.Log("Item Toggle On");
                itemMenuPanel.SetActive(true);
                //if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
                //{
                //    bottomItemButtonImg.color = parsedColor;
                //}
                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
            else
            {
                Debug.Log("Item Toggle Off");
                itemMenuPanel.SetActive(false);
                //if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColor))
                //{
                //    bottomItemButtonImg.color = parsedColor;
                //}
                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
        }
    }

    // ������ �޴� �ǳ� �ݴ� �Լ�(�ڷΰ��� ��ư)
    private void CloseBottomItemMenuPanel()
    {
        if(itemMenuPanel != null)
        {
            isOnToggleItemMenu = false;
            itemMenuPanel.SetActive(false);
            //if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColor))
            //{
            //    bottomItemButtonImg.color = parsedColor;
            //}
            ChangeSelectedButtonColor(isOnToggleItemMenu);
        }
    }

    // ���� �޴� ��ư ���� ����
    private void ChangeSelectedButtonColor(bool isOnToggle)
    {
        if(isOnToggle)
        {
            if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
            {
                if(itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if(buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }
                
            }
        }
        else
        {
            if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColor))
            {
                if(!itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if(!buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }

            }
        }
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
                        // ���󺯰�
                        itemMenuButtons[i].GetComponent<Image>().color = parsedColorT;
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColorF))
                        {
                             // ���󺯰�
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
        Debug.Log("����� �ִ�ġ ���� ��ư Ŭ��");
        if (catMaximumIncreaseButton != null)
        {
            maxCats++;
            catMaximumIncreaseFee *= 2;
            catMaximumIncreaseLv++;
            UpdateCatCountText();
            UpdateIncreaseMaximumFeeText();
            UpdateIncreaseMaximumLvText();
        }
    }

    // ����� �� �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateIncreaseMaximumFeeText()
    {
        if (catMaximumIncreaseText != null)
        {
            if (coin < catMaximumIncreaseFee)
            {
                catMaximumIncreaseButton.interactable = false;
                disabledBg.SetActive(true);
            }
            else
            {
                catMaximumIncreaseButton.interactable = true;
                disabledBg.SetActive(false);
            }
            catMaximumIncreaseText.text = $"{catMaximumIncreaseFee}";
            UpdateCoinText();
            coin -= catMaximumIncreaseFee;

        }
    }

    // ����� �ִ�ġ ���� ���� UI ������Ʈ �ϴ� �Լ�
    private void UpdateIncreaseMaximumLvText()
    {
        catMaximumIncreaseLvText.text = $"Lv.{catMaximumIncreaseLv}";

    }

    // ��ư Ŭ�� �̺�Ʈ
    void OnButtonClicked(GameObject activePanel)
    {
        // ��� UI �г� ��Ȱ��ȭ
        for(int i = 0; i < (int)EitemMenues.end; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        // Ŭ���� ��ư�� UI�� Ȱ��ȭ
        activePanel.SetActive(true);
       
        for(int i = 0; i < (int)EitemMenues.end; i++)
        {
            if(itemMenues[i].activeSelf)
            {
                isItemMenuButtonsOn[i] = true;
            }
        }
        ChangeSelectedButtonColor(activePanel);
    }

    // ======================================================================================================================

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomBuyCatMenuPanel()
    {
        Debug.Log("Buy Cat Toggle");
        if (buyCatMenuPanel != null)
        {
            isOnToggleBuyCat = !isOnToggleBuyCat;
            if (isOnToggleBuyCat)
            {
                Debug.Log("Buy Cat Toggle On");
                buyCatMenuPanel.SetActive(true);
                ChangeSelectedButtonColor(isOnToggleBuyCat);
            }
            else
            {
                Debug.Log("Buy Cat Toggle Off");
                buyCatMenuPanel.SetActive(false);
                ChangeSelectedButtonColor(isOnToggleBuyCat);
            }
        }
    }

    // ������ �޴� �ǳ� �ݴ� �Լ�(�ڷΰ��� ��ư)
    private void CloseBottomBuyCatMenuPanel()
    {
        if (buyCatMenuPanel != null)
        {
            isOnToggleBuyCat = false;
            buyCatMenuPanel.SetActive(false);
            ChangeSelectedButtonColor(isOnToggleBuyCat);
        }
    }

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCoin()
    {
        if (CanSpawnCat())
        {
            Debug.Log("����� ���� ��ư Ŭ��(��������)");
            coin -= buyCatCoinFee;
            buyCatCoinCount++;
            buyCatCoinFee *= 2;

            QuestManager.Instance.AddPurchaseCatsCount();

            CatSpawn catSpawn = GetComponent<CatSpawn>();
            catSpawn.OnClickedSpawn();

            UpdateCoinText();
            UpdateBuyCatUI();
        }
    }

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCash()
    {
        // ���� ����� ���� ����� �ִ� �� �̸��� ��
        if (CanSpawnCat()) 
        {
            Debug.Log("����� ���� ��ư Ŭ��(ĳ����)");
            cash -= buyCatCashFee;
            buyCatCashCount++;

            QuestManager.Instance.AddPurchaseCatsCount();

            CatSpawn catSpawn = GetComponent<CatSpawn>();
            catSpawn.OnClickedSpawn();

            UpdateCashText();
            UpdateBuyCatUI();
        }
    }

    // ����� ���� ���� UI ������Ʈ �Լ�
    private void UpdateBuyCatUI()
    {
        buyCatCountExplainText.text = $"BuyCount :{buyCatCoinCount}cnt + {buyCatCashCount}cnt";
        buyCatCoinFeeText.text = $"{buyCatCoinFee}";
        buyCatCashFeeText.text = $"{buyCatCashFee}";
        if (CanSpawnCat())
        {
            if (coin < buyCatCoinFee)
            {
                buyCatCoinButton.interactable = false;
                buyCatCoinDisabledBg.SetActive(true);
            }
            else
            {
                buyCatCoinButton.interactable = true;
                buyCatCoinDisabledBg.SetActive(false);
            }

            if (cash < buyCatCashFee)
            {
                buyCatCashButton.interactable = false;
                buyCatCashDisabledBg.SetActive(true);
            }
            else
            {
                buyCatCashButton.interactable = true;
                buyCatCashDisabledBg.SetActive(false);
            }      
        }     
    }

    // ======================================================================================================================
}
