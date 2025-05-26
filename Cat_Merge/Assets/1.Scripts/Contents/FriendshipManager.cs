using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

[Serializable]
public class CatFriendship
{
    public int catGrade;                        // ����� ���
    public int currentExp;                      // ���� ����ġ
    public bool[] isLevelUnlocked;              // �� ���� �ر� ����
    public bool[] rewardsClaimed;               // �� ������ ���� ���� ����
    public List<string> activePassiveEffects;   // Ȱ��ȭ�� �нú� ȿ�� ���

    public CatFriendship(int grade)
    {
        catGrade = grade;
        currentExp = 0;
        isLevelUnlocked = new bool[5];
        rewardsClaimed = new bool[5];
        activePassiveEffects = new List<string>();
    }
}

// ����� ������ ��ũ��Ʈ
[DefaultExecutionOrder(-4)]
public class FriendshipManager : MonoBehaviour, ISaveable
{


    #region Variables

    // ���� ��ȯ�� ����ġ 1 ȹ��
    // ���ϵ�� ������ ����ġ 2 ȹ��
    // ���� ��ȯ�� ����ġ 1 ȹ��
    // ��������� ������ �Ǿ� ��ȯ�� �� ������� ����ġ 1ȹ��

    public static FriendshipManager Instance { get; private set; }

    // ������ ���� �ݾ� ����
    private readonly int[] rewardAmounts = new int[] { 5, 10, 15, 20, 25 };

    [Header("---[UI References]")]
    [SerializeField] private Transform friendshipButtonParent;
    [SerializeField] private Button[] friendshipButtonPrefabs;
    [SerializeField] private Transform fullStarImgParent;
    [SerializeField] private GameObject fullStarPrefab;
    [SerializeField] private TextMeshProUGUI expRequirementText;
    [SerializeField] private Slider expGauge;
    [SerializeField] private Transform dictionarySlotParent;

    private Dictionary<int, Button[]> catFriendshipButtons = new Dictionary<int, Button[]>();
    private Dictionary<int, GameObject> fullStars = new Dictionary<int, GameObject>();
    private Button[] activeButtons;
    private bool buttonClick = false;

    private static readonly Vector2 defaultOffset = new Vector2(-210, 0);
    private const float STAR_SPACING = 42f;

    private Dictionary<int, CatFriendship> catFriendships = new Dictionary<int, CatFriendship>();                           // �� ����̺� ������ ���� ����
    private Dictionary<int, List<(int exp, int reward)>> levelByGrade = new Dictionary<int, List<(int exp, int reward)>>(); // ������ �ʿ� ����ġ ������
    private Dictionary<int, int> currentLevels = new Dictionary<int, int>();                                                // ���� ������ �����ϱ� ���� ����
    private Dictionary<int, bool[]> buttonUnlockStatus = new Dictionary<int, bool[]>();                                     // FriendshipManager Ŭ������ ���� ����


    private bool isDataLoaded = false;          // ������ �ε� Ȯ��

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
        InitializeFriendshipData();

        // GoogleManager���� �����͸� �ε����� ���� ��쿡�� �ʱ�ȭ
        if (!isDataLoaded)
        {
            InitializeCatFriendships();
            InitializeCurrentLevels();
        }

