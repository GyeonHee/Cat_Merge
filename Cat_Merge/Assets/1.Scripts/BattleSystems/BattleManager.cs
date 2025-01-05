using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

// BattleManager Script
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("---[Battle System]")]
    [SerializeField] private GameObject bossPrefab;             // ���� ������
    [SerializeField] private Transform bossUIParent;            // ������ ��ġ�� �θ� Transform (UI Panel ��)
    [SerializeField] private Slider respawnSlider;              // ���� ��ȯ���� ���� �ð��� ǥ���� Slider UI

    private float spawnInterval = 10f;                          // ���� ���� �ֱ�
    private float timer = 0f;                                   // ���� ��ȯ Ÿ�̸�
    private float bossDuration = 10f;                           // ���� ���� �ð�
    private int bossStage = 1;                                  // ���� ��������

    private float bossAttackDelay = 2f;                         // ���� ���� ������
    private float catAttackDelay = 1f;                          // ����� ���� ������

    private GameObject currentBoss = null;                      // ���� ����
    private BossHitbox bossHitbox;                              // ���� ��Ʈ�ڽ�
    private bool isBattleActive = false;                        // ���� Ȱ��ȭ ����

    [Header("---[Boss UI]")]
    [SerializeField] private GameObject battleHPUI;             // Battle HP UI (Ȱ��ȭ/��Ȱ��ȭ ����)
    [SerializeField] private TextMeshProUGUI bossNameText;      // Boss Name Text
    [SerializeField] private Slider bossHPSlider;               // HP Slider
    [SerializeField] private TextMeshProUGUI bossHPText;        // HP % Text

    private Mouse currentBossData = null;                       // ���� ���� ������
    private float currentBossHP;                                // ������ ���� HP
    private float maxBossHP;                                    // ������ �ִ� HP

    [Header("---[Warning Panel]")]
    [SerializeField] private GameObject warningPanel;           // �����ý��� ���۽� ������ ��� Panel (warningTimer���� ����)
    [SerializeField] private Slider warningSlider;              // �������ð��� ������ �������� Slider (warningTimer��ŭ ������)
    private float warningTimer = 2.0f;

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
    }

    private void Start()
    {
        StartCoroutine(BossSpawnRoutine());
    }

    private void InitializeBattleManager()
    {
        // WarningPanel ����
        warningPanel.SetActive(false);

        // respawnSlider ����
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        // warning Slider ����
        if (warningSlider != null)
        {
            warningSlider.maxValue = warningTimer;
            warningSlider.value = 0f;
        }

        // Battle HP UI �ʱ�ȭ
        if (battleHPUI != null)
        {
            battleHPUI.SetActive(false);
        }
    }

    // ======================================================================================================================

    // ���� ���� �ڷ�ƾ
    private IEnumerator BossSpawnRoutine()
    {
        while (true)
        {
            // ������ ���� ���� �������� ����
            if (currentBoss == null)
            {
                timer += Time.deltaTime;
                respawnSlider.value = timer;

                // �������� �� ���� ���� ��ȯ
                if (timer >= spawnInterval)
                {
                    timer = 0f;

                    
                    yield return StartCoroutine(LoadWarningPanel());

                    LoadAndDisplayBoss();
                    StartBattle();
                }
            }

            yield return null;
        }
    }

    private IEnumerator LoadWarningPanel()
    {
        warningPanel.SetActive(true);
        float elapsedTime = 0f;

        //// WarningSlider �ʱ�ȭ
        //warningSlider.value = 0f;
        //warningSlider.maxValue = warningTimer;

        // WarningTimer ���� WarningSlider �������� �ϱ�
        while (elapsedTime < warningTimer)
        {
            elapsedTime += Time.deltaTime;
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

        // ������ �׻� ������ �ڽ����� ���� (UI�� ����̵� �ڿ� �����)
        currentBoss.transform.SetAsFirstSibling();

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

    // ��Ʋ �����Ҷ����� Boss UI Panel ���� �Լ�
    private void UpdateBossUI()
    {
        battleHPUI.SetActive(true);
        bossNameText.text = currentBossData.MouseName;

        maxBossHP = currentBossData.MouseHp;
        currentBossHP = maxBossHP;
        bossHPSlider.maxValue = maxBossHP;
        bossHPSlider.value = currentBossHP;
        bossHPText.text = $"{maxBossHP}%";
    }

    // ������ �޴� ������ �Լ�
    public void TakeBossDamage(float damage)
    {
        if (!isBattleActive || currentBoss == null) return;

        currentBossHP -= damage;
        if (currentBossHP < 0)
        {
            currentBossHP = 0;
        }

        UpdateBossHPUI();
    }

    // HP Slider �� �ؽ�Ʈ ������Ʈ �Լ�
    private void UpdateBossHPUI()
    {
        bossHPSlider.value = currentBossHP;

        float hpPercentage = (currentBossHP / maxBossHP) * 100f;
        bossHPText.text = $"{hpPercentage:F2}%";
    }

    // ���� ���� �Լ�
    private void StartBattle()
    {
        // ���� ���۽� ���� ��ɵ� ��Ȱ��ȭ
        SetStartFunctions();

        StartCoroutine(ExecuteBattleSliders(warningTimer, bossDuration));
        StartCoroutine(BossBattleRoutine(warningTimer + bossDuration));
        isBattleActive = true;

        // ���� ��Ʈ�ڽ� ���� �����ϴ� ����̵� �о��
        PushCatsAwayFromBoss();

        // ���� ��Ʈ�ڽ� ���� �ۿ� �ִ� ����̵��� ������ ���� �̵� ��Ű��
        MoveCatsTowardBossBoundary();

        // ���� ���� �ڷ�ƾ ����
        StartCoroutine(BossAttackRoutine());
        
        // ����� ���� �ڷ�ƾ ����
        StartCoroutine(CatsAttackRoutine());
    }

    // [MergeManager, AutoMoveManager, SortManager, AutoMergeManager]
    // [SpawnManager]
    // [ItemMenuManager, BuyCatManager, DictionaryManager, QuestManager]
    private void SetStartFunctions()
    {
        GetComponent<MergeManager>().StartBattleMergeState();
        GetComponent<AutoMoveManager>().StartBattleAutoMoveState();
        GetComponent<SortManager>().StartBattleSortState();
        //GetComponent<AutoMergeManager>().StartBattleAutoMergeState();
        //GetComponent<SpawnManager>().StartBattleSpawnState();
    }
    private void SetEndFunctions()
    {
        GetComponent<MergeManager>().EndBattleMergeState();
        GetComponent<AutoMoveManager>().EndBattleAutoMoveState();
        GetComponent<SortManager>().EndBattleSortState();
        //GetComponent<AutoMergeManager>().EndBattleAutoMergeState();
        //GetComponent<SpawnManager>().EndBattleSpawnState();
    }

    // �����̴� ���� ���� �ڷ�ƾ
    private IEnumerator ExecuteBattleSliders(float warningDuration, float bossDuration)
    {
        // 1. warningSlider ����
        yield return StartCoroutine(DecreaseWarningSliderDuringBossDuration(warningDuration));

        // 2. respawnSlider ����
        yield return StartCoroutine(DecreaseSliderDuringBossDuration(bossDuration));
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

        // �� ����̸� Ȯ���Ͽ� ������ ��Ʈ�ڽ� ���� ������ �о��
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

    // ���� ���� �ڷ�ƾ
    private IEnumerator BossAttackRoutine()
    {
        while (isBattleActive)
        {
            yield return new WaitForSeconds(bossAttackDelay);
            BossAttackCats();
        }
    }

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

    // ������ ��Ʈ�ڽ� �� ����� N������ �����ϴ� �Լ�
    private void BossAttackCats()
    {
        if (currentBoss == null || bossHitbox == null) return;

        // ��Ʈ�ڽ� ��迡 �ִ� ����̸� ã��
        List<CatData> catsAtBoundary = new List<CatData>();
        CatData[] allCats = FindObjectsOfType<CatData>();

        foreach (var cat in allCats)
        {
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

    // ��Ʈ�ڽ� �� ����̵��� ������ �����ϴ� �Լ�
    private void CatsAttackBoss()
    {
        if (currentBoss == null || bossHitbox == null) return;

        CatData[] allCats = FindObjectsOfType<CatData>();

        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            if (bossHitbox.IsAtBoundary(catPosition))
            {
                int damage = cat.catData.CatDamage;
                TakeBossDamage(damage);
            }
        }
    }

    // ���� ���� �Լ�
    public void EndBattle(bool isVictory)
    {
        isBattleActive = false;
        Destroy(currentBoss);
        currentBossData = null;
        currentBoss = null;
        bossHitbox = null;
        battleHPUI.SetActive(false);

        if (isVictory)
        {
            bossStage++;
        }

        // respawnSlider �ʱ�ȭ
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        // warningSlider �ʱ�ȭ
        if (warningSlider != null)
        {
            warningSlider.maxValue = warningTimer;
            warningSlider.value = 0f;
        }

        // ���� ����� ��Ȱ��ȭ�ߴ� ��ɵ� �ٽ� ���� ���·� ����
        SetEndFunctions();

        // ������ ����Ǹ� ��� ������� ü���� �ִ�� ȸ���ϴ� ��ɵ� �־���ҵ�


    }

    // ======================================================================================================================



}
