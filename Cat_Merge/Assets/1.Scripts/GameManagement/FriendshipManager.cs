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

    // �� ����̺� ȣ���� ���� ����
    private Dictionary<int, CatFriendship> catFriendships = new Dictionary<int, CatFriendship>();

    // ������ �ʿ� ����ġ ������
    private List<(int exp, int reward)> levelRequirements;

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
    }


    private void Start()
    {
        // ������ �ʿ� ����ġ ������ �ʱ�ȭ
        levelRequirements = FriendshipDataLoader.Instance.GetDataByGrade(1)
            .Select(data => (data.exp, data.reward))
            .ToList();

        expGauge.value = 0f;
    }


    private void Update()
    {
        // �� ������� �ִ� ����ġ ���� üũ
        foreach (var friendship in catFriendships.Values)
        {
            if (friendship.currentExp >= levelRequirements[4].exp)
            {
                friendship.currentExp = levelRequirements[4].exp;
                UpdateFriendshipUI(friendship.catGrade);
            }
        }
    }


    private void InitializeCatFriendships()
    {
        // Initialize all cat friendships
        for (int i = 1; i <= 60; i++)
        {
            catFriendships[i] = new CatFriendship(i);
        }
    }

    // ����ġ �߰� �� ���� üũ
    public void AddExperience(int catGrade, int expAmount)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        var friendship = catFriendships[catGrade];
        friendship.currentExp += expAmount;

        // �ִ� ����ġ ����
        if (friendship.currentExp >= levelRequirements[4].exp)
        {
            friendship.currentExp = levelRequirements[4].exp;
        }

        // �� ���� �ر� ���� üũ
        for (int i = 0; i < 5; i++)
        {
            if (friendship.currentExp >= levelRequirements[i].exp)
            {
                friendship.isLevelUnlocked[i] = true;
            }
        }

        UpdateFriendshipUI(catGrade);
    }

    // UI ������Ʈ
    public void UpdateFriendshipUI(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        var friendship = catFriendships[catGrade];

        // ���� ���õ� ����̰� �ְ�, �� ������� ������ ���� ���� ���� UI ������Ʈ
        int selectedGrade = DictionaryManager.Instance.GetCurrentSelectedCatGrade();
        if (selectedGrade != -1 && selectedGrade != catGrade)
        {
            // ���õ� ����̿� �ٸ� ������� ����ġ�� ����� ���
            // UI ������Ʈ�� ���� �ʰ� ��ư ���¸� ������Ʈ
            DictionaryManager.Instance.UpdateFriendshipButtonStates(catGrade);
            return;
        }

        // ���� ������ ���� ���� ����ġ ���
        int currentLevel = 0;
        int nextLevelExp = levelRequirements[0].exp;

        for (int i = 4; i >= 0; i--)
        {
            if (friendship.currentExp >= levelRequirements[i].exp)
            {
                currentLevel = i;
                nextLevelExp = i < 4 ? levelRequirements[i + 1].exp : levelRequirements[i].exp;
                break;
            }
        }

        // UI �ؽ�Ʈ ������Ʈ
        if (expRequirementText != null)
        {
            expRequirementText.text = $"{friendship.currentExp} / {nextLevelExp}";
        }

        // ������ ������Ʈ
        if (expGauge != null)
        {
            float progress = (float)friendship.currentExp / nextLevelExp;
            expGauge.value = Mathf.Clamp01(progress);
        }

        // ��ư ���� ������Ʈ
        DictionaryManager.Instance.UpdateFriendshipButtonStates(catGrade);
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

}
