using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;


[System.Serializable]
public class CatFriendship
{
    public int catGrade;           // ����� ���
    public int currentExp;         // ���� ����ġ
    public int currentLevel;       // ���� ���� (0-4)
    public bool[] rewardsClaimed;  // �� ������ ���� ���� ����

    public CatFriendship(int grade)
    {
        catGrade = grade;
        currentExp = 0;
        currentLevel = 0;
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

    //public List<(int[] exp, int[] reward, int[] passive)> listByGrade = new List<(int[] exp, int[] reward, int[] passive)>();

    [SerializeField] public TextMeshProUGUI expRequirementText;
    [SerializeField] public Slider expGauge;

    //public int nowExp;
    //public bool[] unlockLv = new bool[5];

    // ----------------------------------------------------

    // �� ����̺� ȣ���� ���� ����
    public Dictionary<int, CatFriendship> catFriendships = new Dictionary<int, CatFriendship>();

    // ������ �Ǵ� 1��� ������ ĳ��
    public List<(int grade, int exp, int reward, int passive)> baseData;

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

        //InitializeGradeList();
        InitializeCatFriendships();

    }
    private void Start()
    {
        //unlockLv[0] = true;
        //unlockLv[1] = false;
        //unlockLv[2] = false;
        //unlockLv[3] = false;
        //unlockLv[4] = false;

        //expRequirementText.text= ($"{nowExp} / {FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp}");

        // 1��� �����͸� ���� �����ͷ� ĳ��
        baseData = FriendshipDataLoader.Instance.GetDataByGrade(1);

        //UpdateFriendshipUI(2);
        expGauge.value = 0.00f;

       
    }

    private void Update()
    {
        //for (int i = 0; i < unlockLv.Length; i++)
        //{
        //    if (unlockLv[i])
        //    {
        //        int requiredExp = FriendshipDataLoader.Instance.GetDataByGrade(1)[i].exp;

        //        if (nowExp >= requiredExp)
        //        {
        //            nowExp = requiredExp;
        //        }

        //        expRequirementText.text = $"{nowExp} / {requiredExp}";

        //        UnlockLevels(1, i + 1); // ����ȭ�� UnlockLevels �Լ� ���
        //        break; // �ϳ��� ����ǵ���
        //    }
        //}


        // �� ������� �ִ� ����ġ ���� üũ
        foreach (var friendship in catFriendships.Values)
        {
            if (friendship.currentExp >= baseData[4].exp)
            {
                friendship.currentExp = baseData[4].exp;
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

    // ����ġ �߰�
    public void AddExperience(int catGrade, int expAmount)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        var friendship = catFriendships[catGrade];

        // �ִ� ������ ����ġ�� ���� �ʵ��� ����
        int maxExp = baseData[4].exp;
        friendship.currentExp = Mathf.Min(friendship.currentExp + expAmount, maxExp);

        // ������ üũ
        CheckAndUpdateLevel(catGrade);

        // UI ������Ʈ
        UpdateFriendshipUI(friendship.catGrade);
    }

    // ���� üũ �� ������Ʈ
    private void CheckAndUpdateLevel(int catGrade)
    {
        var friendship = catFriendships[catGrade];

        for (int i = 4; i >= 0; i--)
        {
            if (friendship.currentExp >= baseData[i].exp)
            {
                friendship.currentLevel = i;
                break;
            }
        }
    }

    // ���� ���� ���� ���� Ȯ��
    public bool CanClaimReward(int catGrade)
    {
        var friendship = catFriendships[catGrade];
        return !friendship.rewardsClaimed[friendship.currentLevel];
    }

    // ���� ����
    public int ClaimReward(int catGrade)
    {
        var friendship = catFriendships[catGrade];
        if (!CanClaimReward(catGrade)) return 0;

        int reward = baseData[friendship.currentLevel].reward;
        friendship.rewardsClaimed[friendship.currentLevel] = true;
        return reward;
    }

    // Ư�� ������� ȣ���� ���� ��������
    public (int currentExp, int currentLevel, int nextLevelExp, int reward, int passive) GetFriendshipInfo(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade))
            return (0, 0, 0, 0, 0);

        var friendship = catFriendships[catGrade];

        List<(int grade, int exp, int reward, int passive)> data = FriendshipDataLoader.Instance.GetDataByGrade(catGrade);
        int nextLevelExp = data[0].exp;

        return (
            friendship.currentExp,
            friendship.currentLevel,
            nextLevelExp,
            baseData[friendship.currentLevel].reward,
            baseData[friendship.currentLevel].passive
        );
    }


