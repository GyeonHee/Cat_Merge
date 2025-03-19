using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// ����� ���� ��ũ��Ʈ
[DefaultExecutionOrder(-2)]
public class BuyCatManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static BuyCatManager Instance { get; private set; }

    [Header("---[BuyCat]")]
    [SerializeField] private Button bottomBuyCatButton;                                 // ����� ���� ��ư
    [SerializeField] private Image bottomBuyCatButtonImg;                               // ����� ���� ��ư �̹���
    [SerializeField] private GameObject[] buyCatCoinDisabledBg;                         // ��ư Ŭ�� ���� ���� ��� (Coin)
    [SerializeField] private GameObject[] buyCatCashDisabledBg;                         // ��ư Ŭ�� ���� ���� ��� (Cash)
    [SerializeField] private GameObject buyCatMenuPanel;                                // ������ �޴� �ǳ�
    [SerializeField] private Button buyCatBackButton;                                   // ����� ���� �޴� �ڷΰ��� ��ư

    [SerializeField] private Button[] buyCatCoinButtons;                                // ����� ��ȭ�� ���� ��ư
    [SerializeField] private Button[] buyCatCashButtons;                                // ũ����Ż ��ȭ�� ���� ��ư

    [SerializeField] private TextMeshProUGUI[] buyCatCountExplainTexts;                 // ����� ���� Ƚ�� ����â
    [SerializeField] private TextMeshProUGUI[] buyCatCoinFeeTexts;                      // ����� ���� ��� (���)
    [SerializeField] private TextMeshProUGUI[] buyCatCashFeeTexts;                      // ����� ���� ��� (ũ����Ż)

    private ActivePanelManager activePanelManager;                                      // ActivePanelManager

    // ���� ����� ��޿� ���� ���� ���� Ŭ����
    [Serializable]
    public class CatPurchaseInfo
    {
        public int catGrade;
        public int coinPurchaseCount;
        public int cashPurchaseCount;
        public int coinPrice;
        public int cashPrice;
    }

    // ��� ����� ���� ������ �����ϴ� ��ųʸ�
    private Dictionary<int, CatPurchaseInfo> catPurchaseInfos = new Dictionary<int, CatPurchaseInfo>();

    private bool isDataLoaded = false;

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
            InitializeCatPurchaseInfos();
        }

        InitializeBuyCatManager();
        UpdateAllUI();
    }

    #endregion



    #region Initialize

    private void InitializeBuyCatManager()
    {
        buyCatMenuPanel.SetActive(false);

        InitializeActivePanel();
        InitializeButtonListeners();
    }

    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BuyCatMenu", buyCatMenuPanel, bottomBuyCatButtonImg);
    }

    private void InitializeCatPurchaseInfos()
    {
        if (GameManager.Instance == null) return;

        catPurchaseInfos.Clear();
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            catPurchaseInfos[i] = new CatPurchaseInfo
            {
                catGrade = i,
                coinPurchaseCount = 0,
                cashPurchaseCount = 0,
                coinPrice = 5,
                cashPrice = 5
            };
        }

        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        // �ʱ�ȭ �߿��� �������� �ʵ��� ����
        bool shouldSave = isDataLoaded;
        isDataLoaded = false;

        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            CatPurchaseInfo info = GetCatPurchaseInfo(i);
            buyCatCountExplainTexts[i].text = $"���� Ƚ�� : {info.coinPurchaseCount}ȸ + {info.cashPurchaseCount}ȸ";
            buyCatCoinFeeTexts[i].text = $"{info.coinPrice:N0}";
            buyCatCashFeeTexts[i].text = $"{info.cashPrice:N0}";
        }

        isDataLoaded = shouldSave;
    }

    private void InitializeButtonListeners()
    {
        bottomBuyCatButton.onClick.AddListener(() => activePanelManager.TogglePanel("BuyCatMenu"));
        buyCatBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BuyCatMenu"));

        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            int index = i;

            buyCatCoinButtons[index].onClick.RemoveAllListeners();
            buyCatCoinButtons[index].onClick.AddListener(() => BuyCatCoin(buyCatCoinButtons[index]));

            buyCatCashButtons[index].onClick.RemoveAllListeners();
            buyCatCashButtons[index].onClick.AddListener(() => BuyCatCash(buyCatCashButtons[index]));
        }
    }

    #endregion



    #region Button System

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCoin(Button button)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("����� �������� �ִ��Դϴ�!!");
            return;
        }

        int catIndex = GetButtonIndex(button, buyCatCoinButtons);
        if (catIndex == -1) return;

        CatPurchaseInfo info = GetCatPurchaseInfo(catIndex);
        if (GameManager.Instance.Coin < info.coinPrice)
        {
            NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!!");
            return;
        }

        GameManager.Instance.Coin -= info.coinPrice;
        info.coinPurchaseCount++;
        info.coinPrice *= 2;

        QuestManager.Instance.AddPurchaseCatsCount();
        DictionaryManager.Instance.UnlockCat(catIndex);

        SpawnManager catSpawn = GetComponent<SpawnManager>();
        catSpawn.SpawnGradeCat(catIndex);

        // UI ������Ʈ
        UpdateAllUI();

        GoogleSave();
    }

    // ����� ���� ��ư �Լ�(ũ����Ż�� ����)
    private void BuyCatCash(Button button)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("����� �������� �ִ��Դϴ�!!");
            return;
        }

        int catIndex = GetButtonIndex(button, buyCatCashButtons);
        if (catIndex == -1) return;

        CatPurchaseInfo info = GetCatPurchaseInfo(catIndex);
        if (GameManager.Instance.Cash < info.cashPrice)
        {
            NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!!");
            return;
        }

        GameManager.Instance.Cash -= info.cashPrice;
        info.cashPurchaseCount++;
        info.cashPrice *= 2;

        QuestManager.Instance.AddPurchaseCatsCount();
        DictionaryManager.Instance.UnlockCat(catIndex);

        SpawnManager catSpawn = GetComponent<SpawnManager>();
        catSpawn.SpawnGradeCat(catIndex);

        // UI ������Ʈ
        UpdateAllUI();

        GoogleSave();
    }

    // ��ư �迭���� �ε��� ã��
    private int GetButtonIndex(Button button, Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (button == buttons[i])
            {
                return i;
            }
        }
        return -1;
    }

    // ����� ���� ���� ��������
    private CatPurchaseInfo GetCatPurchaseInfo(int catGrade)
    {
        // ���� ������ ����
        //if (!catPurchaseInfos.ContainsKey(catGrade))
        //{
        //    catPurchaseInfos[catGrade] = new CatPurchaseInfo
        //    {
        //        catGrade = catGrade,
        //        coinPurchaseCount = 0,
        //        cashPurchaseCount = 0,
        //        coinPrice = 5,
        //        cashPrice = 5
        //    };
        //}
        return catPurchaseInfos[catGrade];
    }

    #endregion



    #region Battle System

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleBuyCatState()
    {
        bottomBuyCatButton.interactable = false;

        if (buyCatMenuPanel.activeSelf == true)
        {
            activePanelManager.TogglePanel("BuyCatMenu");
        }
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleBuyCatState()
    {
        bottomBuyCatButton.interactable = true;
    }

    #endregion



    #region Save System

    [Serializable]
    private class SaveData
    {
        public List<CatPurchaseInfo> purchaseInfos = new List<CatPurchaseInfo>();
    }

    public string GetSaveData()
    {
        SaveData saveData = new SaveData();
        foreach (var infos in catPurchaseInfos)
        {
            saveData.purchaseInfos.Add(new CatPurchaseInfo
            {
                catGrade = infos.Value.catGrade,
                coinPurchaseCount = infos.Value.coinPurchaseCount,
                cashPurchaseCount = infos.Value.cashPurchaseCount,
                coinPrice = infos.Value.coinPrice,
                cashPrice = infos.Value.cashPrice
            });
        }
        return JsonUtility.ToJson(saveData);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        
        catPurchaseInfos.Clear();
        foreach (var info in savedData.purchaseInfos)
        {
            if (info.catGrade < GameManager.Instance.AllCatData.Length)
            {
                catPurchaseInfos[info.catGrade] = new CatPurchaseInfo
                {
                    catGrade = info.catGrade,
                    coinPurchaseCount = info.coinPurchaseCount,
                    cashPurchaseCount = info.cashPurchaseCount,
                    coinPrice = info.coinPrice,
                    cashPrice = info.cashPrice
                };
            }
        }

        UpdateAllUI();

        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            Debug.Log("���� ����");
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}
