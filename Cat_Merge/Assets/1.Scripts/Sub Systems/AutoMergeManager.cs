using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

// �ڵ��ռ� ���� ��ũ��Ʈ
public class AutoMergeManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static AutoMergeManager Instance { get; private set; }

    [Header("---[UI]")]
    [SerializeField] private GameObject autoMergePanel;             // �ڵ� �ռ� �г�
    [SerializeField] private Button openAutoMergePanelButton;       // �ڵ� �ռ� �г� ���� ��ư
    [SerializeField] private Button closeAutoMergePanelButton;      // �ڵ� �ռ� �г� �ݱ� ��ư
    [SerializeField] private Button autoMergeStateButton;           // �ڵ� �ռ� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMergeCostText;     // �ڵ� �ռ� ��� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI autoMergeTimerText;    // �ڵ� �ռ� Ÿ�̸� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI explainText;           // �ڵ� �ռ� ���� �ؽ�Ʈ

    [Header("---[Auto Merge Settings]")]
    private const float MAX_AUTO_MERGE_DURATION = 86400f;                       // �ִ� �ڵ� �ռ� �ð� (24�ð�)
    private const float MOVE_DURATION = 0.3f;                                   // ����̰� �̵��ϴ� �� �ɸ��� �ð� (�̵� �ӵ�)
    private const float AUTO_MERGE_DURATION = 30.0f;                            // �ڵ� �ռ� �⺻ ���� �ð�
    private const int AUTO_MERGE_COST = 30;                                     // �ڵ� �ռ� ���
    private WaitForSeconds waitAutoMergeInterval = new WaitForSeconds(0.5f);    // �ڵ� �ռ� ����
    private WaitForSeconds waitSpawnInterval = new WaitForSeconds(0.1f);        // ��ȯ ����

    private float startTime;                        // �ڵ� �ռ� ���� �ð�
    private float currentAutoMergeDuration;         // ���� �ڵ� �ռ� ���� �ð�
    private bool isAutoMergeActive = false;         // �ڵ� �ռ� Ȱ��ȭ ����
    private bool isPaused = false;                  // �Ͻ����� ����
    private float pausedTimeRemaining = 0f;         // �Ͻ����� ������ ���� �ð�
    private Coroutine autoMergeCoroutine;           // �ڵ� �ռ� �ڷ�ƾ

    private float panelWidth;           // Panel Width
    private float panelHeight;          // Panel Height
    private Vector2 panelHalfSize;      // Panel Size / 2

    private HashSet<DragAndDropManager> mergingCats = new HashSet<DragAndDropManager>();


    private bool isDataLoaded = false;          // ������ �ε� Ȯ��

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
        InitializeAutoMergeManager();

        // GoogleManager���� �����͸� �ε����� ���� ��쿡�� �ʱ�ȭ
        if (!isDataLoaded)
        {
            InitializeDefaultValues();
        }

        // �г� ���
        ActivePanelManager.Instance.RegisterPanel("AutoMergePanel", autoMergePanel, null, ActivePanelManager.PanelPriority.Medium);
    }

    private void Update()
    {
        if (!isAutoMergeActive && !isPaused) return;

        float remainingTime = isPaused ? pausedTimeRemaining : Mathf.Max(currentAutoMergeDuration - (Time.time - startTime), 0);
        UpdateTimerDisplay((int)remainingTime);

        if (!isPaused && remainingTime <= 0)
        {
            EndAutoMerge();
        }
    }

    #endregion


    #region Initialize

    // �⺻�� �ʱ�ȭ �Լ�
    private void InitializeDefaultValues()
    {
        isAutoMergeActive = false;
        isPaused = false;
        currentAutoMergeDuration = 0f;
        UpdateAutoMergeTimerVisibility(false);
        UpdateExplainText(0);
    }

    // ������Ʈ ĳ�� �� UI �ʱ�ȭ
    private void InitializeAutoMergeManager()
    {
        // UI �ʱ�ȭ
        UpdateAutoMergeCostText();

        // ��ư �̺�Ʈ ������ ����
        InitializeButtonListeners();

        // �г� ũ�� ĳ��
        CachePanelSize();
    }

    // �г� ũ�� ĳ�� �Լ�
    private void CachePanelSize()
    {
        DragAndDropManager anyActiveCat = FindObjectOfType<DragAndDropManager>();
        if (anyActiveCat != null)
        {
            RectTransform parentRect = anyActiveCat.rectTransform?.parent?.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                panelWidth = parentRect.rect.width;
                panelHeight = parentRect.rect.height;
                panelHalfSize = new Vector2(panelWidth / 2, panelHeight / 2);
            }
        }
    }

    // UI ��ư �̺�Ʈ ������ ���� �Լ�
    private void InitializeButtonListeners()
    {
        openAutoMergePanelButton?.onClick.AddListener(() => ActivePanelManager.Instance.TogglePanel("AutoMergePanel"));
        closeAutoMergePanelButton?.onClick.AddListener(() => ActivePanelManager.Instance.ClosePanel("AutoMergePanel"));
        autoMergeStateButton?.onClick.AddListener(StartAutoMerge);
    }

    #endregion


    #region Auto Merge System

    // �ڵ��ռ� ���� �Լ�
    public void StartAutoMerge()
    {
        if (GameManager.Instance.Cash < AUTO_MERGE_COST)
        {
            NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!!");
            return;
        }

        if (currentAutoMergeDuration + AUTO_MERGE_DURATION > MAX_AUTO_MERGE_DURATION)
        {
            NotificationManager.Instance.ShowNotification("�ڵ��ռ��� �ִ� 24�ð������� �����մϴ�!!");
            return;
        }

        GameManager.Instance.Cash -= AUTO_MERGE_COST;
        OnClickedAutoMerge();
    }

    // �ڵ��ռ� ��ư Ŭ�� ó�� �Լ�
    public void OnClickedAutoMerge()
    {
        if (!isAutoMergeActive)
        {
            startTime = Time.time;
            isAutoMergeActive = true;
            currentAutoMergeDuration = AUTO_MERGE_DURATION;
            UpdateAutoMergeTimerVisibility(true);
            StartAutoMergeCoroutine();
        }
        else
        {
            currentAutoMergeDuration += AUTO_MERGE_DURATION;
        }
    }

    // �ڷ�ƾ ���� �� ���� �ڷ�ƾ ����
    private void StartAutoMergeCoroutine()
    {
        StopAutoMergeCoroutine();
        autoMergeCoroutine = StartCoroutine(AutoMergeCoroutine());
    }

    // �ڷ�ƾ �����ϰ� ����
    private void StopAutoMergeCoroutine()
    {
        if (autoMergeCoroutine != null)
        {
            StopCoroutine(autoMergeCoroutine);
            autoMergeCoroutine = null;
        }
    }

    // �ڵ��ռ� �ڷ�ƾ
    private IEnumerator AutoMergeCoroutine()
    {
        StartCoroutine(SpawnCatsWhileAutoMerge());

        while (Time.time - startTime < currentAutoMergeDuration)
        {
            CleanupMergingCats();

            // Ȱ��ȭ�� ����̸� ��޼����� �����Ͽ� ������
            var allCats = FindObjectsOfType<DragAndDropManager>()
                .Where(cat => cat != null &&
                       cat.gameObject.activeSelf &&
                       !cat.isDragging)
                .OrderBy(cat => cat.catData.CatGrade)
                .ToList();

            bool mergeFound = false;

            // ���� ���� ��޺��� ���������� �ռ� �õ�
            for (int i = 0; i < allCats.Count; i++)
            {
                if (allCats[i] == null || !allCats[i].gameObject.activeSelf) continue;

                // ���� ����� �ٸ� ����� ã��
                var sameLevelCats = allCats
                    .Where(cat => cat != null &&
                           cat.gameObject.activeSelf &&
                           cat != allCats[i] &&
                           cat.catData.CatGrade == allCats[i].catData.CatGrade)
                    .ToList();

                if (sameLevelCats.Count > 0)
                {
                    var cat1 = allCats[i];
                    var cat2 = sameLevelCats[0];

                    if (IsValidMergePair(cat1, cat2))
                    {
                        Vector2 mergePosition = GetRandomPosition();
                        yield return ExecuteMerge(cat1, cat2, mergePosition);
                        mergeFound = true;
                        yield return waitAutoMergeInterval;
                        break;
                    }
                }
            }

            if (!mergeFound)
            {
                yield return waitAutoMergeInterval;
            }
        }

        EndAutoMerge();
    }

    // �ռ����� ����� ���� �Լ�
    private void CleanupMergingCats()
    {
        mergingCats.RemoveWhere(cat => cat == null || !cat.gameObject.activeSelf || cat.isDragging);
    }

    // ��ȿ�� �ռ����� Ȯ���ϴ� �Լ�
    private bool IsValidMergePair(DragAndDropManager cat1, DragAndDropManager cat2)
    {
        return cat1 != null && cat2 != null &&
               cat1.gameObject.activeSelf && cat2.gameObject.activeSelf &&
               !cat1.isDragging && !cat2.isDragging &&
               !IsMaxLevelCat(cat1.catData) && !IsMaxLevelCat(cat2.catData) &&
               cat1.catData.CatGrade == cat2.catData.CatGrade;
    }

    // �ڵ��ռ� ������ġ �������� �Լ�
    private Vector2 GetRandomPosition()
    {
        // Ȱ��ȭ�� ����̸� ã���� ����
        DragAndDropManager anyActiveCat = FindObjectsOfType<DragAndDropManager>().FirstOrDefault(cat => cat != null && cat.gameObject.activeSelf);

        if (anyActiveCat != null)
        {
            RectTransform parentRect = anyActiveCat.rectTransform?.parent?.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                panelWidth = parentRect.rect.width;
                panelHeight = parentRect.rect.height;
                panelHalfSize = new Vector2(panelWidth / 2, panelHeight / 2);
            }
        }

        return new Vector2(
            UnityEngine.Random.Range(-panelHalfSize.x, panelHalfSize.x),
            UnityEngine.Random.Range(-panelHalfSize.y, panelHalfSize.y)
        );
    }

    // �ռ� ���� �ڷ�ƾ
    private IEnumerator ExecuteMerge(DragAndDropManager cat1, DragAndDropManager cat2, Vector2 mergePosition)
    {
        if (cat1 == null || cat2 == null) yield break;

        mergingCats.Add(cat1);
        mergingCats.Add(cat2);

        yield return MoveCatsToPosition(cat1, cat2, mergePosition);

        if (cat1 != null && cat2 != null && !cat1.isDragging && !cat2.isDragging)
        {
            CompleteMerge(cat1, cat2);
        }

        mergingCats.Remove(cat1);
        mergingCats.Remove(cat2);
    }

    // ������ ������ġ�� ����̵� �̵��ϴ� �ڷ�ƾ
    private IEnumerator MoveCatsToPosition(DragAndDropManager cat1, DragAndDropManager cat2, Vector2 targetPosition)
    {
        StartCoroutine(MoveCatSmoothly(cat1, targetPosition));
        StartCoroutine(MoveCatSmoothly(cat2, targetPosition));

        yield return new WaitUntil(() =>
            cat1 == null || cat2 == null ||
            (!mergingCats.Contains(cat1) && !mergingCats.Contains(cat2)) ||
            (Vector2.Distance(cat1.rectTransform.anchoredPosition, targetPosition) < 0.1f &&
             Vector2.Distance(cat2.rectTransform.anchoredPosition, targetPosition) < 0.1f)
        );
    }

    // �ռ� �Ϸ� ó�� �Լ�
    private void CompleteMerge(DragAndDropManager cat1, DragAndDropManager cat2)
    {
        if (cat1 == null || cat2 == null || !cat1.gameObject.activeSelf || !cat2.gameObject.activeSelf) return;
        if (cat1 == cat2) return;

        cat1.GetComponent<CatData>()?.CleanupCoroutines();
        cat2.GetComponent<CatData>()?.CleanupCoroutines();

        Cat mergedCat = MergeManager.Instance.MergeCats(cat1.catData, cat2.catData);
        if (mergedCat != null)
        {
            SpawnManager.Instance.ReturnCatToPool(cat2.gameObject);
            GameManager.Instance.DeleteCatCount();

            cat1.catData = mergedCat;
            cat1.UpdateCatUI();
            SpawnManager.Instance.RecallEffect(cat1.gameObject);
        }
    }

    // �ڵ��ռ� �� ����� ��ȯ�ϴ� �ڷ�ƾ
    private IEnumerator SpawnCatsWhileAutoMerge()
    {
        while (isAutoMergeActive && !isPaused)
        {
            while (GameManager.Instance.CanSpawnCat())
            {
                SpawnManager.Instance.SpawnAutoMergeCat();
                yield return waitSpawnInterval;
            }
            yield return null;
        }
    }

    // ����̸� �ε巴�� �̵���Ű�� �ڷ�ƾ
    private IEnumerator MoveCatSmoothly(DragAndDropManager cat, Vector2 targetPosition)
    {
        if (cat == null) yield break;

        Vector2 startPos = cat.rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < MOVE_DURATION)
        {
            if (cat == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / MOVE_DURATION);
            cat.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);

            yield return null;
        }

        if (cat != null)
        {
            cat.rectTransform.anchoredPosition = targetPosition;
        }
    }

    // �ִ뷹�� ��������� Ȯ���ϴ� �Լ�
    private bool IsMaxLevelCat(Cat catData)
    {
        return GameManager.Instance != null &&
               GameManager.Instance.AllCatData != null &&
               GameManager.Instance.AllCatData.All(cat => cat.CatGrade != catData.CatGrade + 1);
    }

    // �ڵ��ռ� ���� �Լ�
    private void EndAutoMerge()
    {
        isAutoMergeActive = false;
        isPaused = false;
        pausedTimeRemaining = 0f;
        UpdateAutoMergeTimerVisibility(false);
        StopAllCoroutines();
        mergingCats.Clear();
    }

    // �ռ����� ����� ���� �Լ�
    public void StopMerging(DragAndDropManager cat)
    {
        mergingCats.Remove(cat);
    }

    // ����̰� �ռ������� Ȯ���ϴ� �Լ�
    public bool IsMerging(DragAndDropManager cat)
    {
        return mergingCats.Contains(cat);
    }

    #endregion


    #region UI System

    // �ڵ��ռ� ��� Text ������Ʈ �Լ�
    private void UpdateAutoMergeCostText()
    {
        if (autoMergeCostText != null)
        {
            autoMergeCostText.text = AUTO_MERGE_COST.ToString();
        }
    }

    // �ڵ��ռ� �г� ���� �Լ�
    private void OpenAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(true);
        }
    }

    // �ڵ��ռ� �г� �ݴ� �Լ�
    private void CloseAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(false);
        }
    }

    // �ڵ��ռ� ���¿� ���� Ÿ�̸� �ؽ�Ʈ ���ü� ������Ʈ �Լ�
    public void UpdateAutoMergeTimerVisibility(bool isVisible)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.gameObject.SetActive(isVisible);
        }
    }

    // Ÿ�̸� �ؽ�Ʈ ������Ʈ �Լ�
    private void UpdateTimerDisplay(int remainingTime)
    {
        UpdateAutoMergeTimerText(remainingTime);
        UpdateExplainText(remainingTime);
    }

    // �ڵ��ռ� Ÿ�̸� ������Ʈ �Լ�
    public void UpdateAutoMergeTimerText(int remainingTime)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.text = $"{remainingTime}��";
        }
    }

    // �ڵ��ռ� ���� �ؽ�Ʈ ������Ʈ �Լ�
    private void UpdateExplainText(int remainingTime)
    {
        if (explainText != null)
        {
            int hours = remainingTime / 3600;
            int minutes = (remainingTime % 3600) / 60;
            int seconds = remainingTime % 60;
            explainText.text = $"�ڵ��ռ� {AUTO_MERGE_DURATION}�� ����\n (Ÿ�̸� {hours:D2}:{minutes:D2}:{seconds:D2})";
        }
    }

    #endregion


    #region Battle System

    // �ڵ��ռ� �Ͻ����� �Լ�
    public void PauseAutoMerge()
    {
        if (isAutoMergeActive && !isPaused)
        {
            isPaused = true;
            pausedTimeRemaining = currentAutoMergeDuration - (Time.time - startTime);
            StopAllCoroutines();
            mergingCats.Clear();
            DisableAutoMergeUI();
        }
        else
        {
            openAutoMergePanelButton.interactable = false;
            DisableAutoMergeUI();
        }
    }

    // �ڵ��ռ� �̾��ϱ� �Լ�
    public void ResumeAutoMerge()
    {
        if (isPaused && pausedTimeRemaining > 0)
        {
            isPaused = false;
            startTime = Time.time;
            currentAutoMergeDuration = pausedTimeRemaining;
            EnableAutoMergeUI();
            StartAutoMergeCoroutine();
        }
        else
        {
            EnableAutoMergeUI();
        }
    }

    // ���� ���۽� �ڵ��ռ� UI ��Ȱ��ȭ �Լ�
    private void DisableAutoMergeUI()
    {
        openAutoMergePanelButton.interactable = false;
        if (autoMergePanel.activeSelf)
        {
            autoMergePanel.SetActive(false);
        }
    }

    // ���� ����� �ڵ��ռ� UI Ȱ��ȭ �Լ�
    private void EnableAutoMergeUI()
    {
        openAutoMergePanelButton.interactable = true;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public float remainingTime;              // ���� �ð�
    }

    public string GetSaveData()
    {
        float remainingTime = 0f;

        if (isAutoMergeActive)
        {
            remainingTime = Mathf.Max(currentAutoMergeDuration - (Time.time - startTime), 0);
        }

        SaveData data = new SaveData
        {
            remainingTime = remainingTime
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        if (savedData.remainingTime > 0)
        {
            // ���� �ð��� ������ �ڵ� �ռ� ����
            this.startTime = Time.time;
            this.currentAutoMergeDuration = savedData.remainingTime;
            this.isAutoMergeActive = true;
            this.isPaused = false;
            UpdateAutoMergeTimerVisibility(true);
            UpdateTimerDisplay((int)savedData.remainingTime);
            StartAutoMergeCoroutine();
        }
        else
        {
            // ���� �ð��� ������ �ڵ� �ռ� ��Ȱ��ȭ
            this.isAutoMergeActive = false;
            this.isPaused = false;
            this.currentAutoMergeDuration = 0f;
            UpdateAutoMergeTimerVisibility(false);
            UpdateTimerDisplay(0);
        }

        isDataLoaded = true;
    }

    #endregion


}

