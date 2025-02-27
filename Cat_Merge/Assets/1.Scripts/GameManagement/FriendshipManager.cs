using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class FriendshipManager : MonoBehaviour
{
    // ���� ��ȯ�� ����ġ 1 ȹ��
    // ���ϵ�� ������ ����ġ 2 ȹ��
    // ���� ��ȯ�� ����ġ 1 ȹ��
    // ��������� ������ �Ǿ� ��ȯ�� �� ������� ����ġ 1ȹ��

    public static FriendshipManager Instance { get; private set; }

    public List<(int[] exp, int[] reward, int[] passive)> listByGrade = new List<(int[] exp, int[] reward, int[] passive)>();

    [SerializeField]
    public TextMeshProUGUI expRequirementText;
    [SerializeField]
    public Slider expGauge;

    //object[] grade = new object[60];

    public int nowExp;
    bool[] unlockLv = new bool[5];
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

        InitializeGradeList();
    }

    private void Update()
    {    
        if (unlockLv[0])
        {
            if(nowExp >= FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp)
            {
                nowExp = FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp;
            }
            expRequirementText.text = ($"{nowExp} / {FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp}");
            UnlockStep1();        
        }
    }

    private void Start()
    {
        unlockLv[0] = true;
        unlockLv[1] = false;
        unlockLv[2] = false;
        unlockLv[3] = false;
        unlockLv[4] = false;

        nowExp = 0;
        expRequirementText.text= ($"{nowExp} / {FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp}");
        expGauge.value = 0.00f;

    }

    public void UnlockStep1()
    {
        if (nowExp >= FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp)
        {
            nowExp = FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp;

            DictionaryManager.Instance.friendshipUnlockButtons[0].interactable = true;

            DictionaryManager.Instance.friendshipLockImg[0].SetActive(false);
            DictionaryManager.Instance.friendshipGetCrystalImg[0].SetActive(true);

            // �� ȣ���� ���� �޼� �� ǥ��
            RectTransform rectTransform = DictionaryManager.Instance.friendshipStarImg[0].GetComponent<RectTransform>();
            Vector2 offset = rectTransform.offsetMax;
            offset.x = -168f;
            rectTransform.offsetMax = offset;

            // ȣ���� 1���� �޼� �� ǥ��
            rectTransform = DictionaryManager.Instance.friendshipStarImg[1].GetComponent<RectTransform>();
            offset = rectTransform.offsetMax;
            offset.x = -168f;
            rectTransform.offsetMax = offset;

            unlockLv[0] = false;

        }
    }

    private void InitializeGradeList()
    {
        for (int grade = 1; grade <= 60; grade++)
        {
            var gradeData = FriendshipDataLoader.Instance.GetDataByGrade(grade);

            if (gradeData != null)
            {
                int[] exp = new int[5];
                int[] rewards = new int[5];
                int[] passiveEffects = new int[5];

                // �����͸� ����Ʈ�� ä��
                for (int i = 0; i < 5; i++)
                {
                    exp[i] = gradeData[i].exp;
                    rewards[i] = gradeData[i].reward;
                    passiveEffects[i] = gradeData[i].passive;
                }

                // listByGrade�� �߰�
                listByGrade.Add((exp, rewards, passiveEffects));
            }
            else
            {
                Debug.LogError($"��� {grade}�� ���� �����Ͱ� �����ϴ�!");
            }
        }
    }
    //private void a()
    //{
    //    // FriendshipDataLoader���� ������ ����ϱ�
    //    if (FriendshipDataLoader.Instance != null && FriendshipDataLoader.Instance.levelDataList.Count > 0)
    //    {
    //        // ù ��° ���� ������ ���÷� ���
    //        var firstLevelData = FriendshipDataLoader.Instance.levelDataList[0]; // ù ��° ������ ��������
    //        Debug.Log($"ù ��° ����: {firstLevelData.grade}");
    //        for (int i = 0; i < 5; i++)
    //        {
    //            Debug.Log($"�ܰ� {i + 1} - ����ġ �䱸��: {firstLevelData.expRequirements[i]}, ����: {firstLevelData.rewards[i]}, �нú� ȿ��: {firstLevelData.passiveEffects[i]}");
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("FriendshipDataLoader�� �����Ͱ� �ε���� �ʾҽ��ϴ�!");
    //    }
    //}
}
