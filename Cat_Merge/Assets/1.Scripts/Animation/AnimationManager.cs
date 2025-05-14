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

    #endregion


    #region Unity Methods

    private void Start()
    {
        StartCoroutine(ScaleAnimation());
        StartCoroutine(SafetyTimeout());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    #endregion


    #region Animation Methods

    // �ִ� ���� �ð� ���� ������Ʈ�� �����ϴ� �ڷ�ƾ
    private IEnumerator SafetyTimeout()
    {
        yield return new WaitForSeconds(maxLifetime);
        Destroy(gameObject);
    }

    // ũ�� ���� �� ���̵� �ƿ� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator ScaleAnimation()
    {
        // 1. ũ�� 30 �� 175 (���� 1 ����)
        // 2. ũ�� 175 �� 150 (���� 1 �� 0)
        // 3. �ִϸ��̼��� ������ �����ų� ������ �߻����� �� ������Ʈ ����

        SetAlpha(1f);
        yield return StartCoroutine(ChangeSize(spriteRect, new Vector2(30, 30), new Vector2(175, 175), duration, false));
        yield return StartCoroutine(ChangeSize(spriteRect, new Vector2(175, 175), new Vector2(150, 150), duration * 0.5f, true));
        Destroy(gameObject);
    }

    // ������ �ð� ���� ũ��� ������ �����ϴ� �ڷ�ƾ
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

    // ��������Ʈ �̹����� ���� ���� �Լ�
    private void SetAlpha(float alpha)
    {
        if (spriteImage != null)
        {
            Color color = spriteImage.color;
            color.a = alpha;
            spriteImage.color = color;
        }
    }

    #endregion


}