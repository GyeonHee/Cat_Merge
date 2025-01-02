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
    public bool isAutoMoveEnabled = true;                           // �ڵ� �̵� Ȱ��ȭ ����
    private bool previousAutoMoveState = true;                      // ���� ���� ����

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
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        UpdateAutoMoveButtonColor();
        CloseAutoMovePanel();
    }

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleAutoMoveState()
    {
        previousAutoMoveState = isAutoMoveEnabled;
        isAutoMoveEnabled = false;

        // ��� ����̿� ���� ����
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        openAutoMovePanelButton.interactable = false;
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleAutoMoveState()
    {
        isAutoMoveEnabled = previousAutoMoveState;

        // ��� ����̿� ���� ����
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        openAutoMovePanelButton.interactable = true;
    }

    // �ڵ��̵� ��ư ���� ������Ʈ �Լ�
    private void UpdateAutoMoveButtonColor()
    {
        if (openAutoMovePanelButton != null)
        {
            string colorCode = !isAutoMoveEnabled ? "#5f5f5f" : "#FFFFFF";
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

}
