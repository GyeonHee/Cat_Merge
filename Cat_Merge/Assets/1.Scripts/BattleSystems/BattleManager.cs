using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

// BattleManager Script
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // ======================================================================================================================

    [Header("---[Battle System]")]
    [SerializeField] private GameObject bossPrefab;             // ���� ������
    [SerializeField] private Transform bossUIParent;            // ������ ��ġ�� �θ� Transform (UI Panel ��)
    [SerializeField] private Slider respawnSlider;              // ���� ��ȯ���� ���� �ð��� ǥ���� Slider UI

    private float spawnInterval = 10f;                          // ���� ���� �ֱ� (���߿� ������ ������ �� �ְ� ������ ��ȹ�� ����)
    private Coroutine respawnSliderCoroutine;                   // Slider �ڷ�ƾ
    private float bossSpawnTimer = 0f;                          // ���� ���� Ÿ�̸�
    private float sliderDuration;                               // Slider ���� �ð�
    private float bossDuration;                                 // ���� ���� �ð� (sliderDuration + warningDuration)
    private int bossStage = 1;                                  // ���� ��������
    public int BossStage { get => bossStage; }

    private float bossAttackDelay = 2f;                         // ���� ���� ������
    private float catAttackDelay = 1f;                          // ����� ���� ������

    private GameObject currentBoss = null;                      // ���� ����
    [HideInInspector] public BossHitbox bossHitbox;             // ���� ��Ʈ�ڽ�
    private bool isBattleActive = false;                        // ���� Ȱ��ȭ ����
    public bool IsBattleActive { get => isBattleActive; }

    // ======================================================================================================================

    [Header("---[Boss UI]")]
    [SerializeField] private GameObject battleHPUI;             // Battle HP UI (Ȱ��ȭ/��Ȱ��ȭ ����)
    [SerializeField] private TextMeshProUGUI bossNameText;      // Boss Name Text
    [SerializeField] private Slider bossHPSlider;               // HP Slider
    [SerializeField] private TextMeshProUGUI bossHPText;        // HP % Text
    [SerializeField] private Button giveupButton;               // �׺� ��ư

    private Mouse currentBossData = null;                       // ���� ���� ������
    private float currentBossHP;                                // ������ ���� HP
    private float maxBossHP;                                    // ������ �ִ� HP

    // ======================================================================================================================

    [Header("---[Warning Panel]")]
    [SerializeField] private GameObject warningPanel;           // �����ý��� ���۽� ������ ��� Panel (warningDuration���� ����)
    [SerializeField] private Slider warningSlider;              // �������ð��� ������ �������� Slider (warningDuration��ŭ ������)
    public float warningDuration = 2f;                          // warningPanel Ȱ��ȭ �ð�
    private Coroutine warningSliderCoroutine;                   // warningSlider �ڷ�ƾ

    [SerializeField] private TextMeshProUGUI topWarningText;    // ��� ��� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI bossDangerText;    // ���� ���� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI bottomWarningText; // �ϴ� ��� �ؽ�Ʈ
    private CanvasGroup warningPanelCanvasGroup;                // Warning Panel�� CanvasGroup
    private CanvasGroup topWarningCanvasGroup;                  // ��� ��� �ؽ�Ʈ�� CanvasGroup
    private CanvasGroup bossDangerCanvasGroup;                  // ���� ���� �ؽ�Ʈ�� CanvasGroup  
    private CanvasGroup bottomWarningCanvasGroup;               // �ϴ� ��� �ؽ�Ʈ�� CanvasGroup

    // ======================================================================================================================

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

        InitializeBattleManager();
        giveupButton.onClick.AddListener(GiveUpState);
    }

    private void Start()
    {
        StartCoroutine(BossSpawnRoutine());
    }

    // BattleManager �ʱ� ����
    private void InitializeBattleManager()
    {
        // warningPanel ����
        warningPanel.SetActive(false);

        // Slider �ð� ���� (���� ������, ���� ���� �ð�)
        spawnInterval = spawnInterval - warningDuration;
        sliderDuration = spawnInterval;
        bossDuration = sliderDuration + warningDuration;

        // respawnSlider ����
        if (respawnSlider != null)
        {
            respawnSliderCoroutine = null;
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        // warning Slider ����
        if (warningSlider != null)
        {
            warningSliderCoroutine = null;
            warningSlider.maxValue = warningDuration;
            warningSlider.value = 0f;
        }

        // Battle HP UI �ʱ�ȭ
        if (battleHPUI != null)
        {
            battleHPUI.SetActive(false);
        }

        InitializeCanvasGroup();
    }

    // CanvasGroup ������Ʈ �ʱ�ȭ
    private void InitializeCanvasGroup()
    {
        warningPanelCanvasGroup = warningPanel.GetComponent<CanvasGroup>();
        if (warningPanelCanvasGroup == null)
            warningPanelCanvasGroup = warningPanel.AddComponent<CanvasGroup>();

        topWarningCanvasGroup = topWarningText.gameObject.GetComponent<CanvasGroup>();
        if (topWarningCanvasGroup == null)
            topWarningCanvasGroup = topWarningText.gameObject.AddComponent<CanvasGroup>();

        bossDangerCanvasGroup = bossDangerText.gameObject.GetComponent<CanvasGroup>();
        if (bossDangerCanvasGroup == null)
            bossDangerCanvasGroup = bossDangerText.gameObject.AddComponent<CanvasGroup>();

        bottomWarningCanvasGroup = bottomWarningText.gameObject.GetComponent<CanvasGroup>();
        if (bottomWarningCanvasGroup == null)
            bottomWarningCanvasGroup = bottomWarningText.gameObject.AddComponent<CanvasGroup>();
    }

    // ======================================================================================================================
    // [BattleManager �ٽ� ���]

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

        // Text �ʱ� ��ġ ����
        topWarningText.rectTransform.anchoredPosition = new Vector2(-755, 0);
        bossDangerText.rectTransform.anchoredPosition = new Vector2(810, 0);
        bottomWarningText.rectTransform.anchoredPosition = new Vector2(-755, 0);

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

            // �ؽ�Ʈ �̵�
            float moveProgress = elapsedTime / warningDuration;
            topWarningText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-755, 0), new Vector2(755, 0), moveProgress);
            bottomWarningText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-755, 0), new Vector2(755, 0), moveProgress);
            bossDangerText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(810, 0), new Vector2(-810, 0), moveProgress);

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

            // �ؽ�Ʈ �̵�
            float moveProgress = elapsedTime / warningDuration;
            topWarningText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-755, 0), new Vector2(755, 0), moveProgress);
            bottomWarningText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-755, 0), new Vector2(755, 0), moveProgress);
            bossDangerText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(810, 0), new Vector2(-810, 0), moveProgress);

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
        bossNameText.text = currentBossData.MouseName;

        maxBossHP = currentBossData.MouseHp;
        currentBossHP = maxBossHP;
        bossHPSlider.maxValue = maxBossHP;
        bossHPSlider.value = currentBossHP;
        bossHPText.text = $"{100f}%";
    }

    

    // ���� ���� �Լ�
    private void StartBattle()
    {
        // ���� ���۽� ���� �ܺ� ��ɵ� ��Ȱ��ȭ
        SetStartFunctions();

        // Slider���� �ڷ�ƾ ����
        StartCoroutine(ExecuteBattleSliders(warningDuration, sliderDuration));
        StartCoroutine(BossBattleRoutine(bossDuration));
        isBattleActive = true;

        // ����� �ڵ� ��ȭ ���� ��Ȱ��ȭ
        SetStartBattleAutoCollectState();

        // ���� ��Ʈ�ڽ� ��,�ܿ� �����ϴ� ����̵� �̵���Ű��
        PushCatsAwayFromBoss();
        MoveCatsTowardBossBoundary();

        // ���� �� ����� ���� �ڷ�ƾ ����
        StartCoroutine(BossAttackRoutine());
        StartCoroutine(CatsAttackRoutine());
    }




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
            Debug.LogError("No boss to push cats away from.");
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            // ����̰� ��Ʈ�ڽ� ��� ���� �ִ��� Ȯ��
            if (bossHitbox.IsInHitbox(catPosition))
            {
                // ����̸� ������ ��Ʈ�ڽ� �ܰ����� �о��
                cat.MoveOppositeBoss(bossHitbox.Position, bossHitbox.Size);
            }
        }
    }

    // ���� ������ ��Ʈ�ڽ� ���� �ۿ� �ִ� ����̸� ��Ʈ�ڽ� ���� �̵���Ű�� �Լ�
    private void MoveCatsTowardBossBoundary()
    {
        if (currentBoss == null || bossHitbox == null)
        {
            Debug.LogError("No boss or BossHitbox component is missing.");
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            // ����̰� ��Ʈ�ڽ� ��� �ۿ� �ִ��� Ȯ��
            if (!bossHitbox.IsInHitbox(catPosition))
            {
                // ����̸� ���� ��Ʈ�ڽ� �ܰ����� ������
                cat.MoveTowardBossBoundary(bossHitbox.Position, bossHitbox.Size);
            }
        }
    }



    // ������ �޴� ������ �Լ�
    public void TakeBossDamage(float damage)
    {
        if (!isBattleActive || currentBoss == null)
        {
            return;
        }
        currentBossHP -= damage;

        UpdateBossHPUI();
    }

    // ���� HP Slider �� �ؽ�Ʈ ������Ʈ �Լ�
    private void UpdateBossHPUI()
    {
        bossHPSlider.value = currentBossHP;

        float hpPercentage = (currentBossHP / maxBossHP) * 100f;
        bossHPText.text = $"{hpPercentage:F2}%";
    }

    // ======================================================================================================================
    // [���� ���� ����]

    // ���� ���� �ڷ�ƾ
    private IEnumerator BossAttackRoutine()
    {
        while (isBattleActive)
        {
            yield return new WaitForSeconds(bossAttackDelay);
            BossAttackCats();
        }
    }

    // ������ ��Ʈ�ڽ� �� ����� N������ �����ϴ� �Լ�
    private void BossAttackCats()
    {
        if (currentBoss == null || bossHitbox == null) return;

        // ��Ʈ�ڽ� ��迡 �ִ� ����̸� ã��
        List<CatData> catsAtBoundary = new List<CatData>();
        CatData[] allCats = FindObjectsOfType<CatData>();

        foreach (var cat in allCats)
        {
            // ���� ������ ����̴� ����
            if (cat.isStuned) continue;

            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            if (bossHitbox.IsAtBoundary(catPosition))
            {
                catsAtBoundary.Add(cat);
            }
        }

        if (catsAtBoundary.Count == 0) return;

        // ���� ���� ��� ����
        int attackCount = Mathf.Min(catsAtBoundary.Count, currentBossData.NumOfAttack);
        List<CatData> selectedCats = new List<CatData>();

        while (selectedCats.Count < attackCount)
        {
            int randomIndex = Random.Range(0, catsAtBoundary.Count);
            CatData selectedCat = catsAtBoundary[randomIndex];

            if (!selectedCats.Contains(selectedCat))
            {
                selectedCats.Add(selectedCat);
            }
        }

        // ���õ� ����̵鿡�� ������ ����
        foreach (var cat in selectedCats)
        {
            int damage = currentBossData.MouseDamage;
            cat.TakeDamage(damage);
        }
    }

    // ======================================================================================================================
    // [����� ���� ����]

    // ����� ���� �ڷ�ƾ
    private IEnumerator CatsAttackRoutine()
    {
        while (isBattleActive)
        {
            // ����� ����
            yield return new WaitForSeconds(catAttackDelay);
            CatsAttackBoss();
        }
    }

    // ��Ʈ�ڽ� �� ����̵��� ������ �����ϴ� �Լ�
    private void CatsAttackBoss()
    {
        if (currentBoss == null || bossHitbox == null) return;

        CatData[] allCats = FindObjectsOfType<CatData>();

        foreach (var cat in allCats)
        {
            // ���� ������ ����̴� �������� �ʵ��� ó��
            if (cat.isStuned) continue;

            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            if (bossHitbox.IsAtBoundary(catPosition))
            {
                int damage = cat.catData.CatDamage;
                TakeBossDamage(damage);
            }
        }
    }

    // ======================================================================================================================
    // [���� ���� ����]

    // �׺� ��ư �Լ�
    private void GiveUpState()
    {
        // giveup Panel���� �߰��ҰŸ� ���⿡ ���� �Լ� �߰�
        EndBattle(false);
    }

    // ���� ���� �Լ�
    public void EndBattle(bool isVictory)
    {
        isBattleActive = false;

        // ���� ���� ���� �����̴� �ڷ�ƾ ����
        if (warningSliderCoroutine != null)
        {
            StopCoroutine(warningSliderCoroutine);
            warningSliderCoroutine = null;

            warningSlider.maxValue = warningDuration;
            warningSlider.value = 0f;
        }
        if (respawnSliderCoroutine != null)
        {
            StopCoroutine(respawnSliderCoroutine);
            respawnSliderCoroutine = null;

            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        Destroy(currentBoss);
        currentBossData = null;
        currentBoss = null;
        bossHitbox = null;
        battleHPUI.SetActive(false);

        if (isVictory)
        {
            bossStage++;
            QuestManager.Instance.AddStageCount();
        }

        // ���� ����� ��Ȱ��ȭ�ߴ� ��ɵ� �ٽ� ���� ���·� ����
        SetEndFunctions();

        // ����� �ڵ� ��ȭ ���� Ȱ��ȭ
        SetEndBattleAutoCollectState();

        // ����Ʈ ����
        QuestManager.Instance.AddBattleCount();

        // ��� ������� ü���� ȸ�����������
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned) continue;

            cat.HealCatHP();
        }

        // �ڵ� ���� �簳
        AutoMergeManager.Instance.ResumeAutoMerge();
    }

    // ======================================================================================================================
    // [������ ����Ǵ� ��ɵ�]

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

    // ======================================================================================================================



}
