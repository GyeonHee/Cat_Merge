using UnityEngine;
using UnityEngine.EventSystems;

public class CatDragAndDrop : MonoBehaviour, IDragHandler, IDropHandler
{
    public Cat catData;                             // �巡���ϴ� ������� ������
    public RectTransform rectTransform;                
    private Canvas parentCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
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
        // ��� �� ��ġ��
            // ���� ����� ����̰� ������
                // �ռ�ó��
                    // �ռ�ó�� �� ���ο� ����� UI ����
            // ���� ����� ����̰� ������ �׳� �� ��ġ�� ����� ���

    }

}
