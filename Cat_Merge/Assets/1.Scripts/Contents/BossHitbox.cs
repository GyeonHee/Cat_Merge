using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("---[Boss Hitbox]")]
    private float width = 200f;     // ��Ʈ�ڽ��� �ʺ�
    private float height = 400f;    // ��Ʈ�ڽ��� ����

    public Vector2 Size => new Vector2(width, height);                              // ��Ʈ�ڽ� ũ��
    public Vector3 Position => GetComponent<RectTransform>().anchoredPosition;      // ������ ��ġ

    // ������ ��Ʈ�ڽ� ���� ���� ����̰� �ִ��� üũ�ϴ� �Լ�
    public bool IsInHitbox(Vector3 targetPosition)
    {
        float halfWidth = width / 2;
        float halfHeight = height / 2;

        // ������ ��ġ�� ������� ��ġ�� RectTransform�� �������� ��
        return Mathf.Abs(targetPosition.x - Position.x) < halfWidth && Mathf.Abs(targetPosition.y - Position.y) < halfHeight;
    }

    // ���� ��� Ȯ�� �Լ�
    public bool IsAtBoundary(Vector3 position)
    {
        // ��Ʈ�ڽ� ��� ���
        float left = Position.x - Size.x / 2;
        float right = Position.x + Size.x / 2;
        float top = Position.y + Size.y / 2;
        float bottom = Position.y - Size.y / 2;

        // ����̰� ��迡 ��ġ�ϴ��� Ȯ�� (�ణ�� ������ �߰�)
        float boundaryTolerance = 1f;   // ��� ������ ���� �� �����ϰ� �ϱ� ���� ������
        bool isAtHorizontalBoundary = Mathf.Abs(position.x - left) <= boundaryTolerance || Mathf.Abs(position.x - right) <= boundaryTolerance;
        bool isAtVerticalBoundary = Mathf.Abs(position.y - top) <= boundaryTolerance || Mathf.Abs(position.y - bottom) <= boundaryTolerance;

        return isAtHorizontalBoundary || isAtVerticalBoundary;
    }

}
