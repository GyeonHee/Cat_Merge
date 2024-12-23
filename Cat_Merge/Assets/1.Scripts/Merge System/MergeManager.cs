using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ����� ���� Script
public class MergeManager : MonoBehaviour
{
    // Singleton Instance
    public static MergeManager Instance { get; private set; }

    // Merge On/Off System
    [Header("---[Merge On/Off System]")]
    [SerializeField] private Button openMergePanelButton;           // ���� �г� ���� ��ư
    [SerializeField] private GameObject mergePanel;                 // ���� On/Off �г�
    [SerializeField] private Button closeMergePanelButton;          // ���� �г� �ݱ� ��ư
    [SerializeField] private Button mergeStateButton;               // ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI mergeStateText;        // ���� ���� ���� �ؽ�Ʈ
    private bool isMergeEnabled = true;

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

        UpdateMergeStateText();
        UpdateMergeButtonColor();
        openMergePanelButton.onClick.AddListener(OpenMergePanel);
        closeMergePanelButton.onClick.AddListener(CloseMergePanel);
        mergeStateButton.onClick.AddListener(ToggleMergeState);
    }

    // ======================================================================================================================

    // ���� On/Off �г� ���� �Լ�
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

    // ���� ��ư ���� ������Ʈ �Լ�
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

    // ���� �г� �ݴ� �Լ�
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

    // ======================================================================================================================

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
            QuestManager.Instance.AddCombineCount();
            return nextCat;
        }
        else
        {
            //Debug.LogWarning("�� ���� ����� ����̰� ����");
            return null;
        }
    }

    // ����� ID ��ȯ �Լ�
    public Cat GetCatByGrade(int grade)
    {
        GameManager gameManager = GameManager.Instance;

        foreach (Cat cat in gameManager.AllCatData)
        {
            if (cat.CatId == grade)
                return cat;
        }
        return null;
    }


}
