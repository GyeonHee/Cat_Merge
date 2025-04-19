using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// ����� �巡�� �� ��� Script
public class DragAndDropManager : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerClickHandler
{


    #region Variables

    public Cat catData;                                 // �巡���ϴ� ������� ������
    public RectTransform rectTransform;                 // RectTransform ����
    private Canvas parentCanvas;                        // �θ� Canvas ����
    private RectTransform parentRect;                   // ĳ�̿� �θ� RectTransform
    private Vector2 panelBoundsMin;                     // �г� ��� �ּҰ� ĳ��
    private Vector2 panelBoundsMax;                     // �г� ��� �ִ밪 ĳ��
    private Vector2 dragOffset;                         // �巡�� ���� ��ġ ������

    public bool isDragging { get; private set; }        // �巡�� ���� Ȯ�� �÷���

    private const float EDGE_THRESHOLD = 10f;           // �����ڸ� ���� �Ӱ谪
    private const float EDGE_PUSH = 30f;                // �����ڸ����� �о�� �Ÿ�
    private const float MERGE_DETECTION_SCALE = 0.1f;   // �ռ� ���� ���� ������

    #endregion


    #region Unity Methods

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        parentRect = rectTransform.parent.GetComponent<RectTransform>();

        Vector2 panelSize = parentRect.rect.size * 0.5f;
        panelBoundsMin = -panelSize;
        panelBoundsMax = panelSize;
    }

    #endregion


    #region Click & Drag and Drop

    // Ŭ���� ����
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;

        GetComponent<AnimatorManager>().ChangeState(CharacterState.isGrab);

        // �巡�� ���� ��ġ ������ ���
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, parentCanvas.worldCamera, out localPointerPosition))
        {
            dragOffset = rectTransform.localPosition - (Vector3)localPointerPosition;
        }

        // �ڵ� ���� ���� ����� ó��
        if (AutoMergeManager.Instance != null && AutoMergeManager.Instance.IsMerging(this))
        {
            AutoMergeManager.Instance.StopMerging(this);
        }
    }

    // Ŭ�� �� ����
    public void OnPointerClick(PointerEventData eventData)
    {
        isDragging = false;
        GetComponent<AnimatorManager>().ChangeState(CharacterState.isIdle);
    }

    // �巡�� ������
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPosition;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, parentCanvas.worldCamera, out localPointerPosition)) return;

        // �巡�� ������ ���� �� ��ġ ����
        Vector2 targetPosition = localPointerPosition + dragOffset;
        targetPosition.x = Mathf.Clamp(targetPosition.x, panelBoundsMin.x, panelBoundsMax.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, panelBoundsMin.y, panelBoundsMax.y);

        rectTransform.localPosition = targetPosition;

        // Y�� ���� ���� (60fps ���� 3�����Ӹ��� ����)
        if (Time.frameCount % 3 == 0)
        {
            UpdateSiblingIndexBasedOnY();
        }
    }

    // Y�� ���� �������� �巡�� ��ü�� ������ ������Ʈ�ϴ� �Լ�
    private void UpdateSiblingIndexBasedOnY()
    {
        int childCount = parentRect.childCount;
        float currentY = rectTransform.localPosition.y;
        int newIndex = 0;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform sibling = parentRect.GetChild(i).GetComponent<RectTransform>();
            if (sibling != null && sibling != rectTransform && currentY < sibling.localPosition.y)
            {
                newIndex = i + 1;
            }
        }

        if (rectTransform.GetSiblingIndex() != newIndex)
        {
            rectTransform.SetSiblingIndex(newIndex);
        }
    }

    // �巡�� ����
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!gameObject.activeSelf) return;

        isDragging = false;

        if (BattleManager.Instance.IsBattleActive)
        {
            GetComponent<AnimatorManager>().ChangeState(CharacterState.isBattle);
            BattleDrop(eventData);
        }
        else
        {
            GetComponent<AnimatorManager>().ChangeState(CharacterState.isIdle);

            // �����ڸ� ��� üũ�� ���� ���¿� ������� �׻� ����
            CheckEdgeDrop();

            // ���� ���°� OFF�� ��� ���� X
            if (!MergeManager.Instance.IsMergeEnabled()) return;

            DragAndDropManager nearbyCat = FindNearbyCat();
            if (nearbyCat != null && nearbyCat != this && nearbyCat.gameObject.activeSelf)
            {
                // ������ ��� Ȯ�� �� �ռ� ó��
                if (nearbyCat.catData.CatGrade == this.catData.CatGrade)
                {
                    Cat nextCat = MergeManager.Instance.GetCatByGrade(this.catData.CatGrade + 1);
                    if (nextCat != null)
                    {
                        StartCoroutine(PullNearbyCat(nearbyCat));
                    }
                }
            }
        }
    }

    // �����ڸ� ������� Ȯ�� �Լ�
    private void CheckEdgeDrop()
    {
        Vector2 currentPos = rectTransform.localPosition;
        Vector2 targetPos = currentPos;
        bool needsAdjustment = false;

        // �����ڸ� üũ �� ����
        if (Mathf.Abs(currentPos.x - panelBoundsMin.x) < EDGE_THRESHOLD) { targetPos.x += EDGE_PUSH; needsAdjustment = true; }
        if (Mathf.Abs(currentPos.x - panelBoundsMax.x) < EDGE_THRESHOLD) { targetPos.x -= EDGE_PUSH; needsAdjustment = true; }
        if (Mathf.Abs(currentPos.y - panelBoundsMax.y) < EDGE_THRESHOLD) { targetPos.y -= EDGE_PUSH; needsAdjustment = true; }
        if (Mathf.Abs(currentPos.y - panelBoundsMin.y) < EDGE_THRESHOLD) { targetPos.y += EDGE_PUSH; needsAdjustment = true; }

        if (needsAdjustment)
        {
            StartCoroutine(SmoothMoveToPosition(targetPos));
        }
    }

    // �����ڸ����� �������� �ε巴�� �̵��ϴ� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator SmoothMoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = rectTransform.localPosition;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);

            yield return null;
        }

        rectTransform.localPosition = targetPosition;
    }

    #endregion


    #region Battle System

    // �������϶� �巡�� �� ��� ���� �Լ�
    private void BattleDrop(PointerEventData eventData)
    {
        if (!BattleManager.Instance.IsBattleActive) return;

        BossHitbox currentBossHitBox = BattleManager.Instance.bossHitbox;
        GameObject droppedObject = eventData.pointerDrag;
        CatData catData = droppedObject.GetComponent<CatData>();

        // ��Ʈ�ڽ� ���ζ��
        if (currentBossHitBox.IsInHitbox(this.rectTransform.anchoredPosition))
        {
            catData.MoveOppositeBoss();
        }
        // ��Ʈ�ڽ� �ܺζ��
        else if (!currentBossHitBox.IsInHitbox(this.rectTransform.anchoredPosition))
        {
            catData.MoveTowardBossBoundary();
        }
    }

    #endregion


    #region Merge System

    // �ռ��� ����̰� �������� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator PullNearbyCat(DragAndDropManager nearbyCat)
    {
        // �ڵ� ���� ���� ��� �ش� ����̵��� �ڵ� �������� ����
        if (AutoMergeManager.Instance != null)
        {
            AutoMergeManager.Instance.StopMerging(this);
            AutoMergeManager.Instance.StopMerging(nearbyCat);
        }

        Vector3 startPosition = nearbyCat.rectTransform.localPosition;
        Vector3 targetPosition = rectTransform.localPosition;
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            if (nearbyCat == null) yield break;

            elapsed += Time.deltaTime;
            nearbyCat.rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);

            yield return null;
        }

        if (nearbyCat != null)
        {
            CompleteMerge(this, nearbyCat);
        }
    }

    // ����� ���� ó�� �Լ�
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

    // ���� ������ ���� ����� ����̸� ã�� �Լ�
    private DragAndDropManager FindNearbyCat()
    {
        // ���� Rect ũ�⸦ �ٿ��� ���� ���� ���� (���� ���� ���� ����)
        Rect thisRect = GetWorldRect(rectTransform);
        thisRect = ShrinkRect(thisRect, MERGE_DETECTION_SCALE);

        DragAndDropManager nearestCat = null;
        float nearestDistance = float.MaxValue;

        int childCount = parentRect.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = parentRect.GetChild(i);
            if (child == transform || !child.gameObject.activeSelf) continue;

            DragAndDropManager otherCat = child.GetComponent<DragAndDropManager>();
            if (otherCat != null && thisRect.Overlaps(GetWorldRect(otherCat.rectTransform)))
            {
                float distance = Vector2.Distance(rectTransform.position, otherCat.rectTransform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCat = otherCat;
                }
            }
        }
        return nearestCat;
    }

    // RectTransform�� ���� ��ǥ ��� Rect�� ��ȯ�ϴ� �Լ�
    private Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }

    // Rect ũ�⸦ ���̴� �Լ� (����� ����� �ռ� ����)
    private Rect ShrinkRect(Rect rect, float scale)
    {
        float widthReduction = rect.width * (1 - scale) * 0.5f;
        float heightReduction = rect.height * (1 - scale) * 0.5f;

        return new Rect(
            rect.x + widthReduction,
            rect.y + heightReduction,
            rect.width * scale,
            rect.height * scale
        );
    }

    #endregion


    #region UI Update

    // CatUI �����ϴ� �Լ�
    public void UpdateCatUI()
    {
        GetComponentInChildren<CatData>()?.SetCatData(catData);
    }

    #endregion


}
