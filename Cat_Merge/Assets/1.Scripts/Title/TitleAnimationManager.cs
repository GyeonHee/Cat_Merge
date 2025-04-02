using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class TitleAnimationManager : MonoBehaviour
{


    #region Variables

    [Header("Animation Objects")]
    [SerializeField] private RectTransform mainUIPanel;         // ���� �г�
    [SerializeField] private RectTransform level1CatLeft;       // ���� 1���� �����
    [SerializeField] private RectTransform level1CatRight;      // ������ 1���� �����
    [SerializeField] private RectTransform level2Cat;           // �߾� 2���� �����
    [SerializeField] private RectTransform animationPanel;      // �ִϸ��̼��� ����� �г�

    [SerializeField] private GameObject effectPrefab;           // �ռ� ����Ʈ ������

    [SerializeField] private RectTransform titleNameImage;      // Ÿ��Ʋ �̸� �̹��� (�ʱ���ǥ:(0,1100), �̵���ǥ:(0,700))

    [SerializeField] private List<RectTransform> animationCats; // Ÿ��Ʋ�� �����ӿ� ����̵�

    [Header("Animation Settings")]
    private float waitDuration = 0.5f;                          // ��� �ð�
    private float moveDuration = 1.0f;                          // ����� �̵� �ð�
    private float zoomDuration = 0.5f;                          // ī�޶� �� �ð�

    private Vector2 leftStartPos = new Vector2(-130f, 0f);      // ���� ����� ���� ��ġ (ȭ�� ����)
    private Vector2 rightStartPos = new Vector2(130f, 0f);      // ������ ����� ���� ��ġ (ȭ�� ������)
    private Vector2 centerPos = Vector2.zero;                   // �߾� ��ġ

    private Camera mainCamera;                                  // ī�޶� ����
    private float originalOrthographicSize;                     // ī�޶� ���� ũ��
    private float zoomedOrthographicSize = 2.0f;                // ī�޶� ���� ũ��

    [Header("Title Drop Animation Settings")]
    private float startY = 1300f;                               // ���� Y ��ǥ
    private float endY = 700f;                                  // ���� Y ��ǥ

    [Header("Title Breathing Animation Settings")]
    private float breathingDuration = 3.0f;                     // �� ���� ȣ�� �ֱ� �ð�
    private float minScale = 0.95f;                             // �ּ� ũ�� (95%)
    private float maxScale = 1.05f;                             // �ִ� ũ�� (105%)
    private Coroutine breathingCoroutine;                       // ȣ�� �ִϸ��̼� �ڷ�ƾ ���� �߰�

    [Header("Touch To Start Text Settings")]
    [SerializeField] private TextMeshProUGUI touchToStartText;  // Touch To Start �ؽ�Ʈ
    private float fadeSpeed = 2f;                               // ���̵� �ӵ�
    private float minAlpha = 0.2f;                              // �ּ� ���İ�
    private float maxAlpha = 0.8f;                              // �ִ� ���İ�
    private Coroutine blinkCoroutine;                           // ������ �ڷ�ƾ ����

    [Header("Cat Auto Movement Settings")]
    private bool isAnimating = false;                           // �̵� �ִϸ��̼� ���� ����
    private float autoMoveInterval = 2f;                        // �ڵ� �̵� ����
    private float moveDurationCat = 1.0f;                       // ����� �̵� �ð�

    [Header("readonly Settings")]
    private readonly List<Coroutine> catMoveCoroutines = new List<Coroutine>();
    private static readonly Vector2[] moveDirections = {
        new Vector2(30f, 0f),
        new Vector2(-30f, 0f),
        new Vector2(0f, 30f),
        new Vector2(0f, -30f),
        new Vector2(30f, 30f),
        new Vector2(30f, -30f),
        new Vector2(-30f, 30f),
        new Vector2(-30f, -30f)
    };

    private static readonly Color colorCache = new Color(1f, 1f, 1f, 0f);

    #endregion


    #region Unity Methods

    private void Start()
    {
        InitializeAnimationManager();
        StartCoroutine(PlayTitleAnimation());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        catMoveCoroutines.Clear();
        isAnimating = false;
    }

    #endregion


    #region Initialize

    // �ʱ� ��ġ �� ���� ���� �Լ�
    private void InitializeAnimationManager()
    {
        mainCamera = Camera.main;
        originalOrthographicSize = mainCamera.orthographicSize;

        level1CatLeft.anchoredPosition = leftStartPos;
        level1CatRight.anchoredPosition = rightStartPos;

        InitializeRectTransform(level1CatLeft);
        InitializeRectTransform(level1CatRight);
        InitializeRectTransform(level2Cat);

        level1CatLeft.gameObject.SetActive(true);
        level1CatRight.gameObject.SetActive(true);
        level2Cat.gameObject.SetActive(false);
    }

    // UI ����� �������� �߾����� �����ϴ� �Լ�
    private void InitializeRectTransform(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    #endregion


    #region Animation Sequence

    // Ÿ��Ʋ�� �ִϸ��̼�
    private IEnumerator PlayTitleAnimation()
    {
        // 0. ��� ���
        yield return new WaitForSeconds(waitDuration);

        // 1. ����� �߾����� �̵� + ī�޶� ����
        StartCoroutine(ZoomCamera(zoomedOrthographicSize));
        yield return StartCoroutine(MoveCatsToCenter());

        // 2. �ռ� ����Ʈ ����
        GameObject recallEffect = Instantiate(effectPrefab, level2Cat.transform.position, Quaternion.identity);
        recallEffect.transform.SetParent(animationPanel.transform);
        recallEffect.transform.localScale = Vector3.one * 2f;

        // 3. 1���� ����� ��Ȱ��ȭ �� 2���� ����� Ȱ��ȭ
        level1CatLeft.gameObject.SetActive(false);
        level1CatRight.gameObject.SetActive(false);
        level2Cat.gameObject.SetActive(true);

        // 4. ��� ���
        yield return new WaitForSeconds(waitDuration);

        // 5. ī�޶� �ܾƿ�
        yield return StartCoroutine(ResetZoom());

        // 6. ���� Ÿ��Ʋ �̸� �������� �ִϸ��̼�
        yield return StartCoroutine(TitleDropAnimation());

        // 7. ���� ���� ��ư Ȱ��ȭ
        GetComponent<TitleManager>().EnableStartButton();

        // 8. ���� ���� Text ������ �ִϸ��̼�
        blinkCoroutine = StartCoroutine(BlinkTouchToStartText());

        // 9. Ÿ��Ʋ ���� ȣ�� �ִϸ��̼�
        breathingCoroutine = StartCoroutine(TitleBreathingAnimation());

        // 10. ����̵� �ڵ� �̵� ����
        StartCatAutoMovement();
    }

    #endregion


    #region Title Merge Animation

    // ����� �����̴� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator MoveCatsToCenter()
    {
        float elapsed = 0f;
        Vector2 leftStart = level1CatLeft.anchoredPosition;
        Vector2 rightStart = level1CatRight.anchoredPosition;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            level1CatLeft.anchoredPosition = Vector2.Lerp(leftStart, centerPos, smoothT);
            level1CatRight.anchoredPosition = Vector2.Lerp(rightStart, centerPos, smoothT);

            yield return null;
        }

        level1CatLeft.anchoredPosition = centerPos;
        level1CatRight.anchoredPosition = centerPos;
    }

    // ���� �ڷ�ƾ
    private IEnumerator ZoomCamera(float targetSize)
    {
        float elapsed = 0f;
        float startSize = mainCamera.orthographicSize;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / zoomDuration);
            yield return null;
        }
    }

    // �ܾƿ� �ڷ�ƾ
    private IEnumerator ResetZoom()
    {
        yield return ZoomCamera(originalOrthographicSize);
    }

    #endregion


    #region Title Name Drop

    //// Ÿ��Ʋ ���� ��� �ִϸ��̼� �ڷ�ƾ (1��)
    //private IEnumerator TitleDropAnimation()
    //{
    //    titleNameImage.anchoredPosition = new Vector2(0, startY);

    //    float gravity = 3000f;              // �߷� ���ӵ�
    //    float dampening = 0.4f;             // ƨ�� �� ������ �ս� ���
    //    float velocity = 0f;                // ���� �ӵ�
    //    float currentY = startY;            // ���� Y ��ġ

    //    float maxDuration = 3f;             // �ִ� ���� �ð� (3��)
    //    float elapsedTime = 0f;             // �� ��� �ð�
    //    float stuckTime = 0f;               // ���� ��ġ�� �ӹ��� �ð�
    //    Vector2 lastPosition = titleNameImage.anchoredPosition;

    //    while (elapsedTime < maxDuration)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float deltaTime = Time.deltaTime;

    //        // �߷� ����
    //        velocity -= gravity * deltaTime;

    //        // ���ο� ��ġ ���
    //        currentY += velocity * deltaTime;

    //        // �ٴ� �浹 üũ
    //        if (currentY < endY + 0.01f)
    //        {
    //            currentY = endY;
    //            velocity = -velocity * dampening;
    //        }

    //        // ���� ��ġ ����
    //        Vector2 newPosition = new Vector2(0, currentY);
    //        titleNameImage.anchoredPosition = newPosition;

    //        // ���� ���� ���� �� ����
    //        if (Vector2.Distance(newPosition, lastPosition) < 0.01f)
    //        {
    //            stuckTime += deltaTime;
    //            if (stuckTime > 0.5f)
    //            {
    //                break;
    //            }
    //        }
    //        else
    //        {
    //            stuckTime = 0f;
    //        }

    //        lastPosition = newPosition;
    //        yield return null;
    //    }

    //    titleNameImage.anchoredPosition = new Vector2(0, endY);
    //}

    // Ÿ��Ʋ ���� ��� �ִϸ��̼� �ڷ�ƾ (2��)
    private IEnumerator TitleDropAnimation()
    {
        titleNameImage.anchoredPosition = new Vector2(0, startY);

        // 1. ������ �Ʒ��� �̵�
        float dropDuration = 0.5f;
        float elapsed = 0f;
        Vector2 startPos = new Vector2(0, startY);
        Vector2 overshootPos = new Vector2(0, endY - 60f);

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            titleNameImage.anchoredPosition = Vector2.Lerp(startPos, overshootPos, smoothT);
            yield return null;
        }

        // 2. �Ʒ����� ���� �ݵ�
        float bounceUpDuration = 0.3f;
        elapsed = 0f;
        startPos = titleNameImage.anchoredPosition;
        Vector2 bouncePos = new Vector2(0, endY + 30f);

        while (elapsed < bounceUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceUpDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            titleNameImage.anchoredPosition = Vector2.Lerp(startPos, bouncePos, smoothT);
            yield return null;
        }

        // 3. ������ ����
        float settleDuration = 0.2f;
        elapsed = 0f;
        startPos = titleNameImage.anchoredPosition;
        Vector2 finalPos = new Vector2(0, endY);

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / settleDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            titleNameImage.anchoredPosition = Vector2.Lerp(startPos, finalPos, smoothT);
            yield return null;
        }

        titleNameImage.anchoredPosition = finalPos;
    }

    #endregion


    #region Touch To Start Text Animation

    // ���� ���� Text ������ �ִϸ��̼� �ڷ�ƾ
    private IEnumerator BlinkTouchToStartText()
    {
        Color textColor = touchToStartText.color;
        touchToStartText.color = colorCache;

        Color maxColor = new Color(textColor.r, textColor.g, textColor.b, maxAlpha);
        Color minColor = new Color(textColor.r, textColor.g, textColor.b, minAlpha);

        while (true)
        {
            // ���̵� �� (minAlpha -> maxAlpha)
            float elapsedTime = 0f;
            Color startColor = touchToStartText.color;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                touchToStartText.color = Color.Lerp(startColor, maxColor, elapsedTime);
                yield return null;
            }

            // ���̵� �ƿ� (maxAlpha -> minAlpha)
            elapsedTime = 0f;
            startColor = touchToStartText.color;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                touchToStartText.color = Color.Lerp(startColor, minColor, elapsedTime);
                yield return null;
            }
        }
    }

    // ���� ���� Text ������ �ִϸ��̼� �ڷ�ƾ �ߴ� �Լ�
    public void StopBlinkAnimation()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);

            touchToStartText.color = colorCache;
        }
    }

    #endregion


    #region Title Breathing Animation

    // Ÿ��Ʋ ���� ȣ�� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator TitleBreathingAnimation()
    {
        Vector3 originalScale = titleNameImage.localScale;
        float currentScale = 1.0f;
        bool isIncreasing = true;

        while (true)
        {
            float elapsed = 0f;
            float startScale = currentScale;
            float targetScale = isIncreasing ? maxScale : minScale;

            while (elapsed < breathingDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (breathingDuration / 2);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                currentScale = Mathf.Lerp(startScale, targetScale, smoothT);
                titleNameImage.localScale = originalScale * currentScale;

                yield return null;
            }

            isIncreasing = !isIncreasing;  // ���� ��ȯ
        }
    }

    // Ÿ��Ʋ ���� ȣ�� �ִϸ��̼� �ڷ�ƾ �ߴ� �Լ�
    public void StopBreathingAnimation()
    {
        if (breathingCoroutine != null)
        {
            StopCoroutine(breathingCoroutine);
        }
    }

    #endregion


    #region Cat Auto Movement

    // �ڵ��̵� ���� �Լ�
    private void StartCatAutoMovement()
    {
        catMoveCoroutines.Clear();

        foreach (RectTransform cat in animationCats)
        {
            catMoveCoroutines.Add(StartCoroutine(AutoMoveCat(cat)));
        }
    }

    // �ڵ��̵� �ڷ�ƾ
    private IEnumerator AutoMoveCat(RectTransform catTransform)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(autoMoveInterval * 0.8f, autoMoveInterval * 1.2f));

            if (!isAnimating)
            {
                Vector3 randomDirection = GetRandomDirection();
                Vector3 targetPosition = (Vector3)catTransform.anchoredPosition + randomDirection;

                targetPosition = AdjustPositionToBounds(targetPosition);

                yield return StartCoroutine(SmoothMoveCat(catTransform, targetPosition));
            }
        }
    }

    // ���� ��ġ ��ǥ �Լ�
    private Vector3 GetRandomDirection()
    {
        return moveDirections[Random.Range(0, moveDirections.Length)];
    }

    // ȭ�� ������ ����� �ʰ� �����ϴ�  �Լ�
    private Vector3 AdjustPositionToBounds(Vector3 position)
    {
        Vector3 adjustedPosition = position;
        float padding = 50f;

        // mainUIPanel�� ũ�⸦ �������� ��� ����
        Rect panelRect = mainUIPanel.rect;
        float minX = panelRect.xMin + padding;
        float maxX = panelRect.xMax - padding;
        float minY = panelRect.yMin + padding;
        float maxY = panelRect.yMax - padding;

        adjustedPosition.x = Mathf.Clamp(adjustedPosition.x, minX, maxX);
        adjustedPosition.y = Mathf.Clamp(adjustedPosition.y, minY, maxY);

        return adjustedPosition;
    }

    // ����� �ε巴�� �̵��ϴ� �ڷ�ƾ
    private IEnumerator SmoothMoveCat(RectTransform catTransform, Vector3 targetPosition)
    {
        isAnimating = true;

        Vector3 startPosition = catTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < moveDurationCat)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDurationCat;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            catTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, smoothT);
            yield return null;
        }

        catTransform.anchoredPosition = targetPosition;
        isAnimating = false;
    }

    // �ڵ� �̵��� �����ϴ� �Լ�
    public void StopCatAutoMovement()
    {
        foreach (Coroutine coroutine in catMoveCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        catMoveCoroutines.Clear();

        isAnimating = false;
    }

    #endregion


}
