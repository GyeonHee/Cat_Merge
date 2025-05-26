using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// ����� ���� ���� ��ũ��Ʈ
public class SortManager : MonoBehaviour
{


    #region Variables

    [Header("---[Sort System]")]
    [SerializeField] private Button sortButton;                     // ���� ��ư
    [SerializeField] private Transform gamePanel;                   // GamePanel

    private const float MOVEMENT_DELAY = 0.1f;                      // ���� ���� �� ���ð�
    private const float SORT_COMPLETE_DELAY = 0.2f;                 // ���� �Ϸ� �� ���ð�
    private const float MOVE_DURATION = 0.1f;                       // �� ������� �̵� �ð�
    private const int CATS_PER_ROW = 7;                             // �� �ٿ� ��ġ�� ����� ��
    private const float SPACING = 10f;                              // ����� �� ����

    private Dictionary<int, List<GameObject>> catsByGrade = new Dictionary<int, List<GameObject>>();  // ��޺� ����� ����
    private bool isSorting = false;                                 // ���� ���� �� ����

    [Header("---[ETC]")]
    private readonly WaitForSeconds waitForMovementDelay = new WaitForSeconds(MOVEMENT_DELAY);
    private readonly WaitForSeconds waitForSortCompleteDelay = new WaitForSeconds(SORT_COMPLETE_DELAY);

    #endregion


    #region Unity Methods

    private void Start()
    {
        InitializeButtonListeners();
    }

    #endregion


    #region Initialize

    // ��ư ������ �ʱ�ȭ �Լ�
    private void InitializeButtonListeners()
    {
        sortButton.onClick.AddListener(SortCats);
    }

    #endregion


    #region Sort System

    // ����� ���� ���� �Լ�
    private void SortCats()
    {
        if (isSorting) return;  // �̹� ���� ���̸� ����

        StartCoroutine(SortCatsCoroutine());
    }

    // ����̵��� ���������� ��ġ�ϴ� �ڷ�ƾ
    private IEnumerator SortCatsCoroutine()
    {
        isSorting = true;

        // ��� ����� �̵� ����
        StopAllCatMovements();
        yield return waitForMovementDelay;

        // ��� ����� ���� �ʱ�ȭ (���� ����)
        ResetAllCatDirections();

        // ��޼� ���� �� ��ġ �̵�
        var sortedCats = GetSortedCats();

        // ������ �ʿ����� Ȯ��
        if (IsSortingNeeded(sortedCats))
        {
            yield return StartCoroutine(MoveCatsToPositions(sortedCats));
            yield return waitForSortCompleteDelay;
        }

        RestoreAutoMoveState();
        isSorting = false;
    }

    // ������ �ʿ����� Ȯ���ϴ� �Լ�
    private bool IsSortingNeeded(List<GameObject> sortedCats)
    {
        if (sortedCats.Count == 0) return false;

        // ���� ��ġ�� ��ǥ ��ġ�� ���Ͽ� ���� �ʿ伺 Ȯ��
        for (int i = 0; i < sortedCats.Count; i++)
        {
            RectTransform rectTransform = sortedCats[i].GetComponent<RectTransform>();
            Vector2 targetPosition = CalculateTargetPosition(i, rectTransform);

            // ���� ��ġ�� ��ǥ ��ġ�� ���̰� ���� �Ÿ� �̻��̸� ���� �ʿ�
            if (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 1f)
            {
                return true;
            }
        }

        return false;
    }

    // ���ĵ� ����̵��� ���������� �̵���Ű�� �ڷ�ƾ
    private IEnumerator MoveCatsToPositions(List<GameObject> sortedCats)
    {
        var moveCoroutines = new List<Coroutine>();
        for (int i = 0; i < sortedCats.Count; i++)
        {
            moveCoroutines.Add(StartCoroutine(MoveCatToPosition(sortedCats[i], i)));
        }

        foreach (var coroutine in moveCoroutines)
        {
            yield return coroutine;
        }
    }

    // ������� ����� ��ȯ�ϴ� �Լ�
    private int GetCatGrade(GameObject catObject)
    {
        return catObject.GetComponent<CatData>().catData.CatGrade;
    }

