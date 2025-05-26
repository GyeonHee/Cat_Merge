using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{


    #region Variables

    public RectTransform spriteRect;        // UI Image�� RectTransform
    public Image spriteImage;               // UI Image�� ���� ������ ���� ����
    private float duration = 0.125f;        // ũ�� ��ȭ �ð�
    private float maxLifetime = 2f;         // �ִ� ���� �ð� (������ġ)

    private bool isAnimating = false;       // �ִϸ��̼� ���� ����
    private bool isDestroyed = false;       // �ı� ���� ����

    #endregion


    #region Unity Methods

    private void Start()
    {
        if (!isDestroyed)
        {
            isAnimating = true;
            StartCoroutine(ScaleAnimation());
            StartCoroutine(SafetyTimeout());
        }
    }

    private void OnDisable()
    {
        CleanupAnimation();
    }

    private void OnDestroy()
    {
        CleanupAnimation();
    }

    #endregion


    #region Animation Methods

    // �ִ� ���� �ð� ���� ������Ʈ�� �����ϴ� �ڷ�ƾ
    private IEnumerator SafetyTimeout()
    {
        yield return new WaitForSeconds(maxLifetime);
        DestroyEffect();
    }

    // ũ�� ���� �� ���̵� �ƿ� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator ScaleAnimation()
    {
        if (spriteRect == null || spriteImage == null || isDestroyed)
        {
            DestroyEffect();
            yield break;
        }

        // 1. ũ�� 30 �� 175 (���� 1 ����)
        SetAlpha(1f);

        var scaleUp = StartCoroutine(ChangeSize(spriteRect, new Vector2(30, 30), new Vector2(175, 175), duration, false));
        yield return scaleUp;

        if (isDestroyed)
        {
            if (scaleUp != null) StopCoroutine(scaleUp);
            DestroyEffect();
            yield break;
        }

        // 2. ũ�� 175 �� 150 (���� 1 �� 0)
        var scaleDown = StartCoroutine(ChangeSize(spriteRect, new Vector2(175, 175), new Vector2(150, 150), duration * 0.5f, true));
        yield return scaleDown;

        if (isDestroyed)
        {
            if (scaleDown != null) StopCoroutine(scaleDown);
        }

        DestroyEffect();
    }

    // ������ �ð� ���� ũ��� ������ �����ϴ� �ڷ�ƾ
    private IEnumerator ChangeSize(RectTransform target, Vector2 startSize, Vector2 endSize, float time, bool fadeOut)
    {
        if (target == null || isDestroyed) yield break;

        float elapsedTime = 0;
        while (elapsedTime < time && !isDestroyed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;

            // ũ�� ����
            if (target != null)
            {
                target.sizeDelta = Vector2.Lerp(startSize, endSize, t);
            }

            // ���� ���� (fadeOut�� true�� ����)
            if (fadeOut)
            {
                SetAlpha(Mathf.Lerp(1f, 0f, t));
            }

            yield return null;
        }

        if (!isDestroyed)
        {
            // ���� ũ��� ���� ����
            if (target != null)
            {
                target.sizeDelta = endSize;
            }
            if (fadeOut)
            {
                SetAlpha(0f);
            }
        }
    }

    // ��������Ʈ �̹����� ���� ���� �Լ�
    private void SetAlpha(float alpha)
    {
        if (spriteImage != null && !isDestroyed)
        {
            Color color = spriteImage.color;
            color.a = alpha;
            spriteImage.color = color;
        }
    }

    // ����Ʈ ���� �Լ�
    private void CleanupAnimation()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            isAnimating = false;
            StopAllCoroutines();
        }
    }

    // ����Ʈ ���� �Լ�
    private void DestroyEffect()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            isAnimating = false;
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }

    #endregion


}
