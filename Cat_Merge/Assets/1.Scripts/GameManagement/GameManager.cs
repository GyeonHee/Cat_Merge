using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// GameManager Script
[DefaultExecutionOrder(-1)]     // ��ũ��Ʈ ���� ���� ����(3��°)
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Main Cat Data
    private Cat[] allCatData;                                               // ��� ����� ������
    public Cat[] AllCatData => allCatData;

    // Main Mouse Data
    private Mouse[] allMouseData;                                           // ��� �� ������
    public Mouse[] AllMouseData => allMouseData;

    [SerializeField] private Transform gamePanel;                           // ����� ������ ������ �θ� Panel
    private List<RectTransform> catUIObjects = new List<RectTransform>();   // ����� UI ��ü ����Ʈ

    // Main UI Text
    [Header("---[Main UI Text]")]
    [SerializeField] private TextMeshProUGUI catCountText;                  // ����� �� �ؽ�Ʈ
    private int currentCatCount = 0;                                        // ȭ�� �� ����� ��
    public int CurrentCatCount
    {
        get => currentCatCount;
        set
        {
            currentCatCount = value;
            UpdateCatCountText();
        }
    }

    private int maxCats = 8;                                                // �ִ� ����� ��
    public int MaxCats
    {
        get => maxCats;
        set
        {
            maxCats = value;
            UpdateCatCountText();
        }
    }

    [SerializeField] private TextMeshProUGUI coinText;                      // �⺻��ȭ �ؽ�Ʈ
    private int coin = 1000000;                                             // �⺻��ȭ
    public int Coin
    {
        get => coin;
        set
        {
            coin = value;
            UpdateCoinText();
        }
    }

    [SerializeField] private TextMeshProUGUI cashText;                      // ĳ����ȭ �ؽ�Ʈ
    private int cash = 1000;                                                // ĳ����ȭ
    public int Cash
    {
        get => cash;
        set
        {
            cash = value;
            UpdateCashText();
        }
    }

    // ======================================================================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // �⺻ ����
        LoadAllCats();
        LoadAllMouses();
        UpdateCatCountText();
        UpdateCoinText();
        UpdateCashText();
    }

    private void Update()
    {
        SortCatUIObjectsByYPosition();
    }

    // ======================================================================================================================

    // ����� ���� Load �Լ�
    private void LoadAllCats()
    {
        // CatDataLoader���� catDictionary ��������
        CatDataLoader catDataLoader = FindObjectOfType<CatDataLoader>();
        if (catDataLoader == null || catDataLoader.catDictionary == null)
        {
            Debug.LogError("CatDataLoader�� ���ų� ����� �����Ͱ� �ε���� �ʾҽ��ϴ�.");
            return;
        }

        // Dictionary�� ��� ���� �迭�� ��ȯ
        allCatData = new Cat[catDataLoader.catDictionary.Count];
        catDataLoader.catDictionary.Values.CopyTo(allCatData, 0);

        //Debug.Log($"����� ������ {allCatData.Length}���� �ε�Ǿ����ϴ�.");
    }

    // �� ���� Load �Լ�
    private void LoadAllMouses()
    {
        // MouseDataLoader mouseDictionary ��������
        MouseDataLoader mouseDataLoader = FindObjectOfType<MouseDataLoader>();
        if (mouseDataLoader == null || mouseDataLoader.mouseDictionary == null)
        {
            Debug.LogError("MouseDataLoader�� ���ų� �� �����Ͱ� �ε���� �ʾҽ��ϴ�.");
            return;
        }

        // Dictionary�� ��� ���� �迭�� ��ȯ
        allMouseData = new Mouse[mouseDataLoader.mouseDictionary.Count];
        mouseDataLoader.mouseDictionary.Values.CopyTo(allMouseData, 0);
    }

    // ======================================================================================================================

    // ����� �� �Ǻ� �Լ�
    public bool CanSpawnCat()
    {
        return CurrentCatCount < MaxCats;
    }

    // ���� ����� �� ������Ű�� �Լ�
    public void AddCatCount()
    {
        if (CurrentCatCount < MaxCats)
        {
            CurrentCatCount++;
        }
    }

    // ���� ����� �� ���ҽ�Ű�� �Լ�
    public void DeleteCatCount()
    {
        if (CurrentCatCount > 0)
        {
            CurrentCatCount--;
        }
    }

    // ����� �� �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }

    // �⺻��ȭ �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    public void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"{coin}";
        }
    }

    // ĳ����ȭ �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    public void UpdateCashText()
    {
        if (cashText != null)
        {
            // ���ڸ� 3�ڸ����� �޸��� �߰��Ͽ� ǥ��
            cashText.text = cash.ToString("N0");
        }
    }

    // ======================================================================================================================

    // GamePanel���� ��� RectTransform �ڽ� ��ü �������� �Լ�
    private void UpdateCatUIObjects()
    {
        catUIObjects.Clear();

        foreach (Transform child in gamePanel)
        {
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                catUIObjects.Add(rectTransform);
            }
        }
    }

    // Y�� ��ġ�� �������� ����� UI ����
    private void SortCatUIObjectsByYPosition()
    {
        UpdateCatUIObjects();

        // Y���� �������� ���� (���� Y���� �ڷ� ���� ����)
        catUIObjects.Sort((a, b) => b.anchoredPosition.y.CompareTo(a.anchoredPosition.y));

        // ���ĵ� ������� UI ���� ���� ������Ʈ
        for (int i = 0; i < catUIObjects.Count; i++)
        {
            catUIObjects[i].SetSiblingIndex(i);
        }
    }

    // ======================================================================================================================


}