using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int grade; // ��� (int)
    public int[] expRequirements = new int[5]; // 1~5�ܰ� ����ġ �䱸��
    public int[] rewards = new int[5]; // 1~5�ܰ� ���� (int�� ����)
    public string[] passiveEffects = new string[5]; // 1~5�ܰ� �нú� ȿ�� ��ġ
}

[DefaultExecutionOrder(-10)]
public class FriendshipDataLoader : MonoBehaviour
{

    public static FriendshipDataLoader Instance { get; private set; }

    // ����� �����͸� ������ Dictionary
    public Dictionary<int, List<(int grade, int exp, int reward, string passive)>> dataByGrade = 
        new Dictionary<int, List<(int grade, int exp, int reward, string passive)>>();

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

        LoadCSV("FriendshipDB");

        //PrintLevelData();
    }

    void LoadCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV ������ ã�� �� �����ϴ�: " + fileName);
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // ù ��(���) ����
        {
            string line = lines[i].Trim(); // ���� ����
            if (string.IsNullOrEmpty(line)) continue; // �� �� �ǳʶ�

            string[] values = line.Split(',');

            if (values.Length < 16)
            {
                Debug.LogWarning($"������ ���� (���� {i + 1}): {line}");
                continue;
            }

            LevelData data = new LevelData();

            // grade (���) ��ȯ
            if (int.TryParse(values[0].Trim(), out int parsedGrade))
            {
                data.grade = parsedGrade;
            }
            else
            {
                Debug.LogError($"��� �Ľ� ���� (���� {i + 1}): {values[0]}");
                continue; // ��� ��ȯ ���� �� �ش� ������ ��ŵ
            }

            for (int j = 0; j < 5; j++)
            {
                // ����ġ �䱸�� ��ȯ
                if (int.TryParse(values[1 + j].Trim(), out int exp))
                    data.expRequirements[j] = exp;
                else
                    Debug.LogError($"����ġ �Ľ� ���� (���� {i + 1}): {values[1 + j]}");

                // ���� ��ȯ (string �� int)
                if (int.TryParse(values[6 + j].Trim(), out int reward))
                    data.rewards[j] = reward;
                else
                    Debug.LogError($"���� �Ľ� ���� (���� {i + 1}): {values[6 + j]}");

                // �нú� ȿ�� ��ȯ
                string passive = values[11 + j].Trim();
                data.passiveEffects[j] = passive;
            }

            // ��ȣ���� ������ �߰�
            if (!dataByGrade.ContainsKey(data.grade))
            {
                dataByGrade[data.grade] = new List<(int grade, int exp, int reward, string passive)>();
            }
            for(int k = 0; k < 5; k++)
            {
                dataByGrade[data.grade].Add((data.grade, data.expRequirements[k], data.rewards[k], data.passiveEffects[k]));             
            }

           
        }
    }

    public List<(int grade, int exp, int reward, string passive)> GetDataByGrade(int grade)
    {
        if (dataByGrade.ContainsKey(grade))
        {
            return dataByGrade[grade];
        }
        else
        {
            Debug.LogWarning($"No data found for number {grade}");
            return null;
        }
    }

    //// ������
    //private void PrintLevelData()
    //{
    //    foreach (var data in dataByGrade)
    //    {
    //        Debug.Log($"{data.Key} ���"); // Key = grade

    //        for (int i = 0; i < data.Value.Count; i++) // List ��ȸ
    //        {
    //            var levelInfo = data.Value[i]; // Ʃ�� ������ ����
    //            Debug.Log($"{i + 1}�ܰ� - ����ġ �䱸��: {levelInfo.exp}, ����: {levelInfo.reward}, �нú� ȿ��: {levelInfo.passive}");
    //        }
    //    }
    //}

    // Resources �������� ��������Ʈ �ε� (������ ���ϵ�)
    //private Sprite LoadSprite(string path)
    //{
    //    Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
    //    if (sprite == null)
    //    {
    //        Debug.LogError($"�̹����� ã�� �� �����ϴ�: {path}");
    //    }
    //    return sprite;
    //}

}

