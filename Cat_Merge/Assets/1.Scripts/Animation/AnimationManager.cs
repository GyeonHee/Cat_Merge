using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ����� ��ȯ/�ռ� ���� ����Ʈ �ִϸ��̼� ��ũ��Ʈ
public class AnimationManager : MonoBehaviour
{


    #region Variables

    public RectTransform spriteRect;        // UI Image�� RectTransform
    public Image spriteImage;               // UI Image�� ���� ������ ���� ����
    private float duration = 0.125f;        // ũ�� ��ȭ �ð�
    private float maxLifetime = 2f;         // �ִ� ���� �ð� (������ġ)

    private bool isDestroyed = false;       // �ı� ���� ����

    private Coroutine scaleAnimationCoroutine;  // ���� �ִϸ��̼� �ڷ�ƾ
    private Coroutine safetyTimeoutCoroutine;   // �ִ� ���� �ð� �ڷ�ƾ

    // ���� ������
    private readonly Vector2 INITIAL_SIZE = new Vector2(30, 30);
    private readonly Vector2 PEAK_SIZE = new Vector2(175, 175);
    private readonly Vector2 FINAL_SIZE = new Vector2(150, 150);

    #endregion


    #region Unity Methods

    private void Start()
    {
        if (!isDestroyed)
        {
            scaleAnimationCoroutine = StartCoroutine(ScaleAnimation());
            safetyTimeoutCoroutine = StartCoroutine(SafetyTimeout());
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

    private void OnApplicationQuit()
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

        SetAlpha(1f);

        // 1. ũ�� 30 �� 175 (���� 1 ����)
        var scaleUp = StartCoroutine(ChangeSize(spriteRect, INITIAL_SIZE, PEAK_SIZE, duration, false));
        yield return scaleUp;

        if (isDestroyed)
        {
            StopCoroutine(scaleUp);
            DestroyEffect();
            yield break;
        }

        // 2. ũ�� 175 �� 150 (���� 1 �� 0)
        var scaleDown = StartCoroutine(ChangeSize(spriteRect, PEAK_SIZE, FINAL_SIZE, duration * 0.5f, true));
        yield return scaleDown;

        if (isDestroyed)
        {
            StopCoroutine(scaleDown);
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

            if (target == null) break;

            target.sizeDelta = Vector2.Lerp(startSize, endSize, t);

            if (fadeOut)
            {
                SetAlpha(Mathf.Lerp(1f, 0f, t));
            }

            yield return null;
        }

        if (!isDestroyed && target != null)
        {
            target.sizeDelta = endSize;
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

            if (scaleAnimationCoroutine != null)
            {
                StopCoroutine(scaleAnimationCoroutine);
                scaleAnimationCoroutine = null;
            }

            if (safetyTimeoutCoroutine != null)
            {
                StopCoroutine(safetyTimeoutCoroutine);
                safetyTimeoutCoroutine = null;
            }

            StopAllCoroutines();
        }
    }

    // ����Ʈ ���� �Լ�
    private void DestroyEffect()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            CleanupAnimation();
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }

    #endregion


}
