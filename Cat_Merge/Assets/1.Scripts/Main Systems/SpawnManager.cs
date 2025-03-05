using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

// ����� ���� Script
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private Button spawnButton;                    // ���� ��ư

    [SerializeField] private GameObject catPrefab;                  // ����� UI ������
    [SerializeField] private Transform catUIParent;                 // ����̸� ��ġ�� �θ� Transform (UI Panel ��)
    private RectTransform panelRectTransform;                       // Panel�� ũ�� ���� (��ġ�� ����)
    private GameManager gameManager;                                // GameManager

    [SerializeField] private TextMeshProUGUI nowAndMaxFoodText;     // ������ư �ؿ� ���� ���� ������ �ִ� ���� ���� (?/?)
    private int nowFood = 5;                                        // ���� ���� ����
    public int NowFood
    {
        get => nowFood;
        set
        {
            nowFood = value;
            UpdateFoodText();
        }
    }
    private bool isStoppedReduceCoroutine = false;                  // �ڷ�ƾ ���� �Ǻ�
    private bool isStoppedAutoCoroutine = false;

    [SerializeField] private Image foodFillAmountImg;               // ��ȯ �̹���
    [SerializeField] public Image autoFillAmountImg;                // �ڵ���ȯ �̹���

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
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent�� RectTransform�� ������ ���� �ʽ��ϴ�.");
            return;
        }

        UpdateFoodText();
        StartCoroutine(CreateFoodTime());
        StartCoroutine(AutoCollectingTime());
    }

    // ======================================================================================================================

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleSpawnState()
    {
        spawnButton.interactable = false;
    }

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleSpawnState()
    {
        spawnButton.interactable = true;
    }

    // ======================================================================================================================

    // ����� ���� ��ư Ŭ��
    public void OnClickedSpawn()
    {
        if (gameManager.CanSpawnCat())
        {
            // ���̰� 1�� �̻��̰ų� �ִ�ġ ������ �� ����
            if (NowFood > 0 && NowFood <= ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
            {
                // ���̰� �ٰ� �ڷ�ƾ�� ����Ǿ��ִٸ� �ٽ� ����(���� ���̰� �ִ�ġ�϶� �ڷ�ƾ ����Ǿ�����)
                NowFood--;
                if (isStoppedReduceCoroutine)
                {
                    StartCoroutine(CreateFoodTime());
                    isStoppedReduceCoroutine = false;
                }
                if (isStoppedAutoCoroutine)
                {
                    StartCoroutine(AutoCollectingTime());
                    isStoppedAutoCoroutine = false;
                }

                // ���׷��̵� �ý��� Ȯ�强�� ���� ������ ����� ���� �ڵ�
                Cat catData = GetCatDataForSpawn();
                LoadAndDisplayCats(catData);
                QuestManager.Instance.AddSpawnCount();
                gameManager.AddCatCount();
                FriendshipManager.Instance.nowExp += 1;
                FriendshipManager.Instance.expGauge.value += 0.05f;

                // ���� ��ȯ �� ���� ���� �ڷ�ƾ �����
                if (isStoppedReduceCoroutine)
                {
                    StartCoroutine(CreateFoodTime());
                    isStoppedReduceCoroutine = false;
                }
            }
            else
            {
                NotificationManager.Instance.ShowNotification("���̰� �����մϴ�!!");
            }

        }
        else
        {
            NotificationManager.Instance.ShowNotification("����� �������� �ִ��Դϴ�!!");
        }
    }

    // �ڵ������� ����� ���� �Լ�
    public void SpawnCat()
    {
        if (!gameManager.CanSpawnCat()) return;

        // ���׷��̵� �ý��� Ȯ�强�� ���� ������ ����� ���� �ڵ�
        Cat catData = GetCatDataForSpawn();
        GameObject newCat = LoadAndDisplayCats(catData);

        DragAndDropManager catDragAndDrop = newCat.GetComponent<DragAndDropManager>();
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = gameManager.AllCatData[0];
            catDragAndDrop.UpdateCatUI();
        }

        gameManager.AddCatCount();
        FriendshipManager.Instance.nowExp += 1;
        FriendshipManager.Instance.expGauge.value += 0.05f;

        // �ڵ� ��ȯ �� ���� ���� �ڷ�ƾ �����
        if (isStoppedReduceCoroutine)
        {
            StartCoroutine(CreateFoodTime());
            isStoppedReduceCoroutine = false;
        }
    }

    // �������� �̿�� ��޿� ���� ���� �� ���� (12/26 ���� �ۼ�)
    public void SpawnGradeCat(int grade)
    {
        if (!gameManager.CanSpawnCat()) return;
        Cat catData = gameManager.AllCatData[grade];

        GameObject newCat = LoadAndDisplayCats(catData);

        DragAndDropManager catDragAndDrop = newCat.GetComponent<DragAndDropManager>();
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = gameManager.AllCatData[grade];
            catDragAndDrop.UpdateCatUI();
        }

        gameManager.AddCatCount();
        FriendshipManager.Instance.nowExp += 1;
        FriendshipManager.Instance.expGauge.value += 0.05f;
    }

    // ����� ���� �����͸� �����ϴ� �Լ� (���׷��̵� �ý��� ���)
    private Cat GetCatDataForSpawn()
    {
        // ����: �������� ����� ����� �����ϰų� ���� ���׷��̵带 ����Ͽ� ����
        // return gameManager.GetRandomCatForSpawn();  // �� �κ��� ���߿� ���׷��̵� �ý��ۿ� �°� ����
        int catGrade = 0;
        DictionaryManager.Instance.UnlockCat(catGrade);
        return gameManager.AllCatData[catGrade];
    }

    // Panel�� ���� ��ġ�� ����� ��ġ�ϴ� �Լ�
    private GameObject LoadAndDisplayCats(Cat catData)
    {
        GameObject catUIObject = Instantiate(catPrefab, catUIParent);

        // CatData ����
        CatData catUIData = catUIObject.GetComponent<CatData>();
        if (catUIData != null)
        {
            catUIData.SetCatData(catData);

            // �ڵ� �̵� ���¸� ����ȭ
            if (gameManager != null)
            {
                catUIData.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
            }
        }

        // ���� ��ġ ����
        Vector2 randomPos = GetRandomPosition(panelRectTransform);
        RectTransform catRectTransform = catUIObject.GetComponent<RectTransform>();
        if (catRectTransform != null)
        {
            catRectTransform.anchoredPosition = randomPos;
        }

        return catUIObject;
    }

    // Panel�� ���� ��ġ ����ϴ� �Լ�
    Vector3 GetRandomPosition(RectTransform panelRectTransform)
    {
        float panelWidth = panelRectTransform.rect.width;
        float panelHeight = panelRectTransform.rect.height;

        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);

        Vector3 respawnPos = new Vector3(randomX, randomY, 0f);
        return respawnPos;
    }

    // ======================================================================================================================

    // ���� ���� �ð�
    private IEnumerator CreateFoodTime()
    {
        float elapsed = 0f; // ��� �ð�

        // ���� ���̰� �ִ�ġ ������ �� �ڷ�ƾ ����
        while (NowFood < ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
        {
            foodFillAmountImg.fillAmount = 0f; // ��Ȯ�� 1�� ����
            while (elapsed < ItemFunctionManager.Instance.reduceProducingFoodTimeList[ItemMenuManager.Instance.ReduceProducingFoodTimeLv].value)
            {
                elapsed += Time.deltaTime; // �� �����Ӹ��� ��� �ð� ����
                foodFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.reduceProducingFoodTimeList[ItemMenuManager.Instance.ReduceProducingFoodTimeLv].value); // 0 ~ 1 ���̷� ���� ���
                yield return null; // ���� �����ӱ��� ���
            }
            NowFood++;
            foodFillAmountImg.fillAmount = 1f; // ��Ȯ�� 1�� ����
            elapsed = 0f;
        }

        // ���� ���̰����� �ִ�ġ�̸� �ڷ�ƾ�� �����Ų��.
        if (NowFood == ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
        {
            StopCoroutine(CreateFoodTime());
            isStoppedReduceCoroutine = true;
        }
    }

    private IEnumerator AutoCollectingTime()
    {
        float elapsed = 0f;

        // ���� ������ Ȯ���ϸ� ����
        while (true)
        {
            // ���� ���� �� ���
            if (BattleManager.Instance.IsBattleActive)
            {
                yield return null;
                continue;
            }

            // ���̰� 1 �̻��ϰ�� �ڵ� ���� ���� (������..)
            if (NowFood >= 1)
            {
                autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / (float)ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value);

                // ���� �ð� �Ϸ�ɶ����� ���
                while (elapsed < ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
                {
                    if (BattleManager.Instance.IsBattleActive)
                    {
                        yield return null;
                        break;
                    }

                    elapsed += Time.deltaTime;
                    autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / (float)ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value);
                    yield return null;
                }

                // �Ϸ�Ǹ� ���� ���̰� ����� ����
                if (!BattleManager.Instance.IsBattleActive && elapsed >= ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
                {
                    if (gameManager.CanSpawnCat())
                    {
                        NowFood--;
                        SpawnCat();

                        // �ڵ� ���� �� ���� ���� �ڷ�ƾ �����
                        if (isStoppedReduceCoroutine)
                        {
                            StartCoroutine(CreateFoodTime());
                            isStoppedReduceCoroutine = false;
                        }
                    }
                    elapsed = 0f; // ���� ���� �ʱ�ȭ

                }
            }

            // ���̰� 0���� ����
            if (NowFood == 0)
            {
                isStoppedAutoCoroutine = true;
                yield break;
            }

            yield return null;
        }
    }

    // �ִ� ���� ������ �������� �� ȣ��Ǵ� �Լ�
    public void OnMaxFoodIncreased()
    {
        // �ִ� ������ �����ߴ��� Ȯ��
        if (ItemMenuManager.Instance.MaxFoodsLv >= ItemFunctionManager.Instance.maxFoodsList.Count)
        {
            return;
        }

        // ���� ���� ���� �ִ�ġ���� ������ ���� ���� �ڷ�ƾ ����
        int currentMaxFood = ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value;
        if (NowFood < currentMaxFood)
        {
            if (isStoppedReduceCoroutine)
            {
                StartCoroutine(CreateFoodTime());
                isStoppedReduceCoroutine = false;
            }
        }
    }

    public void UpdateFoodText()
    {
        if (ItemMenuManager.Instance.MaxFoodsLv >= ItemFunctionManager.Instance.maxFoodsList.Count)
        {
            // �ִ� ������ ��� ������ �� ���
            int lastMaxFood = ItemFunctionManager.Instance.maxFoodsList[ItemFunctionManager.Instance.maxFoodsList.Count - 1].value;
            nowAndMaxFoodText.text = $"({nowFood} / {lastMaxFood})";
        }
        else
        {
            // ���� ������ �� ���
            int currentMaxFood = ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value;
            nowAndMaxFoodText.text = $"({nowFood} / {currentMaxFood})";
        }
    }

}
