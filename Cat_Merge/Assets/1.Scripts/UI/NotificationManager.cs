using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private GameObject notificationPanel;      // Notification Panel
    [SerializeField] private RectTransform panelTransform;      // Panel RectTransform
    [SerializeField] private TextMeshProUGUI notificationText;  // Text (TMP)
    [SerializeField] private CanvasGroup canvasGroup;           // CanvasGroup for fading

    private Vector2 startPosition = new Vector2(0, 144);        // Panel ������ġ
    private Vector2 endPosition = new Vector2(0, 96);           // Panel ������ġ
    private Coroutine currentCoroutine;                         // ���� ���� ���� �ڷ�ƾ

    // ======================================================================================================================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        notificationPanel.SetActive(false);
        canvasGroup.alpha = 0;
    }

    // ======================================================================================================================================================================

    /// <summary> �˸� ǥ�� �Լ� (string �޽���) </summary>
    public void ShowNotification(string message)
    {
        notificationText.text = message;
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(NotificationRoutine());
    }

    // �˸� �г� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator NotificationRoutine()
    {
        // �˸� �г� Ȱ��ȭ
        notificationPanel.SetActive(true);
        panelTransform.anchoredPosition = startPosition;
        canvasGroup.alpha = 1;

        // �г� �̵�
        float elapsedTime = 0f;
        float moveDuration = 0.2f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            panelTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        // ���� ��ġ���� ����
        yield return new WaitForSeconds(0.3f);

        // �г� ����ȭ
        elapsedTime = 0f;
        float fadeDuration = 0.5f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        // �˸� �г� ��Ȱ��ȭ
        notificationPanel.SetActive(false);
        currentCoroutine = null;
    }
}


/*
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private GameObject notificationPanel;      // Notification Panel
    [SerializeField] private RectTransform panelTransform;      // Panel RectTransform
    [SerializeField] private TextMeshProUGUI notificationText;  // Text (TMP)
    [SerializeField] private CanvasGroup canvasGroup;           // CanvasGroup for fading

    private Vector2 startPosition = new Vector2(0, 144);        // Panel ������ġ
    private Vector2 endPosition = new Vector2(0, 96);           // Panel ������ġ
    private Coroutine currentCoroutine;                         // ���� ���� ���� �ڷ�ƾ
    private string currentMessage;                              // ���� ǥ�� ���� �޽���

    // �ʱ�ȭ �� �г��� ��Ȱ��ȭ
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        notificationPanel.SetActive(false);
        canvasGroup.alpha = 0;
    }

    /// <summary> �˸� ǥ�� �Լ� (string �޽���) </summary>
    public void ShowNotification(string message)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        if (message == currentMessage)
        {
            currentCoroutine = StartCoroutine(NotificationRoutine(false));
        }
        else
        {
            notificationText.text = message;
            currentMessage = message;
            currentCoroutine = StartCoroutine(NotificationRoutine(true));
        }
    }

    private IEnumerator NotificationRoutine(bool includeMoveAnimation)
    {
        // �˸� �г� Ȱ��ȭ
        notificationPanel.SetActive(true);
        panelTransform.anchoredPosition = startPosition;
        canvasGroup.alpha = 1;

        // �г� �̵�
        float elapsedTime = 0f;
        float moveDuration = 0.2f;
        if (includeMoveAnimation)
        {
            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / moveDuration;
                panelTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
                yield return null;
            }
        }
        else
        {
            panelTransform.anchoredPosition = endPosition;
        }

        // ���� ��ġ���� ����
        yield return new WaitForSeconds(0.3f);

        // �г� ����ȭ
        elapsedTime = 0f;
        float fadeDuration = 0.5f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        // �˸� �г� ��Ȱ��ȭ
        notificationPanel.SetActive(false);
        currentCoroutine = null;
        currentMessage = null;
    }
}
*/