using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ������� ������ �ൿ�� �����ϴ� ��ũ��Ʈ
public class CatData : MonoBehaviour
{
    #region Variables
    [Header("Cat Data")]
    public Cat catData;                             // ����� �⺻ ������
    private Image catImage;                         // ����� �̹��� ������Ʈ
    public int catHp;                               // ���� ����� ü��
    public bool isStuned = false;                   // ���� ���� ����
    private float stunTime = 10f;                   // ���� �ð�

    [Header("HP UI")]
    [SerializeField] private GameObject hpImage;    // HP �̹��� ������Ʈ
    [SerializeField] private Image hpFillImage;     // HP Fill �̹���

    [Header("Transform")]
    private RectTransform rectTransform;            // ���� ������Ʈ�� RectTransform
    private RectTransform parentPanel;              // �θ� �г��� RectTransform
    private DragAndDropManager catDragAndDrop;      // �巡�� �� ��� �Ŵ���

    [Header("Movement")]
    private bool isAnimating = false;               // �̵� �ִϸ��̼� ���� ����
    private Coroutine autoMoveCoroutine;            // �ڵ� �̵� �ڷ�ƾ
    private Coroutine currentMoveCoroutine;         // ���� ���� ���� �̵� �ڷ�ƾ

    [Header("Coin Collection")]
    private TextMeshProUGUI collectCoinText;        // ���� ȹ�� �ؽ�Ʈ
    private Image collectCoinImage;                 // ���� ȹ�� �̹���
    private float collectingTime;                   // ���� ���� ����
    public float CollectingTime { get => collectingTime; set => collectingTime = value; }
    private bool isCollectingCoins = true;          // ���� ���� Ȱ��ȭ ����
    private Coroutine autoCollectCoroutine;         // ���� ���� �ڷ�ƾ
    #endregion

    // ======================================================================================================================

