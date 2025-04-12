using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ������� ������ �ൿ�� �����ϴ� ��ũ��Ʈ
public class CatData : MonoBehaviour, ICanvasRaycastFilter
{


    #region Variables

    private const float CLICK_AREA_SCALE = 0.9f;    // Ŭ�� ���� ������
    private WaitForSeconds COLLECT_ANIMATION_DELAY = new WaitForSeconds(1f); // 4�� 5�� 0.5->1�� �ٲ�

    [Header("Cat Data")]
    public Cat catData;                             // ����� �⺻ ������
    private Image catImage;                         // ����� �̹��� ������Ʈ
    public double catHp;                            // ���� ����� ü��
    public bool isStuned = false;                   // ���� ���� ����
    private const float STUN_TIME = 10f;            // ���� �ð�

    [Header("HP UI")]
    [SerializeField] private GameObject hpImage;    // HP �̹��� ������Ʈ
    [SerializeField] private Image hpFillImage;     // HP Fill �̹���

    [Header("Transform")]
    private RectTransform rectTransform;            // ���� ������Ʈ�� RectTransform
    private RectTransform parentPanel;              // �θ� �г��� RectTransform
    private DragAndDropManager catDragAndDrop;      // �巡�� �� ��� �Ŵ���
    private Vector2 rectSize;
    private float rectHalfWidth;
    private float rectHalfHeight;

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


    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
        CacheRectTransformData();
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


    #region Initialization

    // ������Ʈ �ʱ�ȭ
    private void InitializeComponents()
    {
        catImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentPanel = rectTransform.parent.GetComponent<RectTransform>();
        catDragAndDrop = GetComponentInParent<DragAndDropManager>();

        Transform coinTextTransform = transform.Find("CollectCoinText");
        Transform coinImageTransform = transform.Find("CollectCoinImage");
        if (coinTextTransform != null) collectCoinText = coinTextTransform.GetComponent<TextMeshProUGUI>();
        if (coinImageTransform != null) collectCoinImage = coinImageTransform.GetComponent<Image>();

        hpImage = transform.Find("HP Image").gameObject;
        hpFillImage = hpImage.transform.Find("Fill Image").GetComponent<Image>();
        hpImage.SetActive(false);

        if (collectCoinText != null) collectCoinText.gameObject.SetActive(false);
        if (collectCoinImage != null) collectCoinImage.gameObject.SetActive(false);
    }

