using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ��ü�� �������ִ� ������� ������ ��� Script
public class CatData : MonoBehaviour
{
    public Cat catData;                         // ����� ������
    private Image catImage;                     // ����� �̹���
    
    private RectTransform rectTransform;        // RectTransform ����
    private RectTransform parentPanel;          // �θ� �г� RectTransform
    private DragAndDropManager catDragAndDrop;  // DragAndDropManager ����
    
    private bool isAnimating = false;           // �ִϸ��̼� ������ Ȯ�� �÷���
    private bool isAutoMoveEnabled = true;      // �ڵ� �̵� Ȱ��ȭ ����
    private Coroutine autoMoveCoroutine;        // �ڵ� �̵� �ڷ�ƾ

    private TextMeshProUGUI collectCoinText;    // �ڵ� ��ȭ ȹ�� �ؽ�Ʈ
    private Image collectCoinImage;             // �ڵ� ��ȭ ȹ�� �̹���
    //private Animator catAnimator;               // �ڵ� ��ȭ ȹ�� Animator ������Ʈ ����

    private float collectingTime = 2f;          // �ڵ� ��ȭ ���� �ð�
    public float CollectingTime { get => collectingTime; set => collectingTime = value; }
    private bool isCollectingCoins = true;      // �ڵ� ��ȭ ���� Ȱ��ȭ ����

    public int catHp;                           // ����� ü��

    // ======================================================================================================================

    private void Awake()
    {
        catImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentPanel = rectTransform.parent.GetComponent<RectTransform>();
        catDragAndDrop = GetComponentInParent<DragAndDropManager>();

        collectCoinText = transform.Find("CollectCoinText").GetComponent<TextMeshProUGUI>();
        collectCoinImage = transform.Find("CollectCoinImage").GetComponent<Image>();
        //catAnimator = GetComponent<Animator>();

        collectCoinText.gameObject.SetActive(false);
        collectCoinImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        UpdateCatUI();
        StartCoroutine(AutoCollectCoins());
    }

    // ======================================================================================================================

