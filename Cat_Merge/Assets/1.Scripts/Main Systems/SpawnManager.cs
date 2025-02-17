using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

// ����� ���� Script
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private Button spawnButton;                    // ���� ��ư

    [SerializeField] private GameObject catPrefab;      // ����� UI ������
    [SerializeField] private Transform catUIParent;     // ����̸� ��ġ�� �θ� Transform (UI Panel ��)
    private RectTransform panelRectTransform;           // Panel�� ũ�� ���� (��ġ�� ����)
    private GameManager gameManager;                    // GameManager

    [SerializeField] private TextMeshProUGUI nowAndMaxFoodText;     // ������ư �ؿ� ���� ���� ������ �ִ� ���� ����
    private int nowFood = 5;                                        // ���� ���� ����
    private bool isStoppedReduceCoroutine = false;                        // �ڷ�ƾ ���� �Ǻ�
    private bool isStoppedAutoCoroutine = false;

    [SerializeField] private Image foodFillAmountImg;
    [SerializeField] public Image autoFillAmountImg;

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

        StartCoroutine(CreateFoodTime());
        StartCoroutine(AutoCollectingTime());
    }

    private void Update()
    {
        // ���� ���� ������ �ִ�ġ ���� ǥ��(���߿� �Լ��� ���� ��)
        nowAndMaxFoodText.text = $"({nowFood} / {ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value})";
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
            if (nowFood > 0 && nowFood <= ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
            {
                // ���̰� �ٰ� �ڷ�ƾ�� ����Ǿ��ִٸ� �ٽ� ����(���� ���̰� �ִ�ġ�϶� �ڷ�ƾ ����Ǿ�����)
                nowFood--;
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
            }
            else
            {
                NotificationManager.Instance.ShowNotification("No Food");
                Debug.Log("���� ���� ����");
            }

        }
        else
        {
            NotificationManager.Instance.ShowNotification("Full Cats");
            Debug.Log("����� �ִ� ���� ������ �����߽��ϴ�!");
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
        while (nowFood < ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
        {
            foodFillAmountImg.fillAmount = 0f; // ��Ȯ�� 1�� ����
            while (elapsed < ItemFunctionManager.Instance.reduceProducingFoodTimeList[ItemMenuManager.Instance.ReduceProducingFoodTimeLv].value)
            {
                elapsed += Time.deltaTime; // �� �����Ӹ��� ��� �ð� ����
                foodFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.reduceProducingFoodTimeList[ItemMenuManager.Instance.ReduceProducingFoodTimeLv].value); // 0 ~ 1 ���̷� ���� ���
                yield return null; // ���� �����ӱ��� ���
            }
            nowFood++;
            foodFillAmountImg.fillAmount = 1f; // ��Ȯ�� 1�� ����
            elapsed = 0f;
        }

        // ���� ���̰����� �ִ�ġ�̸� �ڷ�ƾ�� �����Ų��.
        if (nowFood == ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
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
            if (nowFood >= 1)
            {
                autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value);

                // ���� �ð� �Ϸ�ɶ����� ���
                while (elapsed < ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
                {
                    if (BattleManager.Instance.IsBattleActive)
                    {
                        yield return null;
                        break;
                    }

                    elapsed += Time.deltaTime;
                    autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value);
                    yield return null;
                }

                // �Ϸ�Ǹ� ���� ���̰� ����� ����
                if (!BattleManager.Instance.IsBattleActive && elapsed >= ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
                {
                    nowFood--;
                    SpawnCat();
                    elapsed = 0f; // ���� ���� �ʱ�ȭ
                }
            }

            // ���̰� 0���� ����
            if (nowFood == 0)
            {
                isStoppedAutoCoroutine = true;
                yield break;
            }

            yield return null;
        }

        //float elapsed = 0f;
        //// ���� ���̰� 1�� �̻��϶��� �ڷ�ƾ ����
        //while(nowFood >= 1)
        //{
        //    autoFillAmountImg.fillAmount = 0f;
        //    while (elapsed < ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
        //    {
        //        elapsed += Time.deltaTime;
        //        autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value); // 0 ~ 1 ���̷� ���� ���
        //        yield return null; // ���� �����ӱ��� ���
        //    }
        //    nowFood--;
        //    SpawnCat();
        //    elapsed = 0f;
        //}

        //if(nowFood == 0)
        //{
        //    StopCoroutine(AutoCollectingTime());
        //    isStoppedAutoCoroutine = true;
        //}
    }


}