    // ���� ����� �̵� ó�� �Լ�(����̵��� �ε巴�� ���ĵ� ��ġ�� �̵���Ű�� �Լ�)
    private IEnumerator MoveCatToPosition(GameObject catObject, int index)
    {
        // ���� ��ġ�� ��ǥ ��ġ�� ���̸� ����Ͽ� �ε巴�� �̵�
        RectTransform rectTransform = catObject.GetComponent<RectTransform>();
        Vector2 targetPosition = CalculateTargetPosition(index, rectTransform);
        Vector2 initialPosition = rectTransform.anchoredPosition;

        float elapsedTime = 0f;
        while (elapsedTime < MOVE_DURATION)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, elapsedTime / MOVE_DURATION);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �̵��� ������ �� ��Ȯ�� ��ǥ ��ġ�� ����
        rectTransform.anchoredPosition = targetPosition;
    }

    // ���� ��ġ ��� �Լ�
    private Vector2 CalculateTargetPosition(int index, RectTransform rectTransform)
    {
        float targetX = (index % CATS_PER_ROW - 3) * (rectTransform.rect.width + SPACING);
        float targetY = (index / CATS_PER_ROW - 3) * (rectTransform.rect.height + SPACING);
        return new Vector2(targetX, -targetY);
    }

    // �ձ޼����� �����Ͽ� ����Ʈ�� ��ȯ�ϴ� �Լ�
    private List<GameObject> GetSortedCats()
    {
        List<GameObject> sortedCats = new List<GameObject>();
        catsByGrade.Clear();

        foreach (Transform child in gamePanel)
        {
            if (!child.gameObject.activeSelf) continue;

            // �ڵ� ���� ���� ����̴� ����
            DragAndDropManager dragManager = child.GetComponent<DragAndDropManager>();
            if (dragManager != null && AutoMergeManager.Instance != null &&
                !AutoMergeManager.Instance.IsMerging(dragManager))
            {
                int grade = GetCatGrade(child.gameObject);

                if (!catsByGrade.ContainsKey(grade))
                {
                    catsByGrade[grade] = new List<GameObject>();
                }
                catsByGrade[grade].Add(child.gameObject);
            }
        }

        // ��޺��� �����ϵ�, ���� ��� �������� ���� ���� ����
        foreach (var grade in catsByGrade.Keys.OrderByDescending(k => k))
        {
            sortedCats.AddRange(catsByGrade[grade]);
        }

        return sortedCats;
    }

    // ��� ������� �ڵ��̵� ���¸� �����ϴ� �Լ�
    private void SetAutoMoveState(bool isEnabled)
    {
        foreach (Transform child in gamePanel)
        {
            if (!child.gameObject.activeSelf) continue;

            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(isEnabled);
            }
        }
    }

    // ��� ������� ������ �ʱ�ȭ�ϴ� �Լ� (������ ������)
    private void ResetAllCatDirections()
    {
        foreach (Transform child in gamePanel)
        {
            if (!child.gameObject.activeSelf) continue;

            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                // Transform ���� "Cat Image" �Ǵ� "Image" ������Ʈ ã��
                Transform catImageTransform = child.Find("Cat Image");
                if (catImageTransform == null)
                {
                    catImageTransform = child.Find("Image");
                }

                // ã�� ������Ʈ�� ������ �ʱ�ȭ (Y ȸ���� 0����)
                if (catImageTransform != null)
                {
                    catImageTransform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }
    }

    #endregion


    #region Move Control

    // ��� ������� �̵��� ������Ű�� �Լ�
    private void StopAllCatMovements()
    {
        AutoMoveManager.Instance.StopAllCatsMovement();
        StopCatMovementsInPanel();
    }

    // ���� �г� �� ��� ������� �̵��� �����ϴ� �Լ�
    private void StopCatMovementsInPanel()
    {
        foreach (Transform child in gamePanel)
        {
            if (!child.gameObject.activeSelf) continue;

            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.StopAllMovement();
                catData.SetAutoMoveState(false);
            }
        }
    }

    // �ڵ� �̵� ���¸� �����ϴ� �Լ�
    private void RestoreAutoMoveState()
    {
        SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
    }

    #endregion


    #region Battle System

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleSortState()
    {
        SetSortButtonState(false);
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleSortState()
    {
        SetSortButtonState(true);
    }

    // ���� ��ư�� ���¸� �����ϴ� �Լ�
    private void SetSortButtonState(bool state)
    {
        sortButton.interactable = state;
    }

    #endregion


}
