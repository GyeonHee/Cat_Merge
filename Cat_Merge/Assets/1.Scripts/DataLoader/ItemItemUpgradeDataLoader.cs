using System.Collections.Generic;
using UnityEngine;

// ������ �����͸� �ε��ϰ� �����ϴ� ��ũ��Ʈ
[DefaultExecutionOrder(-10)]
public class ItemItemUpgradeDataLoader : MonoBehaviour
{


    #region Variables

    public static ItemItemUpgradeDataLoader Instance { get; private set; }

    // ����� �����͸� ������ Dictionary
    public Dictionary<int, List<(string title, int type, int step, float value, decimal fee)>> dataByNumber
        = new Dictionary<int, List<(string title, int type, int step, float value, decimal fee)>>();

    private readonly List<(string title, int type, int step, float value, decimal fee)> tempDataList =
        new List<(string title, int type, int step, float value, decimal fee)>(10);

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

        ParseCSV("ItemUpgradeDB");
    }

    #endregion


    #region Data Loading

    // ������ �����͸� �Ľ��ϰ� Dictionary�� �߰��ϴ� �Լ�
    public void ParseCSV(string fileName)
    {
        // Resources �������� ���� �б�
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);

        if (csvFile == null)
        {
            //Debug.LogError($"CSV file '{fileName}' not found in Resources folder!");
            return;
        }

        // ���� ���� �б�
        string[] lines = csvFile.text.Split('\n');

        // ù ��° ��(���) �����ϰ� ������ �Ľ�
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            string title = values[0];
            int type = int.Parse(values[1]);
            int step = int.Parse(values[2]);
            float value = float.Parse(values[3]);
            decimal fee = decimal.Parse(values[4]);

            // ��ȣ���� ������ �߰�
            if (!dataByNumber.ContainsKey(type))
            {
                tempDataList.Clear();
                dataByNumber[type] = new List<(string title, int type, int step, float value, decimal fee)>();
            }

            tempDataList.Add((title, type, step, value, fee));
            dataByNumber[type] = new List<(string, int, int, float, decimal)>(tempDataList);
        }
    }

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

    // Ư�� ��ȣ�� �ش��ϴ� ������ ��ȯ�ϴ� �Լ�
    public List<(string title, int type, int step, float value, decimal fee)> GetDataByNumber(int typeNum)
    {
        if (dataByNumber.ContainsKey(typeNum))
        {
            return dataByNumber[typeNum];
        }
        //Debug.LogWarning($"No data found for number {typeNum}");
        return null;
    }

    #endregion

    
}
