using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("---[Boss Hitbox]")]
    private float width = 200f;     // ��Ʈ�ڽ��� �ʺ�
    private float height = 400f;    // ��Ʈ�ڽ��� ����

    public Vector2 Size => new Vector2(width, height);  // ��Ʈ�ڽ� ũ�� (width, height)
    public Vector3 Position => transform.position;      // ������ ��ġ

    // ������ ��Ʈ�ڽ� ���� ���� ����̰� �ִ��� üũ�ϴ� �Լ�
    public bool IsInHitbox(Vector3 targetPosition)
    {
        RectTransform bossRectTransform = GetComponent<RectTransform>();
        Vector3 bossPosition = bossRectTransform.anchoredPosition;  // RectTransform �������� ��ġ ��������
        float halfWidth = width / 2;
        float halfHeight = height / 2;

        // ������ ��ġ�� ������� ��ġ�� RectTransform�� �������� ��
        return Mathf.Abs(targetPosition.x - bossPosition.x) < halfWidth && Mathf.Abs(targetPosition.y - bossPosition.y) < halfHeight;
    }

}