    private void CacheRectTransformData()
    {
        rectSize = rectTransform.rect.size;
        rectHalfWidth = rectSize.x * 0.5f;
        rectHalfHeight = rectSize.y * 0.5f;
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


    #region Battle System

    // ���� ��Ʈ�ڽ� ���� ����� �о��
    public void MoveOppositeBoss()
    {
        Vector3 catPosition = rectTransform.anchoredPosition;
        BossHitbox bossHitbox = BattleManager.Instance.bossHitbox;

        // ����̰� ��Ʈ�ڽ� ���ο� �ִ� ���
        if (bossHitbox.IsInHitbox(catPosition))
        {
            // ��Ʈ�ڽ� ��迡 ��ġ�ϵ��� �̵�
            Vector3 targetPosition = bossHitbox.GetClosestBoundaryPoint(catPosition);
            StartCoroutine(SmoothMoveToPosition(targetPosition));
        }
    }

    // ���� ��Ʈ�ڽ� ���� ����� �̵�
    public void MoveTowardBossBoundary()
    {
        Vector3 catPosition = rectTransform.anchoredPosition;
        BossHitbox bossHitbox = BattleManager.Instance.bossHitbox;

        // ����̰� ��Ʈ�ڽ� �ܺο� �ִ� ���
        if (!bossHitbox.IsInHitbox(catPosition))
        {
            // ��Ʈ�ڽ� ��迡 ��ġ�ϵ��� �̵�
            Vector3 targetPosition = bossHitbox.GetClosestBoundaryPoint(catPosition);
            StartCoroutine(SmoothMoveToPosition(targetPosition));
        }
    }

    // ������ ó��
    public void TakeDamage(double damage)
    {
        catHp -= damage;
        UpdateHPBar();

        if (catHp <= 0)
        {
            StartCoroutine(StunAndRecover(STUN_TIME));
        }
    }

    // ���� �� ȸ�� ó��
    private IEnumerator StunAndRecover(float stunTime)
    {
        SetStunState(true);
        yield return new WaitForSeconds(stunTime);
        SetStunState(false);
    }

    // ���� ���� ����
    private void SetStunState(bool isStunned)
    {
        UpdateHPBar();
        //SetCollectingCoinsState(!isStunned);
        //SetAutoMoveState(!isStunned);
        //SetRaycastTarget(!isStunned);
        isStuned = isStunned;
        catImage.color = isStunned ? new Color(1f, 0.5f, 0.5f, 0.7f) : Color.white;

        

        if (!isStunned)
        {
            HealCatHP();

            // ���� ������ Ȯ��
            if (BattleManager.Instance != null && BattleManager.Instance.IsBattleActive)
            {
                // ���� ���̸� �ڵ� ��ȭ ������ �ڵ� �̵��� ��Ȱ��ȭ ���� ����
                SetCollectingCoinsState(false);
                SetAutoMoveState(false);
                SetRaycastTarget(true);  // �巡�״� �����ϵ��� ����

                // ���� ��Ʈ�ڽ� ���� �̵�
                MoveTowardBossBoundary();               
            }
            else
            {
                // ���� ���� �ƴϸ� ��� ��� Ȱ��ȭ
                SetCollectingCoinsState(true);
                SetAutoMoveState(true);
                SetRaycastTarget(true);
            }
        }
        else
        {
            // ���� ���·� ������ ���� ��� ��� ��Ȱ��ȭ
            SetCollectingCoinsState(false);
            SetAutoMoveState(false);
            SetRaycastTarget(false);
        }
    }

    // Raycast Target ����
    private void SetRaycastTarget(bool isActive)
    {
        catImage.raycastTarget = isActive;
    }

    // ü�� ȸ��
    public void HealCatHP()
    {
        if (isStuned)
        {
            StopCoroutine(StunAndRecover(STUN_TIME));
            SetStunState(false);
        }
        catHp = catData.CatHp;
        UpdateHPBar();
    }

    #endregion


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
        float currentDelayTime = 0f;
        WaitForSeconds delay = null;

        
        while (isCollectingCoins)
        {
            collectingTime = ItemFunctionManager.Instance.reduceCollectingTimeList[ItemMenuManager.Instance.ReduceCollectingTimeLv].value;

            // ������ �ð��� ����Ǿ����� Ȯ��
            if (delay == null || !Mathf.Approximately(currentDelayTime, collectingTime))
            {
                currentDelayTime = collectingTime;
                delay = new WaitForSeconds(currentDelayTime);
            }

            yield return delay;


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
        if (collectCoinText != null)
        {
            collectCoinText.text = $"+{collectedCoins}";
            collectCoinText.gameObject.SetActive(true);
        }
        if (collectCoinImage != null)
        {
            collectCoinImage.gameObject.SetActive(true);
        }

        yield return COLLECT_ANIMATION_DELAY;

        if (collectCoinText != null) collectCoinText.gameObject.SetActive(false);
        if (collectCoinImage != null) collectCoinImage.gameObject.SetActive(false);
    }
    
    #endregion


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


    #region Raycast

    // ����ĳ��Ʈ ���͸� �Լ� ����
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPoint)) 
            return false;

        Vector2 normalizedPoint = new Vector2(
            //localPoint.x / rectHalfWidth,
            (localPoint.x + 20f) / rectHalfWidth,
            localPoint.y / rectHalfHeight
        );

        return (normalizedPoint.x * normalizedPoint.x + normalizedPoint.y * normalizedPoint.y) <= (CLICK_AREA_SCALE * CLICK_AREA_SCALE);
    }

    #endregion


}
