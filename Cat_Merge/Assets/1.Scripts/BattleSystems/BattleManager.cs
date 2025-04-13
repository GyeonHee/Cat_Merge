using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

// BattleManager Script
public class BattleManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static BattleManager Instance { get; private set; }

    [Header("---[Battle System]")]
    [SerializeField] private GameObject bossPrefab;             // ���� ������
    [SerializeField] private Transform bossUIParent;            // ������ ��ġ�� �θ� Transform (UI Panel ��)
    [SerializeField] private Slider respawnSlider;              // ���� ��ȯ���� ���� �ð��� ǥ���� Slider UI

    private const float DEFAULT_SPAWN_INTERVAL = 20f;          // ���� ���� �ֱ�
    private float spawnInterval;                                // ���� ���� �ֱ�
    private Coroutine respawnSliderCoroutine;                   // Slider �ڷ�ƾ
    private float bossSpawnTimer = 0f;                          // ���� ���� Ÿ�̸�
    private float sliderDuration;                               // Slider ���� �ð�
    private const float DEFAULT_BOSS_DURATION = 30f;            // ���� ���� �ð�
    private float bossDuration;                                 // ���� ���� �ð�
    private int bossStage = 1;                                  // ���� ��������
    public int BossStage => bossStage;

    private const float BOSS_ATTACK_DELAY = 2f;                 // ���� ���� ������
    private const float CAT_ATTACK_DELAY = 1f;                  // ����� ���� ������
    private const float GIVEUP_BUTTON_DELAY = 2f;               // �׺� ��ư Ȱ��ȭ ������

    private GameObject currentBoss;                             // ���� ����
    [HideInInspector] public BossHitbox bossHitbox;             // ���� ��Ʈ�ڽ�
    private bool isBattleActive;                                // ���� Ȱ��ȭ ����
    public bool IsBattleActive => isBattleActive;

    private HashSet<int> clearedStages = new HashSet<int>();    // Ŭ������ �������� ����

    [Header("---[Boss UI]")]
    [SerializeField] private GameObject battleHPUI;             // Battle HP UI (Ȱ��ȭ/��Ȱ��ȭ ����)
    [SerializeField] private TextMeshProUGUI bossStageText;     // Boss Stage Text
    [SerializeField] private Slider bossHPSlider;               // HP Slider
    [SerializeField] private TextMeshProUGUI bossHPText;        // HP % Text
    [SerializeField] private Button giveupButton;               // �׺� ��ư

    private Mouse currentBossData;                              // ���� ���� ������
    private double currentBossHP;                               // ������ ���� HP
    private double maxBossHP;                                   // ������ �ִ� HP


    [Header("---[Boss Result UI]")]
    [SerializeField] private GameObject battleResultPanel;              // ���� ��� �г�
    [SerializeField] private GameObject winPanel;                       // �¸� UI �г�
    [SerializeField] private GameObject losePanel;                      // �й� UI �г�
    [SerializeField] private Button battleResultCloseButton;            // ���� ��� �г� �ݱ� ��ư
    [SerializeField] private TextMeshProUGUI battleResultCountdownText; // ���� ��� �г� ī��Ʈ�ٿ� Text
    private Coroutine resultPanelCoroutine;                             // ��� �г� �ڵ� �ݱ� �ڷ�ƾ

    [SerializeField] private GameObject rewardSlotPrefab;          // Reward Slot ������
    [SerializeField] private Transform winRewardPanel;             // Win Panel�� Reward Panel
    [SerializeField] private Transform loseRewardPanel;            // Lose Panel�� Reward Panel
    private Sprite cashSprite;                                     // ĳ�� �̹���
    private Sprite coinSprite;                                     // ���� �̹���
    private List<GameObject> activeRewardSlots = new List<GameObject>();  // ���� Ȱ��ȭ�� ���� ���Ե�

    [Header("---[Boss AutoRetry UI]")]
    [SerializeField] private Button autoRetryPanelButton;       // ���� �ܰ� �ڵ� ���� �г� ��ư
    [SerializeField] private Image autoRetryPanelButtonImage;   // �г� ��ư �̹���
    [SerializeField] private GameObject autoRetryPanel;         // ���� �ܰ� �ڵ� ���� �г�
    [SerializeField] private Button closeAutoRetryPanelButton;  // ���� �ܰ� �ڵ� ���� �г� �ݱ� ��ư
    [SerializeField] private Button autoRetryButton;            // ���� �ܰ� �ڵ� ���� ��� ��ư
    [SerializeField] private RectTransform autoRetryHandle;     // ��� �ڵ�
    [SerializeField] private Image autoRetryButtonImage;        // ���� �ܰ� �ڵ� ���� ��ư �̹���
    private int currentMaxBossStage;                            // ���� ������ ���� �ִ� ��������
    private bool isAutoRetryEnabled;                            // ���� �ܰ� �ڵ� ���� ����
    private Coroutine autoRetryToggleCoroutine;                 // ��� �ִϸ��̼� �ڷ�ƾ


    [Header("---[Warning UI]")]
    [SerializeField] private GameObject warningPanel;           // �����ý��� ���۽� ������ ��� Panel (warningDuration���� ����)
    [SerializeField] private Slider warningSlider;              // �������ð��� ������ �������� Slider (warningDuration��ŭ ������)
    public float warningDuration = 2f;                          // warningPanel Ȱ��ȭ �ð�
    private Coroutine warningSliderCoroutine;                   // warningSlider �ڷ�ƾ

    [SerializeField] private Image topWarningImage;             // ��� ��� �̹���
    [SerializeField] private Image bossDangerImage;             // ���� ���� �̹���
    [SerializeField] private Image bottomWarningImage;          // �ϴ� ��� �̹���
    private CanvasGroup warningPanelCanvasGroup;                // Warning Panel�� CanvasGroup
    private CanvasGroup topWarningCanvasGroup;                  // ��� ��� �̹����� CanvasGroup
    private CanvasGroup bossDangerCanvasGroup;                  // ���� ���� �̹����� CanvasGroup  
    private CanvasGroup bottomWarningCanvasGroup;               // �ϴ� ��� �̹����� CanvasGroup

    private const float WARNING_IMAGE_START_X = -640f;          // ��� �̹��� ���� ��ǥ
    private const float WARNING_IMAGE_END_X = 640f;             // ��� �̹��� �� ��ǥ
    private const float BOSS_DANGER_START_X = 1040f;            // Boss Danger �̹��� ���� ��ǥ
    private const float BOSS_DANGER_END_X = -1040f;             // Boss Danger �̹��� �� ��ǥ
    private const float WARNING_IMAGE_Y = 0f;                   // ��� �̹��� Y ��ǥ


    [Header("---[ETC]")]
    private Coroutine bossBattleCoroutine;                      // BossBattleRoutine �ڷ�ƾ ������ ���� ���� �߰�
    private Coroutine bossSpawnRoutine;                         // BossSpawnRoutine �ڷ�ƾ ������ ���� ���� �߰�
    private Coroutine bossAttackRoutine;                        // BossAttackRoutine �ڷ�ƾ ������ ���� ���� �߰�
    private Coroutine catsAttackRoutine;                        // CatsAttackRoutine �ڷ�ƾ ������ ���� ���� �߰�


    private bool isDataLoaded = false;                          // ������ �ε� Ȯ��

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
    }

    private void Start()
    {
        InitializeBattleManager();

        // GoogleManager���� �����͸� �ε����� ���� ��쿡�� �ʱ�ȭ
        if (!isDataLoaded)
        {
            bossStage = 1;
            currentMaxBossStage = bossStage;

            isAutoRetryEnabled = false;
        }

        UpdateAutoRetryUI(isAutoRetryEnabled, true);
        bossSpawnRoutine = StartCoroutine(BossSpawnRoutine());
    }

    #endregion


    #region Initialization

    // BattleManager �ʱ� ����
    private void InitializeBattleManager()
    {
        InitializeWarningPanel();
        InitializeTimers();
        InitializeSliders();
        InitializeBattleUI();
        InitializeCanvasGroups();
        InitializeButtonListeners();
        InitializeRewardSprites();
    }

    // warningPanel �ʱ�ȭ �Լ�
    private void InitializeWarningPanel()
    {
        warningPanel.SetActive(false);
    }

    // �ð� �ʱ�ȭ �Լ� (���� ������, ���� ���� �ð�)
    private void InitializeTimers()
    {
        spawnInterval = DEFAULT_SPAWN_INTERVAL - warningDuration;
        bossDuration = DEFAULT_BOSS_DURATION;
        sliderDuration = bossDuration - warningDuration;
    }

    // Sliders UI �ʱ�ȭ �Լ�
    private void InitializeSliders()
    {
        if (respawnSlider != null)
        {
            respawnSliderCoroutine = null;
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        if (warningSlider != null)
        {
            warningSliderCoroutine = null;
            warningSlider.maxValue = warningDuration;
            warningSlider.value = 0f;
        }
    }

    // Battle HP UI �ʱ�ȭ �Լ�
    private void InitializeBattleUI()
    {
        if (battleHPUI != null)
        {
            battleHPUI.SetActive(false);
        }
    }

    // ��� UI ���� CanvasGroup �ʱ�ȭ �Լ�
    private void InitializeCanvasGroups()
    {
        warningPanelCanvasGroup = SetupCanvasGroup(warningPanel);
        topWarningCanvasGroup = SetupCanvasGroup(topWarningImage.gameObject);
        bossDangerCanvasGroup = SetupCanvasGroup(bossDangerImage.gameObject);
        bottomWarningCanvasGroup = SetupCanvasGroup(bottomWarningImage.gameObject);
    }

    // UI ������Ʈ�� CanvasGroup ������Ʈ�� ������ �߰��ϰ� ��ȯ�ϴ� �Լ�
    private CanvasGroup SetupCanvasGroup(GameObject uiObject)
    {
        // ���� CanvasGroup�� ������ ���, ������ ���� �߰�
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = uiObject.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    // ��ư ������ �ʱ�ȭ �Լ�
    private void InitializeButtonListeners()
    {
        giveupButton.onClick.AddListener(GiveUpState);
        battleResultCloseButton.onClick.AddListener(CloseBattleResultPanel);

        autoRetryPanelButton.onClick.AddListener(OpenAutoRetryPanel);
        autoRetryButton.onClick.AddListener(ToggleAutoRetry);
        closeAutoRetryPanelButton.onClick.AddListener(CloseAutoRetryPanel);

        autoRetryPanelButtonImage = autoRetryPanelButton.GetComponent<Image>();
        autoRetryButtonImage = autoRetryButton.GetComponent<Image>();
        UpdateToggleButtonImage(autoRetryButtonImage, isAutoRetryEnabled);
        UpdateAutoRetryPanelButtonColor(isAutoRetryEnabled);
        UpdateAutoRetryUI(isAutoRetryEnabled, true);
    }

    // ���� �̹��� �ʱ�ȭ �Լ�
    private void InitializeRewardSprites()
    {
        cashSprite = Resources.Load<Sprite>("Sprites/UI/I_UI_Main/I_UI_paidcoin.9");
        coinSprite = Resources.Load<Sprite>("Sprites/UI/I_UI_Main/I_UI_coin.9");
    }

    #endregion


    #region Battle

    #region Battle Core

    // ���� ���� �ڷ�ƾ
    private IEnumerator BossSpawnRoutine()
    {
        while (true)
        {
            // ������ ���� ���� �������� ����
            if (currentBoss == null)
            {
                bossSpawnTimer += Time.deltaTime;
                respawnSlider.value = bossSpawnTimer;

                // �������� �� ���� ���� ��ȯ
                if (bossSpawnTimer >= spawnInterval)
                {
                    bossSpawnTimer = 0f;

                    yield return StartCoroutine(LoadWarningPanel());

                    LoadAndDisplayBoss();
                    StartBattle();
                }
            }

            yield return null;
        }
    }

    // warningPanel Ȱ��ȭ�� �ڷ�ƾ
    private IEnumerator LoadWarningPanel()
    {
        // �ڵ� ���� �Ͻ�����
        AutoMergeManager.Instance.PauseAutoMerge();

        warningPanel.SetActive(true);

        float elapsedTime = 0f;
        float halfDuration = warningDuration / 2f;

        // �ʱ� ���� ����
        warningPanelCanvasGroup.alpha = 0f;
        topWarningCanvasGroup.alpha = 0f;
        bossDangerCanvasGroup.alpha = 0f;
        bottomWarningCanvasGroup.alpha = 0f;

        // Image �ʱ� ��ġ ����
        topWarningImage.rectTransform.anchoredPosition = new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y);
        bossDangerImage.rectTransform.anchoredPosition = new Vector2(BOSS_DANGER_START_X, WARNING_IMAGE_Y);
        bottomWarningImage.rectTransform.anchoredPosition = new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y);

        // ù 1��: ���� -> ������
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / halfDuration;

            // ���� ���� (0 -> 1.0)
            float alpha = Mathf.Lerp(0f, 1.0f, normalizedTime);
            warningPanelCanvasGroup.alpha = alpha;
            topWarningCanvasGroup.alpha = alpha;
            bossDangerCanvasGroup.alpha = alpha;
            bottomWarningCanvasGroup.alpha = alpha;

            // �̹��� �̵�
            float moveProgress = elapsedTime / warningDuration;
            topWarningImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y),
                new Vector2(WARNING_IMAGE_END_X, WARNING_IMAGE_Y),
                moveProgress);
            bottomWarningImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y),
                new Vector2(WARNING_IMAGE_END_X, WARNING_IMAGE_Y),
                moveProgress);
            bossDangerImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(BOSS_DANGER_START_X, WARNING_IMAGE_Y),
                new Vector2(BOSS_DANGER_END_X, WARNING_IMAGE_Y),
                moveProgress);

            // slider ������Ʈ
            warningSlider.value = elapsedTime;

            yield return null;
        }
        // ���� 1��: ������ -> ����
        while (elapsedTime < warningDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = (elapsedTime - halfDuration) / halfDuration;

            // ���� ���� (1.0 -> 0)
            float alpha = Mathf.Lerp(1.0f, 0f, normalizedTime);
            warningPanelCanvasGroup.alpha = alpha;
            topWarningCanvasGroup.alpha = alpha;
            bossDangerCanvasGroup.alpha = alpha;
            bottomWarningCanvasGroup.alpha = alpha;

            // �̹��� �̵�
            float moveProgress = elapsedTime / warningDuration;
            topWarningImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y),
                new Vector2(WARNING_IMAGE_END_X, WARNING_IMAGE_Y),
                moveProgress);
            bottomWarningImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y),
                new Vector2(WARNING_IMAGE_END_X, WARNING_IMAGE_Y),
                moveProgress);
            bossDangerImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(BOSS_DANGER_START_X, WARNING_IMAGE_Y),
                new Vector2(BOSS_DANGER_END_X, WARNING_IMAGE_Y),
                moveProgress);

            // slider ������Ʈ
            warningSlider.value = elapsedTime;

            yield return null;
        }

        warningPanel.SetActive(false);
    }

    // ���� ���� �Լ�
    private void LoadAndDisplayBoss()
    {
        // �ڵ� �絵�� ���¿� ���� ������ �������� ����
        if (isAutoRetryEnabled)
        {
            bossStage = Mathf.Max(1, currentMaxBossStage - 1);
        }
        else
        {
            bossStage = currentMaxBossStage;
        }

        // bossStage�� �´� Mouse�� �����ͼ� ������ ����
        currentBossData = GetBossData();
        currentBoss = Instantiate(bossPrefab, bossUIParent);
        bossHitbox = currentBoss.GetComponent<BossHitbox>();

        // ������ MouseData�� ����
        MouseData mouseUIData = currentBoss.GetComponent<MouseData>();
        mouseUIData.SetMouseData(currentBossData);

        // ���� ��ġ ����
        RectTransform bossRectTransform = currentBoss.GetComponent<RectTransform>();
        bossRectTransform.anchoredPosition = new Vector2(0f, 250f);

        UpdateBossUI();
    }

    // �ش� ���������� ������ ����� ���� ���� ������ �ҷ����� �Լ� (MouseGrade)
    private Mouse GetBossData()
    {
        // ��� Mouse �����͸� �����ͼ� bossStage�� �´� MouseGrade�� ã��
        foreach (Mouse mouse in GameManager.Instance.AllMouseData)
        {
            if (mouse.MouseGrade == bossStage)
            {
                return mouse;
            }
        }

        return null;
    }

    // ���� �����Ҷ����� Boss UI Panel ���� �Լ�
    private void UpdateBossUI()
    {
        battleHPUI.SetActive(true);
        bossStageText.text = $"{currentBossData.MouseGrade} Stage";

        maxBossHP = currentBossData.MouseHp;
        currentBossHP = maxBossHP;
        bossHPSlider.maxValue = (float)maxBossHP;
        bossHPSlider.value = (float)currentBossHP;
        bossHPText.text = $"{100f}%";
    }

    // ���� ���� �Լ�
    private void StartBattle()
    {
        // ���� ���۽� ���� �ܺ� ��ɵ� ��Ȱ��ȭ
        SetStartFunctions();

        // �׺� ��ư ��Ȱ��ȭ �� 2�� �� Ȱ��ȭ
        giveupButton.interactable = false;
        StartCoroutine(EnableGiveupButton());

        // Slider���� �ڷ�ƾ ����
        StartCoroutine(ExecuteBattleSliders(warningDuration, sliderDuration));
        bossBattleCoroutine = StartCoroutine(BossBattleRoutine(bossDuration));
        isBattleActive = true;

        // ����� �ڵ� ��ȭ ���� ��Ȱ��ȭ
        SetStartBattleAutoCollectState();

        // ���� ��Ʈ�ڽ� ��,�ܿ� �����ϴ� ����̵� �̵���Ű��
        PushCatsAwayFromBoss();
        MoveCatsTowardBossBoundary();

        // ���� �� ����� ���� �ڷ�ƾ ����
        bossAttackRoutine = StartCoroutine(BossAttackRoutine());
        catsAttackRoutine = StartCoroutine(CatsAttackRoutine());
    }

    // �׺� ��ư Ȱ��ȭ �ڷ�ƾ
    private IEnumerator EnableGiveupButton()
    {
        yield return new WaitForSeconds(GIVEUP_BUTTON_DELAY);
        giveupButton.interactable = true;
    }

    #endregion


    #region Battle Sliders

    // Slider ���� ���� �ڷ�ƾ
    private IEnumerator ExecuteBattleSliders(float warningDuration, float sliderDuration)
    {
        // 1. warningSlider ����
        warningSliderCoroutine = StartCoroutine(DecreaseWarningSliderDuringBossDuration(warningDuration));
        yield return warningSliderCoroutine;

        // 2. respawnSlider ����
        respawnSliderCoroutine = StartCoroutine(DecreaseSliderDuringBossDuration(sliderDuration));
        yield return respawnSliderCoroutine;
    }

    // ���� �����ð����� warningSlider�� �����ϴ� �ڷ�ƾ
    private IEnumerator DecreaseWarningSliderDuringBossDuration(float duration)
    {
        float elapsedTime = 0f;
        warningSlider.maxValue = duration;
        warningSlider.value = duration;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            warningSlider.value = duration - elapsedTime;

            yield return null;
        }

        warningSliderCoroutine = null;
    }

    // ���� �����ð����� respawnSlider�� �����ϴ� �ڷ�ƾ
    private IEnumerator DecreaseSliderDuringBossDuration(float duration)
    {
        float elapsedTime = 0f;
        respawnSlider.maxValue = duration;
        respawnSlider.value = duration;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            respawnSlider.value = duration - elapsedTime;

            yield return null;
        }

        respawnSliderCoroutine = null;
    }

    #endregion


    #region Battle Management

    // ���� ��Ʋ �ڷ�ƾ
    private IEnumerator BossBattleRoutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // ���� ü���� 0 ���ϰ� �Ǹ� ��� ���� ����
            if (currentBossHP <= 0)
            {
                EndBattle(true);
                yield break;
            }

            yield return null;
        }

        // ���� �ð� �ʰ��� ���� ����
        EndBattle(false);
    }

    // ���� ������ ��Ʈ�ڽ� ���� ���� �ִ� ����̸� �о�� �Լ�
    private void PushCatsAwayFromBoss()
    {
        if (currentBoss == null)
        {
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            // ����̰� ��Ʈ�ڽ� ��� ���� �ִ��� Ȯ�� �� ��Ʈ�ڽ� �ܰ����� �о��
            if (bossHitbox.IsInHitbox(catPosition))
            {
                cat.MoveOppositeBoss();
            }
        }
    }

    // ���� ������ ��Ʈ�ڽ� ���� �ۿ� �ִ� ����̸� ��Ʈ�ڽ� ���� �̵���Ű�� �Լ�
    private void MoveCatsTowardBossBoundary()
    {
        if (currentBoss == null || bossHitbox == null)
        {
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            // ����̰� ��Ʈ�ڽ� ��� �ۿ� �ִ��� Ȯ�� �� ��Ʈ�ڽ� �ܰ����� ������
            if (!bossHitbox.IsInHitbox(catPosition))
            {
                cat.MoveTowardBossBoundary();
            }
        }
    }

    #endregion


    #region Battle System

    // ������ �޴� ������ �Լ�
    public void TakeBossDamage(float damage)
    {
        if (!IsBattleActive || currentBoss == null)
        {
            return;
        }
        currentBossHP -= damage;

        // ������ MouseData ������Ʈ�� ���� ������ �ؽ�Ʈ ǥ��
        MouseData mouseData = currentBoss.GetComponent<MouseData>();
        if (mouseData != null)
        {
            mouseData.ShowDamageText(damage);
        }

        UpdateBossHPUI();
    }

    // ���� HP Slider �� �ؽ�Ʈ ������Ʈ �Լ�
    private void UpdateBossHPUI()
    {
        bossHPSlider.value = (float)currentBossHP;

        double hpPercentage = (currentBossHP / maxBossHP) * 100f;
        bossHPText.text = $"{hpPercentage:F2}%";
    }

    // ���� ���� �ڷ�ƾ
    private IEnumerator BossAttackRoutine()
    {
        while (IsBattleActive)
        {
            yield return new WaitForSeconds(BOSS_ATTACK_DELAY);
            BossAttackCats();
        }
    }

    // ������ ��Ʈ�ڽ� �� ����� N������ �����ϴ� �Լ�
    private void BossAttackCats()
    {
        if (currentBoss == null || bossHitbox == null)
        {
            return;
        }

        // ��Ʈ�ڽ� ��迡 �ִ� ����̸� ã��
        List<CatData> catsAtBoundary = new List<CatData>();
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned)
            {
                continue;
            }

            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            if (bossHitbox.IsAtBoundary(catPosition))
            {
                catsAtBoundary.Add(cat);
            }
        }

        if (catsAtBoundary.Count == 0)
        {
            return;
        }

        // ���� ���� ��� ����
        int attackCount = Mathf.Min(catsAtBoundary.Count, currentBossData.NumOfAttack);
        List<CatData> selectedCats = new List<CatData>();
        while (selectedCats.Count < attackCount)
        {
            int randomIndex = UnityEngine.Random.Range(0, catsAtBoundary.Count);
            CatData selectedCat = catsAtBoundary[randomIndex];

            if (!selectedCats.Contains(selectedCat))
            {
                selectedCats.Add(selectedCat);
            }
        }

        // ���õ� ����̵鿡�� ������ ����
        foreach (var cat in selectedCats)
        {
            double damage = currentBossData.MouseDamage;
            cat.TakeDamage(damage);
        }
    }

    // ����� ���� �ڷ�ƾ
    private IEnumerator CatsAttackRoutine()
    {
        while (IsBattleActive)
        {
            yield return new WaitForSeconds(CAT_ATTACK_DELAY);
            CatsAttackBoss();
        }
    }

    // ��Ʈ�ڽ� �� ����̵��� ������ �����ϴ� �Լ�
    private void CatsAttackBoss()
    {
        if (currentBoss == null || bossHitbox == null)
        {
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned)
            {
                continue;
            }

            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            if (bossHitbox.IsAtBoundary(catPosition))
            {
                int damage = cat.catData.CatDamage;
                TakeBossDamage(damage);
            }
        }
    }

    #endregion


    #region Battle Plus UI

    // ���� ��� �����ִ� �Լ�
    private void ShowBattleResult(bool isVictory)
    {
        battleResultPanel.SetActive(true);
        winPanel.SetActive(isVictory);
        losePanel.SetActive(!isVictory);

        ClearRewardSlots();

        // �¸����� �� ���� ���� ����
        if (isVictory)
        {
            bool isFirstClear = !clearedStages.Contains(bossStage);
            if (isFirstClear)
            {
                // ���� Ŭ���� ����
                CreateRewardSlot(winRewardPanel, cashSprite, currentBossData.ClearCashReward, true);
                CreateRewardSlot(winRewardPanel, coinSprite, currentBossData.ClearCoinReward, true);
            }
            else
            {
                // �ݺ� Ŭ���� ����
                CreateRewardSlot(winRewardPanel, coinSprite, currentBossData.RepeatclearCoinReward, false);
            }
        }
        else
        {
            // �й� ������ ����
        }

        if (resultPanelCoroutine != null)
        {
            StopCoroutine(resultPanelCoroutine);
        }
        resultPanelCoroutine = StartCoroutine(AutoCloseBattleResultPanel());
    }

    // ���� ���� ���� �Լ�
    private void ClearRewardSlots()
    {
        foreach (GameObject slot in activeRewardSlots)
        {
            Destroy(slot);
        }
        activeRewardSlots.Clear();
    }

    // ���� ��� �г� �ݴ� �Լ�
    private void CloseBattleResultPanel()
    {
        if (resultPanelCoroutine != null)
        {
            StopCoroutine(resultPanelCoroutine);
            resultPanelCoroutine = null;
        }
        ClearRewardSlots();
        battleResultPanel.SetActive(false);
    }

    // ���� ��� �г� �ڵ����� �ݴ� �ڷ�ƾ
    private IEnumerator AutoCloseBattleResultPanel()
    {
        float countdown = 3f;
        while (countdown > 0)
        {
            battleResultCountdownText.text = $"{countdown:F0}�� �� �ڵ� ����";
            countdown -= Time.deltaTime;
            yield return null;
        }
        CloseBattleResultPanel();
    }

    // �ڵ� �絵�� �г� ����
    private void OpenAutoRetryPanel()
    {
        autoRetryPanel.SetActive(true);
    }

    // �ڵ� �絵�� �г� �ݱ�
    private void CloseAutoRetryPanel()
    {
        autoRetryPanel.SetActive(false);
    }

    // ��� ��ư �̹��� ������Ʈ
    private void UpdateToggleButtonImage(Image buttonImage, bool isOn)
    {
        string imagePath = isOn ? "Sprites/UI/I_UI_Option/I_UI_option_on_Frame.9" : "Sprites/UI/I_UI_Option/I_UI_option_off_Frame.9";
        buttonImage.sprite = Resources.Load<Sprite>(imagePath);
    }

    // �ڵ� �絵�� ��� �Լ�
    private void ToggleAutoRetry()
    {
        isAutoRetryEnabled = !isAutoRetryEnabled;
        UpdateAutoRetryUI(isAutoRetryEnabled);
        UpdateToggleButtonImage(autoRetryButtonImage, isAutoRetryEnabled);
        UpdateAutoRetryPanelButtonColor(isAutoRetryEnabled);

        GoogleSave();
    }

    // �ڵ� �絵�� UI ������Ʈ
    private void UpdateAutoRetryUI(bool state, bool instant = false)
    {
        float targetX = state ? 65f : -65f;

        if (instant)
        {
            autoRetryHandle.anchoredPosition = new Vector2(targetX, autoRetryHandle.anchoredPosition.y);
        }
        else
        {
            if (autoRetryToggleCoroutine != null)
            {
                StopCoroutine(autoRetryToggleCoroutine);
            }
            autoRetryToggleCoroutine = StartCoroutine(AnimateAutoRetryHandle(targetX));
        }
    }

    // ��� �ڵ� �ִϸ��̼�
    private IEnumerator AnimateAutoRetryHandle(float targetX)
    {
        float elapsedTime = 0f;
        float startX = autoRetryHandle.anchoredPosition.x;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            autoRetryHandle.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, t), autoRetryHandle.anchoredPosition.y);
            yield return null;
        }

        autoRetryHandle.anchoredPosition = new Vector2(targetX, autoRetryHandle.anchoredPosition.y);
    }

    // �г� ��ư ���� ������Ʈ �Լ� �߰�
    private void UpdateAutoRetryPanelButtonColor(bool isEnabled)
    {
        if (autoRetryPanelButtonImage != null)
        {
            autoRetryPanelButtonImage.color = isEnabled ?
                new Color32(130, 255, 0, 255) :
                new Color32(255, 153, 21, 255);
        }
    }

    // ���� ���� ���� �Լ�
    private void CreateRewardSlot(Transform parent, Sprite rewardSprite, decimal amount, bool isFirstClear = false)
    {
        GameObject rewardSlot = Instantiate(rewardSlotPrefab, parent);
        activeRewardSlots.Add(rewardSlot);

        // Reward Image ����
        Image rewardImage = rewardSlot.transform.Find("Background Image/Reward Image").GetComponent<Image>();
        rewardImage.sprite = rewardSprite;

        // Reward Text ����
        TextMeshProUGUI rewardText = rewardSlot.transform.Find("Reward Text").GetComponent<TextMeshProUGUI>();
        rewardText.text = GameManager.Instance.FormatPriceNumber(amount);

        // First Clear Image ����
        GameObject firstClearImage = rewardSlot.transform.Find("First Clear Image").gameObject;
        firstClearImage.SetActive(isFirstClear);
    }

    #endregion


    #region Battle End

    // �׺� ��ư �Լ�
    private void GiveUpState()
    {
        // giveup Panel���� �߰��ҰŸ� ���⿡ ���� �Լ� �߰�
        EndBattle(false);
    }

    // ���� ���� �Լ�
    public void EndBattle(bool isVictory)
    {
        if (!isBattleActive)
        {
            return;
        }

        isBattleActive = false;

        // ��� ���� ���� �ڷ�ƾ ����
        StopAllBattleCoroutines();

        // �����̴� �ʱ�ȭ
        InitializeSliders();

        ShowBattleResult(isVictory);
        if (isVictory)
        {
            GiveStageReward();

            currentMaxBossStage = Mathf.Max(currentMaxBossStage, bossStage + 1);
            QuestManager.Instance.AddStageCount();
        }

        Destroy(currentBoss);
        currentBossData = null;
        currentBoss = null;
        bossHitbox = null;
        battleHPUI.SetActive(false);

        GoogleSave();

        // ���� ����� ��Ȱ��ȭ�ߴ� ��ɵ� �ٽ� ���� ���·� ����
        SetEndFunctions();

        // ����� �ڵ� ��ȭ ���� Ȱ��ȭ
        SetEndBattleAutoCollectState();

        // ����Ʈ ����
        QuestManager.Instance.AddBattleCount();

        // ����̵��� ü�� ȸ��
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.HealCatHP();
        }

        // �ڵ� ���� �簳
        AutoMergeManager.Instance.ResumeAutoMerge();
    }

    // ���� ���� �Լ�
    private void GiveStageReward()
    {
        if (currentBossData == null)
        {
            return;
        }

        bool isFirstClear = !clearedStages.Contains(bossStage);
        if (isFirstClear)
        {
            // ���� Ŭ���� ����
            GameManager.Instance.Cash += currentBossData.ClearCashReward;
            GameManager.Instance.Coin += currentBossData.ClearCoinReward;
            clearedStages.Add(bossStage);
        }
        else
        {
            // �ݺ� Ŭ���� ����
            GameManager.Instance.Coin += currentBossData.RepeatclearCoinReward;
        }
    }

    // ��� ���� ���� �ڷ�ƾ�� �����ϴ� �Լ� �߰�
    private void StopAllBattleCoroutines()
    {
        if (warningSliderCoroutine != null)
        {
            StopCoroutine(warningSliderCoroutine);
            warningSliderCoroutine = null;
        }

        if (respawnSliderCoroutine != null)
        {
            StopCoroutine(respawnSliderCoroutine);
            respawnSliderCoroutine = null;
        }

        if (bossBattleCoroutine != null)
        {
            StopCoroutine(bossBattleCoroutine);
            bossBattleCoroutine = null;
        }

        // ������ ������� ���� �ڷ�ƾ�� ����
        if (bossAttackRoutine != null)
        {
            StopCoroutine(bossAttackRoutine);
            bossAttackRoutine = null;
        }

        if (catsAttackRoutine != null)
        {
            StopCoroutine(catsAttackRoutine);
            catsAttackRoutine = null;
        }
    }

    #endregion

    #endregion


    #region State Management

    // ��� ������� �ڵ� ��ȭ ���� ��Ȱ��ȭ
    private void SetStartBattleAutoCollectState()
    {
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (!cat.isStuned)
            {
                cat.SetCollectingCoinsState(false);
                cat.DisableCollectUI();
            }
        }
    }

    // ��� ������� �ڵ� ��ȭ ���� ���� ���·� ���� (Ȱ��ȭ)
    private void SetEndBattleAutoCollectState()
    {
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (!cat.isStuned)
            {
                cat.SetCollectingCoinsState(true);
            }
        }
    }

    // �������۽� ��Ȱ��ȭ�Ǵ� �ܺ� ��ɵ�
    private void SetStartFunctions()
    {
        MergeManager.Instance.StartBattleMergeState();
        AutoMoveManager.Instance.StartBattleAutoMoveState();
        GetComponent<SortManager>().StartBattleSortState();

        SpawnManager.Instance.StartBattleSpawnState();

        ItemMenuManager.Instance.StartBattleItemMenuState();
        BuyCatManager.Instance.StartBattleBuyCatState();
    }

    // ��������� Ȱ��ȭ�Ǵ� �ܺ� ��ɵ�
    private void SetEndFunctions()
    {
        MergeManager.Instance.EndBattleMergeState();
        AutoMoveManager.Instance.EndBattleAutoMoveState();
        GetComponent<SortManager>().EndBattleSortState();

        SpawnManager.Instance.EndBattleSpawnState();

        ItemMenuManager.Instance.EndBattleItemMenuState();
        BuyCatManager.Instance.EndBattleBuyCatState();
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public int bossStage;               // ���� ���� ��������
        public bool isAutoRetryEnabled;     // ���� �ܰ� �ڵ� ���� ����
        public List<int> clearedStages;
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            bossStage = this.bossStage,
            isAutoRetryEnabled = this.isAutoRetryEnabled,
            clearedStages = new List<int>(this.clearedStages)
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        this.bossStage = savedData.bossStage;
        this.currentMaxBossStage = savedData.bossStage;
        this.isAutoRetryEnabled = savedData.isAutoRetryEnabled;
        this.clearedStages = new HashSet<int>(savedData.clearedStages ?? new List<int>());

        UpdateToggleButtonImage(autoRetryButtonImage, isAutoRetryEnabled);
        UpdateAutoRetryPanelButtonColor(isAutoRetryEnabled);
        UpdateAutoRetryUI(isAutoRetryEnabled, true);

        // ���� ���̾��ٸ� ���� ���� �ʱ�ȭ
        if (isBattleActive)
        {
            EndBattle(false);
        }

        // Ÿ�̸� �ʱ�ȭ
        bossSpawnTimer = 0f;
        respawnSlider.value = 0f;

        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}
