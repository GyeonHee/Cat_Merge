using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortManager : MonoBehaviour
{
    [Header("---[Sort System]")]
    [SerializeField] private Button sortButton;                     // ���� ��ư
    [SerializeField] private Transform gamePanel;                   // GamePanel

    // ======================================================================================================================

    private void Awake()
    {
        sortButton.onClick.AddListener(SortCats);
    }

    // ======================================================================================================================

    // ����� ���� �Լ�
    private void SortCats()
    {
        StartCoroutine(SortCatsCoroutine());
    }

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleSortState()
    {
        sortButton.interactable = false;
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleSortState()
    {
        sortButton.interactable = true;
    }

    // ����̵��� ���ĵ� ��ġ�� ��ġ�ϴ� �ڷ�ƾ
    private IEnumerator SortCatsCoroutine()
    {
        // ���� �� �ڵ� �̵� ��� ���� (���� �ڵ��̵��� �������� ����̵� ���� = CatData�� AutoMove�� �������̾ ����)
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(false); // �ڵ� �̵� ��Ȱ��ȭ
            }
        }

        // ����� ��ü���� ����� �������� ���� (���� ����� ���� ������)
        List<GameObject> sortedCats = new List<GameObject>();
        foreach (Transform child in gamePanel)
        {
            sortedCats.Add(child.gameObject);
        }

        sortedCats.Sort((cat1, cat2) =>
        {
            int grade1 = GetCatGrade(cat1);
            int grade2 = GetCatGrade(cat2);

            if (grade1 == grade2) return 0;
            return grade1 > grade2 ? -1 : 1;
        });

        // ����� �̵� �ڷ�ƾ ����
        List<Coroutine> moveCoroutines = new List<Coroutine>();
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

        // ���� �� �ڵ� �̵� ���� ���� (������ �Ϸ�ǰ� ����̵��� �ٽ� �ֱ⸶�� �ڵ��̵��� �����ϰ� �������·� ����)
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
            }
        }
    }

    // ������� ����� ��ȯ�ϴ� �Լ�
    private int GetCatGrade(GameObject catObject)
    {
        int grade = catObject.GetComponent<CatData>().catData.CatGrade;
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
}
