using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{


    public static LoadingScreen Instance { get; private set; }

    [SerializeField] private RectTransform circleImage;     // �߾� ���� �̹���
    [SerializeField] private RectTransform[] borderImages;  // 8���� �׵θ� �̹���

    private Canvas loadingScreenCanvas;
    private CanvasScaler canvasScaler;
    [HideInInspector] public float animationDuration = 1.25f;
    private Coroutine currentAnimation;
    private Vector2 maxMaskSize;
    private Vector2 minMaskSize = Vector2.zero;

    // �׵θ� �̹������� �ʱ� ��ġ ������ (�ð�������� �»�ܺ���)
    private Vector2[] borderOffsets = new Vector2[8];




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




    private void InitializeLoadingScreen()
    {
        loadingScreenCanvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 1f;

        maxMaskSize = new Vector2(4000, 4000);

        // �߾� ���� �̹��� ����
        circleImage.anchorMin = new Vector2(0.5f, 0.5f);
        circleImage.anchorMax = new Vector2(0.5f, 0.5f);
        circleImage.pivot = new Vector2(0.5f, 0.5f);
        circleImage.anchoredPosition = Vector2.zero;
        circleImage.sizeDelta = maxMaskSize;

        // �׵θ� �̹������� �ʱ� ������ ����
        SetupBorderOffsets();
        SetupBorderImages();

        gameObject.SetActive(false);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

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

    private void SetupBorderImages()
    {
        for (int i = 0; i < borderImages.Length; i++)
        {
            borderImages[i].anchorMin = new Vector2(0.5f, 0.5f);
            borderImages[i].anchorMax = new Vector2(0.5f, 0.5f);
            borderImages[i].pivot = new Vector2(0.5f, 0.5f);
            borderImages[i].sizeDelta = maxMaskSize;
        }
        UpdateBorderPositions(maxMaskSize);
    }

    private void UpdateBorderPositions(Vector2 currentSize)
    {
        float currentHalf = currentSize.x * 0.5f;
        float borderHalf = maxMaskSize.x * 0.5f;
        Vector2 basePosition = circleImage.anchoredPosition; // ���� ��ġ ���

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

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateLoadingScreenCamera();
    }

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
                circleImage.anchoredPosition = Vector2.zero;
                foreach (var borderImage in borderImages)
                {
                    borderImage.anchoredPosition = Vector2.zero;
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

            if (!GoogleManager.Instance.isDeletingData)
            {
                Time.timeScale = show ? 0f : 1f;
            }

            if (show)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

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



}
