using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int grade;                               // ��� (int)
    public int[] expRequirements = new int[5];      // 1~5�ܰ� ����ġ �䱸��
    public int[] rewards = new int[5];              // 1~5�ܰ� ���� (int�� ����)
    public string[] passiveEffects = new string[5]; // 1~5�ܰ� �нú� ȿ�� ��ġ
}

// ������ �����͸� �ε��ϰ� �����ϴ� ��ũ��Ʈ
[DefaultExecutionOrder(-10)]
public class FriendshipDataLoader : MonoBehaviour
{


    #region Variables

    public static FriendshipDataLoader Instance { get; private set; }

    // ����� �����͸� ������ Dictionary
    public Dictionary<int, List<(int grade, int exp, int reward, string passive)>> dataByGrade = 
        new Dictionary<int, List<(int grade, int exp, int reward, string passive)>>();

    private readonly LevelData levelData = new LevelData();
    private readonly List<(int grade, int exp, int reward, string passive)> gradeDataList = 
        new List<(int grade, int exp, int reward, string passive)>(5);

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
            return;
        }

        LoadCSV("FriendshipDB");

        //PrintLevelData();
    }

    #endregion


    #region Data Loading

    // CSV ������ �а� �����͸� �Ľ��Ͽ� �����ϴ� �Լ�
    private void LoadCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            //Debug.LogError("CSV ������ ã�� �� �����ϴ�: " + fileName);
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            if (values.Length < 16)
            {
                //Debug.LogWarning($"������ ���� (���� {i + 1}): {line}");
                continue;
            }

            if (!ParseLevelData(values, i + 1, levelData)) continue;

            // ��ȣ���� ������ �߰�
            if (!dataByGrade.ContainsKey(levelData.grade))
            {
                gradeDataList.Clear(); // ����Ʈ ����
                for (int k = 0; k < 5; k++)
                {
                    gradeDataList.Add((levelData.grade,
                                     levelData.expRequirements[k],
                                     levelData.rewards[k],
                                     levelData.passiveEffects[k]));
                }
                dataByGrade[levelData.grade] = new List<(int, int, int, string)>(gradeDataList);
            }
        }
    }

    // CSV �����͸� �Ľ��Ͽ� LevelData ��ü�� �����ϴ� �Լ�
    private bool ParseLevelData(string[] values, int lineNumber, LevelData data)
    {
        if (!int.TryParse(values[0].Trim(), out data.grade))
        {
            //Debug.LogError($"��� �Ľ� ���� (���� {lineNumber}): {values[0]}");
            return false;
        }

        for (int j = 0; j < 5; j++)
        {
            if (!int.TryParse(values[1 + j].Trim(), out data.expRequirements[j]))
            {
                //Debug.LogError($"����ġ �Ľ� ���� (���� {lineNumber}): {values[1 + j]}");
                return false;
            }

            if (!int.TryParse(values[6 + j].Trim(), out data.rewards[j]))
            {
                //Debug.LogError($"���� �Ľ� ���� (���� {lineNumber}): {values[6 + j]}");
                return false;
            }

            data.passiveEffects[j] = values[11 + j].Trim();
        }

        return true;
    }

    //// ������ �Լ�
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

    // Resources �������� ��������Ʈ �ε��ϴ� �Լ�
    //private Sprite LoadSprite(string path)
    //{
    //    Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
    //    if (sprite == null)
    //    {
    //        Debug.LogError($"�̹����� ã�� �� �����ϴ�: {path}");
    //    }
    //    return sprite;
    //}

    #endregion


    #region Data Access

    // Ư�� ����� �����͸� ��ȯ�ϴ� �Լ�
    public List<(int grade, int exp, int reward, string passive)> GetDataByGrade(int grade)
    {
        if (dataByGrade.ContainsKey(grade))
        {
            return dataByGrade[grade];
        }
        //Debug.LogWarning($"No data found for number {grade}");
        return null;
    }

    #endregion


}
