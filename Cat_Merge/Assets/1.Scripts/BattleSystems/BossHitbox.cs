using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("---[Boss Hitbox]")]
    private float width = 260f;                                 // ��Ʈ�ڽ��� �ʺ� (���� ���� ����)
    private float height = 420f;                                // ��Ʈ�ڽ��� ���� (���� ���� ����)

    private RectTransform rectTransform;

    // ������� �̹��� ������ ��ġ �̼� ����
    public Vector3 Position => new Vector3(rectTransform.anchoredPosition.x + 20, rectTransform.anchoredPosition.y - 10, 0);

    // ======================================================================================================================

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // ======================================================================================================================

    // ������ ��Ʈ�ڽ� ���� ���� ����̰� �ִ��� üũ�ϴ� �Լ�
    public bool IsInHitbox(Vector3 targetPosition)
    {
        // Ÿ�� ���ο� �ִ��� Ȯ���ϱ� ���� Ÿ�� ������ ��� : (x/a)^2 + (y/b)^2 <= 1
        float dx = targetPosition.x - Position.x;
        float dy = targetPosition.y - Position.y;
        float normalizedX = dx / (width / 2f);
        float normalizedY = dy / (height / 2f);

        return (normalizedX * normalizedX + normalizedY * normalizedY) <= 1f;
    }

    // ���� ��� Ȯ�� �Լ�
    public bool IsAtBoundary(Vector3 position)
    {
        // Ÿ�� ��迡 �ִ��� Ȯ���ϱ� ���� Ÿ�� ������ ��� : (x/a)^2 + (y/b)^2 <= 1
        float dx = position.x - Position.x;
        float dy = position.y - Position.y;
        float normalizedX = dx / (width / 2f);
        float normalizedY = dy / (height / 2f);

        // ��� ������ ���� �����ϰ� �ϱ� ���� ������
        float boundaryTolerance = 0.05f;

        float ellipseEquation = normalizedX * normalizedX + normalizedY * normalizedY;
        return Mathf.Abs(ellipseEquation - 1f) <= boundaryTolerance;
    }

    // �־��� ��ġ���� ���� �������� ���� ����� ���� ����ϴ� �Լ�
    public Vector3 GetClosestBoundaryPoint(Vector3 position)
    {
        // ������ ����� ������ ���� ���
        Vector3 direction = (position - Position).normalized;

        // Ÿ���� �������� ���� ������ ����
        float angle = Mathf.Atan2(direction.y * (width / height), direction.x);
        float closestX = Mathf.Cos(angle) * (width / 2f);
        float closestY = Mathf.Sin(angle) * (height / 2f);

        return Position + new Vector3(closestX, closestY, 0);
    }

}