    #region Unity Methods
    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        UpdateCatUI();
        autoCollectCoroutine = StartCoroutine(AutoCollectCoins());
        UpdateHPBar();
    }

    // ������Ʈ �ı��� ����� �� ����
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DeleteCatCount();
        }
    }
    #endregion

    // ======================================================================================================================

    #region Initialization
    // ������Ʈ �ʱ�ȭ
    private void InitializeComponents()
    {
        catImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentPanel = rectTransform.parent.GetComponent<RectTransform>();
        catDragAndDrop = GetComponentInParent<DragAndDropManager>();

        collectCoinText = transform.Find("CollectCoinText").GetComponent<TextMeshProUGUI>();
        collectCoinImage = transform.Find("CollectCoinImage").GetComponent<Image>();

        hpImage = transform.Find("HP Image").gameObject;
        hpFillImage = hpImage.transform.Find("Fill Image").GetComponent<Image>();
        hpImage.SetActive(false);

        collectCoinText.gameObject.SetActive(false);
        collectCoinImage.gameObject.SetActive(false);
    }

    // UI ������Ʈ
    public void UpdateCatUI()
    {
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = catData;
        }
        catImage.sprite = catData.CatImage;
    }

    // ����� ������ ����
    public void SetCatData(Cat cat)
    {
        catData = cat;
        catHp = catData.CatHp;
        UpdateCatUI();
        UpdateHPBar();
    }

    // HP �� ������Ʈ �Լ�
    private void UpdateHPBar()
    {
        float hpRatio = (float)catHp / catData.CatHp;
        hpFillImage.fillAmount = hpRatio;

        // �������̰�, ü���� �ִ�ġ�� �ƴ� �� HP �� ǥ��
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleActive && hpRatio < 1f && hpRatio > 0f)
        {
            hpImage.SetActive(true);
        }
        else
        {
            hpImage.SetActive(false);
        }
    }
    #endregion

    // ======================================================================================================================

    #region Battle System
    // ���� ��Ʈ�ڽ� ���� ����� �о��
    public void MoveOppositeBoss(Vector3 bossPosition, Vector2 bossSize)
    {
        Vector3 catPosition = rectTransform.anchoredPosition;
        Vector3 offset = catPosition - bossPosition;
        Vector3 targetPosition = CalculateBoundaryPosition(bossPosition, bossSize, offset, true);
        StartCoroutine(SmoothMoveToPosition(targetPosition));
    }

    // ���� ��Ʈ�ڽ� ���� ����� �̵�
    public void MoveTowardBossBoundary(Vector3 bossPosition, Vector2 bossSize)
    {
        Vector3 catPosition = rectTransform.anchoredPosition;
        Vector3 offset = bossPosition - catPosition;
        Vector3 targetPosition = CalculateBoundaryPosition(bossPosition, bossSize, offset, false);
        StartCoroutine(SmoothMoveToPosition(targetPosition));
    }

    // ��Ʈ�ڽ� ��� ��ġ ���
    private Vector3 CalculateBoundaryPosition(Vector3 bossPosition, Vector2 bossSize, Vector3 offset, bool isOpposite)
    {
        float halfWidth = bossSize.x / 2f;
        float halfHeight = bossSize.y / 2f;
        Vector3 catPosition = rectTransform.anchoredPosition;

        float closestX = Mathf.Clamp(catPosition.x, bossPosition.x - halfWidth, bossPosition.x + halfWidth);
        float closestY = Mathf.Clamp(catPosition.y, bossPosition.y - halfHeight, bossPosition.y + halfHeight);

        float distanceToVerticalEdge = Mathf.Min(Mathf.Abs(catPosition.x - (bossPosition.x - halfWidth)), Mathf.Abs(catPosition.x - (bossPosition.x + halfWidth)));
        float distanceToHorizontalEdge = Mathf.Min(Mathf.Abs(catPosition.y - (bossPosition.y - halfHeight)), Mathf.Abs(catPosition.y - (bossPosition.y + halfHeight)));

        if (distanceToVerticalEdge < distanceToHorizontalEdge)
        {
            closestX = isOpposite ?
                (offset.x < 0 ? bossPosition.x - halfWidth : bossPosition.x + halfWidth) :
                (offset.x > 0 ? bossPosition.x - halfWidth : bossPosition.x + halfWidth);
        }
        else
        {
            closestY = isOpposite ?
                (offset.y < 0 ? bossPosition.y - halfHeight : bossPosition.y + halfHeight) :
                (offset.y > 0 ? bossPosition.y - halfHeight : bossPosition.y + halfHeight);
        }

        return new Vector3(closestX, closestY, catPosition.z);
    }

    // ������ ó��
    public void TakeDamage(int damage)
    {
        catHp -= damage;
        UpdateHPBar();
        //Debug.Log($"���� ü�� : {catHp}");

        if (catHp <= 0)
        {
            StartCoroutine(StunAndRecover(stunTime));
        }
    }

    // ���� �� ȸ�� ó��
    private IEnumerator StunAndRecover(float stunTime)
    {
        SetStunState(true);
        yield return new WaitForSeconds(stunTime);
        SetStunState(false);
        //Debug.Log("����� ü�� ȸ�� �Ϸ�!");
    }

    // ���� ���� ����
    private void SetStunState(bool isStunned)
    {
        UpdateHPBar();
        SetCollectingCoinsState(!isStunned);
        SetAutoMoveState(!isStunned);
        SetRaycastTarget(!isStunned);
        isStuned = isStunned;
        catImage.color = isStunned ? new Color(1f, 0.5f, 0.5f, 0.7f) : Color.white;

        if (!isStunned)
        {
            HealCatHP();
        }
    }

    // ����ĳ��Ʈ Ÿ�� ����
    private void SetRaycastTarget(bool isActive)
    {
        catImage.raycastTarget = isActive;
    }

    // ü�� ȸ��
    public void HealCatHP()
    {
        catHp = catData.CatHp;
        UpdateHPBar();
    }
    #endregion

    // ======================================================================================================================

    #region Auto Movement
    // �ڵ� �̵� ���� ����
    public void SetAutoMoveState(bool isEnabled)
    {
        if (isEnabled && !isStuned)
        {
            if (!isAnimating && autoMoveCoroutine == null)
            {
                autoMoveCoroutine = StartCoroutine(AutoMove());
            }
        }
        else
        {
            if (autoMoveCoroutine != null)
            {
                StopCoroutine(autoMoveCoroutine);
                autoMoveCoroutine = null;
            }
            isAnimating = false;
        }
    }

    // �ڵ� �̵� ����
    private IEnumerator AutoMove()
    {
        while (true)
        {
            yield return new WaitForSeconds(AutoMoveManager.Instance.AutoMoveTime());

            if (!isAnimating && (catDragAndDrop == null || !catDragAndDrop.isDragging))
            {
                Vector3 randomDirection = GetRandomDirection();
                Vector3 targetPosition = (Vector3)rectTransform.anchoredPosition + randomDirection;

                yield return StartCoroutine(SmoothMoveToPosition(targetPosition));

                if (IsOutOfBounds(targetPosition))
                {
                    Vector3 adjustedPosition = AdjustPositionToBounds(targetPosition);
                    StartCoroutine(SmoothMoveToPosition(adjustedPosition));
                }
            }
        }
    }

    // ���� �̵� ���� ��ȯ
    private Vector3 GetRandomDirection()
    {
        float moveRange = 30f;
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

        return directions[Random.Range(0, directions.Length)];
    }

    // �̵� ���� �ʰ� Ȯ��
    private bool IsOutOfBounds(Vector3 position)
    {
        Vector2 minBounds = (Vector2)parentPanel.rect.min + parentPanel.anchoredPosition;
        Vector2 maxBounds = (Vector2)parentPanel.rect.max + parentPanel.anchoredPosition;

        return position.x <= minBounds.x || position.x >= maxBounds.x || position.y <= minBounds.y || position.y >= maxBounds.y;
    }

    // ���� �ʰ��� ��ġ ����
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

    // �ε巯�� �̵� ����
    private IEnumerator SmoothMoveToPosition(Vector3 targetPosition)
    {
        isAnimating = true;

        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
        }

        currentMoveCoroutine = StartCoroutine(DoSmoothMove(targetPosition));
        yield return currentMoveCoroutine;
    }

    // ���� �̵� ����
    private IEnumerator DoSmoothMove(Vector3 targetPosition)
    {
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
        currentMoveCoroutine = null;
    }
    #endregion

    // ======================================================================================================================

    #region Auto Coin Collection
    // �ڵ� ��ȭ ���� ���� ����
    public void SetCollectingCoinsState(bool isEnabled)
    {
        isCollectingCoins = isEnabled;

        if (isCollectingCoins)
        {
            if (autoCollectCoroutine == null)
            {
                autoCollectCoroutine = StartCoroutine(AutoCollectCoins());
            }
        }
        else
        {
            if (autoCollectCoroutine != null)
            {
                StopCoroutine(autoCollectCoroutine);
                autoCollectCoroutine = null;
            }
            DisableCollectUI();
        }
    }

    // ���� ���� UI ��Ȱ��ȭ
    public void DisableCollectUI()
    {
        if (collectCoinText != null)
        {
            collectCoinText.gameObject.SetActive(false);
        }
        if (collectCoinImage != null)
        {
            collectCoinImage.gameObject.SetActive(false);
        }
    }

    // �ڵ� ��ȭ ���� ����
    private IEnumerator AutoCollectCoins()
    {
        while (isCollectingCoins)
        {
            collectingTime = ItemFunctionManager.Instance.reduceCollectingTimeList[ItemMenuManager.Instance.ReduceCollectingTimeLv].value;
            yield return new WaitForSeconds(collectingTime);

            if (catData != null && GameManager.Instance != null)
            {
                int collectedCoins = catData.CatGetCoin;
                GameManager.Instance.Coin += collectedCoins;
                StartCoroutine(PlayCollectingAnimation(collectedCoins));
            }
        }
    }

    // ��ȭ ���� �ִϸ��̼� ����
    private IEnumerator PlayCollectingAnimation(int collectedCoins)
    {
        collectCoinText.text = $"+{collectedCoins}";
        collectCoinText.gameObject.SetActive(true);
        collectCoinImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        collectCoinText.gameObject.SetActive(false);
        collectCoinImage.gameObject.SetActive(false);
    }
    #endregion

    // ======================================================================================================================

    #region Movement Control
    // ��� �̵� �ڷ�ƾ ����
    public void StopAllMovement()
    {
        if (autoMoveCoroutine != null)
        {
            StopCoroutine(autoMoveCoroutine);
            autoMoveCoroutine = null;
        }

        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            currentMoveCoroutine = null;
        }

        isAnimating = false;
    }
    #endregion

    // ======================================================================================================================


}
