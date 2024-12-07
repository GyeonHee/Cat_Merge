using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatDragAndDrop : MonoBehaviour, IDragHandler, IBeginDragHandler, IDropHandler
{
    public Cat catData;                             // �巡���ϴ� ������� ������
    public RectTransform rectTransform;             // RectTransform ����
    private Canvas parentCanvas;                    // �θ� Canvas ����

    public bool isDragging { get; private set; }    // �巡�� ���� Ȯ�� �÷���

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // �巡�� ����
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        // �巡���ϴ� ��ü�� �θ��� �ڽ� ��ü �� �ֻ������ �̵� (�巡������ ��ü�� UI�� ���� ���� ������ �� �ְ�)
        rectTransform.SetAsLastSibling();
    }

    // �巡�� ��
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

        // �巡�� ��ġ ������Ʈ (UI�� localPosition ���)
        rectTransform.localPosition = localPointerPosition;
    }

    // ��� ��
    public void OnDrop(PointerEventData eventData)
    {
        isDragging = false;

        // ��� ��ġ�� ��ó�� catPrefab ã��
        CatDragAndDrop nearbyCat = FindNearbyCat();
        if (nearbyCat != null && nearbyCat != this)
        {
            // �ڵ� ���� ������ Ȯ��
            if (IsAutoMerging(nearbyCat))
            {
                Debug.LogWarning("�ڵ� ���� ���� ����̿� �ռ��� �� �����ϴ�.");
                return;
            }

            // ������ ID Ȯ�� �� �ռ� ó��
            if (nearbyCat.catData.CatId == this.catData.CatId)
            {
                // �ռ��� ������� ���� ����� �����ϴ��� Ȯ��
                Cat nextCat = FindObjectOfType<CatMerge>().GetCatById(this.catData.CatId + 1);
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

    // �ڵ� ���� ������ Ȯ��
    private bool IsAutoMerging(CatDragAndDrop nearbyCat)
    {
        AutoMerge autoMerge = FindObjectOfType<AutoMerge>();
        return autoMerge != null && autoMerge.IsMerging(nearbyCat);
    }

    // �ռ��� ����̰� �������� �ִϸ��̼� ó��
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
        Cat mergedCat = FindObjectOfType<CatMerge>().MergeCats(this.catData, nearbyCat.catData);
        if (mergedCat != null)
        {
            Debug.Log($"�ռ� ����: {mergedCat.CatName}");
            this.catData = mergedCat;
            UpdateCatUI();
            Destroy(nearbyCat.gameObject);
        }
        else
        {
            Debug.LogWarning("�ռ� ����");
        }
    }

    // ���� ������ ���� ����� CatPrefab ã��
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

    // RectTransform�� ���� ��ǥ ��� Rect ��ȯ
    private Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }

    // Rect ũ�⸦ ���̴� �޼���
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

    // CatUI ����
    public void UpdateCatUI()
    {
        GetComponentInChildren<CatData>()?.SetCatData(catData);
    }

}