    // UI ������Ʈ
    public void UpdateFriendshipUI(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        // ���� ���õ� ����̰� �ְ�, �� ������� ������ ���� �ִٸ� �ش� ������� ������ ������Ʈ
        int selectedGrade = DictionaryManager.Instance.GetCurrentSelectedCatGrade();

        if (selectedGrade != -1)
        {
            catGrade = selectedGrade;
        }


        var info = GetFriendshipInfo(catGrade);

        if (expRequirementText != null)
        {
            if (info.currentLevel == 0)
            {
                if (info.currentExp >= info.nextLevelExp)
                {
                    //DictionaryManager.Instance.friendshipUnlockButtonss[info.currentLevel].gameObject.transform.Find("LockBG").gameObject.SetActive(false);
                    //DictionaryManager.Instance.friendshipUnlockButtonss[info.currentLevel].gameObject.transform.Find("FirstOpenBG").gameObject.SetActive(true);

                    // DictionaryManager.Instance.characterButtons[catGrade][info.currentLevel].gameObject.transform.Find("LockBG").gameObject.SetActive(false);               
                    // DictionaryManager.Instance.characterButtons[catGrade][info.currentLevel].gameObject.transform.Find("FirstOpenBG").gameObject.SetActive(true);

                    // buttons[info.currentLevel].gameObject.transform.Find("LockBG").gameObject.SetActive(false);
                    //buttons[info.currentLevel].gameObject.transform.Find("FirstOpenBG").gameObject.SetActive(false);

                }
            }

            Debug.Log($"{catGrade}����� ������� ���� ����ġ : {info.currentExp}, ���� ���� : {info.currentLevel}");
            expRequirementText.text = $"{info.currentExp} / {info.nextLevelExp}";
        }

        if (expGauge != null)
        {
            expGauge.value = (float)info.currentExp / info.nextLevelExp;
        }
    }

