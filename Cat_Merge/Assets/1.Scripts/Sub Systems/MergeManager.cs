using UnityEngine;
using UnityEngine.UI;

// ����� ���� Script
public class MergeManager : MonoBehaviour
{
    #region Variables
    public static MergeManager Instance { get; private set; }

    [Header("---[Merge On/Off System]")]
    [SerializeField] private Button openMergePanelButton;           // ���� �г� ���� ��ư
    [SerializeField] private GameObject mergePanel;                 // ���� On/Off �г�
    [SerializeField] private Button closeMergePanelButton;          // ���� �г� �ݱ� ��ư
    [SerializeField] private Button mergeStateButton;               // ���� ���� ��ư
    private bool isMergeEnabled;                                    // ���� Ȱ��ȭ ����
    private bool previousMergeState;                                // ���� ���� ����

    [Header("---[UI Color]")]
    private const string activeColorCode = "#FFCC74";               // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";             // ��Ȱ��ȭ���� Color
    #endregion

    // ======================================================================================================================

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

        isMergeEnabled = true;
    }

    private void Start()
    {
        UpdateMergeButtonColor();
        InitializeButtonListeners();
    }

    // ��ư ������ �ʱ�ȭ �Լ�
    private void InitializeButtonListeners()
    {
        openMergePanelButton.onClick.AddListener(OpenMergePanel);
        closeMergePanelButton.onClick.AddListener(CloseMergePanel);
        mergeStateButton.onClick.AddListener(ToggleMergeState);
    }
    #endregion

    // ======================================================================================================================

    #region Panel Control
    // ���� �г� ���� �Լ�
    private void OpenMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(true);
        }
    }

    // ���� �г� �ݴ� �Լ�
    public void CloseMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
    }

    // ���� ���� ��� �Լ�
    private void ToggleMergeState()
    {
        isMergeEnabled = !isMergeEnabled;
        UpdateMergeButtonColor();
        CloseMergePanel();
    }
    #endregion

    // ======================================================================================================================

    #region Battle System
    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleMergeState()
    {
        previousMergeState = isMergeEnabled;
        isMergeEnabled = false;

        openMergePanelButton.interactable = false;
        if (mergePanel.activeSelf)
        {
            mergePanel.SetActive(false);
        }
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleMergeState()
    {
        isMergeEnabled = previousMergeState;

        openMergePanelButton.interactable = true;
    }
    #endregion

    // ======================================================================================================================

    #region UI System
    // ���� ��ư ���� ������Ʈ �Լ�
    private void UpdateMergeButtonColor()
    {
        if (openMergePanelButton != null)
        {
            string colorCode = !isMergeEnabled ? activeColorCode : inactiveColorCode;
            if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
            {
                openMergePanelButton.GetComponent<Image>().color = color;
            }
        }
    }

    // ���� ���� ��ȯ �Լ�
    public bool IsMergeEnabled()
    {
        return isMergeEnabled;
    }
    #endregion

    // ======================================================================================================================

    #region Merge System
    // ����� Merge �Լ�
    public Cat MergeCats(Cat cat1, Cat cat2)
    {
        if (cat1.CatGrade != cat2.CatGrade)
        {
            //Debug.LogWarning("����� �ٸ�");
            return null;
        }

        Cat nextCat = GetCatByGrade(cat1.CatGrade + 1);
        if (nextCat != null)
        {
            //Debug.Log($"�ռ� ���� : {nextCat.CatName}");
            DictionaryManager.Instance.UnlockCat(nextCat.CatGrade - 1);
            QuestManager.Instance.AddMergeCount();

            if (cat1.CatGrade == 1 && cat2.CatGrade == 1)
            {
                FriendshipManager.Instance.nowExp += 2;
                FriendshipManager.Instance.expGauge.value += 0.1f;
            }

                return nextCat;
        }
        else
        {
            //Debug.LogWarning("�� ���� ����� ����̰� ����");
            return null;
        }
    }

    // ����� ��� ��ȯ �Լ�
    public Cat GetCatByGrade(int grade)
    {
        GameManager gameManager = GameManager.Instance;
        foreach (Cat cat in gameManager.AllCatData)
        {
            if (cat.CatGrade == grade)
            {
                return cat;
            }
        }
        return null;
    }
    #endregion

}
