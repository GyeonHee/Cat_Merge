using UnityEngine;
using UnityEngine.UI;

public class AutoMoveManager : MonoBehaviour
{
    // Singleton Instance
    public static AutoMoveManager Instance { get; private set; }

    [Header("---[AutoMove On/Off System]")]
    [SerializeField] private Button openAutoMovePanelButton;        // �ڵ� �̵� �г� ���� ��ư
    [SerializeField] private GameObject autoMovePanel;              // �ڵ� �̵� On/Off �г�
    [SerializeField] private Button closeAutoMovePanelButton;       // �ڵ� �̵� �г� �ݱ� ��ư
    [SerializeField] private Button autoMoveStateButton;            // �ڵ� �̵� ���� ��ư
    private float autoMoveTime = 3f;                                // �ڵ� �̵� �ð�
    private bool isAutoMoveEnabled;                                 // �ڵ� �̵� Ȱ��ȭ ����
    private bool previousAutoMoveState;                             // ���� ���� ����

    // ======================================================================================================================

    [Header("---[UI Color]")]
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

        isAutoMoveEnabled = true;
    }

    private void Start()
    {
        UpdateAutoMoveButtonColor();

        openAutoMovePanelButton.onClick.AddListener(OpenAutoMovePanel);
        closeAutoMovePanelButton.onClick.AddListener(CloseAutoMovePanel);
        autoMoveStateButton.onClick.AddListener(ToggleAutoMoveState);
    }

    // ======================================================================================================================

    // �ڵ��̵� �г� ���� �Լ�
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // �ڵ��̵� ���� ��ȯ �Լ�
    public void ToggleAutoMoveState()
    {
        isAutoMoveEnabled = !isAutoMoveEnabled;

        // ��� ����̿� ���� ����
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned) continue;
            cat.SetAutoMoveState(isAutoMoveEnabled);
        }

        UpdateAutoMoveButtonColor();
        CloseAutoMovePanel();
    }

    // �ڵ��̵� ��ư ���� ������Ʈ �Լ�
    private void UpdateAutoMoveButtonColor()
    {
        if (openAutoMovePanelButton != null)
        {
            string colorCode = !isAutoMoveEnabled ? activeColorCode : inactiveColorCode;
            if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
            {
                openAutoMovePanelButton.GetComponent<Image>().color = color;
            }
        }
    }

    // �ڵ��̵� �г� �ݴ� �Լ�
    public void CloseAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // �ڵ��̵� ���� ���� ��ȯ�ϴ� �Լ�
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

    // �ڵ��̵� �ֱ� ��ȯ�ϴ� �Լ�
    public float AutoMoveTime()
    {
        return autoMoveTime;
    }

    // ======================================================================================================================
    // [���� ����]

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleAutoMoveState()
    {
        previousAutoMoveState = isAutoMoveEnabled;
        isAutoMoveEnabled = false;

        // ��� ����̿� ���� ����
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned) continue;
            cat.SetAutoMoveState(isAutoMoveEnabled);
        }

        openAutoMovePanelButton.interactable = false;

        if (autoMovePanel.activeSelf == true)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleAutoMoveState()
    {
        isAutoMoveEnabled = previousAutoMoveState;

        // ��� ����̿� ���� ����
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned) continue;
            cat.SetAutoMoveState(isAutoMoveEnabled);
        }

        openAutoMovePanelButton.interactable = true;
    }

    // ======================================================================================================================

    

}
