using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        StartCoroutine(SortCatsCoroutine());
    }

    // ����̵��� ���������� ��ġ�ϴ� �ڷ�ƾ
    private IEnumerator SortCatsCoroutine()
    {
        // ��� ����� �̵� ����
        StopAllCatMovements();
        yield return new WaitForSeconds(MOVEMENT_DELAY);

        // ��޼� ���� �� ��ġ �̵�
        var sortedCats = GetSortedCats();
        yield return StartCoroutine(MoveCatsToPositions(sortedCats));

        // �Ϸ� ��� �� �ڵ��̵� ���� ����
        yield return new WaitForSeconds(SORT_COMPLETE_DELAY);
        RestoreAutoMoveState();
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
        foreach (Transform child in gamePanel)
        {
            if (!child.gameObject.activeSelf) continue;

            // �ڵ� ���� ���� ����̴� ����
            DragAndDropManager dragManager = child.GetComponent<DragAndDropManager>();
            if (dragManager != null && AutoMergeManager.Instance != null &&
                !AutoMergeManager.Instance.IsMerging(dragManager))
            {
                sortedCats.Add(child.gameObject);
            }
        }

        sortedCats.Sort((cat1, cat2) =>
        {
            int grade1 = GetCatGrade(cat1);
            int grade2 = GetCatGrade(cat2);
            return grade2.CompareTo(grade1);
        });

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
