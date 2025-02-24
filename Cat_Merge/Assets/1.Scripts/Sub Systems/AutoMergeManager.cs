using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

// �ڵ����� ���� ��ũ��Ʈ
public class AutoMergeManager : MonoBehaviour
{
    #region Variables
    public static AutoMergeManager Instance { get; private set; }

    [Header("---[AutoMerge System]")]
    [SerializeField] private Button openAutoMergePanelButton;       // �ڵ� ���� �г� ���� ��ư
    [SerializeField] private GameObject autoMergePanel;             // �ڵ� ���� �г�
    [SerializeField] private Button closeAutoMergePanelButton;      // �ڵ� ���� �г� �ݱ� ��ư
    [SerializeField] private Button autoMergeStateButton;           // �ڵ� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMergeCostText;     // �ڵ� ���� ���� ��ư
    [SerializeField] private TextMeshProUGUI autoMergeTimerText;    // �ڵ� ���� Ÿ�̸� �ؽ�Ʈ
    private int autoMergeCost = 30;

    [Header("---[???]")]
    private float startTime;                                // �ڵ� ���� ���� �ð�
    private float autoMergeDuration = 10.0f;                // �ڵ� ���� �⺻ ���� �ð�
    private float currentAutoMergeDuration;                 // ���� �ڵ� ���� ���� �ð�
    private float plusAutoMergeDuration;                    // �ڵ� ���� �߰� �ð�
    private float autoMergeInterval = 0.5f;                 // �ڵ� ���� ����
    private float moveDuration = 0.2f;                      // ����̰� �̵��ϴ� �� �ɸ��� �ð� (�̵� �ӵ�)
    private bool isAutoMergeActive = false;                 // �ڵ� ���� Ȱ��ȭ ����

    private HashSet<DragAndDropManager> mergingCats;        // ���� ���� ����� ����

    [Header("---[Battle System]")]
    private bool isPaused = false;                          // �Ͻ����� ����
    private float pausedTimeRemaining = 0f;                 // �Ͻ����� ������ ���� �ð�
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

