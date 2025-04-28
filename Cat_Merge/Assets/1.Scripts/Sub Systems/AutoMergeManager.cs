using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

// �ڵ����� ���� ��ũ��Ʈ
public class AutoMergeManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static AutoMergeManager Instance { get; private set; }

    [Header("---[UI]")]
    [SerializeField] private GameObject autoMergePanel;             // �ڵ� ���� �г�
    [SerializeField] private Button openAutoMergePanelButton;       // �ڵ� ���� �г� ���� ��ư
    [SerializeField] private Button closeAutoMergePanelButton;      // �ڵ� ���� �г� �ݱ� ��ư
    [SerializeField] private Button autoMergeStateButton;           // �ڵ� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMergeCostText;     // �ڵ� ���� ��� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI autoMergeTimerText;    // �ڵ� ���� Ÿ�̸� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI explainText;           // �ڵ� ���� ���� �ؽ�Ʈ

    [Header("---[Auto Merge Settings]")]
    private const float MAX_AUTO_MERGE_DURATION = 86400f;                       // �ִ� �ڵ� ���� �ð� (24�ð�)
    private const float MOVE_DURATION = 0.3f;                                   // ����̰� �̵��ϴ� �� �ɸ��� �ð� (�̵� �ӵ�)
    private const float AUTO_MERGE_DURATION = 10.0f;                            // �ڵ� ���� �⺻ ���� �ð�
    private const int AUTO_MERGE_COST = 30;                                     // �ڵ� ���� ���
    private WaitForSeconds waitAutoMergeInterval = new WaitForSeconds(0.5f);    // �ڵ� ���� ����
    private WaitForSeconds waitSpawnInterval = new WaitForSeconds(0.1f);        // ��ȯ ����

    private float startTime;                        // �ڵ� ���� ���� �ð�
    private float currentAutoMergeDuration;         // ���� �ڵ� ���� ���� �ð�
    private bool isAutoMergeActive = false;         // �ڵ� ���� Ȱ��ȭ ����
    private bool isPaused = false;                  // �Ͻ����� ����
    private float pausedTimeRemaining = 0f;         // �Ͻ����� ������ ���� �ð�
    private Coroutine autoMergeCoroutine;           // �ڵ� ���� �ڷ�ƾ

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
        openAutoMergePanelButton?.onClick.AddListener(OpenAutoMergePanel);
        closeAutoMergePanelButton?.onClick.AddListener(CloseAutoMergePanel);
        autoMergeStateButton?.onClick.AddListener(StartAutoMerge);
    }

    #endregion
    

    #region Auto Merge System

    // �ڵ����� ���� �Լ�
    public void StartAutoMerge()
    {
        if (GameManager.Instance.Cash < AUTO_MERGE_COST)
        {
            NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!!");
            return;
        }

        if (currentAutoMergeDuration + AUTO_MERGE_DURATION > MAX_AUTO_MERGE_DURATION)
        {
            NotificationManager.Instance.ShowNotification("�ڵ������� �ִ� 24�ð����� �����մϴ�!!");
            return;
        }

        GameManager.Instance.Cash -= AUTO_MERGE_COST;
        OnClickedAutoMerge();

        SaveToLocal();
    }

    // �ڵ����� ��ư Ŭ�� ó�� �Լ�
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

    // �ڵ����� �ڷ�ƾ
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

            // ���� ���� ��޺��� ���������� ���� �õ�
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

    // �������� ����� ���� �Լ�
    private void CleanupMergingCats()
    {
        mergingCats.RemoveWhere(cat => cat == null || !cat.gameObject.activeSelf || cat.isDragging);
    }

    // ��ȿ�� �������� Ȯ���ϴ� �Լ�
    private bool IsValidMergePair(DragAndDropManager cat1, DragAndDropManager cat2)
    {
        return cat1 != null && cat2 != null &&
               cat1.gameObject.activeSelf && cat2.gameObject.activeSelf &&
               !cat1.isDragging && !cat2.isDragging &&
               !IsMaxLevelCat(cat1.catData) && !IsMaxLevelCat(cat2.catData) &&
               cat1.catData.CatGrade == cat2.catData.CatGrade;
    }

    // �ڵ����� ������ġ �������� �Լ�
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

    // ���� ���� �ڷ�ƾ
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

    // ���� �Ϸ� ó�� �Լ�
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

    // �ڵ����� �� ����� ��ȯ�ϴ� �ڷ�ƾ
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

    // �ڵ����� ���� �Լ�
    private void EndAutoMerge()
    {
        isAutoMergeActive = false;
        isPaused = false;
        pausedTimeRemaining = 0f;
        UpdateAutoMergeTimerVisibility(false);
        StopAllCoroutines();
        mergingCats.Clear();

        SaveToLocal();
    }

    // �������� ����� ���� �Լ�
    public void StopMerging(DragAndDropManager cat)
    {
        mergingCats.Remove(cat);
    }

    // ����̰� ���������� Ȯ���ϴ� �Լ�
    public bool IsMerging(DragAndDropManager cat)
    {
        return mergingCats.Contains(cat);
    }

    #endregion


    #region UI System

    // �ڵ����� ��� Text ������Ʈ �Լ�
    private void UpdateAutoMergeCostText()
    {
        if (autoMergeCostText != null)
        {
            autoMergeCostText.text = AUTO_MERGE_COST.ToString();
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

    // �ڵ����� ���¿� ���� Ÿ�̸� �ؽ�Ʈ ���ü� ������Ʈ �Լ�
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

    // �ڵ����� Ÿ�̸� ������Ʈ �Լ�
    public void UpdateAutoMergeTimerText(int remainingTime)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.text = $"{remainingTime}��";
        }
    }

    // �ڵ����� ���� �ؽ�Ʈ ������Ʈ �Լ�
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

    // �ڵ� ���� �Ͻ����� �Լ�
    public void PauseAutoMerge()
    {
        if (isAutoMergeActive && !isPaused)
        {
            isPaused = true;
            pausedTimeRemaining = currentAutoMergeDuration - (Time.time - startTime);
            StopAllCoroutines();
            mergingCats.Clear();
            DisableAutoMergeUI();

            SaveToLocal();
        }
        else
        {
            openAutoMergePanelButton.interactable = false;
        }
    }

    // �ڵ� ���� �̾��ϱ� �Լ�
    public void ResumeAutoMerge()
    {
        if (isPaused && pausedTimeRemaining > 0)
        {
            isPaused = false;
            startTime = Time.time;
            currentAutoMergeDuration = pausedTimeRemaining;
            EnableAutoMergeUI();
            StartAutoMergeCoroutine();

            SaveToLocal();
        }
        else
        {
            EnableAutoMergeUI();
        }
    }

    // ���� ���۽� �ڵ����� UI ��Ȱ��ȭ �Լ�
    private void DisableAutoMergeUI()
    {
        openAutoMergePanelButton.interactable = false;
        if (autoMergePanel.activeSelf)
        {
            autoMergePanel.SetActive(false);
        }
    }

    // ���� ����� �ڵ����� UI Ȱ��ȭ �Լ�
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
            // ���� �ð��� ������ �ڵ� ���� ����
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
            // ���� �ð��� ������ �ڵ� ���� ��Ȱ��ȭ
            this.isAutoMergeActive = false;
            this.isPaused = false;
            this.currentAutoMergeDuration = 0f;
            UpdateAutoMergeTimerVisibility(false);
            UpdateTimerDisplay(0);
        }

        isDataLoaded = true;
    }

    private void SaveToLocal()
    {
        string data = GetSaveData();
        string key = this.GetType().FullName;
        GoogleManager.Instance?.SaveToPlayerPrefs(key, data);
    }

    #endregion


}

