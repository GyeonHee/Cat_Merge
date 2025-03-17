using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BuyCatManager : MonoBehaviour, ISaveable
{
    // Singleton Instance
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

    private int[] buyCatCoinCounts;                                                     // ����� ���� Ƚ�� (����)
    private int[] buyCatCashCounts;                                                     // ����� ���� Ƚ�� (ũ����Ż)
    private int[] buyCatCoinFee;                                                        // ����� ���� ��� (����)
    private int[] buyCatCashFee;                                                        // ����� ���� ��� (ũ����Ż)

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

        buyCatCoinCounts = new int[GameManager.Instance.AllCatData.Length];   // ����� ���� Ƚ�� (����)
        buyCatCashCounts = new int[GameManager.Instance.AllCatData.Length];   // ����� ���� Ƚ�� (ũ����Ż)
        buyCatCoinFee = new int[GameManager.Instance.AllCatData.Length];      // ����� ���� ��� (����)
        buyCatCashFee = new int[GameManager.Instance.AllCatData.Length];      // ����� ���� ��� (ũ����Ż)

        // ����� ���� �޴� ����
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            buyCatCoinCounts[i] = 0;
            buyCatCashCounts[i] = 0;
            buyCatCoinFee[i] = i * 5 + 10;
            buyCatCashFee[i] = 5;

            buyCatCountExplainTexts[i].text = $"���� Ƚ�� : {buyCatCoinCounts[i]}ȸ + {buyCatCashCounts[i]}ȸ";
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

    // ======================================================================================================================

    private void InitializeItemMenuManager()
    {
        buyCatMenuPanel.SetActive(false);
        OpenCloseBottomBuyCatMenuPanel();
    }

    // ======================================================================================================================
    // [Battle]

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

    // ======================================================================================================================

    // ����� ���� �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomBuyCatMenuPanel()
    {
        bottomBuyCatButton.onClick.AddListener(() => activePanelManager.TogglePanel("BuyCatMenu"));
        buyCatBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BuyCatMenu"));
    }

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCoin(Button button)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("����� �������� �ִ��Դϴ�!!");
            return;
        }

        int catIndex = -1;
        for (int i = 0; i < buyCatCoinButtons.Length; i++)
        {
            if (button == buyCatCoinButtons[i])
            {
                catIndex = i;
                break;
            }
        }

        if (catIndex == -1) return;

        if (GameManager.Instance.Coin < buyCatCoinFee[catIndex])
        {
            NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!!");
            return;
        }

        GameManager.Instance.Coin -= buyCatCoinFee[catIndex];
        buyCatCoinCounts[catIndex]++;
        buyCatCoinFee[catIndex] *= 2;

        QuestManager.Instance.AddPurchaseCatsCount();
        DictionaryManager.Instance.UnlockCat(catIndex);

        SpawnManager catSpawn = GetComponent<SpawnManager>();
        catSpawn.SpawnGradeCat(catIndex);

        buyCatCountExplainTexts[catIndex].text = $"���� Ƚ�� : {buyCatCoinCounts[catIndex]}ȸ + {buyCatCashCounts[catIndex]}ȸ";
        buyCatCoinFeeTexts[catIndex].text = $"{buyCatCoinFee[catIndex]:N0}";

        if (GoogleManager.Instance != null)
        {
            Debug.Log("���� ����");
            GoogleManager.Instance.SaveGameState();
        }
    }

    // ����� ���� ��ư �Լ�(ũ����Ż�� ����)
    private void BuyCatCash(Button button)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("����� �������� �ִ��Դϴ�!!");
            return;
        }

        int catIndex = -1;
        for (int i = 0; i < buyCatCashButtons.Length; i++)
        {
            if (button == buyCatCashButtons[i])
            {
                catIndex = i;
                break;
            }
        }

        if (catIndex == -1) return;

        if (GameManager.Instance.Cash < buyCatCashFee[catIndex])
        {
            NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!!");
            return;
        }

        GameManager.Instance.Cash -= buyCatCashFee[catIndex];
        buyCatCashCounts[catIndex]++;
        buyCatCashFee[catIndex] *= 2;

        QuestManager.Instance.AddPurchaseCatsCount();
        DictionaryManager.Instance.UnlockCat(catIndex);

        SpawnManager catSpawn = GetComponent<SpawnManager>();
        catSpawn.SpawnGradeCat(catIndex);

        buyCatCountExplainTexts[catIndex].text = $"���� Ƚ�� : {buyCatCoinCounts[catIndex]}ȸ + {buyCatCashCounts[catIndex]}ȸ";
        buyCatCashFeeTexts[catIndex].text = $"{buyCatCashFee[catIndex]:N0}";

        if (GoogleManager.Instance != null)
        {
            Debug.Log("���� ����");
            GoogleManager.Instance.SaveGameState();
        }
    }

    // ======================================================================================================================

    #region Save System
    [Serializable]
    private class SaveData
    {
        public int[] buyCatCoinCounts;      // ����̺� ���� ���� Ƚ��
        public int[] buyCatCashCounts;      // ����̺� ĳ�� ���� Ƚ��
        public int[] buyCatCoinFee;         // ����̺� ���� ���� ���
        public int[] buyCatCashFee;         // ����̺� ĳ�� ���� ���
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            buyCatCoinCounts = this.buyCatCoinCounts,
            buyCatCashCounts = this.buyCatCashCounts,
            buyCatCoinFee = this.buyCatCoinFee,
            buyCatCashFee = this.buyCatCashFee
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        // ������ ����
        this.buyCatCoinCounts = savedData.buyCatCoinCounts;
        this.buyCatCashCounts = savedData.buyCatCashCounts;
        this.buyCatCoinFee = savedData.buyCatCoinFee;
        this.buyCatCashFee = savedData.buyCatCashFee;

        // UI ������Ʈ
        UpdateAllBuyCatUI();
    }

    // UI ���� ������Ʈ
    private void UpdateAllBuyCatUI()
    {
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            if (i < buyCatCountExplainTexts.Length)
            {
                buyCatCountExplainTexts[i].text = $"���� Ƚ�� : {buyCatCoinCounts[i]}ȸ + {buyCatCashCounts[i]}ȸ";
                buyCatCoinFeeTexts[i].text = $"{buyCatCoinFee[i]:N0}";
                buyCatCashFeeTexts[i].text = $"{buyCatCashFee[i]:N0}";
            }
        }
    }
    #endregion

}
