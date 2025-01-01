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
    //private RectTransform panelRectTransform;                   // Panel�� ũ�� ���� (��ġ�� ����)

    private float spawnInterval = 5f;                           // ���� ���� �ֱ�
    private float timer = 0f;                                   // ���� ��ȯ Ÿ�̸�
    private float bossDuration = 60f;                           // ���� ���� �ð�
    //private float bossAttackDelay = 2f;                         // ���� ���� ������
    private int bossStage = 1;                                  // ���� ��������

    private GameObject currentBoss = null;                      // ���� ����
    private bool isBattleActive = false;                        // ���� Ȱ��ȭ ����

    [Header("---[Boss UI]")]
    [SerializeField] private GameObject battleHPUI;             // Battle HP UI (Ȱ��ȭ/��Ȱ��ȭ ����)
    [SerializeField] private TextMeshProUGUI bossNameText;      // Boss Name Text
    [SerializeField] private Slider bossHPSlider;               // HP Slider
    [SerializeField] private TextMeshProUGUI bossHPText;        // HP % Text

    private Mouse currentBossData = null;                       // ���� ���� ������
    private float currentBossHP;                                // ������ ���� HP
    private float maxBossHP;                                    // ������ �ִ� HP

    private Coroutine bossDurationCoroutine;                    // 
    private Coroutine bossSliderCoroutine;                      // 

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
        //panelRectTransform = bossUIParent.GetComponent<RectTransform>();
        StartCoroutine(BossSpawnRoutine());
    }

    private void InitializeBattleManager()
    {
        // �����̴� UI ����
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
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
                    LoadAndDisplayBoss();
                    StartBattle();
                }
            }

            yield return null;
        }
    }

    // ���� ���� �Լ�
    private void LoadAndDisplayBoss()
    {
        // bossStage�� �´� Mouse�� �����ͼ� ������ ����
        currentBossData = GetBossData();
        currentBoss = Instantiate(bossPrefab, bossUIParent);

        // ������ MouseData�� ����
        MouseData mouseUIData = currentBoss.GetComponent<MouseData>();
        mouseUIData.SetMouseData(currentBossData);

        // ���� ��ġ ����
        RectTransform bossRectTransform = currentBoss.GetComponent<RectTransform>();
        bossRectTransform.anchoredPosition = new Vector2(0f, 250f);

        // ������ �׻� ������ �ڽ����� ���� (UI�� ����̵� �ڿ� �����)
        currentBoss.transform.SetAsFirstSibling();

        UpdateBossUI();

        StartCoroutine(DecreaseSliderDuringBossDuration(bossDuration));
        StartCoroutine(BossBattleRoutine(bossDuration));
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

        Debug.Log(currentBossData.MouseGrade + ", " + currentBossData.MouseHp);

        maxBossHP = currentBossData.MouseHp;
        currentBossHP = maxBossHP;
        bossHPSlider.maxValue = maxBossHP;
        bossHPSlider.value = currentBossHP;
        bossHPText.text = $"{maxBossHP}%";
    }

    // ���� �����ð����� �����̴��� �����ϴ� �ڷ�ƾ 
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

            // ���Ƿ� �׽�Ʈ
            TakeBossDamage(Time.deltaTime * 5);

            // ���� ü���� 0 ���ϰ� �Ǹ� ��� ���� ����
            if (currentBossHP <= 0)
            {
                EndBattle(true); // �¸��� ���� ����
                yield break;
            }

            yield return null;
        }

        // ���� �ð� �ʰ��� ���� ����
        EndBattle(false);
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
        isBattleActive = true;

        // ����̵��� ��� �ൿ ����
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.SetAutoMoveState(false);
        }

        //PushCatsAwayFromBoss();

        // ����̵��� ������ ���� �̵�
        foreach (var cat in allCats)
        {
            cat.MoveTowardsBoss(currentBoss.transform.position);
        }

        //StartCoroutine(BossAttackRoutine());
    }

    /*
    // ���� ������ �ش� ������ ����̸� �о�� �Լ�
    private void PushCatsAwayFromBoss()
    {
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            Vector3 direction = cat.transform.position - currentBoss.transform.position;
            float distance = direction.magnitude;

            if (distance < 5f)
            {
                Vector3 pushDirection = direction.normalized * 2f;
                cat.transform.position += pushDirection;
            }
        }
    }

    // ���� ���� �ڷ�ƾ
    private IEnumerator BossAttackRoutine()
    {
        while (isBattleActive)
        {
            yield return new WaitForSeconds(bossAttackDelay);

            // ������ ������ ����� 3������ ����
            Mouse bossData = GetBossData();
            CatData[] allCats = FindObjectsOfType<CatData>();
            List<CatData> randomCats = new List<CatData>();

            for (int i = 0; i < bossData.NumOfAttack; i++)
            {
                int randomIndex = Random.Range(0, allCats.Length);
                CatData selectedCat = allCats[randomIndex];
                randomCats.Add(selectedCat);
            }

            // ����̵鿡�� ���� �ֱ� (�ӽ÷� ������ ����)
            foreach (var cat in randomCats)
            {
                cat.TakeDamage(10); // TakeDamage �޼��带 ����� ��ũ��Ʈ���� �����ؾ� ��
            }
        }
    }
    */

    // ���� ���� �Լ�
    public void EndBattle(bool isVictory)
    {
        isBattleActive = false;
        Destroy(currentBoss);
        currentBoss = null;
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

        // ����̵��� ���� ���� (�ڵ� �̵� Ȱ��ȭ) (������ �׳� Ȱ��ȭ ������ �ڵ��̵� ���� �����;���)
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
        }
    }

}
