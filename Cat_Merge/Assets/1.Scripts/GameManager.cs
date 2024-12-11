using UnityEngine;
using TMPro;
using UnityEngine.UI;

// GameManager Script
public class GameManager : MonoBehaviour
{
    // Data
    private Cat[] allCatData;                                   // ��� ����� ������ ����
    public Cat[] AllCatData => allCatData;

    private int maxCats = 8;                                    // ȭ�� �� �ִ� ����� ��
    private int currentCatCount = 0;                            // ȭ�� �� ����� ��

    // UI
    [SerializeField] private TextMeshProUGUI catCountText;      // ����� �� �ؽ�Ʈ

    [Header ("Auto Move")]
    [SerializeField] private Button openAutoMovePanelButton;    // �ڵ� �̵� �г� ���� ��ư
    [SerializeField] private GameObject autoMovePanel;          // �ڵ� �̵� On/Off �г�
    [SerializeField] private Button closeAutoMovePanelButton;   // �ڵ� �̵� �г� �ݱ� ��ư
    [SerializeField] private Button autoMoveStateButton;        // �ڵ� �̵� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMoveStateText; // �ڵ� �̵� ���� ���� �ؽ�Ʈ
    private bool isAutoMoveEnabled = true;                      // �ڵ� �̵� Ȱ��ȭ ����

    private void Awake()
    {
        LoadAllCats();
        UpdateCatCountText();

        // �ڵ��̵� ����
        UpdateAutoMoveStateText();
        UpdateOpenButtonColor();
        openAutoMovePanelButton.onClick.AddListener(OpenAutoMovePanel);
        closeAutoMovePanelButton.onClick.AddListener(CloseAutoMovePanel);
        autoMoveStateButton.onClick.AddListener(ToggleAutoMove);
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

    // �ڵ� �̵� ���� Text
    private void UpdateAutoMoveStateText()
    {
        if (autoMoveStateText != null)
        {
            autoMoveStateText.text = isAutoMoveEnabled ? "OFF" : "ON";
        }
    }

    // �ڵ� �̵� �г� ����
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // �ڵ� �̵� ���� ��ȯ
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

    // ��ư ���� ������Ʈ
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

    // �г� �ݱ�
    public void CloseAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // ���� ���� ��ȯ (�ʿ� �� �ܺο��� Ȯ�� ����)
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

}