    // CatUI �ֽ�ȭ�ϴ� �Լ�
    public void UpdateCatUI()
    {
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = catData;
        }
        catImage.sprite = catData.CatImage;
    }

    // Cat ������ ���� �Լ�
    public void SetCatData(Cat cat)
    {
        catData = cat;
        catHp = catData.CatHp;
        UpdateCatUI();
    }

    // ======================================================================================================================
    // [���� ����]

    // �ڵ��̵� ���� ���� ��ȯ�ϴ� �Լ�
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

    // ���� ��Ʈ�ڽ� ��� ���� �����ϴ� ����̸� ��Ʈ�ڽ� ���� �̵� (���� ������ ��Ʈ�ڽ� ���ο� �ִ� ����̵��� ��Ʈ�ڽ� ���� �з����� ����)
    public void MoveOppositeBoss(Vector3 bossPosition, Vector2 bossSize)
    {
        Vector3 catPosition = rectTransform.anchoredPosition;
        Vector3 offset = catPosition - bossPosition;

        float halfWidth = bossSize.x / 2f;
        float halfHeight = bossSize.y / 2f;

        // ��Ʈ�ڽ��� �ܰ� ���� �󿡼� ���� ����� x, y ��ǥ�� ���
        float closestX = Mathf.Clamp(catPosition.x, bossPosition.x - halfWidth, bossPosition.x + halfWidth);
        float closestY = Mathf.Clamp(catPosition.y, bossPosition.y - halfHeight, bossPosition.y + halfHeight);

        // x ����� y ���� �� � ���� �� ������� �Ǵ��Ͽ� �̵� ��ġ ����
        float distanceToVerticalEdge = Mathf.Min(Mathf.Abs(catPosition.x - (bossPosition.x - halfWidth)), Mathf.Abs(catPosition.x - (bossPosition.x + halfWidth)));
        float distanceToHorizontalEdge = Mathf.Min(Mathf.Abs(catPosition.y - (bossPosition.y - halfHeight)), Mathf.Abs(catPosition.y - (bossPosition.y + halfHeight)));

        if (distanceToVerticalEdge < distanceToHorizontalEdge)
        {
            // x ���� ���� ����� �ܰ����� �̵�
            closestX = offset.x < 0 ? bossPosition.x - halfWidth : bossPosition.x + halfWidth;
        }
        else
        {
            // y ���� ���� ����� �ܰ����� �̵�
            closestY = offset.y < 0 ? bossPosition.y - halfHeight : bossPosition.y + halfHeight;
        }

        Vector3 targetPosition = new Vector3(closestX, closestY, catPosition.z);
        StartCoroutine(SmoothMoveToPosition(targetPosition));
    }

    // ���� ��Ʈ�ڽ� ��� �ܿ� �����ϴ� ����̸� ��Ʈ�ڽ� ���� �̵� (���� ���� �Ϸ� �� ���� ���۽� ��Ʈ�ڽ� ��迡�� ������ �� �ֵ��� �̵�)
    public void MoveTowardBossBoundary(Vector3 bossPosition, Vector2 bossSize)
    {
        Vector3 catPosition = rectTransform.anchoredPosition;
        Vector3 offset = bossPosition - catPosition;

        float halfWidth = bossSize.x / 2f;
        float halfHeight = bossSize.y / 2f;

        // ��Ʈ�ڽ� �ܰ� ���� �󿡼� ���� ����� x, y ��ǥ ���
        float closestX = Mathf.Clamp(catPosition.x, bossPosition.x - halfWidth, bossPosition.x + halfWidth);
        float closestY = Mathf.Clamp(catPosition.y, bossPosition.y - halfHeight, bossPosition.y + halfHeight);

        // x ����� y ���� �� � ���� �� ������� �Ǵ��Ͽ� �̵� ��ġ ����
        float distanceToVerticalEdge = Mathf.Min(Mathf.Abs(catPosition.x - (bossPosition.x - halfWidth)), Mathf.Abs(catPosition.x - (bossPosition.x + halfWidth)));
        float distanceToHorizontalEdge = Mathf.Min(Mathf.Abs(catPosition.y - (bossPosition.y - halfHeight)), Mathf.Abs(catPosition.y - (bossPosition.y + halfHeight)));

        if (distanceToVerticalEdge < distanceToHorizontalEdge)
        {
            // x ���� ���� ����� �ܰ����� �̵�
            closestX = offset.x > 0 ? bossPosition.x - halfWidth : bossPosition.x + halfWidth;
        }
        else
        {
            // y ���� ���� ����� �ܰ����� �̵�
            closestY = offset.y > 0 ? bossPosition.y - halfHeight : bossPosition.y + halfHeight;
        }

        Vector3 targetPosition = new Vector3(closestX, closestY, catPosition.z);
        StartCoroutine(SmoothMoveToPosition(targetPosition));
    }

    // �������� ���ݴ������� ����Ǵ� �Լ�
    public void TakeDamage(int damage)
    {
        catHp -= damage;
        Debug.Log($"���� ü�� : {catHp}");

        // �ӽ÷� �ı� (��ȹ��θ� ���� �� ȸ��)
        if (catHp <= 0)
        {
            Destroy(gameObject);
        }
    }

    // ======================================================================================================================

    // �ڵ� �̵��� Ȱ��ȭ/��Ȱ��ȭ�ϴ� �Լ�
    public void SetAutoMoveState(bool isEnabled)
    {
        isAutoMoveEnabled = isEnabled;

        // �ڵ� �̵��� Ȱ��ȭ�Ϸ��� �ڷ�ƾ ����
        if (isAutoMoveEnabled)
        {
            if (!isAnimating && autoMoveCoroutine == null)
            {
                autoMoveCoroutine = StartCoroutine(AutoMove());
            }
        }
        else
        {
            // ��� �ڷ�ƾ �ߴ� -> �ڵ� �̵��� �ߴ�
            if (autoMoveCoroutine != null)
            {
                StopCoroutine(autoMoveCoroutine);
                autoMoveCoroutine = null;
            }
            isAnimating = false;
        }
    }

    // 10�ʸ��� �ڵ����� �̵��ϴ� �ڷ�ƾ
    public IEnumerator AutoMove()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            if (!isAnimating && (catDragAndDrop == null || !catDragAndDrop.isDragging))
            {
                Vector3 randomDirection = GetRandomDirection();
                Vector3 targetPosition = (Vector3)rectTransform.anchoredPosition + randomDirection;

                yield return StartCoroutine(SmoothMoveToPosition(targetPosition));

                // ��ġ�� ������ �ʰ������� �������� ����
                if (IsOutOfBounds(targetPosition))
                {
                    Vector3 adjustedPosition = AdjustPositionToBounds(targetPosition);
                    StartCoroutine(SmoothMoveToPosition(adjustedPosition));
                }
            }
        }
    }

    // ���� ���� ��� �Լ�
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

    // ���� �ʰ� ���θ� Ȯ���ϴ� �Լ�
    private bool IsOutOfBounds(Vector3 position)
    {
        Vector2 minBounds = (Vector2)parentPanel.rect.min + parentPanel.anchoredPosition;
        Vector2 maxBounds = (Vector2)parentPanel.rect.max + parentPanel.anchoredPosition;

        return position.x <= minBounds.x || position.x >= maxBounds.x || position.y <= minBounds.y || position.y >= maxBounds.y;
    }

    // �ʰ��� ��ġ�� �������� �����ϴ� �Լ�
    private Vector3 AdjustPositionToBounds(Vector3 position)
    {
        Vector3 adjustedPosition = position;

        Vector2 minBounds = (Vector2)parentPanel.rect.min + parentPanel.anchoredPosition;
        Vector2 maxBounds = (Vector2)parentPanel.rect.max + parentPanel.anchoredPosition;

        if (position.x <= minBounds.x) adjustedPosition.x = minBounds.x + 30f;
        if (position.x >= maxBounds.x) adjustedPosition.x = maxBounds.x - 30f;
        if (position.y <= minBounds.y) adjustedPosition.y = minBounds.y + 30f;
        if (position.y >= maxBounds.y) adjustedPosition.y = maxBounds.y - 30f;

        return adjustedPosition;
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

    // ����̰� �ı��ɶ� ����� �� ���ҽ�Ű�� �Լ�
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DeleteCatCount();
        }
    }

    // ======================================================================================================================

    // �ڵ� ��ȭ ���� �ڷ�ƾ
    private IEnumerator AutoCollectCoins()
    {
        while (isCollectingCoins)
        {
            //yield return new WaitForSeconds(collectingTime);

            // ��ȭ ȹ�� �ð�(������ ���� ����)
            yield return new WaitForSeconds(ItemFunctionManager.Instance.reduceCollectingTimeList[ItemMenuManager.Instance.ReduceCollectingTimeLv].value); 

            if (catData != null && GameManager.Instance != null)
            {
                int collectedCoins = catData.CatGetCoin;
                GameManager.Instance.Coin += collectedCoins;
                QuestManager.Instance.AddGetCoinCount(collectedCoins);

                // ��� ���� �� ��ȭ ȹ�� �ؽ�Ʈ Ȱ��ȭ
                StartCoroutine(PlayCollectingAnimation(collectedCoins));
            }
        }
    }

    // ��ǰ� ��ȭ ȹ�� �ؽ�Ʈ ó�� �ڷ�ƾ
    private IEnumerator PlayCollectingAnimation(int collectedCoins)
    {
        //// �ִϸ��̼� ���� (���� ���� ����)
        //if (catAnimator != null)
        //{
        //    catAnimator.SetTrigger("CollectCoin");      // CollectCoin Ʈ���� ����
        //}

        collectCoinText.text = $"+{collectedCoins}";
        collectCoinText.gameObject.SetActive(true);
        collectCoinImage.gameObject.SetActive(true);

        // �ִϸ��̼� ��� (�ִϸ��̼� ���� ������ ���߱�)
        float animationDuration = 0.5f;     // �ִϸ��̼� ���� (Animator ������ ���� ����)
        yield return new WaitForSeconds(animationDuration);

        collectCoinText.gameObject.SetActive(false);
        collectCoinImage.gameObject.SetActive(false);
    }

}
