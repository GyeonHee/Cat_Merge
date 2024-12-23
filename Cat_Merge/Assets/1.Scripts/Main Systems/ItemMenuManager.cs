using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemMenuManager : MonoBehaviour
{
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
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseText;    // ����� �ִ�ġ ���� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseLvText;  // ����� �ִ�ġ ���� ���� �ؽ�Ʈ
    private int catMaximumIncreaseLv = 1;                               // ����� �ִ�ġ ���� ����

    // ======================================================================================================================

    [Header("---[BuyCat]")]
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

        bottomItemButton.onClick.AddListener(OpenCloseBottomItemMenuPanel);
        catMaximumIncreaseButton.onClick.AddListener(IncreaseCatMaximum);
        itemBackButton.onClick.AddListener(CloseBottomItemMenuPanel);




        // ����� ���� �޴� ����
        bottomBuyCatButton.onClick.AddListener(OpenCloseBottomBuyCatMenuPanel);
        buyCatBackButton.onClick.AddListener(CloseBottomBuyCatMenuPanel);

        buyCatCoinButton.onClick.AddListener(BuyCatCoin);
        buyCatCashButton.onClick.AddListener(BuyCatCash);
    }

    // ======================================================================================================================

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomItemMenuPanel()
    {
        //Debug.Log("Item Toggle");
        if (itemMenuPanel != null)
        {
            isOnToggleItemMenu = !isOnToggleItemMenu;
            if (isOnToggleItemMenu)
            {
                //Debug.Log("Item Toggle On");
                itemMenuPanel.SetActive(true);

                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
            else
            {
                //Debug.Log("Item Toggle Off");
                itemMenuPanel.SetActive(false);

                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
        }
    }

    // ������ �޴� �ǳ� �ݴ� �Լ�(�ڷΰ��� ��ư)
    private void CloseBottomItemMenuPanel()
    {
        if (itemMenuPanel != null)
        {
            isOnToggleItemMenu = false;
            itemMenuPanel.SetActive(false);

            ChangeSelectedButtonColor(isOnToggleItemMenu);
        }
    }

    // ���� �޴� ��ư ���� ����
    private void ChangeSelectedButtonColor(bool isOnToggle)
    {
        if (isOnToggle)
        {
            if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
            {
                if (itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if (buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }
            }
        }
        else
        {
            if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColor))
            {
                if (!itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if (!buyCatMenuPanel.activeSelf)
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
            GameManager.Instance.MaxCats++;
            catMaximumIncreaseFee *= 2;
            catMaximumIncreaseLv++;
            UpdateIncreaseMaximumFeeText();
            UpdateIncreaseMaximumLvText();
        }
    }

    // ����� �� �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateIncreaseMaximumFeeText()
    {
        if (catMaximumIncreaseText != null)
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
            catMaximumIncreaseText.text = $"{catMaximumIncreaseFee}";
            GameManager.Instance.Coin -= catMaximumIncreaseFee;

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

    // ======================================================================================================================

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomBuyCatMenuPanel()
    {
        //Debug.Log("Buy Cat Toggle");
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
        if (GameManager.Instance.CanSpawnCat())
        {
            Debug.Log("����� ���� ��ư Ŭ��(��������)");
            GameManager.Instance.Coin -= buyCatCoinFee;
            buyCatCoinCount++;
            buyCatCoinFee *= 2;

            QuestManager.Instance.AddPurchaseCatsCount();

            SpawnManager catSpawn = GetComponent<SpawnManager>();
            catSpawn.OnClickedSpawn();

            UpdateBuyCatUI();
        }
    }

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCash()
    {
        // ���� ����� ���� ����� �ִ� �� �̸��� ��
        if (GameManager.Instance.CanSpawnCat())
        {
            Debug.Log("����� ���� ��ư Ŭ��(ĳ����)");
            GameManager.Instance.Cash -= buyCatCashFee;
            buyCatCashCount++;

            QuestManager.Instance.AddPurchaseCatsCount();

            SpawnManager catSpawn = GetComponent<SpawnManager>();
            catSpawn.OnClickedSpawn();

            UpdateBuyCatUI();
        }
    }

    // ����� ���� ���� UI ������Ʈ �Լ�
    private void UpdateBuyCatUI()
    {
        buyCatCountExplainText.text = $"BuyCount :{buyCatCoinCount}cnt + {buyCatCashCount}cnt";
        buyCatCoinFeeText.text = $"{buyCatCoinFee}";
        buyCatCashFeeText.text = $"{buyCatCashFee}";
        if (GameManager.Instance.CanSpawnCat())
        {
            if (GameManager.Instance.Coin < buyCatCoinFee)
            {
                buyCatCoinButton.interactable = false;
                buyCatCoinDisabledBg.SetActive(true);
            }
            else
            {
                buyCatCoinButton.interactable = true;
                buyCatCoinDisabledBg.SetActive(false);
            }

            if (GameManager.Instance.Cash < buyCatCashFee)
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


}
