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
    private Dictionary<int, List<(int exp, int reward)>> levelByGrade = new Dictionary<int, List<(int exp, int reward)>>();

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
        //// ������ �ʿ� ����ġ ������ �ʱ�ȭ
        //levelRequirements = FriendshipDataLoader.Instance.GetDataByGrade(1)
        //    .Select(data => (data.exp, data.reward))
        //    .ToList();

        // ��� ����� ����ġ �����͸� �ʱ�ȭ
        for (int i = 0; i < 3; i++)
        {
            levelByGrade[i] = FriendshipDataLoader.Instance.GetDataByGrade(i + 1)
                .Select(data => (data.exp, data.reward))
                .ToList();        
        }

        expGauge.value = 0f;


        saveNextExpLv1 = levelByGrade[0][0].exp;
        saveNextExpLv2 = levelByGrade[1][0].exp;
        saveNextExpLv3 = levelByGrade[2][0].exp;
    }


    private void Update()
    {
        //// �� ������� �ִ� ����ġ ���� üũ
        //foreach (var friendship in catFriendships.Values)
        //{
        //    if (friendship.currentExp >= levelRequirements[4].exp)
        //    {
        //        friendship.currentExp = levelRequirements[4].exp;
        //        UpdateFriendshipUI(friendship.catGrade);
        //    }
        //}

        // �� ������� �ִ� ����ġ ���� üũ
        foreach (var friendship in catFriendships.Values)
        {
            int grade = friendship.catGrade;
            if (levelByGrade.ContainsKey(grade) && friendship.currentExp >= levelByGrade[grade - 1][4].exp)
            {
                friendship.currentExp = levelByGrade[grade - 1][4].exp;
                UpdateFriendshipUI(grade);
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
        if (friendship.currentExp >= levelByGrade[catGrade - 1][4].exp)
        {
            friendship.currentExp = levelByGrade[catGrade - 1][4].exp;
        }

        // �� ���� �ر� ���� üũ
        for (int i = 0; i < 5; i++)
        {
            if (friendship.currentExp >= levelByGrade[catGrade - 1][i].exp)
            {
                friendship.isLevelUnlocked[i] = true;
            }
        }

        UpdateFriendshipUI(catGrade);
    }

    
    int saveNextExpLv1;
    int saveNextExpLv2;
    int saveNextExpLv3;
    // UI ������Ʈ
    public void UpdateFriendshipUI(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        var friendship = catFriendships[catGrade];

        // ���� ������ ���� ���� ����ġ ���
        int nextLevelExp = levelByGrade[catGrade - 1][0].exp;

        // ���� ����ġ�� �ʿ� ����ġ �̻��� ��
        for (int i = 4; i >= 0; i--)
        {
            if (friendship.currentExp >= levelByGrade[catGrade - 1][i].exp)
            {
                if (DictionaryManager.Instance.buttonClick)
                {
                    friendship.currentExp -= nextLevelExp;
                    nextLevelExp = i < 4 ? levelByGrade[catGrade - 1][i + 1].exp : levelByGrade[catGrade - 1][i].exp;

                    switch (catGrade)
                    {
                        case 1:
                            saveNextExpLv1 = nextLevelExp;
                            expRequirementText.text = $"{friendship.currentExp} / {saveNextExpLv1}";
                            break;
                        case 2:
                            saveNextExpLv2 = nextLevelExp;
                            expRequirementText.text = $"{friendship.currentExp} / {saveNextExpLv2}";
                            break;
                        case 3:
                            saveNextExpLv3 = nextLevelExp;
                            expRequirementText.text = $"{friendship.currentExp} / {saveNextExpLv3}";
                            break;
                    }

                    //saveNextExpLv1 = nextLevelExp;
                    //expRequirementText.text = $"{friendship.currentExp} / {saveNextExpLv1}";
                    DictionaryManager.Instance.buttonClick = false;
                    break;
                }
            }
        }

        // UI �ؽ�Ʈ ������Ʈ
        if (expRequirementText != null)
        {
            switch(catGrade)
            {
                case 1:
                    expRequirementText.text = $"{friendship.currentExp} / {saveNextExpLv1}";
                    break;
                case 2:
                    expRequirementText.text = $"{friendship.currentExp} / {saveNextExpLv2}";
                    break;
                case 3:
                    expRequirementText.text = $"{friendship.currentExp} / {saveNextExpLv3}";
                    break;

            }
            //expRequirementText.text = $"{friendship.currentExp} / {saveNextExpLv1}";
        }

        // ������ ������Ʈ
        if (expGauge != null)
        {
            float progress = 0f;
            switch (catGrade)
            {
                case 1:
                    progress = (float)friendship.currentExp / saveNextExpLv1;
                    break;
                case 2:
                    progress = (float)friendship.currentExp / saveNextExpLv2;
                    break;
                case 3:
                    progress = (float)friendship.currentExp / saveNextExpLv3;
                    break;

            }

            //float progress = (float)friendship.currentExp / saveNextExpLv1;
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