    // Ư�� ������ ������ ������ �� �ִ��� Ȯ��
    public bool CanClaimLevelReward(int catGrade, int level)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.currentExp >= baseData[level].exp && !friendship.rewardsClaimed[level];
    }

    //private void UnlockLevels(int grade, int level)
    //{
    //    if (nowExp < FriendshipDataLoader.Instance.GetDataByGrade(grade)[level - 1].exp)
    //        return;

    //    nowExp = FriendshipDataLoader.Instance.GetDataByGrade(grade)[level - 1].exp;

    //    // ��ư, UI ����
    //    DictionaryManager.Instance.friendshipUnlockButtons[level - 1].interactable = true;
    //    DictionaryManager.Instance.friendshipLockImg[level - 1].SetActive(false);
    //    DictionaryManager.Instance.friendshipGetCrystalImg[level - 1].SetActive(true);

    //    // �� ȣ���� ���� �޼� �� ǥ��
    //    RectTransform rectTransform = DictionaryManager.Instance.friendshipStarImg[0].GetComponent<RectTransform>();
    //    Vector2 offset = rectTransform.offsetMax;
    //    offset.x = -168f + (42f * (level - 1)); // level�� ���� ��ġ �ڵ� ����
    //    rectTransform.offsetMax = offset;

    //    // ���� ȣ���� ���� �� ǥ��
    //    rectTransform = DictionaryManager.Instance.friendshipStarImg[level].GetComponent<RectTransform>();
    //    offset = rectTransform.offsetMax;
    //    offset.x = -168f + (42f * (level - 1));
    //    rectTransform.offsetMax = offset;

    //    unlockLv[level - 1] = false;

    //    //if (nowExp[grade - 1] < FriendshipDataLoader.Instance.GetDataByGrade(grade)[level - 1].exp)
    //    //    return;

    //    //nowExp[grade - 1] = FriendshipDataLoader.Instance.GetDataByGrade(grade)[level - 1].exp;

    //    //// ��ư, UI ����
    //    //DictionaryManager.Instance.friendshipUnlockButtons[grade - 1][level - 1].interactable = true;
    //    //DictionaryManager.Instance.friendshipLockImg[grade - 1][level - 1].SetActive(false);
    //    //DictionaryManager.Instance.friendshipGetCrystalImg[grade - 1][level - 1].SetActive(true);

    //    //// �� ȣ���� ���� �޼� �� ǥ��
    //    //RectTransform rectTransform = DictionaryManager.Instance.friendshipStarImg[grade - 1][0].GetComponent<RectTransform>();
    //    //Vector2 offset = rectTransform.offsetMax;
    //    //offset.x = -168f + (42f * (level - 1)); // level�� ���� ��ġ �ڵ� ����
    //    //rectTransform.offsetMax = offset;

    //    //// ���� ȣ���� ���� �� ǥ��
    //    //rectTransform = DictionaryManager.Instance.friendshipStarImg[grade - 1][level].GetComponent<RectTransform>();
    //    //offset = rectTransform.offsetMax;
    //    //offset.x = -168f + (42f * (level - 1));
    //    //rectTransform.offsetMax = offset;

    //    //unlockLv[grade - 1, level - 1] = false;
    //}

    //private void InitializeGradeList()
    //{
    //    for (int grade = 1; grade <= FriendshipDataLoader.Instance.dataByGrade.Count; grade++)
    //    {
    //        var gradeData = FriendshipDataLoader.Instance.GetDataByGrade(grade);

    //        if (gradeData != null)
    //        {
    //            int[] exp = new int[5];
    //            int[] rewards = new int[5];
    //            int[] passiveEffects = new int[5];

    //            // �����͸� ����Ʈ�� ä��
    //            for (int i = 0; i < 5; i++)
    //            {
    //                exp[i] = gradeData[i].exp;
    //                rewards[i] = gradeData[i].reward;
    //                passiveEffects[i] = gradeData[i].passive;
    //            }

    //            // listByGrade�� �߰�
    //            baseData.Add((grade, exp, rewards, passiveEffects));
    //        }
    //        else
    //        {
    //            Debug.LogError($"��� {grade}�� ���� �����Ͱ� �����ϴ�!");
    //        }
    //    }
    //}

    // ------------------------------------------------------------------------------------------------

    ///////////////////////// ���� 0��

    // ���� ������ �нú� ȿ�� �� ��ȯ
    public int GetCurrentPassiveEffect(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return 0;

        var friendship = catFriendships[catGrade];
        return baseData[friendship.currentLevel].passive;
    }

    // ����/�ε� ��� �߰� (���߿� ����)
    public void SaveFriendshipData()
    {
        // TODO: ȣ���� ������ ���� ����
    }

    public void LoadFriendshipData()
    {
        // TODO: ȣ���� ������ �ε� ����
    }

    // Ư�� ������ ���� ����
    public int ClaimLevelReward(int catGrade, int level)
    {
        if (!CanClaimLevelReward(catGrade, level)) return 0;

        var friendship = catFriendships[catGrade];
        friendship.rewardsClaimed[level] = true;
        return baseData[level].reward;
    }

    // Ư�� ������ �޼� ���� Ȯ��
    public bool IsLevelAchieved(int catGrade, int level)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.currentExp >= baseData[level].exp;
    }

    // Ư�� ������� ���� ���� ��������
    public int GetCurrentLevel(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return 0;
        return catFriendships[catGrade].currentLevel + 1;
    }

    // Ư�� ������� ���� ����ġ ��������
    public int GetCurrentExp(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return 0;
        return catFriendships[catGrade].currentExp;
    }
}
