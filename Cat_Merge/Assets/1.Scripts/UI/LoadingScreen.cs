using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

// �ε� ȭ�� ��ũ��Ʈ
public class LoadingScreen : MonoBehaviour
{


    #region Variables

    public static LoadingScreen Instance { get; private set; }

    [Header("---[UI Components]")]
    [SerializeField] private RectTransform circleImage;         // �߾� ���� �̹���
    [SerializeField] private RectTransform[] borderImages;      // 8���� �׵θ� �̹���

    [Header("---[Canvas Settings]")]
    private Canvas loadingScreenCanvas;
    private CanvasScaler canvasScaler;
    [HideInInspector] public float animationDuration = 1.25f;

    [Header("---[Animation Settings]")]
    private Coroutine currentAnimation;
    private Vector2 maxMaskSize;
    private Vector2 minMaskSize = Vector2.zero;

    private Vector2[] borderOffsets = new Vector2[8];           // �׵θ� �̹������� �ʱ� ��ġ ������ (�ð�������� �»�ܺ���)

    [Header("---[ETC]")]
    private static readonly Vector2 anchorValue = new Vector2(0.5f, 0.5f);
    private static readonly Vector2 zeroVector = Vector2.zero;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeLoadingScreen();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // �� �ε� �Ϸ� �� ī�޶� ������Ʈ�ϴ� �Լ�
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateLoadingScreenCamera();
    }

    #endregion


    #region Initialize

    // �ε� ��ũ�� �ʱ�ȭ �Լ�
    private void InitializeLoadingScreen()
    {
        loadingScreenCanvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 1f;

        maxMaskSize = new Vector2(20000, 20000);

        // �߾� ���� �̹��� ����
        circleImage.anchorMin = anchorValue;
        circleImage.anchorMax = anchorValue;
        circleImage.pivot = anchorValue;
        circleImage.anchoredPosition = zeroVector;
        circleImage.sizeDelta = maxMaskSize;

        // �׵θ� �̹������� �ʱ� ������ ����
        SetupBorderOffsets();
        SetupBorderImages();

        gameObject.SetActive(false);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // �׵θ� �̹��� ������ ���� �Լ�
    private void SetupBorderOffsets()
    {
        float size = maxMaskSize.x;
        borderOffsets[0] = new Vector2(-size, size);    // �»�
        borderOffsets[1] = new Vector2(0, size);        // ��
        borderOffsets[2] = new Vector2(size, size);     // ���

        borderOffsets[3] = new Vector2(-size, 0);       // ��
        borderOffsets[4] = new Vector2(size, 0);        // ��

        borderOffsets[5] = new Vector2(-size, -size);   // ����
        borderOffsets[6] = new Vector2(0, -size);       // ��
        borderOffsets[7] = new Vector2(size, -size);    // ����
    }

    // �׵θ� �̹��� �⺻ ���� �Լ�
    private void SetupBorderImages()
    {
        for (int i = 0; i < borderImages.Length; i++)
        {
            borderImages[i].anchorMin = anchorValue;
            borderImages[i].anchorMax = anchorValue;
            borderImages[i].pivot = anchorValue;
            borderImages[i].sizeDelta = maxMaskSize;
        }
        UpdateBorderPositions(maxMaskSize);
    }

    #endregion


    #region UI Management

    // �׵θ� �̹��� ��ġ ������Ʈ �Լ�
    private void UpdateBorderPositions(Vector2 currentSize)
    {
        float currentHalf = currentSize.x * 0.5f;
        float borderHalf = maxMaskSize.x * 0.5f;
        Vector2 basePosition = circleImage.anchoredPosition;

        for (int i = 0; i < borderImages.Length; i++)
        {
            Vector2 offset;
            borderImages[i].sizeDelta = maxMaskSize;

            switch (i)
            {
                case 0: // �»�
                    offset = new Vector2(-borderHalf - currentHalf, borderHalf + currentHalf);
                    break;
                case 1: // ��
                    offset = new Vector2(0, borderHalf + currentHalf);
                    break;
                case 2: // ���
                    offset = new Vector2(borderHalf + currentHalf, borderHalf + currentHalf);
                    break;
                case 3: // ��
                    offset = new Vector2(-borderHalf - currentHalf, 0);
                    break;
                case 4: // ��
                    offset = new Vector2(borderHalf + currentHalf, 0);
                    break;
                case 5: // ����
                    offset = new Vector2(-borderHalf - currentHalf, -borderHalf - currentHalf);
                    break;
                case 6: // ��
                    offset = new Vector2(0, -borderHalf - currentHalf);
                    break;
                case 7: // ����
                    offset = new Vector2(borderHalf + currentHalf, -borderHalf - currentHalf);
                    break;
                default:
                    offset = Vector2.zero;
                    break;
            }

            borderImages[i].anchoredPosition = basePosition + offset;
        }
    }

    // �ε� ��ũ�� ī�޶� ������Ʈ �Լ�
    public void UpdateLoadingScreenCamera()
    {
        if (loadingScreenCanvas != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                loadingScreenCanvas.worldCamera = mainCamera;
                loadingScreenCanvas.planeDistance = 100f;
            }
        }
    }

    #endregion


    #region Animation Control

    // �ε� ��ũ�� ǥ��/���� ���� �Լ�
    public void Show(bool show, Vector2 position = default)
    {
        if (gameObject != null)
        {
            gameObject.SetActive(true);

            // ��ġ ���� (position�� default(0,0)�� �ƴ� ����)
            if (position != default)
            {
                // Canvas ��ǥ��� ��ȯ
                Vector2 canvasPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)loadingScreenCanvas.transform,
                    position,
                    loadingScreenCanvas.worldCamera,
                    out canvasPosition
                );

                // �߾� �̹����� ��� Border �̹����� �θ� ����
                circleImage.anchoredPosition = canvasPosition;
                foreach (var borderImage in borderImages)
                {
                    borderImage.anchoredPosition = canvasPosition;
                }
            }
            else
            {
                // �⺻������ �߾� ����
                circleImage.anchoredPosition = zeroVector;
                foreach (var borderImage in borderImages)
                {
                    borderImage.anchoredPosition = zeroVector;
                }
            }

            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }

            if (show)
            {
                currentAnimation = StartCoroutine(LoadingAnimationCoroutine());
            }
            else
            {
                currentAnimation = StartCoroutine(ExitAnimationCoroutine());
            }

            if (!GoogleManager.Instance.isDeleting)
            {
                Time.timeScale = show ? 0f : 1f;
            }

            if (show)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    // �ε� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator LoadingAnimationCoroutine()
    {
        circleImage.sizeDelta = maxMaskSize;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;
            progress = progress < 0.5f ? 2f * progress * progress : -1f + (4f - 2f * progress) * progress;

            Vector2 currentSize = Vector2.Lerp(maxMaskSize, minMaskSize, progress);
            circleImage.sizeDelta = currentSize;
            UpdateBorderPositions(currentSize);
            yield return null;
        }

        circleImage.sizeDelta = minMaskSize;
        UpdateBorderPositions(minMaskSize);
    }

    // ���� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator ExitAnimationCoroutine()
    {
        circleImage.sizeDelta = minMaskSize;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;
            progress = progress < 0.5f ? 2f * progress * progress : -1f + (4f - 2f * progress) * progress;

            Vector2 currentSize = Vector2.Lerp(minMaskSize, maxMaskSize, progress);
            circleImage.sizeDelta = currentSize;
            UpdateBorderPositions(currentSize);
            yield return null;
        }

        circleImage.sizeDelta = maxMaskSize;
        UpdateBorderPositions(maxMaskSize);
        gameObject.SetActive(false);
    }

    #endregion


}
