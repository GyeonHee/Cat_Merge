using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("---[Boss Hitbox]")]
    private float width = 200f;     // ��Ʈ�ڽ��� �ʺ�
    private float height = 400f;    // ��Ʈ�ڽ��� ����

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public Vector2 Size => new Vector2(width, height);                          // ��Ʈ�ڽ� ũ�� 
    public Vector3 Position => rectTransform.anchoredPosition;                  // ������ ��ġ

    // ������ ��Ʈ�ڽ� ���� ���� ����̰� �ִ��� üũ�ϴ� �Լ�
    public bool IsInHitbox(Vector3 targetPosition)
    {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        // ������ ��ġ�� ������� ��ġ�� RectTransform�� �������� ��
        return Mathf.Abs(targetPosition.x - Position.x) < halfWidth && 
            Mathf.Abs(targetPosition.y - Position.y) < halfHeight;
    }

    // ���� ��� Ȯ�� �Լ�
    public bool IsAtBoundary(Vector3 position)
    {
        Vector2 size = Size;
        Vector2 pos = Position;
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;

        // ��� ������ ���� �����ϰ� �ϱ� ���� ������
        const float boundaryTolerance = 1f;

        return Mathf.Abs(position.x - (pos.x - halfWidth)) <= boundaryTolerance ||
               Mathf.Abs(position.x - (pos.x + halfWidth)) <= boundaryTolerance ||
               Mathf.Abs(position.y - (pos.y - halfHeight)) <= boundaryTolerance ||
               Mathf.Abs(position.y - (pos.y + halfHeight)) <= boundaryTolerance;
    }
}
