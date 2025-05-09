using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public RectTransform spriteRect;                    // UI Image�� RectTransform
    public Image spriteImage;                           // UI Image�� ���� ������ ���� ����
    public float duration = 1.5f;                       // ũ�� ��ȭ �ð�
    private float maxLifetime = 3f;                     // �ִ� ���� �ð� (������ġ)

    private Vector2 minSize = new Vector2(30, 30);
    private Vector2 maxSize = new Vector2(175, 175);
    private Vector2 endSize = new Vector2(150, 150);

    private void Start()
    {
        StartCoroutine(ScaleAnimation());
        StartCoroutine(SafetyTimeout());
    }

    private IEnumerator SafetyTimeout()
    {
        yield return new WaitForSeconds(maxLifetime);

        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ScaleAnimation()
    {
        // 1. ũ�� 30 �� 175 (���� 1 ����)
        SetAlpha(1f);
        yield return StartCoroutine(ChangeSize(spriteRect, minSize, maxSize, duration, false));

        // 2. ũ�� 175 �� 150 (���� 1 �� 0)
        yield return StartCoroutine(ChangeSize(spriteRect, maxSize, endSize, duration * 0.5f, true));

        // �ִϸ��̼��� ������ �����ų� ������ �߻����� �� ������Ʈ ����
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ChangeSize(RectTransform target, Vector2 startSize, Vector2 endSize, float time, bool fadeOut)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;

            // ũ�� ����
            target.sizeDelta = Vector2.Lerp(startSize, endSize, t);

            // ���� ���� (fadeOut�� true�� ����)
            if (fadeOut)
            {
                SetAlpha(Mathf.Lerp(1f, 0f, t));
            }

            yield return null;
        }

        // ���� ũ��� ���� ����
        target.sizeDelta = endSize;
        if (fadeOut)
        {
            SetAlpha(0f);
        }
    }

    private void SetAlpha(float alpha)
    {
        if (spriteImage != null)
        {
            Color color = spriteImage.color;
            color.a = alpha;
            spriteImage.color = color;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
