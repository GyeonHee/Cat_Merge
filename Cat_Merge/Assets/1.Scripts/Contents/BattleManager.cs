using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// BattleManager Script
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("---[Battle System]")]
    [SerializeField] private GameObject bossPrefab;             // ���� ������
    [SerializeField] private Transform bossUIParent;            // ������ ��ġ�� �θ� Transform (UI Panel ��)
    [SerializeField] private Slider respawnSlider;              // ���� ��ȯ���� ���� �ð��� ǥ���� Slider UI
    private RectTransform panelRectTransform;                   // Panel�� ũ�� ���� (��ġ�� ����)

    private float spawnInterval = 5f;                           // ���� ���� �ֱ�
    private float timer = 0f;                                   // ���� ��ȯ Ÿ�̸�
    private float bossDuration = 5f;                            // ���� ���� �ð�

    private float bossAttackDelay = 2f;                         // ���� ���� ������
    private int bossStage = 1;                                  // ���� ��������

    private GameObject currentBoss = null;                      // ���� ����
    private bool isBattleActive = false;                        // ���� Ȱ��ȭ ����

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
        panelRectTransform = bossUIParent.GetComponent<RectTransform>();
        StartCoroutine(BossSpawnRoutine());
    }

    // ======================================================================================================================

    private void InitializeBattleManager()
    {
        // �����̴� UI ����
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
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
        Mouse bossData = GetBossData();
        currentBoss = Instantiate(bossPrefab, bossUIParent);

        // ������ MouseData�� ����
        MouseData mouseUIData = currentBoss.GetComponent<MouseData>();
        if (mouseUIData != null)
        {
            mouseUIData.SetMouseData(bossData);
        }

        // ���� ��ġ ����
        RectTransform bossRectTransform = currentBoss.GetComponent<RectTransform>();
        if (bossRectTransform != null)
        {
            bossRectTransform.anchoredPosition = new Vector2(0f, 250f);
        }

        // ������ �׻� ������ �ڽ����� ���� (UI�� ����̵� �ڿ� �����)
        currentBoss.transform.SetAsFirstSibling();

        Debug.Log("���� ����");

        StartCoroutine(DecreaseSliderDuringBossDuration(bossDuration));
        StartCoroutine(DestroyBossAfterDelay(bossDuration));
    }

    // ���� �����ð����� �����̴��� �����ϴ� �ڷ�ƾ
    private IEnumerator DecreaseSliderDuringBossDuration(float duration)
    {
        float elapsedTime = 0f;
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = duration;
            respawnSlider.value = duration;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            if (respawnSlider != null)
            {
                respawnSlider.value = duration - elapsedTime;
            }
            yield return null;
        }
    }

    // ���� �����ð� ���� ���� �����ϴ� �ڷ�ƾ
    private IEnumerator DestroyBossAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentBoss != null)
        {
            Destroy(currentBoss);
            currentBoss = null;
            Debug.Log("���� ����");
            EndBattle();
        }
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

    //// ���� ������ �ش� ������ ����̸� �о�� �Լ�
    //private void PushCatsAwayFromBoss()
    //{
    //    CatData[] allCats = FindObjectsOfType<CatData>();
    //    foreach (var cat in allCats)
    //    {
    //        Vector3 direction = cat.transform.position - currentBoss.transform.position;
    //        float distance = direction.magnitude;

    //        if (distance < 5f)
    //        {
    //            Vector3 pushDirection = direction.normalized * 2f;
    //            cat.transform.position += pushDirection;
    //        }
    //    }
    //}

    //// ���� ���� �ڷ�ƾ
    //private IEnumerator BossAttackRoutine()
    //{
    //    while (isBattleActive)
    //    {
    //        yield return new WaitForSeconds(bossAttackDelay);

    //        // ������ ������ ����� 3������ ����
    //        Mouse bossData = GetBossData();
    //        CatData[] allCats = FindObjectsOfType<CatData>();
    //        List<CatData> randomCats = new List<CatData>();

    //        for (int i = 0; i < bossData.NumOfAttack; i++)
    //        {
    //            int randomIndex = Random.Range(0, allCats.Length);
    //            CatData selectedCat = allCats[randomIndex];
    //            randomCats.Add(selectedCat);
    //        }

    //        // ����̵鿡�� ���� �ֱ� (�ӽ÷� ������ ����)
    //        foreach (var cat in randomCats)
    //        {
    //            cat.TakeDamage(10); // TakeDamage �޼��带 ����� ��ũ��Ʈ���� �����ؾ� ��
    //        }
    //    }
    //}

    // ���� ���� �Լ�
    public void EndBattle()
    {
        isBattleActive = false;
        Destroy(currentBoss);
        currentBoss = null;
        //bossStage++;

        // ����̵��� ���� ���� (�ڵ� �̵� Ȱ��ȭ) (������ �׳� Ȱ��ȭ ������ �ڵ��̵� ���� �����;���)
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
        }
    }


}
