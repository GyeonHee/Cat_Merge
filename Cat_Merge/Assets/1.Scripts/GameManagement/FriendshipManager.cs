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
         //expRequirementText.text= $"{nowExp} / {FriendshipDataLoader.Instance.levelDataList[0].expRequirements[0]}";
    }

    private void Start()
    {
        nowExp = 0;
        expRequirementText.text= ($"{nowExp} / {FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp}");
        expGauge.value = 0.00f;

        //Debug.Log($"{nowExp} / {FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp}");
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
