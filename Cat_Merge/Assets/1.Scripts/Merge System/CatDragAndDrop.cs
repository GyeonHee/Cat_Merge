using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// ����� �巡�� �� ��� Script
public class CatDragAndDrop : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Cat catData;                             // �巡���ϴ� ������� ������
    public RectTransform rectTransform;             // RectTransform ����
    private Canvas parentCanvas;                    // �θ� Canvas ����

    public bool isDragging { get; private set; }    // �巡�� ���� Ȯ�� �÷���

    // ======================================================================================================================

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // ======================================================================================================================

    // �巡�� ���� �Լ�
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        // �巡�׵� ����̸� mergingCats���� ����
        AutoMergeManager autoMerge = FindObjectOfType<AutoMergeManager>();
        if (autoMerge != null && autoMerge.IsMerging(this))
        {
            autoMerge.StopMerging(this);
        }

        // �巡���ϴ� ��ü�� �θ��� �ڽ� ��ü �� �ֻ������ �̵� (�巡������ ��ü�� UI�� ���� ���� ������ �� �ְ�)
        rectTransform.SetAsLastSibling();
    }

    // �巡�� ������ �Լ�
    public void OnDrag(PointerEventData eventData)
    {
        // ȭ�� ��ǥ�� UI ��ǥ�� ��ȯ
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent.GetComponent<RectTransform>(),
            eventData.position,
            parentCanvas.worldCamera,
            out localPointerPosition
        );

        // GamePanel RectTransform ��������
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
        Rect panelRect = new Rect(
            -parentRect.rect.width / 2,
            -parentRect.rect.height / 2,
            parentRect.rect.width,
            parentRect.rect.height
        );

        // �巡�� ��ġ�� �г� ���� ���� ����
        localPointerPosition.x = Mathf.Clamp(localPointerPosition.x, panelRect.xMin, panelRect.xMax);
        localPointerPosition.y = Mathf.Clamp(localPointerPosition.y, panelRect.yMin, panelRect.yMax);

        // �巡�� ��ġ ������Ʈ (UI�� localPosition ���)
        rectTransform.localPosition = localPointerPosition;
    }

    // �巡�� ���� �Լ�
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // ���� ���°� OFF�� ��� ���� X
        if (!MergeManager.Instance.IsMergeEnabled())
        {
            return;
        }

        CheckEdgeDrop();

        CatDragAndDrop nearbyCat = FindNearbyCat();
        if (nearbyCat != null && nearbyCat != this)
        {
            // �ڵ� ���� ������ Ȯ��
            if (IsAutoMerging(nearbyCat))
            {
                Debug.Log("�ڵ� ���� ���� ����̿� �ռ��� �� �����ϴ�.");
                return;
            }

            // ������ ��� Ȯ�� �� �ռ� ó��
            if (nearbyCat.catData.CatGrade == this.catData.CatGrade)
            {
                // �ռ��� ������� ���� ����� �����ϴ��� Ȯ��
                Cat nextCat = FindObjectOfType<MergeManager>().GetCatByGrade(this.catData.CatGrade + 1);
                if (nextCat != null)
                {
                    // nextCat�� ������ ��쿡�� �ִϸ��̼� ����
                    StartCoroutine(PullNearbyCat(nearbyCat));
                }
                else
                {
                    //Debug.LogWarning("���� ����� ����̰� ����");
                }
            }
            else
            {
                //Debug.LogWarning("����� �ٸ�");
            }
        }
        else
        {
            //Debug.Log("����� ��ġ�� ��ġ");
        }
    }

    // �����ڸ� ������� Ȯ�� �Լ�
    private void CheckEdgeDrop()
    {
        // GamePanel RectTransform ��������
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
        Rect panelRect = new Rect(
            -parentRect.rect.width / 2,
            -parentRect.rect.height / 2,
            parentRect.rect.width,
            parentRect.rect.height
        );

        Vector2 currentPos = rectTransform.localPosition;

        // �����ڸ��� �´�� �ִ��� Ȯ��
        bool isNearLeft = Mathf.Abs(currentPos.x - panelRect.xMin) < 10;
        bool isNearRight = Mathf.Abs(currentPos.x - panelRect.xMax) < 10;
        bool isNearTop = Mathf.Abs(currentPos.y - panelRect.yMax) < 10;
        bool isNearBottom = Mathf.Abs(currentPos.y - panelRect.yMin) < 10;

        Vector3 targetPos = currentPos;

        // �����ڸ��� ������ �߽� �������� �̵�
        if (isNearLeft) targetPos.x += 30;
        if (isNearRight) targetPos.x -= 30;
        if (isNearTop) targetPos.y -= 30;
        if (isNearBottom) targetPos.y += 30;

        // ��ġ ���� �ִϸ��̼� ����
        StartCoroutine(SmoothMoveToPosition(targetPos));
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

    // �ڵ� ���� ������ Ȯ���ϴ� �Լ�
    private bool IsAutoMerging(CatDragAndDrop nearbyCat)
    {
        AutoMergeManager autoMerge = FindObjectOfType<AutoMergeManager>();
        return autoMerge != null && autoMerge.IsMerging(nearbyCat);
    }

    // �ռ��� ����̰� �������� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator PullNearbyCat(CatDragAndDrop nearbyCat)
    {
        Vector3 startPosition = nearbyCat.rectTransform.localPosition;
        Vector3 targetPosition = rectTransform.localPosition;
        float elapsed = 0f;
        float duration = 0.1f;

        // nearbyCat�� �巡�׵� ��ü�� �������� ����
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            nearbyCat.rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        // ������ �� �ռ� ó��
        Cat mergedCat = FindObjectOfType<MergeManager>().MergeCats(this.catData, nearbyCat.catData);
        if (mergedCat != null)
        {
            //Debug.Log($"�ռ� ����: {mergedCat.CatName}");
            this.catData = mergedCat;
            UpdateCatUI();
            Destroy(nearbyCat.gameObject);
        }
        else
        {
            //Debug.LogWarning("�ռ� ����");
        }
    }

    // ���� ������ ���� ����� CatPrefab�� ã�� �Լ�
    private CatDragAndDrop FindNearbyCat()
    {
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
        Rect thisRect = GetWorldRect(rectTransform);

        // ���� Rect ũ�⸦ �ٿ��� ���� ���� ���� (���� ���� ���� ����)
        thisRect = ShrinkRect(thisRect, 0.2f);

        foreach (Transform child in parentRect)
        {
            if (child == this.transform) continue;

            CatDragAndDrop otherCat = child.GetComponent<CatDragAndDrop>();
            if (otherCat != null)
            {
                Rect otherRect = GetWorldRect(otherCat.rectTransform);

                // Rect �� �浹 Ȯ��
                if (thisRect.Overlaps(otherRect))
                {
                    return otherCat;
                }
            }
        }
        return null;
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
        float widthReduction = rect.width * (1 - scale) / 2;
        float heightReduction = rect.height * (1 - scale) / 2;

        return new Rect(
            rect.x + widthReduction,
            rect.y + heightReduction,
            rect.width * scale,
            rect.height * scale
        );
    }

    // CatUI �����ϴ� �Լ�
    public void UpdateCatUI()
    {
        GetComponentInChildren<CatData>()?.SetCatData(catData);
    }

}
