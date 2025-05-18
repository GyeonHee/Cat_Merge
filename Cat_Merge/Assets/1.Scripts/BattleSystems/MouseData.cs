using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MouseData : MonoBehaviour
{


    #region Variables

    [HideInInspector] public Mouse mouseData;   // �� ������
    private Image mouseImage;                   // �� �̹���
    private RectTransform rectTransform;        // RectTransform ����

    [Header("---[Damage Text]")]
    [SerializeField] private GameObject damageTextPrefab;   // ������ �ؽ�Ʈ ������
    private Queue<GameObject> damageTextPool;               // ������ �ؽ�Ʈ ������Ʈ Ǯ
    private const int POOL_SIZE = 200;                      // Ǯ ������

    private const float DAMAGE_TEXT_START_Y = 200f;         // ������ �ؽ�Ʈ ���� Y ��ġ
    private const float DAMAGE_TEXT_MOVE_DISTANCE = 50f;    // ������ �ؽ�Ʈ �̵� �Ÿ�
    private const float DAMAGE_TEXT_DURATION = 1f;          // ������ �ؽ�Ʈ ���� �ð�

    private readonly Vector3 damageTextMoveOffset = Vector3.up * DAMAGE_TEXT_MOVE_DISTANCE;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        mouseImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        InitializeDamageTextPool();
    }

    private void Start()
    {
        UpdateMouseUI();
    }

    #endregion


    #region Pool Management

    // ������ �ؽ�Ʈ Ǯ �ʱ�ȭ �Լ�
    private void InitializeDamageTextPool()
    {
        damageTextPool = new Queue<GameObject>();
        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject damageTextObj = Instantiate(damageTextPrefab, transform);
            damageTextObj.SetActive(false);
            damageTextPool.Enqueue(damageTextObj);
        }
    }

    #endregion


    #region Mouse Management

    // MouseUI �ֽ�ȭ�ϴ� �Լ�
    public void UpdateMouseUI()
    {
        mouseImage.sprite = mouseData.MouseImage;
    }

    // Mouse ������ ���� �Լ�
    public void SetMouseData(Mouse mouse)
    {
        mouseData = mouse;
        UpdateMouseUI();
    }

    #endregion


    #region Damage Text Management

    // ������ �ؽ�Ʈ ���� �Լ�
    public void ShowDamageText(float damage)
    {
        GameObject damageTextObj;
        if (damageTextPool.Count > 0)
        {
            damageTextObj = damageTextPool.Dequeue();
        }
        else
        {
            // Ǯ�� ��������� ���� ������ �ؽ�Ʈ�� ��Ȱ��
            damageTextObj = transform.GetChild(0).gameObject;
            transform.GetChild(0).SetSiblingIndex(transform.childCount - 1);
        }

        damageTextObj.SetActive(true);

        RectTransform textRect = damageTextObj.GetComponent<RectTransform>();
        Vector3 startPosition = rectTransform.anchoredPosition;
        startPosition.y = DAMAGE_TEXT_START_Y;
        textRect.anchoredPosition = startPosition;

        TextMeshProUGUI damageText = damageTextObj.GetComponent<TextMeshProUGUI>();
        damageText.text = GameManager.Instance.FormatNumber((decimal)damage);

        StartCoroutine(AnimateDamageText(damageTextObj, textRect));
    }

    // ������ �ؽ�Ʈ �ִϸ��̼� �ڷ�ƾ
    private IEnumerator AnimateDamageText(GameObject textObj, RectTransform textRect)
    {
        float elapsedTime = 0f;
        Vector3 startPos = textRect.anchoredPosition;
        Vector3 endPos = startPos + damageTextMoveOffset;

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        Color originalColor = text.color;

        while (elapsedTime < DAMAGE_TEXT_DURATION)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / DAMAGE_TEXT_DURATION;

            textRect.anchoredPosition = Vector3.Lerp(startPos, endPos, progress);

            Color newColor = originalColor;
            newColor.a = 1 - progress;
            text.color = newColor;

            yield return null;
        }

        textObj.SetActive(false);
        damageTextPool.Enqueue(textObj);
    }

    #endregion


}
