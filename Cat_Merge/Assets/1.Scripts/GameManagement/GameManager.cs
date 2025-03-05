using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

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
    private decimal coin = 100;                                      // �⺻��ȭ
    public decimal Coin
    {
        get => coin;
        set
        {
            coin = value;
            UpdateCoinText();
        }
    }

    [SerializeField] private TextMeshProUGUI cashText;                      // ĳ����ȭ �ؽ�Ʈ
    private decimal cash = 1000;                                            // ĳ����ȭ
    public decimal Cash
    {
        get => cash;
        set
        {
            cash = value;
            UpdateCashText();
        }
    }

    [Header("---[Exit Panel]")]
    [SerializeField] private GameObject exitPanel;                          // ���� Ȯ�� �г�
    [SerializeField] private Button closeButton;                            // ���� �г� �ݱ� ��ư
    [SerializeField] private Button okButton;                               // ���� Ȯ�� ��ư
    private bool isBackButtonPressed = false;                               // �ڷΰ��� ��ư�� ���ȴ��� ����

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

        InitializeExitSystem();
    }

    private void Update()
    {
        SortCatUIObjectsByYPosition();
        CheckExitInput();
    }

    // ======================================================================================================================

    // ����� ���� Load �Լ�
    private void LoadAllCats()
    {
        CatDataLoader catDataLoader = FindObjectOfType<CatDataLoader>();

        if (catDataLoader == null || catDataLoader.catDictionary == null)
        {
            Debug.LogError("CatDataLoader�� ���ų� ����� �����Ͱ� �ε���� �ʾҽ��ϴ�.");
            return;
        }

        allCatData = new Cat[catDataLoader.catDictionary.Count];
        catDataLoader.catDictionary.Values.CopyTo(allCatData, 0);
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
            // ���ڸ� 3�ڸ����� �޸��� �߰��Ͽ� ǥ��
            coinText.text = coin.ToString("N0");
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

    // ���� ���� ��ư �ʱ�ȭ
    private void InitializeExitSystem()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(false);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CancelQuit);
        }
        if (okButton != null)
        {
            okButton.onClick.AddListener(QuitGame);
        }
    }

    // ���� �Է� üũ
    private void CheckExitInput()
    {
        // ����Ƽ ������ �� �ȵ���̵忡�� �ڷΰ��� ��ư
        if ((Application.platform == RuntimePlatform.Android && Input.GetKey(KeyCode.Escape)) ||
            (Application.platform == RuntimePlatform.WindowsEditor && Input.GetKeyDown(KeyCode.Escape)))
        {
            HandleExitInput();
        }
    }

    // ���� �Է� ó��
    public void HandleExitInput()
    {
        if (exitPanel != null && !isBackButtonPressed)
        {
            isBackButtonPressed = true;
            if (exitPanel.activeSelf)
            {
                CancelQuit();
            }
            else
            {
                ShowExitPanel();
            }
            StartCoroutine(ResetBackButtonPress());
        }
    }

    // �ڷΰ����ư �Է� ������ ����
    private IEnumerator ResetBackButtonPress()
    {
        yield return new WaitForSeconds(0.2f);
        isBackButtonPressed = false;
    }

    // ���� �г� ǥ��
    private void ShowExitPanel()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(true);
        }
    }

    // ���� ����
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ���� ���
    public void CancelQuit()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(false);
        }
    }

    // ======================================================================================================================

    // �ʵ��� ��� ����� ���� ������Ʈ
    public void UpdateAllCatsInField()
    {
        // �̹� ������ ������� ������ ��ġ�� ���� ������Ʈ�� ��ġ�� ����
        // ���� Cat ��ġ�� ������ ����̵��� �������� �߰��� ��ġ�� ����� Cat���� ������Ʈ�ϱ� ����
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                foreach (Cat cat in allCatData)
                {
                    if (cat.CatId == catData.catData.CatId)
                    {
                        catData.SetCatData(cat);
                        //Debug.Log($"{catData.catData.CatDamage}, {catData.catData.CatHp}");
                        break;
                    }
                }
            }
        }
    }

    // ��� ������� �Ʒ� �����͸� �����ϴ� �޼���
    public void SaveTrainingData(Cat[] cats)
    {
        // ���⿡ ���� ���� ���� ����
        // ��: PlayerPrefs�� ���� �ý����� ����Ͽ� �� ������� ���� ���� ����
    }


}