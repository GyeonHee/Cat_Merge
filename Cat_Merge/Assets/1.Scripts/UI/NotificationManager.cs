using UnityEngine;
using TMPro;
using System.Collections;

// �˸�(��: ��ȭ�� �����մϴ�!!) ��ũ��Ʈ
public class NotificationManager : MonoBehaviour
{


    #region Variables

    public static NotificationManager Instance { get; private set; }

    [Header("---[UI Components]")]
    [SerializeField] private GameObject notificationPanel;              // Notification Panel
    [SerializeField] private RectTransform panelTransform;              // Panel RectTransform
    [SerializeField] private TextMeshProUGUI notificationText;          // Text (TMP)
    [SerializeField] private CanvasGroup canvasGroup;                   // CanvasGroup for fading

    [Header("---[Position Settings]")]
    private readonly Vector2 startPosition = new Vector2(0, 144);       // Panel ������ġ
    private readonly Vector2 endPosition = new Vector2(0, 96);          // Panel ������ġ

    [Header("---[Animation Settings]")]
    private readonly WaitForSeconds waitNotificationDuration = new WaitForSeconds(0.3f);    // �˸� ǥ�� ��� �ð�
    private Coroutine currentCoroutine;                         // ���� ���� ���� �ڷ�ƾ

    #endregion


    #region Unity Methods

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

    #endregion


    #region Notification Control

    // �˸� ǥ�� �Լ� (string �޽���)
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
        yield return waitNotificationDuration;

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

    #endregion


}