        InitializeAutoMergeManager();
    }

    private void Start()
    {
        mergingCats = new HashSet<DragAndDropManager>();
    }

    private void Update()
    {
        if (isAutoMergeActive && !isPaused)
        {
            // ���� �ð� ���
            float remainingTime = currentAutoMergeDuration - (Time.time - startTime);
            remainingTime = Mathf.Max(remainingTime, 0);

            // Ÿ�̸� ������Ʈ
            UpdateAutoMergeTimerText((int)remainingTime);

            // �ڵ� ���� ���� ó��
            if (remainingTime <= 0)
            {
                isAutoMergeActive = false;
                isPaused = false;
                pausedTimeRemaining = 0f;
                UpdateAutoMergeTimerVisibility(false);
            }
        }
        else if (isPaused)
        {
            // �Ͻ����� ������ ���� ���� �ð��� ������ ������ ǥ��
            UpdateAutoMergeTimerText((int)pausedTimeRemaining);
        }
    }
    #endregion

    // ======================================================================================================================

    #region Initialize
    // AutoMergeManager �ʱ� ����
    private void InitializeAutoMergeManager()
    {
        plusAutoMergeDuration = autoMergeDuration;
        currentAutoMergeDuration = autoMergeDuration;

        UpdateAutoMergeCostText();
        UpdateAutoMergeTimerVisibility(false);

        openAutoMergePanelButton.onClick.AddListener(OpenAutoMergePanel);
        closeAutoMergePanelButton.onClick.AddListener(CloseAutoMergePanel);
        autoMergeStateButton.onClick.AddListener(StartAutoMerge);
    }
    #endregion

    // ======================================================================================================================

    #region UI System
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

    // �ڵ� ���� ���� �Լ� (��ȭ �Ǻ�)
    private void StartAutoMerge()
    {
        if (GameManager.Instance.Cash >= autoMergeCost)
        {
            GameManager.Instance.Cash -= autoMergeCost;

            AutoMergeManager autoMergeScript = FindObjectOfType<AutoMergeManager>();
            if (autoMergeScript != null)
            {
                autoMergeScript.OnClickedAutoMerge();
            }
        }
        else
        {
            //Debug.Log("Not enough coins");
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
    #endregion

    // ======================================================================================================================

    #region Auto Merge System
    // �ڵ� ���� ���� �Լ�
    public void OnClickedAutoMerge()
    {
        if (!isAutoMergeActive)
        {
            Debug.Log("�ڵ� ���� ����");
            startTime = Time.time;
            isAutoMergeActive = true;
            currentAutoMergeDuration = autoMergeDuration;
            UpdateAutoMergeTimerVisibility(true);
            StartCoroutine(AutoMergeCoroutine());
        }
        else
        {
            Debug.Log($"{plusAutoMergeDuration}�� �߰�");
            currentAutoMergeDuration += plusAutoMergeDuration;
        }
    }

    // �ڵ� ���� ������ Ȯ���ϴ� �Լ�
    public bool IsMerging(DragAndDropManager cat)
    {
        return mergingCats.Contains(cat);
    }

    // �ڵ� ���� �ڷ�ƾ
    private IEnumerator AutoMergeCoroutine()
    {
        // �ִ� ����� ���� ���� ����
        StartCoroutine(SpawnCatsWhileAutoMerge());

        while (Time.time - startTime - currentAutoMergeDuration < 0)
        {
            // mergingCats ���� ����
            mergingCats.RemoveWhere(cat => cat == null || cat.isDragging);

            var allCats = FindObjectsOfType<DragAndDropManager>().OrderBy(cat => cat.catData.CatGrade).ToList();
            var groupedCats = allCats.GroupBy(cat => cat.catData.CatGrade).Where(group => group.Count() > 1).ToList();

            if (!groupedCats.Any())
            {
                yield return new WaitForSeconds(autoMergeInterval);
                continue;
            }

            foreach (var group in groupedCats)
            {
                var catsInGroup = group.ToList();

                while (catsInGroup.Count >= 2 && (Time.time - startTime - currentAutoMergeDuration < 0))
                {
                    // �� �ݺ����� cat1�� cat2�� ������ �����ϴ��� Ȯ��
                    if (catsInGroup.Count < 2)
                    {
                        break;
                    }

                    var cat1 = catsInGroup[0];
                    var cat2 = catsInGroup[1];

                    // ����̰� �̹� �ı��Ǿ��ų� �巡�� ������ Ȯ��
                    if (cat1 == null || cat2 == null || cat1.isDragging || cat2.isDragging)
                    {
                        // ������ ����� ����
                        if (cat1 == null || cat1.isDragging) catsInGroup.Remove(cat1);
                        if (cat2 == null || cat2.isDragging) catsInGroup.Remove(cat2);
                        mergingCats.Remove(cat1);
                        mergingCats.Remove(cat2);
                        continue;
                    }

                    // �ְ� ��� ��������� Ȯ��
                    if (IsMaxLevelCat(cat1.catData) || IsMaxLevelCat(cat2.catData))
                    {
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }

                    mergingCats.Add(cat1);
                    mergingCats.Add(cat2);

                    RectTransform parentRect = cat1.rectTransform?.parent?.GetComponent<RectTransform>();
                    if (parentRect == null)
                    {
                        mergingCats.Remove(cat1);
                        mergingCats.Remove(cat2);
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }

                    Vector2 mergePosition = GetRandomPosition(parentRect);

                    // �̵� �ڷ�ƾ ���� �� �ٽ� �ѹ� Ȯ��
                    if (cat1 != null && cat2 != null && !cat1.isDragging && !cat2.isDragging)
                    {
                        StartCoroutine(MoveCatSmoothly(cat1, mergePosition));
                        StartCoroutine(MoveCatSmoothly(cat2, mergePosition));

                        yield return new WaitUntil(() =>
                            cat1 == null || cat2 == null ||
                            (!mergingCats.Contains(cat1) && !mergingCats.Contains(cat2)) ||
                            (cat1.rectTransform.anchoredPosition == mergePosition && cat2.rectTransform.anchoredPosition == mergePosition)
                        );

                        // ���������� �ѹ� �� Ȯ�� �� ���� ����
                        if (cat1 != null && cat2 != null &&
                            mergingCats.Contains(cat1) && mergingCats.Contains(cat2) &&
                            !cat1.isDragging && !cat2.isDragging)
                        {
                            Cat mergedCat = FindObjectOfType<MergeManager>().MergeCats(cat1.catData, cat2.catData);
                            if (mergedCat != null)
                            {
                                cat1.catData = mergedCat;
                                cat1.UpdateCatUI();
                                Destroy(cat2.gameObject);
                            }
                        }
                    }

                    mergingCats.Remove(cat1);
                    mergingCats.Remove(cat2);
                    catsInGroup.Remove(cat1);
                    catsInGroup.Remove(cat2);

                    yield return new WaitForSeconds(autoMergeInterval);
                }

                if (Time.time - startTime - currentAutoMergeDuration >= 0) break;
            }

            yield return null;
        }

        isAutoMergeActive = false;
        UpdateAutoMergeTimerVisibility(false);
        Debug.Log("�ڵ� ���� ����");
    }

    // �ڵ������� ����� �ִ�ġ�� ��ȯ�ϴ� �Լ�
    private IEnumerator SpawnCatsWhileAutoMerge()
    {
        SpawnManager catSpawn = FindObjectOfType<SpawnManager>();
        while (isAutoMergeActive)
        {
            while (GameManager.Instance.CanSpawnCat())
            {
                catSpawn.SpawnCat();
                yield return new WaitForSeconds(0.1f); // ����� �ڵ� ���� ����
            }

            yield return null;
        }
    }

    // �θ� RectTransform���� ���� ��ġ ��� �Լ�
    private Vector2 GetRandomPosition(RectTransform parentRect)
    {
        float panelWidth = parentRect.rect.width;
        float panelHeight = parentRect.rect.height;
        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);
        return new Vector2(randomX, randomY);
    }

    // ����̰� �ε巴�� �̵��ϴ� �ڷ�ƾ
    private IEnumerator MoveCatSmoothly(DragAndDropManager cat, Vector2 targetPosition)
    {
        if (cat == null) yield break;

        Vector2 startPos = cat.rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            if (cat == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            cat.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
            yield return null;
        }
    }

    // �ִ� ���� ����� Ȯ�� �Լ�
    private bool IsMaxLevelCat(Cat catData)
    {
        if (GameManager.Instance == null || GameManager.Instance.AllCatData == null)
        {
            return false;
        }

        return GameManager.Instance.AllCatData.All(cat => cat.CatGrade != catData.CatGrade + 1);
    }

    // Ư�� ������� mergingCats ���� ���� �Լ�
    public void StopMerging(DragAndDropManager cat)
    {
        mergingCats.Remove(cat);
    }
    #endregion

    // ======================================================================================================================

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

            openAutoMergePanelButton.interactable = false;
            if (autoMergePanel.activeSelf == true)
            {
                autoMergePanel.SetActive(false);
            }
        }
        else
        {
            openAutoMergePanelButton.interactable = false;
        }

        DisableAutoMergeUI();
    }

    // �ڵ� ���� �̾��ϱ� �Լ�
    public void ResumeAutoMerge()
    {
        if (isPaused && pausedTimeRemaining > 0)
        {
            isPaused = false;
            startTime = Time.time;
            currentAutoMergeDuration = pausedTimeRemaining;
            openAutoMergePanelButton.interactable = true;
            StartCoroutine(AutoMergeCoroutine());
        }
        else
        {
            openAutoMergePanelButton.interactable = true;
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
    #endregion

    // ======================================================================================================================


}