        InitializeUI();
    }

    private void OnDestroy()
    {
        CleanupUI();
    }

    #endregion


    #region Initialize

    // �� ����̺� ������ ������ �ʱ�ȭ �Լ�
    private void InitializeCatFriendships()
    {
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            catFriendships[i] = new CatFriendship(i);
            buttonUnlockStatus[i] = new bool[5]; // �� ����̺��� 5���� ��ư ���� ����
        }
    }

    // ��� ������� ���� ���� �ʱ�ȭ �Լ�
    private void InitializeCurrentLevels()
    {
        // ��� ������� ���� ������ 0���� �ʱ�ȭ
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            currentLevels[i] = 0;
        }
    }

    // UI ������Ʈ �ʱ�ȭ �Լ�
    private void InitializeUI()
    {
        InitializeFriendshipButtons();
        InitializeFullStars();
        if (expGauge != null) expGauge.value = 0f;
    }

    // ������ ��ư ���� �� �ʱ�ȭ �Լ�
    private void InitializeFriendshipButtons()
    {
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            Button[] buttons = new Button[5];
            for (int j = 0; j < 5; j++)
            {
                buttons[j] = Instantiate(friendshipButtonPrefabs[j], friendshipButtonParent);
                buttons[j].gameObject.SetActive(false);

                int level = j;
                int catGrade = i;
                buttons[j].onClick.AddListener(() => OnFriendshipButtonClick(catGrade, level));
            }
            catFriendshipButtons[i] = buttons;
        }
    }

    // fullStar �ʱ�ȭ �Լ�
    private void InitializeFullStars()
    {
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            GameObject fullStar = Instantiate(fullStarPrefab, fullStarImgParent);
            fullStar.gameObject.SetActive(false);
            fullStars[i] = fullStar;
        }
    }

    // ������ ������ �ʱ�ȭ �Լ�
    private void InitializeFriendshipData()
    {
        // ��� ����� ����ġ �����͸� �ʱ�ȭ
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            levelByGrade[i] = FriendshipDataLoader.Instance.GetDataByGrade(i + 1)
                .Select(data => (data.exp, data.reward))
                .ToList();
        }
    }

    #endregion


    #region UI Management

    // ����� ���ý� UI ������Ʈ �Լ�
    public void OnCatSelected(int catGrade)
    {
        DeactivateCurrentButtons();
        ActivateButtonsForCat(catGrade);
        UpdateFullStarVisibility(catGrade);
        UpdateFriendshipUI(catGrade);
    }

    // ���� Ȱ��ȭ�� ��ư�� ��Ȱ��ȭ �Լ�
    private void DeactivateCurrentButtons()
    {
        if (activeButtons != null)
        {
            foreach (var button in activeButtons)
            {
                if (button != null)
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }

    // ���õ� ������� ��ư�� Ȱ��ȭ �Լ�
    private void ActivateButtonsForCat(int catGrade)
    {
        if (catFriendshipButtons.TryGetValue(catGrade, out Button[] buttons))
        {
            activeButtons = buttons;
            foreach (var button in buttons)
            {
                button.gameObject.SetActive(true);
                UpdateButtonUI(button, catGrade);
            }
        }
    }

    // ��ư UI ������Ʈ �Լ�
    private void UpdateButtonUI(Button button, int catGrade)
    {
        // ���� �ݾ� �ؽ�Ʈ ����
        Transform firstOpenBG = button.transform.Find("FirstOpenBG");
        if (firstOpenBG != null)
        {
            TextMeshProUGUI cashText = firstOpenBG.Find("Cash Text")?.GetComponent<TextMeshProUGUI>();
            if (cashText != null)
            {
                int level = System.Array.IndexOf(catFriendshipButtons[catGrade], button);
                int rewardAmount = GetRewardAmount(level);
                cashText.text = $"+ {rewardAmount}";
            }
        }
    }

    // fullStar ���� ������Ʈ �Լ�
    private void UpdateFullStarVisibility(int catGrade)
    {
        foreach (var pair in fullStars)
        {
            if (pair.Value != null)
            {
                pair.Value.gameObject.SetActive(pair.Key == catGrade);
            }
        }
    }

    // UI ���� �Լ�
    public void ResetUI()
    {
        DeactivateCurrentButtons();
        foreach (var star in fullStars.Values)
        {
            if (star != null)
            {
                star.gameObject.SetActive(false);
            }
        }
        if (expGauge != null) expGauge.value = 0f;
        if (expRequirementText != null) expRequirementText.text = "";
    }

    // UI ��� ���� �Լ�(OnDestroy)
    private void CleanupUI()
    {
        foreach (var buttons in catFriendshipButtons.Values)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
        }

        foreach (var star in fullStars.Values)
        {
            if (star != null)
            {
                Destroy(star.gameObject);
            }
        }

        catFriendshipButtons.Clear();
        fullStars.Clear();
    }

    #endregion


    #region Friendship System

    // ������ ��ư Ŭ�� ó�� �Լ�
    private void OnFriendshipButtonClick(int catGrade, int level)
    {
        if (CanClaimLevelReward(catGrade, level))
        {
            ClaimReward(catGrade, level);
            buttonClick = true;
            UpdateFriendshipUI(catGrade);
        }
    }

    // ����ġ �߰� �� ���� üũ �Լ�
    public void AddExperience(int catGrade, int expAmount)
    {
        if (!catFriendships.ContainsKey(catGrade) || IsMaxLevel(catGrade)) return; // MAX �����̸� ����ġ ȹ�� �ߴ�

        var friendship = catFriendships[catGrade];
        friendship.currentExp += expAmount;

        UpdateFriendshipUI(catGrade);
    }

    // ������ UI ������Ʈ �Լ�
    private void UpdateFriendshipUI(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        var friendship = catFriendships[catGrade];
        int currentLevel = currentLevels[catGrade];

        // ��ư Ŭ������ ���� ���� ȹ�� ó��
        if (buttonClick)
        {
            // ���� ������ ����ġ��ŭ ����
            friendship.currentExp -= levelByGrade[catGrade - 1][currentLevel].exp;

            // ���� ���� ���
            currentLevel = Mathf.Min(currentLevel + 1, 4);
            currentLevels[catGrade] = currentLevel;

            buttonClick = false;
        }

        // ���� Dictionary���� ���õ� ����̿� ������Ʈ�Ϸ��� ����̰� ���� ���� UI �ؽ�Ʈ ������Ʈ
        if (DictionaryManager.Instance.GetCurrentSelectedCatGrade() == catGrade)
        {
            if (expRequirementText != null)
            {
                if (IsMaxLevel(catGrade))
                {
                    expRequirementText.text = "MAX";
                    if (expGauge != null)
                    {
                        expGauge.value = 1f; // �������� �ִ�� ä��
                    }
                }
                else
                {
                    int nextExp = GetNextLevelExp(catGrade);
                    expRequirementText.text = $"{friendship.currentExp} / {nextExp}";

                    // ������ ������Ʈ
                    if (expGauge != null)
                    {
                        float progress = (float)friendship.currentExp / nextExp;
                        expGauge.value = Mathf.Clamp01(progress);
                    }
                }
            }
        }

        // ���� �ر� ���� ������Ʈ
        UpdateLevelUnlockStatus(friendship, catGrade);

        // ��ư ���� ������Ʈ - ���� ���õ� ������� ����
        if (DictionaryManager.Instance.GetCurrentSelectedCatGrade() == catGrade)
        {
            UpdateFriendshipButtonStates(catGrade);
        }

        // ���� ������ friendshipNewImage ������Ʈ
        if (dictionarySlotParent != null)
        {
            Transform slot = dictionarySlotParent.GetChild(catGrade - 1);
            if (slot != null)
            {
                GameObject friendshipNewImage = slot.transform.Find("Button/New Image")?.gameObject;
                if (friendshipNewImage != null)
                {
                    bool hasRewards = HasUnclaimedFriendshipRewards(catGrade);
                    friendshipNewImage.SetActive(hasRewards);

                    // DictionaryManager�� New Image ���µ� ������Ʈ
                    DictionaryManager.Instance.UpdateNewImageStatus();
                }
            }
        }
    }

    // ��ư ���� ������Ʈ �Լ�
    private void UpdateFriendshipButtonStates(int catGrade)
    {
        if (!catFriendshipButtons.ContainsKey(catGrade)) return;

        var friendshipInfo = GetFriendshipInfo(catGrade);
        var buttons = catFriendshipButtons[catGrade];
        bool canActivateNextLevel = true;

        // MAX ���� üũ
        bool isMaxLevel = IsMaxLevel(catGrade);

        // ���� ���� ���� ���� ã��
        int currentProgressLevel = 0;
        for (int i = 0; i < friendshipInfo.isClaimed.Length; i++)
        {
            if (!friendshipInfo.isClaimed[i])
            {
                currentProgressLevel = i;
                break;
            }
            if (i == friendshipInfo.isClaimed.Length - 1)
            {
                currentProgressLevel = i;
            }
        }

        // �нú� ȿ�� ������ ��������
        var passiveEffects = FriendshipDataLoader.Instance.GetDataByGrade(catGrade);

        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button != null)
            {
                bool isAlreadyClaimed = friendshipInfo.isClaimed[i];

                // Passive Text ������Ʈ
                Transform passiveTextTr = button.transform.Find("Passive Text");
                if (passiveTextTr != null)
                {
                    TextMeshProUGUI passiveText = passiveTextTr.GetComponent<TextMeshProUGUI>();
                    if (passiveText != null)
                    {
                        passiveText.text = passiveEffects[i].passive;
                    }
                }

                // LockBG ���� ������Ʈ
                Transform lockBG = button.transform.Find("LockBG");
                if (lockBG != null)
                {
                    bool shouldLock = !isAlreadyClaimed && (!canActivateNextLevel || !friendshipInfo.isUnlocked[i]);
                    lockBG.gameObject.SetActive(shouldLock && !isMaxLevel);
                }

                // FirstOpenBG ���� ������Ʈ
                Transform firstOpenBG = button.transform.Find("FirstOpenBG");
                if (firstOpenBG != null)
                {
                    bool canShowReward = !isAlreadyClaimed && canActivateNextLevel && friendshipInfo.isUnlocked[i];
                    firstOpenBG.gameObject.SetActive(canShowReward && !isMaxLevel);
                }

                // Star ���� ������Ʈ
                Transform star = FindDeepChild(button.transform, "Star");
                if (star != null)
                {
                    star.gameObject.SetActive(isAlreadyClaimed);
                }

                // ��ư ��ȣ�ۿ� ���� ����
                button.interactable = !isMaxLevel && !isAlreadyClaimed && canActivateNextLevel && friendshipInfo.isUnlocked[i];

                // ���� ���� Ȱ��ȭ ���� üũ
                if (i == currentProgressLevel && !friendshipInfo.isClaimed[i] && !isMaxLevel)
                {
                    canActivateNextLevel = false;
                }
            }
        }

        // fullStar ������Ʈ
        UpdateFullStarUI(catGrade, friendshipInfo);
    }

    // fullStar UI ������Ʈ �Լ�
    private void UpdateFullStarUI(int catGrade, (int currentExp, bool[] isUnlocked, bool[] isClaimed) friendshipInfo)
    {
        if (!fullStars.ContainsKey(catGrade)) return;

        GameObject fullStar = fullStars[catGrade];
        if (fullStar != null)
        {
            Transform fullStarBG = fullStar.transform.Find("fullStar");
            if (fullStarBG != null)
            {
                Vector2 newOffsetMax = defaultOffset;
                int claimedCount = friendshipInfo.isClaimed.Count(claimed => claimed);
                newOffsetMax.x = defaultOffset.x + (claimedCount * STAR_SPACING);
                fullStarBG.GetComponent<RectTransform>().offsetMax = newOffsetMax;
            }
        }
    }

    // ���� �ر� ���� ������Ʈ �Լ�
    private void UpdateLevelUnlockStatus(CatFriendship friendship, int catGrade)
    {
        int currentLevel = currentLevels[catGrade];

        for (int i = 0; i < 5; i++)
        {
            // �̹� ������ ���� ������ �׻� �ر� ���� ����
            if (friendship.rewardsClaimed[i])
            {
                friendship.isLevelUnlocked[i] = true;
                buttonUnlockStatus[catGrade][i] = true;
                continue;
            }

            if (i <= currentLevel)
            {
                // ���� ���������� ����ġ üũ
                bool isUnlocked = friendship.currentExp >= levelByGrade[catGrade - 1][i].exp;
                friendship.isLevelUnlocked[i] = isUnlocked;
                buttonUnlockStatus[catGrade][i] = isUnlocked;
            }
            else
            {
                // ���� ������ ��� ��� (������ ���� ���� ������)
                friendship.isLevelUnlocked[i] = false;
                buttonUnlockStatus[catGrade][i] = false;
            }
        }
    }

    #endregion


    #region Friendship Passive Effect System

    // �нú� ȿ�� ���� ����Ʈ
    private readonly List<string> passiveEffectTypes = new List<string>
    {
        "���ݷ� 1% ����",
        "���ݷ� 2% ����",
        "���ݷ� 3% ����",
        "���ݷ� 4% ����",
        "���ݷ� 5% ����",
        "����� ���� ���� 1 ����",
        "���� �ӵ� 0.05�� ����",
        "���� ȹ�� �ӵ� 0.05�� ����",
        "���� ���̾� ȹ�淮 1 ����",
        "���� ���̾� ȹ�� ��Ÿ�� 1�� ����",
        "���� ���̾� ȹ�淮 5 ����",
        "���� ���̾� ȹ�� ��Ÿ�� 1�� ����",
        "���� ȹ�� ���� ���� �ð� 1�� ����",
        "���� ȹ�� ���� ��Ÿ�� 1�� ����"
    };

    // �нú� ȿ�� ���� �Լ�
    private void ApplyPassiveEffect(int catGrade, int level)
    {
        var passiveEffect = FriendshipDataLoader.Instance.GetDataByGrade(catGrade)[level].passive;
        var friendship = catFriendships[catGrade];

        if (!friendship.activePassiveEffects.Contains(passiveEffect))
        {
            friendship.activePassiveEffects.Add(passiveEffect);

            //Debug.Log($"[{catGrade}��� �����] Ȱ��ȭ�� �нú� ȿ�� ���: {string.Join(", ", friendship.activePassiveEffects)}");

            // �нú� ȿ�� ��Ī �� ����
            int effectIndex = passiveEffectTypes.IndexOf(passiveEffect);
            if (effectIndex != -1)
            {
                switch (effectIndex)
                {
                    case 0:
                        ApplyAttackDamageBuff(0.01f);
                        break;
                    case 1:
                        ApplyAttackDamageBuff(0.02f);
                        break;
                    case 2:
                        ApplyAttackDamageBuff(0.03f);
                        break;
                    case 3:
                        ApplyAttackDamageBuff(0.04f);
                        break;
                    case 4:
                        ApplyAttackDamageBuff(0.05f);
                        break;
                    case 5:
                        ApplyCatCapacityIncrease();
                        break;
                    case 6:
                        ApplyAttackSpeedBuff();
                        break;
                    case 7:
                        ApplyCoinCollectSpeedBuff();
                        break;
                    case 8:
                        ApplyCashForTimeAmountBuff();
                        break;
                    case 9:
                        ApplyCashForTimeCoolTimeBuff();
                        break;
                    case 10:
                        ApplyCashForAdAmountBuff();
                        break;
                    case 11:
                        ApplyCashForAdCoolTimeBuff();
                        break;
                    case 12:
                        ApplyDoubleCoinForAdDurationBuff();
                        break;
                    case 13:
                        ApplyDoubleCoinForAdCoolTimeBuff();
                        break;
                }
            }
        }
    }

    // ���ݷ� 1~5% ���� ȿ�� �нú� �Լ�
    private void ApplyAttackDamageBuff(float percentage)
    {
        var allCats = GameManager.Instance.AllCatData;
        for (int i = 0; i < allCats.Length; i++)
        {
            if (allCats[i] != null)
            {
                allCats[i].AddPassiveAttackDamageBuff(percentage);
            }
        }

        var activeCats = SpawnManager.Instance.GetActiveCats();
        foreach (var catObj in activeCats)
        {
            catObj.GetComponent<CatData>().SetCatData(catObj.GetComponent<CatData>().catData);
        }
    }

    // ����� ���� ���� 1 ���� �нú� �Լ�
    private void ApplyCatCapacityIncrease() 
    {
        GameManager.Instance.AddPassiveCatCapacity(1);
    }

    // ���� �ӵ� 0.05�� ���� �нú� �Լ�
    private void ApplyAttackSpeedBuff() 
    {
        var allCats = GameManager.Instance.AllCatData;
        for (int i = 0; i < allCats.Length; i++)
        {
            if (allCats[i] != null)
            {
                allCats[i].AddPassiveAttackSpeedBuff(0.05f);
            }
        }

        var activeCats = SpawnManager.Instance.GetActiveCats();
        foreach (var catObj in activeCats)
        {
            catObj.GetComponent<CatData>().SetCatData(catObj.GetComponent<CatData>().catData);
        }
    }

    // ���� ȹ�� �ӵ� 0.05�� ���� �нú� �Լ�
    private void ApplyCoinCollectSpeedBuff()
    {
        var allCats = GameManager.Instance.AllCatData;
        for (int i = 0; i < allCats.Length; i++)
        {
            if (allCats[i] != null)
            {
                allCats[i].AddPassiveCoinCollectSpeedBuff(0.05f);
            }
        }

        var activeCats = SpawnManager.Instance.GetActiveCats();
        foreach (var catObj in activeCats)
        {
            catObj.GetComponent<CatData>().SetCatData(catObj.GetComponent<CatData>().catData);
        }
    }

    // ���� ���̾� ȹ�淮 1 ���� �нú� �Լ�
    private void ApplyCashForTimeAmountBuff()
    {
        ShopManager.Instance.AddPassiveCashForTimeAmount(1);
    }

    // ���� ���̾� ȹ�� ��Ÿ�� 1�� ���� �нú� �Լ�
    private void ApplyCashForTimeCoolTimeBuff()
    {
        ShopManager.Instance.AddPassiveCashForTimeCoolTimeReduction(1);
    }

    // ���� ���̾� ȹ�淮 5 ���� �нú� �Լ�
    private void ApplyCashForAdAmountBuff()
    {
        ShopManager.Instance.AddPassiveCashForAdAmount(5);
    }

    // ���� ���̾� ȹ�� ��Ÿ�� 1�� ���� �нú� �Լ�
    private void ApplyCashForAdCoolTimeBuff()
    {
        ShopManager.Instance.AddPassiveCashForAdCoolTimeReduction(1);
    }

    // ���� ȹ�� ���� ���� �ð� 1�� ���� �нú� �Լ�
    private void ApplyDoubleCoinForAdDurationBuff()
    {
        ShopManager.Instance.AddPassiveDoubleCoinDurationIncrease(1f);
    }

    // ���� ȹ�� ���� ��Ÿ�� 1�� ���� �нú� �Լ�
    private void ApplyDoubleCoinForAdCoolTimeBuff()
    {
        ShopManager.Instance.AddPassiveDoubleCoinForAdCoolTimeReduction(1);
    }

    #endregion


    #region Data Management

    // Ư�� ������� ������ ���� ��ȸ �Լ�
    private (int currentExp, bool[] isUnlocked, bool[] isClaimed) GetFriendshipInfo(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade))
        {
            return (0, new bool[5], new bool[5]);
        }

        var friendship = catFriendships[catGrade];
        return (friendship.currentExp, friendship.isLevelUnlocked, friendship.rewardsClaimed);
    }

    // Ư�� ����� ����̰� ���� �� �ִ� ������ �ִ��� Ȯ���ϴ� �Լ�
    public bool HasUnclaimedFriendshipRewards(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        for (int i = 0; i < friendship.isLevelUnlocked.Length; i++)
        {
            // �ش� ������ �رݵǾ���, ���� ������ ���� ���� ���¶�� true ��ȯ
            if (friendship.isLevelUnlocked[i] && !friendship.rewardsClaimed[i])
            {
                return true;
            }
        }
        return false;
    }

    // ���� ���� ���� ���� Ȯ�� �Լ�
    private bool CanClaimLevelReward(int catGrade, int level)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.isLevelUnlocked[level] && !friendship.rewardsClaimed[level];
    }

    // ���� �ݾ� ��ȸ �Լ�
    private int GetRewardAmount(int level)
    {
        if (level >= 0 && level < rewardAmounts.Length)
        {
            return rewardAmounts[level];
        }
        return 0;
    }

    // ���� ���� ó�� �Լ�
    private void ClaimReward(int catGrade, int level)
    {
        if (!CanClaimLevelReward(catGrade, level)) return;

        var friendship = catFriendships[catGrade];
        friendship.rewardsClaimed[level] = true;

        // Cash ���� ����
        GameManager.Instance.Cash += GetRewardAmount(level);

        // �нú� ȿ�� ����
        ApplyPassiveEffect(catGrade, level);

        UpdateFriendshipUI(catGrade);

        // DictionaryManager�� New Image ���µ� ������Ʈ
        DictionaryManager.Instance.UpdateNewImageStatus();
    }

    // ���� ���� �ʿ� ����ġ ��ȸ �Լ�
    private int GetNextLevelExp(int catGrade)
    {
        int currentLevel = currentLevels[catGrade];
        return levelByGrade[catGrade - 1][currentLevel].exp;
    }

    // �ִ� ���� ���� ���� Ȯ�� �Լ�
    private bool IsMaxLevel(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.rewardsClaimed.All(claimed => claimed); // ��� ������ �޾Ҵ��� Ȯ��
    }

    #endregion


    #region Utility

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            Transform found = FindDeepChild(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public List<CatFriendshipSaveData> friendshipList = new List<CatFriendshipSaveData>();
    }

    [Serializable]
    private class CatFriendshipSaveData
    {
        public int catGrade;                        // ����� ���
        public int currentExp;                      // ���� ����ġ
        public int currentLevel;                    // ���� ����
        public bool[] isLevelUnlocked;              // ���� �ر� ����
        public bool[] rewardsClaimed;               // ���� ���� ����
        public List<string> activePassiveEffects;   // Ȱ��ȭ�� �нú� ȿ�� ���
    }

    public string GetSaveData()
    {
        SaveData saveData = new SaveData();

        foreach (var pair in catFriendships)
        {
            saveData.friendshipList.Add(new CatFriendshipSaveData
            {
                catGrade = pair.Value.catGrade,
                currentExp = pair.Value.currentExp,
                currentLevel = currentLevels[pair.Key],
                isLevelUnlocked = pair.Value.isLevelUnlocked,
                rewardsClaimed = pair.Value.rewardsClaimed,
                activePassiveEffects = pair.Value.activePassiveEffects
            });
        }

        return JsonUtility.ToJson(saveData);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        // ������ ����
        catFriendships.Clear();
        currentLevels.Clear();
        buttonUnlockStatus.Clear();

        foreach (var savedItem in savedData.friendshipList)
        {
            catFriendships[savedItem.catGrade] = new CatFriendship(savedItem.catGrade)
            {
                currentExp = savedItem.currentExp,
                isLevelUnlocked = savedItem.isLevelUnlocked,
                rewardsClaimed = savedItem.rewardsClaimed,
                activePassiveEffects = savedItem.activePassiveEffects ?? new List<string>()
            };

            currentLevels[savedItem.catGrade] = savedItem.currentLevel;
            buttonUnlockStatus[savedItem.catGrade] = new bool[5];

            // ����� �нú� ȿ������ �ٽ� ����
            if (savedItem.activePassiveEffects != null)
            {
                foreach (var effect in savedItem.activePassiveEffects)
                {
                    int effectIndex = passiveEffectTypes.IndexOf(effect);
                    if (effectIndex != -1)
                    {
                        switch (effectIndex)
                        {
                            case 0:
                                ApplyAttackDamageBuff(0.01f);
                                break;
                            case 1:
                                ApplyAttackDamageBuff(0.02f);
                                break;
                            case 2:
                                ApplyAttackDamageBuff(0.03f);
                                break;
                            case 3:
                                ApplyAttackDamageBuff(0.04f);
                                break;
                            case 4:
                                ApplyAttackDamageBuff(0.05f);
                                break;
                            case 5:
                                ApplyCatCapacityIncrease();
                                break;
                            case 6:
                                ApplyAttackSpeedBuff();
                                break;
                            case 7:
                                ApplyCoinCollectSpeedBuff();
                                break;
                            case 8:
                                ApplyCashForTimeAmountBuff();
                                break;
                            case 9:
                                ApplyCashForTimeCoolTimeBuff();
                                break;
                            case 10:
                                ApplyCashForAdAmountBuff();
                                break;
                            case 11:
                                ApplyCashForAdCoolTimeBuff();
                                break;
                            case 12:
                                ApplyDoubleCoinForAdDurationBuff();
                                break;
                            case 13:
                                ApplyDoubleCoinForAdCoolTimeBuff();
                                break;
                        }
                    }
                }
            }
        }

        StartCoroutine(UpdateAllFriendshipUI());

        isDataLoaded = true;
    }

    // ������ �ε� �� ������ UI ������Ʈ �ڷ�ƾ
    private IEnumerator UpdateAllFriendshipUI()
    {
        // �� ������ ���
        yield return null;

        // �رݵ� ��� ������� ������ ���� ������Ʈ
        if (DictionaryManager.Instance != null)
        {
            for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
            {
                if (DictionaryManager.Instance.IsCatUnlocked(i))
                {
                    // �� ������� UI ������Ʈ
                    if (dictionarySlotParent != null)
                    {
                        Transform slot = dictionarySlotParent.GetChild(i);
                        if (slot != null)
                        {
                            GameObject friendshipNewImage = slot.transform.Find("Button/New Image")?.gameObject;
                            if (friendshipNewImage != null)
                            {
                                bool hasRewards = HasUnclaimedFriendshipRewards(i + 1);
                                friendshipNewImage.SetActive(hasRewards);
                            }
                        }
                    }
                }
            }
        }

        // ��� UI ������Ʈ�� �Ϸ�� �� DictionaryManager�� New Image ���� ������Ʈ
        if (DictionaryManager.Instance != null)
        {
            DictionaryManager.Instance.UpdateNewImageStatus();
        }
    }

    #endregion


}
