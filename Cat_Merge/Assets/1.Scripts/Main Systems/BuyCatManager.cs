using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyCatManager : MonoBehaviour
{
    // Singleton Instance
    public static BuyCatManager Instance { get; private set; }

    [Header("---[BuyCat]")]
    [SerializeField] private Button bottomBuyCatButton;                                 // ����� ���� ��ư
    [SerializeField] private Image bottomBuyCatButtonImg;                               // ����� ���� ��ư �̹���
    [SerializeField] private GameObject[] buyCatCoinDisabledBg;                         // ��ư Ŭ�� ���� ���� ���
    [SerializeField] private GameObject[] buyCatCashDisabledBg;                         // ��ư Ŭ�� ���� ���� ���
    [SerializeField] private GameObject buyCatMenuPanel;                                // ������ �޴� �ǳ�
    private bool isOnToggleBuyCat;                                                      // ������ �޴� �ǳ� ���
    [SerializeField] private Button buyCatBackButton;                                   // ����� ���� �޴� �ڷΰ��� ��ư

    [SerializeField] private Button[] buyCatCoinButtons;                                // ����� ��ȭ�� ���� ��ư
    [SerializeField] private Button[] buyCatCashButtons;                                // ũ����Ż ��ȭ�� ���� ��ư

    [SerializeField] private TextMeshProUGUI[] buyCatCountExplainTexts;                 // ����� ���� Ƚ�� ����â
    [SerializeField] private TextMeshProUGUI[] buyCatCoinFeeTexts;                      // ����� ���� ��� (���)
    [SerializeField] private TextMeshProUGUI[] buyCatCashFeeTexts;                      // ����� ���� ��� (ũ����Ż)

    private ActivePanelManager activePanelManager;                                      // ActivePanelManager

    private int[] buyCatCoinCounts;                                                     // ����� ���� Ƚ��(����)
    private int[] buyCatCashCounts;                                                     // ����� ���� Ƚ��(ũ����Ż)
    private int[] buyCatCoinFee;                                                        // ����� ���� ��� (����)
    private int[] buyCatCashFee;                                                        // ����� ���� ��� (ũ����Ż)

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

        buyCatCoinCounts = new int[GameManager.Instance.AllCatData.Length];   // ����� ���� Ƚ��(����)
        buyCatCashCounts = new int[GameManager.Instance.AllCatData.Length];   // ����� ���� Ƚ��(ũ����Ż)
        buyCatCoinFee = new int[GameManager.Instance.AllCatData.Length];      // ����� ���� ��� (����)
        buyCatCashFee = new int[GameManager.Instance.AllCatData.Length];      // ����� ���� ��� (ũ����Ż)

        // ����� ���� �޴� ����
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            buyCatCoinCounts[i] = 0;
            buyCatCashCounts[i] = 0;
            buyCatCoinFee[i] = i * 5 + 10;
            buyCatCashFee[i] = 5;

            buyCatCountExplainTexts[i].text = $"BuyCount :{buyCatCoinCounts[i]}cnt + {buyCatCashCounts[i]}cnt";
            buyCatCoinFeeTexts[i].text = $"{buyCatCoinFee[i]}";
            buyCatCashFeeTexts[i].text = $"{buyCatCashFee[i]}";
        }

        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {

            if (i < buyCatCoinButtons.Length && i < buyCatCashButtons.Length) // �굵 ����
            {
                int index = i; // �갡 �־�� �� �ٵ� ������ ��
                buyCatCoinButtons[index].onClick.AddListener(() => BuyCatCoin(buyCatCoinButtons[index]));
                buyCatCashButtons[index].onClick.AddListener(() => BuyCatCash(buyCatCashButtons[index]));
            }
            else
            {
                Debug.LogWarning($"Index {i} is out of bounds for buyCatCoinButtons array.");
            }
        }
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BuyCatMenu", buyCatMenuPanel, bottomBuyCatButtonImg);
    }

    // ����� ���� �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomBuyCatMenuPanel()
    {
        bottomBuyCatButton.onClick.AddListener(() => activePanelManager.TogglePanel("BuyCatMenu"));
        buyCatBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BuyCatMenu"));
    }

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCoin(Button b)
    {
        if (GameManager.Instance.CanSpawnCat())
        {
            if (b == buyCatCoinButtons[0])
            {
                Debug.Log("ù��° ����� ���� ��ư Ŭ��(��������)");

                GameManager.Instance.Coin -= buyCatCoinFee[0];
                buyCatCoinCounts[0]++;
                buyCatCoinFee[0] *= 2;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(0);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(0);

                buyCatCountExplainTexts[0].text = $"BuyCount :{buyCatCoinCounts[0]}cnt + {buyCatCashCounts[0]}cnt";
                buyCatCoinFeeTexts[0].text = $"{buyCatCoinFee[0]}";
            }
            else if (b == buyCatCoinButtons[1])
            {
                Debug.Log("�ι��� ����� ���� ��ư Ŭ��(��������)");

                GameManager.Instance.Coin -= buyCatCoinFee[1];
                buyCatCoinCounts[1]++;
                buyCatCoinFee[1] *= 2;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(1);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(1);

                buyCatCountExplainTexts[1].text = $"BuyCount :{buyCatCoinCounts[1]}cnt + {buyCatCashCounts[1]}cnt";
                buyCatCoinFeeTexts[1].text = $"{buyCatCoinFee[1]}";
            }
            else if (b == buyCatCoinButtons[2])
            {
                Debug.Log("����° ����� ���� ��ư Ŭ��(��������)");

                GameManager.Instance.Coin -= buyCatCoinFee[2];
                buyCatCoinCounts[2]++;
                buyCatCoinFee[2] *= 2;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(2);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(2);

                buyCatCountExplainTexts[2].text = $"BuyCount :{buyCatCoinCounts[2]}cnt + {buyCatCashCounts[2]}cnt";
                buyCatCoinFeeTexts[2].text = $"{buyCatCoinFee[2]}";
            }
        }
    }

    // ����� ���� ��ư �Լ�(ũ����Ż�� ����)
    private void BuyCatCash(Button b)
    {
        // ���� ����� ���� ����� �ִ� �� �̸��� ��
        if (GameManager.Instance.CanSpawnCat())
        {
            if (b == buyCatCashButtons[0])
            {
                Debug.Log("ù��° ����� ���� ��ư Ŭ��(ĳ����)");

                GameManager.Instance.Cash -= buyCatCashFee[0];
                buyCatCashCounts[0]++;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(0);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(0);

                buyCatCountExplainTexts[0].text = $"BuyCount :{buyCatCoinCounts[0]}cnt + {buyCatCashCounts[0]}cnt";
                buyCatCashFeeTexts[0].text = $"{buyCatCashFee[0]}";
            }
            else if (b == buyCatCashButtons[1])
            {
                Debug.Log("�ι�° ����� ���� ��ư Ŭ��(ĳ����)");

                GameManager.Instance.Cash -= buyCatCashFee[1];
                buyCatCashCounts[1]++;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(1);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(1);

                buyCatCountExplainTexts[1].text = $"BuyCount :{buyCatCoinCounts[1]}cnt + {buyCatCashCounts[1]}cnt";
                buyCatCashFeeTexts[1].text = $"{buyCatCashFee[1]}";
            }
            else if (b == buyCatCashButtons[2])
            {
                Debug.Log("����° ����� ���� ��ư Ŭ��(ĳ����)");

                GameManager.Instance.Cash -= buyCatCashFee[2];
                buyCatCashCounts[2]++;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(2);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(2);

                buyCatCountExplainTexts[2].text = $"BuyCount :{buyCatCoinCounts[2]}cnt + {buyCatCashCounts[2]}cnt";
                buyCatCashFeeTexts[2].text = $"{buyCatCashFee[2]}";
            }
        }
    }

    // ����� ���� ���� UI ������Ʈ �Լ�
    private void UpdateBuyCatUI()
    {
        if (GameManager.Instance.CanSpawnCat())
        {
            for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
            {
                // ���� ���¿� ���� UI ������Ʈ
                bool canAffordWithCoin = GameManager.Instance.Coin >= buyCatCoinFee[i];
                buyCatCoinButtons[i].interactable = canAffordWithCoin;
                buyCatCoinDisabledBg[i].SetActive(!canAffordWithCoin);

                // ũ����Ż ���¿� ���� UI ������Ʈ
                bool canAffordWithCash = GameManager.Instance.Cash >= buyCatCashFee[i];
                buyCatCoinButtons[i].interactable &= canAffordWithCash;
                buyCatCashDisabledBg[i].SetActive(!canAffordWithCash);
            }
        }
    }

    private void InitializeItemMenuManager()
    {
        OpenCloseBottomBuyCatMenuPanel();
    }

    private void Update()
    {
        UpdateBuyCatUI();
    }
}

