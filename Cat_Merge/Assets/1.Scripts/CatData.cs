using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ����� ���ε��� ����
public class CatData : MonoBehaviour
{
    private Cat catData;                        // ����� ������
    private Image catImage;                     // ����� �̹���

    private RectTransform rectTransform;        // RectTransform ����
    private bool isAnimating = false;           // �ִϸ��̼� ������ Ȯ�� �÷���

    private void Awake()
    {
        catImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateCatUI();
        StartCoroutine(PeriodicMovement());
    }

    // CatUI �ֽ�ȭ
    public void UpdateCatUI()
    {
        CatDragAndDrop catDragAndDrop = GetComponentInParent<CatDragAndDrop>();

        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = catData;
        }
        catImage.sprite = catData.CatImage;
    }

    // Cat ������ ����
    public void SetCatData(Cat cat)
    {
        catData = cat;
        UpdateCatUI();
    }

    // 10�ʸ��� �ڵ����� �̵��ϴ� �ڷ�ƾ
    private IEnumerator PeriodicMovement()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            if (!isAnimating)
            {
                Vector3 randomDirection = GetRandomDirection();
                Vector3 targetPosition = (Vector3)rectTransform.anchoredPosition + randomDirection;
                StartCoroutine(SmoothMoveToPosition(targetPosition));
            }
        }
    }

    // ���� ���� ���
    private Vector3 GetRandomDirection()
    {
        float moveRange = 30f;

        // 8���� (��, ��, ��, ��, �밢�� ����)
        Vector2[] directions = new Vector2[]
        {
            new Vector2(moveRange, 0f),
            new Vector2(-moveRange, 0f),
            new Vector2(0f, moveRange),
            new Vector2(0f, -moveRange),
            new Vector2(moveRange, moveRange),
            new Vector2(moveRange, -moveRange),
            new Vector2(-moveRange, moveRange),
            new Vector2(-moveRange, -moveRange)
        };

        // �������� ���� ����
        int randomIndex = Random.Range(0, directions.Length);
        return directions[randomIndex];
    }

    // �ε巴�� �̵��ϴ� �ڷ�ƾ
    private IEnumerator SmoothMoveToPosition(Vector3 targetPosition)
    {
        isAnimating = true;

        Vector3 startPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        isAnimating = false;
    }

}
