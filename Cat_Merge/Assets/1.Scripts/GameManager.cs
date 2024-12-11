using UnityEngine;
using TMPro;
using UnityEngine.UI;

// GameManager Script
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }    // SingleTon

    // Data
    private Cat[] allCatData;                                   // ��� ����� ������ ����
    public Cat[] AllCatData => allCatData;

    private int maxCats = 8;                                    // ȭ�� �� �ִ� ����� ��
    private int currentCatCount = 0;                            // ȭ�� �� ����� ��

    // UI
    [SerializeField] private TextMeshProUGUI catCountText;      // ����� �� �ؽ�Ʈ

    // Merge On/Off
    [Header("Merge On/Off")]
    [SerializeField] private Button openMergePanelButton;       // ���� �г� ���� ��ư
    [SerializeField] private GameObject mergePanel;             // ���� On/Off �г�
    [SerializeField] private Button closeMergePanelButton;      // ���� �г� �ݱ� ��ư
    [SerializeField] private Button mergeStateButton;           // ���� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI mergeStateText;    // ���� ���� ���� �ؽ�Ʈ
    private bool isMergeEnabled = true;

    // AutoMove On/Off
    [Header ("AutoMove On/Off")]
    [SerializeField] private Button openAutoMovePanelButton;    // �ڵ� �̵� �г� ���� ��ư
    [SerializeField] private GameObject autoMovePanel;          // �ڵ� �̵� On/Off �г�
    [SerializeField] private Button closeAutoMovePanelButton;   // �ڵ� �̵� �г� �ݱ� ��ư
    [SerializeField] private Button autoMoveStateButton;        // �ڵ� �̵� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMoveStateText; // �ڵ� �̵� ���� ���� �ؽ�Ʈ
    private bool isAutoMoveEnabled = true;                      // �ڵ� �̵� Ȱ��ȭ ����

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

        LoadAllCats();
        UpdateCatCountText();

        // �ڵ��̵� On/Off ����
        UpdateAutoMoveStateText();
        UpdateOpenButtonColor();
        openAutoMovePanelButton.onClick.AddListener(OpenAutoMovePanel);
        closeAutoMovePanelButton.onClick.AddListener(CloseAutoMovePanel);
        autoMoveStateButton.onClick.AddListener(ToggleAutoMove);

        // ���� On/Off ����
        UpdateMergeStateText();
        UpdateMergeButtonColor();
        openMergePanelButton.onClick.AddListener(OpenMergePanel);
        closeMergePanelButton.onClick.AddListener(CloseMergePanel);
        mergeStateButton.onClick.AddListener(ToggleMergeState);
    }

    // ����� ���� Load �Լ�
    private void LoadAllCats()
    {
        allCatData = Resources.LoadAll<Cat>("Cats");
    }

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



    // ���� �г� ����
    private void OpenMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(true);
        }
    }

    // ���� ���� ��ȯ �Լ�
    private void ToggleMergeState()
    {
        isMergeEnabled = !isMergeEnabled;
        UpdateMergeStateText();
        UpdateMergeButtonColor();
        CloseMergePanel();
    }

    // ���� ���� Text ������Ʈ �Լ�
    private void UpdateMergeStateText()
    {
        if (mergeStateText != null)
        {
            mergeStateText.text = isMergeEnabled ? "OFF" : "ON";
        }
    }

    // ���� ��ư ���� ������Ʈ
    private void UpdateMergeButtonColor()
    {
        if (openMergePanelButton != null)
        {
            Color normalColor = isMergeEnabled ? new Color(1f, 1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorBlock colors = openMergePanelButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;
            colors.selectedColor = normalColor;
            openMergePanelButton.colors = colors;
        }
    }

    // ���� �г� �ݱ�
    public void CloseMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
    }

    // ���� ���� ��ȯ �Լ�
    public bool IsMergeEnabled()
    {
        return isMergeEnabled;
    }



    // �ڵ��̵� �г� ����
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // �ڵ��̵� ���� ��ȯ
    public void ToggleAutoMove()
    {
        isAutoMoveEnabled = !isAutoMoveEnabled;

        // ��� ����̿� ���� ����
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        // ���� ������Ʈ �� ��ư ���� ����
        UpdateAutoMoveStateText();
        UpdateOpenButtonColor();

        // �г� �ݱ�
        CloseAutoMovePanel();
    }

    // �ڵ��̵� ���� Text ������Ʈ �Լ�
    private void UpdateAutoMoveStateText()
    {
        if (autoMoveStateText != null)
        {
            autoMoveStateText.text = isAutoMoveEnabled ? "OFF" : "ON";
        }
    }

    // �ڵ��̵� ��ư ���� ������Ʈ
    private void UpdateOpenButtonColor()
    {
        if (openAutoMovePanelButton != null)
        {
            Color normalColor = isAutoMoveEnabled ? new Color(1f, 1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorBlock colors = openAutoMovePanelButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;
            colors.selectedColor = normalColor;
            openAutoMovePanelButton.colors = colors;
        }
    }

    // �ڵ��̵� �г� �ݱ�
    public void CloseAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // �ڵ��̵� ���� ���� ��ȯ
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

}
