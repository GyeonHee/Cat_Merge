using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// GameManager Script
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }        // SingleTon

    // Data
    private Cat[] allCatData;                                       // ��� ����� ������ ����
    public Cat[] AllCatData => allCatData;

    // Main UI Text
    [Header("Main UI Text")]
    // ����̼�/�ִ����̼�
    [SerializeField] private TextMeshProUGUI catCountText;          // ����� �� �ؽ�Ʈ
    private int currentCatCount = 0;                                // ȭ�� �� ����� ��
    private int maxCats = 8;                                        // �ִ� ����� ��

    // �⺻ ��ȭ
    [SerializeField] private TextMeshProUGUI coinText;              // �⺻��ȭ �ؽ�Ʈ
    private int coin = 5000;                                        // �⺻��ȭ

    // ĳ�� ��ȭ
    [SerializeField] private TextMeshProUGUI cashText;              // ĳ����ȭ �ؽ�Ʈ
    private int cash = 1000;                                        // ĳ����ȭ

    // Merge On/Off
    [Header("Merge On/Off")]
    [SerializeField] private Button openMergePanelButton;           // ���� �г� ���� ��ư
    [SerializeField] private GameObject mergePanel;                 // ���� On/Off �г�
    [SerializeField] private Button closeMergePanelButton;          // ���� �г� �ݱ� ��ư
    [SerializeField] private Button mergeStateButton;               // ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI mergeStateText;        // ���� ���� ���� �ؽ�Ʈ
    private bool isMergeEnabled = true;

    // AutoMove On/Off
    [Header ("AutoMove On/Off")]
    [SerializeField] private Button openAutoMovePanelButton;        // �ڵ� �̵� �г� ���� ��ư
    [SerializeField] private GameObject autoMovePanel;              // �ڵ� �̵� On/Off �г�
    [SerializeField] private Button closeAutoMovePanelButton;       // �ڵ� �̵� �г� �ݱ� ��ư
    [SerializeField] private Button autoMoveStateButton;            // �ڵ� �̵� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMoveStateText;     // �ڵ� �̵� ���� ���� �ؽ�Ʈ
    private bool isAutoMoveEnabled = true;                          // �ڵ� �̵� Ȱ��ȭ ����

    // AutoMerge
    [Header ("AutoMerge")]
    [SerializeField] private Button openAutoMergePanelButton;       // �ڵ� ���� �г� ���� ��ư
    [SerializeField] private GameObject autoMergePanel;             // �ڵ� ���� �г�
    [SerializeField] private Button closeAutoMergePanelButton;      // �ڵ� ���� �г� �ݱ� ��ư
    [SerializeField] private Button autoMergeStateButton;           // �ڵ� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMergeCostText;     // �ڵ� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMergeTimerText;    // �ڵ� ���� Ÿ�̸� �ؽ�Ʈ
    private int autoMergeCost = 30;                                 // �ڵ� ���� ���

    // Sort System
    [Header("Sort")]
    [SerializeField] private Button sortButton;                     // ���� ��ư
    [SerializeField] private Transform gamePanel;                   // GamePanel

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

        // �⺻ ����
        LoadAllCats();
        UpdateCatCountText();
        UpdateCoinText();
        UpdateCashText();

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

        // ���� ����
        sortButton.onClick.AddListener(SortCats);

        // �ڵ����� ����
        UpdateAutoMergeCostText();
        openAutoMergePanelButton.onClick.AddListener(OpenAutoMergePanel);
        closeAutoMergePanelButton.onClick.AddListener(CloseAutoMergePanel);
        autoMergeStateButton.onClick.AddListener(StartAutoMerge);
        UpdateAutoMergeTimerVisibility(false);
    }

    // ����� ���� Load �Լ�
    private void LoadAllCats()
    {
        allCatData = Resources.LoadAll<Cat>("Cats");
    }

    // ============================================================================================

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

    // ============================================================================================

    // �⺻��ȭ �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"{coin}";
        }
    }

    // ============================================================================================

    // ĳ����ȭ �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateCashText()
    {
        if (cashText != null)
        {
            // ���ڸ� 3�ڸ����� �޸��� �߰��Ͽ� ǥ��
            cashText.text = cash.ToString("N0");
        }
    }

    // ============================================================================================

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

    // ============================================================================================

    // �ڵ��̵� �г� ���� �Լ�
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // �ڵ��̵� ���� ��ȯ �Լ�
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

    // �ڵ��̵� ��ư ���� ������Ʈ �Լ�
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

    // ============================================================================================

    // ����� ���� �Լ�
    private void SortCats()
    {
        StartCoroutine(SortCatsCoroutine());
    }

    // ����̵��� ���ĵ� ��ġ�� ��ġ�ϴ� �ڷ�ƾ
    private IEnumerator SortCatsCoroutine()
    {
        // ����� ��ü���� ��� �ε巴�� �����ϴ� �ڷ�ƾ�� ���ÿ� ����
        List<Coroutine> moveCoroutines = new List<Coroutine>();

        // ����� ��ü���� ����� �������� ���� (���� ����� ���� ������)
        List<GameObject> sortedCats = new List<GameObject>();

        // ���� �г� �� ��� �ڽ� ����� ��ü�鿡 ����
        foreach (Transform child in gamePanel)
        {
            sortedCats.Add(child.gameObject);
        }

        // ��� �������� ����� ��ü ���� (��������)
        sortedCats.Sort((cat1, cat2) =>
        {
            int grade1 = GetCatGrade(cat1);
            int grade2 = GetCatGrade(cat2);

            if (grade1 == grade2) return 0;
            return grade1 > grade2 ? -1 : 1;
        });

        // ���ĵ� ����� ��ü���� �ε巴�� �̵�
        for (int i = 0; i < sortedCats.Count; i++)
        {
            GameObject cat = sortedCats[i];
            Coroutine moveCoroutine = StartCoroutine(MoveCatToPosition(cat, i));
            moveCoroutines.Add(moveCoroutine);
        }

        // ��� ����̰� �̵��� ��ĥ ������ ��ٸ���
        foreach (Coroutine coroutine in moveCoroutines)
        {
            yield return coroutine;
        }
    }

    // ������� ����� ��ȯ�ϴ� �Լ�
    private int GetCatGrade(GameObject catObject)
    {
        int grade = catObject.GetComponent<CatData>().catData.CatId;
        return grade;
    }

    // ����̵��� �ε巴�� ���ĵ� ��ġ�� �̵���Ű�� �Լ�
    private IEnumerator MoveCatToPosition(GameObject catObject, int index)
    {
        RectTransform rectTransform = catObject.GetComponent<RectTransform>();

        // ��ǥ ��ġ ��� (index�� ���ĵ� ����)
        float targetX = (index % 7 - 3) * (rectTransform.rect.width + 10);
        float targetY = (index / 7) * (rectTransform.rect.height + 10);
        Vector2 targetPosition = new Vector2(targetX, -targetY);

        // ���� ��ġ�� ��ǥ ��ġ�� ���̸� ����Ͽ� �ε巴�� �̵�
        float elapsedTime = 0f;
        float duration = 0.1f;          // �̵� �ð� (��)

        Vector2 initialPosition = rectTransform.anchoredPosition;

        // ��ǥ ��ġ�� �ε巴�� �̵�
        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �̵��� ������ �� ��Ȯ�� ��ǥ ��ġ�� ����
        rectTransform.anchoredPosition = targetPosition;
    }

    // ============================================================================================

    // �ڵ����� ��� Text ������Ʈ �Լ�
    private void UpdateAutoMergeCostText()
    {
        if (autoMergeCostText != null)
        {
            autoMergeCostText.text = $"{autoMergeCost}";
        }
    }

    // �ڵ����� �г� ���� �Լ�
    private void OpenAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(true);
        }
    }

    // �ڵ����� �г� �ݴ� �Լ�
    private void CloseAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(false);
        }
    }

    // �ڵ����� ���� �Լ�
    private void StartAutoMerge()
    {
        if (cash >= autoMergeCost)
        {
            cash -= autoMergeCost;
            UpdateCashText();

            AutoMerge autoMergeScript = FindObjectOfType<AutoMerge>();
            if (autoMergeScript != null)
            {
                autoMergeScript.OnClickedAutoMerge();
            }
        }
        else
        {
            Debug.Log("Not enough coins to start AutoMerge!");
        }
    }

    // �ڵ����� ���¿� ���� Ÿ�̸� �ؽ�Ʈ ���ü� ������Ʈ �Լ�
    public void UpdateAutoMergeTimerVisibility(bool isVisible)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.gameObject.SetActive(isVisible);
        }
    }

    // �ڵ����� Ÿ�̸� ������Ʈ �Լ�
    public void UpdateAutoMergeTimerText(int remainingTime)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.text = $"{remainingTime}";
        }
    }

}
