using UnityEngine;
using TMPro;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI touchToStartText;
    private Coroutine blinkCoroutine;

    private void Start()
    {
        blinkCoroutine = StartCoroutine(BlinkText());
    }

    // ���� ���� �� ȣ��� �޼���
    public void OnGameStart()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        // �ؽ�Ʈ ���İ��� 0���� ����
        Color textColor = touchToStartText.color;
        touchToStartText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
    }

    // �ؽ�Ʈ ������ �ڷ�ƾ
    private IEnumerator BlinkText()
    {
        // ������ ���� ������
        float fadeSpeed = 2f;        // ���̵� �ӵ�
        float minAlpha = 0.2f;       // �ּ� ���İ�
        float maxAlpha = 1f;         // �ִ� ���İ�

        while (true)
        {
            // ���̵� �ƿ�
            float elapsedTime = 0f;
            Color startColor = touchToStartText.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, minAlpha);

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                touchToStartText.color = Color.Lerp(startColor, endColor, elapsedTime);
                yield return null;
            }

            // ���̵� ��
            elapsedTime = 0f;
            startColor = touchToStartText.color;
            endColor = new Color(startColor.r, startColor.g, startColor.b, maxAlpha);

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                touchToStartText.color = Color.Lerp(startColor, endColor, elapsedTime);
                yield return null;
            }
        }
    }

}
