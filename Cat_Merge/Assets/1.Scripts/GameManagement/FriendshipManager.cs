using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class CatFriendship
{
    public int catGrade;           // ����� ���
    public int currentExp;         // ���� ����ġ
    public bool[] isLevelUnlocked; // �� ���� �ر� ����
    public bool[] rewardsClaimed;  // �� ������ ���� ���� ����

    public CatFriendship(int grade)
    {
        catGrade = grade;
        currentExp = 0;
        isLevelUnlocked = new bool[5];
        rewardsClaimed = new bool[5];
    }
}

public class FriendshipManager : MonoBehaviour
{
    // ���� ��ȯ�� ����ġ 1 ȹ��
    // ���ϵ�� ������ ����ġ 2 ȹ��
    // ���� ��ȯ�� ����ġ 1 ȹ��
    // ��������� ������ �Ǿ� ��ȯ�� �� ������� ����ġ 1ȹ��

    public static FriendshipManager Instance { get; private set; }

    // ������ ���� �ݾ� ����
    private int[] rewardAmounts = new int[] { 5, 10, 15, 20, 25 };

    [SerializeField] public TextMeshProUGUI expRequirementText;
    [SerializeField] public Slider expGauge;

    // ======================================================================================================================

    // �� ����̺� ������ ���� ����
    private Dictionary<int, CatFriendship> catFriendships = new Dictionary<int, CatFriendship>();

    // ������ �ʿ� ����ġ ������
    private Dictionary<int, List<(int exp, int reward)>> levelByGrade = new Dictionary<int, List<(int exp, int reward)>>();

    // ���� ������ �����ϱ� ���� ���� �߰�
    private Dictionary<int, int> currentLevels = new Dictionary<int, int>();

    // FriendshipManager Ŭ������ ���� ���� �߰�
    private Dictionary<int, bool[]> buttonUnlockStatus = new Dictionary<int, bool[]>();

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

        InitializeCatFriendships();
        InitializeCurrentLevels();
    }

    private void Start()
    {
        // ��� ����� ����ġ �����͸� �ʱ�ȭ
        for (int i = 0; i < 3; i++)
        {
            levelByGrade[i] = FriendshipDataLoader.Instance.GetDataByGrade(i + 1)
                .Select(data => (data.exp, data.reward))
                .ToList();
        }

        expGauge.value = 0f;
    }

    private void InitializeCatFriendships()
    {
        // Initialize all cat friendships and button states
        for (int i = 1; i <= 60; i++)
        {
            catFriendships[i] = new CatFriendship(i);
            buttonUnlockStatus[i] = new bool[5]; // �� ����̺��� 5���� ��ư ���� ����
        }
    }

    private void InitializeCurrentLevels()
    {
        // ��� ������� ���� ������ 0���� �ʱ�ȭ
        for (int i = 1; i <= 60; i++)
        {
            currentLevels[i] = 0;
        }
    }

    // ����ġ �߰� �� ���� üũ
    public void AddExperience(int catGrade, int expAmount)
    {
        if (!catFriendships.ContainsKey(catGrade) || IsMaxLevel(catGrade)) return; // MAX �����̸� ����ġ ȹ�� �ߴ�

        var friendship = catFriendships[catGrade];
        friendship.currentExp += expAmount;

        UpdateFriendshipUI(catGrade);
    }

    // UI ������Ʈ
    public void UpdateFriendshipUI(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        // ���� Dictionary���� ���õ� ����̿� ������Ʈ�Ϸ��� ����̰� �ٸ��� UI ������Ʈ ��ŵ
        if (DictionaryManager.Instance.GetCurrentSelectedCatGrade() != catGrade &&
            DictionaryManager.Instance.GetCurrentSelectedCatGrade() != -1)
        {
            DictionaryManager.Instance.UpdateFriendshipButtonStates(catGrade);
            return;
        }

        var friendship = catFriendships[catGrade];
        int currentLevel = currentLevels[catGrade];

        // ��ư Ŭ������ ���� ���� ȹ�� ó��
        if (DictionaryManager.Instance.buttonClick)
        {
            // ���� ������ ����ġ��ŭ ����
            friendship.currentExp -= levelByGrade[catGrade - 1][currentLevel].exp;

            // ���� ���� ���
            currentLevel = Mathf.Min(currentLevel + 1, 4);
            currentLevels[catGrade] = currentLevel;

            DictionaryManager.Instance.buttonClick = false;
        }

        // UI �ؽ�Ʈ ������Ʈ
        if (expRequirementText != null && DictionaryManager.Instance.GetCurrentSelectedCatGrade() == catGrade)
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

        // ���� �ر� ���� ������Ʈ
        UpdateLevelUnlockStatus(friendship, catGrade);

        // ��ư ���� ������Ʈ
        DictionaryManager.Instance.UpdateFriendshipButtonStates(catGrade);
    }

    // UpdateLevelUnlockStatus �޼��� ����
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

    // Ư�� ������� ������ ���� ��������
    public (int currentExp, bool[] isUnlocked, bool[] isClaimed) GetFriendshipInfo(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade))
            return (0, new bool[5], new bool[5]);

        var friendship = catFriendships[catGrade];
        return (friendship.currentExp, friendship.isLevelUnlocked, friendship.rewardsClaimed);
    }

    // ���� ���� ���� ���� Ȯ��
    public bool CanClaimLevelReward(int catGrade, int level)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.isLevelUnlocked[level] && !friendship.rewardsClaimed[level];
    }

    // ���� �ݾ� ��������
    public int GetRewardAmount(int level)
    {
        if (level >= 0 && level < rewardAmounts.Length)
        {
            return rewardAmounts[level];
        }
        return 0;
    }

    // ���� ����
    public void ClaimReward(int catGrade, int level)
    {
        if (!CanClaimLevelReward(catGrade, level)) return;

        var friendship = catFriendships[catGrade];
        friendship.rewardsClaimed[level] = true;
        GameManager.Instance.Cash += GetRewardAmount(level);

        UpdateFriendshipUI(catGrade);
    }

    // ���ο� �޼��� �߰�
    private int GetNextLevelExp(int catGrade)
    {
        int currentLevel = currentLevels[catGrade];
        return levelByGrade[catGrade - 1][currentLevel].exp;
    }

    // FriendshipManager�� ���ο� �޼��� �߰�
    public bool IsMaxLevel(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;
        var friendship = catFriendships[catGrade];
        return friendship.rewardsClaimed.All(claimed => claimed); // ��� ������ �޾Ҵ��� Ȯ��
    }

}
