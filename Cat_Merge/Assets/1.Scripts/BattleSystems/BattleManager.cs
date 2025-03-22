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

    [Header("---[Boss UI]")]
    [SerializeField] private GameObject battleHPUI;             // Battle HP UI (Ȱ��ȭ/��Ȱ��ȭ ����)
    [SerializeField] private TextMeshProUGUI bossStageText;     // Boss Stage Text
    [SerializeField] private Slider bossHPSlider;               // HP Slider
    [SerializeField] private TextMeshProUGUI bossHPText;        // HP % Text
    [SerializeField] private Button giveupButton;               // �׺� ��ư

    private Mouse currentBossData;                              // ���� ���� ������
    private double currentBossHP;                               // ������ ���� HP
    private double maxBossHP;                                   // ������ �ִ� HP

    [Header("---[Warning Panel]")]
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

        if (!isDataLoaded)
        {
            bossStage = 1;
        }

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
        //StartCoroutine(ExecuteBattleSliders(warningDuration, bossDuration));
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
        if (!isBattleActive) return;

        isBattleActive = false;

        //// ���� ���� ���� �����̴� �ڷ�ƾ ����
        //if (warningSliderCoroutine != null)
        //{
        //    StopCoroutine(warningSliderCoroutine);
        //    warningSliderCoroutine = null;

        //    warningSlider.maxValue = warningDuration;
        //    warningSlider.value = 0f;
        //}
        //if (respawnSliderCoroutine != null)
        //{
        //    StopCoroutine(respawnSliderCoroutine);
        //    respawnSliderCoroutine = null;

        //    respawnSlider.maxValue = spawnInterval;
        //    respawnSlider.value = 0f;
        //}

        // ��� ���� ���� �ڷ�ƾ ����
        StopAllBattleCoroutines();

        // �����̴� �ʱ�ȭ
        InitializeSliders();

        Destroy(currentBoss);
        currentBossData = null;
        currentBoss = null;
        bossHitbox = null;
        battleHPUI.SetActive(false);

        if (isVictory)
        {
            bossStage++;
            QuestManager.Instance.AddStageCount();

            GoogleSave();
        }

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
            //if (!cat.isStuned)
            {
                cat.HealCatHP();
            }
        }

        // �ڵ� ���� �簳
        AutoMergeManager.Instance.ResumeAutoMerge();
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
        public int bossStage;           // ���� ���� ��������
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            bossStage = this.bossStage
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        this.bossStage = savedData.bossStage;

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
            Debug.Log("���� ����");
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}
